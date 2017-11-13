// Copyright (c) 2017, Marko Vasic and Zuhair Parvez
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Diagnostics;

namespace EkstaziSharp.Util
{
    public static class CILTransformer
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Contains mapping from absolute path of a module to ModuleDefinition.
        /// TODO: Consider using WeakReference for the values
        /// </summary>
        private static Dictionary<string, Tuple<DateTime, ModuleDefinition>> moduleDictionary = new Dictionary<string, Tuple<DateTime, ModuleDefinition>>();

        private static void UpdateModuleDictionary(ModuleDefinition module, string moduleFilePath)
        {
            moduleDictionary[moduleFilePath.CleanFileName()] = new Tuple<DateTime, ModuleDefinition>(File.GetLastWriteTime(moduleFilePath), module);
        }

        /// <summary>
        /// Returns a module definition for a file with a path <paramref name="moduleFilePath"/>
        /// Caches read modules, so if you request twice module on the same path, it will return a reference to the same object.
        /// If object is updated on disk meanwhile (last modified time is different than initially) it will return new version.
        /// </summary>
        /// <param name="moduleFilePath">Path to the module on a disk</param>
        public static ModuleDefinition GetModuleDefinition(String moduleFilePath)
        {
            FileInfo fileInfo = new FileInfo(moduleFilePath);
            if (!fileInfo.Exists)
            {
                throw new ArgumentException("Invalid path to a module: " + moduleFilePath);
            }
            // get absolute path to the module
            string absolutePath = fileInfo.FullName;
            // when module file was last modified
            DateTime lastModified = File.GetLastWriteTime(moduleFilePath);
            ModuleDefinition module = null;
            Tuple<DateTime, ModuleDefinition> moduleTuple;
            moduleDictionary.TryGetValue(absolutePath.CleanFileName(), out moduleTuple);
            if (moduleTuple == null || lastModified != moduleTuple.Item1)
            {
                // if information about module is not in dictionary or if file was overwritten meanwhile
                // Include debug symbols
                // We use this only when we are not using smart checksums
                // TODO: Do not read symbols if we do not compute smart checksums
                ReaderParameters parameters = new ReaderParameters { ReadSymbols = true };
                try
                {
                    module = ModuleDefinition.ReadModule(moduleFilePath, parameters);
                }
                catch (Exception e)
                {
                    // Mono.Cecil reading of symbols breaks in some cases, causing Exception
                    // I observed OutOfMemoryException in case of MoreLinq project
                    // they should add support for reading symbol files of portable version in release 10
                    logger.Warn("Cannot read debugging symbols for following module: " + moduleFilePath);
                    module = ModuleDefinition.ReadModule(moduleFilePath);
                }
                UpdateModuleDictionary(module, absolutePath);
            }
            else
            {
                module = moduleTuple.Item2;
            }
            return module;
        }

        /// <summary>
        /// Writes the module <paramref name="module"/> to the file specified by the path <paramref name="moduleFilePath"/>
        /// </summary>
        /// <param name="module"></param>
        /// <param name="moduleFilePath"></param>
        public static void WriteModule(ModuleDefinition module, String moduleFilePath)
        {
            // Quick fix for AssemblyResolutionException 
            // occurring in RequestReduce and Newtonsoft.Json projects
            // detailed info documented in challenges.txt
            // TODO: Check if there is more elegant solution
            if (module.AssemblyResolver is DefaultAssemblyResolver)
            {
                DefaultAssemblyResolver resolver = module.AssemblyResolver as DefaultAssemblyResolver;
                resolver.AddSearchDirectory(System.IO.Path.GetDirectoryName(module.FullyQualifiedName));
            }
            module.Write(moduleFilePath);
            UpdateModuleDictionary(module, moduleFilePath);
        }

        /// <summary>
        /// Returns <see cref="Mono.Cecil.ModuleDefinition"/> for provided <see cref="System.Reflection.Module"/>
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static ModuleDefinition ConvertToModuleDefinition(Module module)
        {
            if (module == null)
            {
                return null;
            }
            return GetModuleDefinition(module.FullyQualifiedName);
        }

        /// <summary>
        /// Replaces all calls to a method <paramref name="oldMethodName"/> with the calls to a method <paramref name="newMethodName"/>.
        /// Both method that should be replaced and replacing method should be in module <paramref name="moduleFileName"/>.
        /// </summary>
        /// <param name="oldMethodName"></param>
        /// <param name="newMethodName"></param>
        public static void ReplaceCalls(ModuleDefinition module, String oldMethodName, String newMethodName)
        {
            foreach (var targetType in module.Types)
            {
                ReplaceCalls(targetType, oldMethodName, newMethodName);
            }
        }

        public static void ReplaceCalls(TypeDefinition targetType, String oldMethodName, String newMethodName)
        {
            try
            {
                var newMethod = targetType.Methods.Single(m => m.Name == newMethodName);

                foreach (MethodDefinition method in targetType.Methods)
                {
                    var il = method.Body.GetILProcessor();
                    var callToNewMethod = il.Create(OpCodes.Call, newMethod);
                    var callsToOldMethod = method
                        .Body
                        .Instructions
                        .Where(i =>
                        {
                            return i.OpCode == OpCodes.Call && ((MethodReference)i.Operand).Name == oldMethodName;
                        });

                    foreach (var call in callsToOldMethod.ToList())
                    {
                        il.Replace(call, callToNewMethod);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        /// <summary>
        /// Inserts call to the method <paramref name="callee"/> at the beggining of the method <paramref name="caller"/>.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="callee"></param>
        public static Instruction InsertMethodCallAtBeginning(MethodDefinition caller, MethodReference callee)
        {
            if (caller == null || callee == null || !caller.HasBody || caller.Body.Instructions.Count == 0)
            {
                return null;
            }

            MethodReference methodToCall = null;
            if (caller.Module.FullyQualifiedName != callee.Module.FullyQualifiedName)
            {
                // if method to be called is not in the same module where from it will be called
                // we have to import method to be called to the module,
                // otherwise exception will be thrown
                methodToCall = caller.Module.Import(callee);
            }
            else
            {
                methodToCall = callee;
            }

            var il = caller.Body.GetILProcessor();

            // Creates the CIL instruction for calling our method
            Instruction callInstruction = il.Create(OpCodes.Call, methodToCall);
            Instruction firstInstruction = caller.Body.Instructions[0];
            il.InsertBefore(firstInstruction, callInstruction);
            return callInstruction;
        }

        /// <summary>
        /// Returns all types within <paramref name="module"/> that contain custom attribute of type <paramref name="attributeName"/>        /// 
        /// </summary>
        /// <param name="module">Module which types should be returned</param>
        /// <param name="attributeName">Name of the custom attribute that returned types should have. In format Namespace.Type, e.g. NUnit.Framework.TestFixtureAttribute</param>
        /// <returns>Types with specified characteristics, or NULL if <paramref name="module"/> is NULL.</returns>
        public static IEnumerable<TypeDefinition> GetTypesWithCustomAttribute(ModuleDefinition module, String attributeName)
        {
            if (module == null)
            {
                return null;
            }

            // return all types that meet specified condition
            return module.Types.Where(type =>
            {
                // return types that contain attribute of type attributeName
                return type.CustomAttributes.Where(attribute => attribute.AttributeType.FullName == attributeName).Count() > 0;
            });
        }

        /// <summary>
        /// Adds attribute to <paramref name="method"/>
        /// Requires: Attribute with provided name exists in <paramref name="attributeModule"/>
        /// </summary>
        /// <param name="method">Method to which attribute is to be added.</param>
        /// <param name="attributeModule">Module where attribute to be added is located.</param>
        /// <param name="attributeName">Name of attribute to be added in format Namespace.Type</param>
        public static void AddCustomAttribute(MethodDefinition method, ModuleDefinition attributeModule, String attributeName)
        {
            if (method == null || attributeModule == null || attributeName == null)
            {
                throw new ArgumentNullException();
            }

            TypeDefinition attributeType = attributeModule.GetType(attributeName);
            if (attributeType == null)
            {
                throw new ArgumentException($"Module: {attributeModule.Name}[{attributeModule.RuntimeVersion}] does not contain attribute type: {attributeName}");
            }
            var constructor = attributeType.GetDefaultConstructor();
            var constructorMethodRef = method.Module.Import(constructor);
            method.CustomAttributes.Add(new CustomAttribute(constructorMethodRef));
        }

        public static String GetAssemblyString(String moduleFileName)
        {
            StringBuilder builder = new StringBuilder();
            ModuleDefinition module = CILTransformer.GetModuleDefinition(moduleFileName);
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    foreach (var instruction in method.Body.Instructions)
                    {
                        builder.AppendLine(instruction.ToString());
                    }
                }
            }
            return builder.ToString();
        }
    }
}
