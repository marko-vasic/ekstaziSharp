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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;
using Crc32C;

namespace EkstaziSharp.Util
{
    public static class CecilExtensions
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ICustomAttributeProvider Extensions

        /// <summary>
        /// Searches for custom attribute of provided type.
        /// </summary>
        /// <param name="attributeProvider">Provider in which we are looking for an attribute.</param>
        /// <param name="nameMatch">A function that determines which attribute we are looking for.</param>
        /// <returns>CustomAttribute of specified type if such exists, Null, otherwise.</returns>        ///
        public static CustomAttribute GetCustomAttribute(this ICustomAttributeProvider attributeProvider, Func<string, bool> nameMatch, bool includeSubtypes)
        {
            if (!attributeProvider.HasCustomAttributes)
            {
                return null;
            }

            return attributeProvider.CustomAttributes.FirstOrDefault(attribute =>
            {
                if (nameMatch(attribute.AttributeType.FullName))
                {
                    return true;
                }
                if (includeSubtypes)
                {
                    for (TypeReference parent = attribute.AttributeType.GetParentType(); parent != null; parent = parent.GetParentType())
                    {
                        if (nameMatch(parent.FullName))
                        {
                            return true;
                        }
                    }
                }

                return false;
            });
        }
        
        /// <summary>
        /// Searches for custom attribute of provided type.
        /// </summary>
        /// <param name="attributeProvider">Provider in which we are looking for an attribute.</param>
        /// <param name="attributeFullName">Full name of attribute type we are looking for.</param>
        /// <param name="includeSubtypes">If true, match subtypes of a custom attribute as well, if not only match exact attribute.</param>
        /// <returns>True, if custom attribute of specified type exists, False, otherwise.</returns>
        public static bool HasCustomAttribute(this ICustomAttributeProvider attributeProvider, string attributeFullName, bool includeSubtypes)
        {
            return GetCustomAttribute(attributeProvider, name => name == attributeFullName, includeSubtypes) != null;
        }

        public static bool HasCustomAttribute(this ICustomAttributeProvider attributeProvider, Func<string, bool> nameMatch, bool includeSubtypes)
        {
            return GetCustomAttribute(attributeProvider, nameMatch, includeSubtypes) != null;
        }

        #endregion

        #region ModuleDefinition Extensions

        public static TypeDefinition GetTypeByName(this ModuleDefinition module, string name)
        {
            if (module == null)
            {
                return null;
            }

            return module.Types.FirstOrDefault(t => t.Name == name);
        }

        /// <summary>
        /// Gets the TypeDefinition inside of the module
        /// If module is null returns null.
        /// If there is no such type returns null.
        /// </summary>
        /// <param name="module">Module in which we search for the type</param>
        /// <param name="fullName">Full name of the type we are looking for</param>
        public static TypeDefinition GetTypeByFullName(this ModuleDefinition module, string fullName)
        {
            if (module == null)
            {
                return null;
            }

            return module.Types.FirstOrDefault(t => t.FullName == fullName);
        }

        public static IEnumerable<TypeDefinition> GetTypes(this ModuleDefinition module, bool includeNestedTypes)
        {
            if (module == null || !module.HasTypes)
            {
                return null;
            }

            if (includeNestedTypes)
            {
                return module.GetTypes();
            }
            else
            {
                return module.Types;
            }
        }

        public static MethodDefinition GetMethod(this ModuleDefinition module, string typeFullName, string methodName)
        {
            if (module == null)
            {
                return null;
            }
            TypeDefinition type = module.GetTypeByFullName(typeFullName);
            if (type == null)
            {
                return null;
            }
            return type.GetMethodByName(methodName);
        }

        public static IEnumerable<MethodDefinition> GetConstructors(this ModuleDefinition module)
        {
            if (module == null)
            {
                return null;
            }

            // GetTypes includes nested types
            var types = module.GetTypes(includeNestedTypes: true);
            return types.AsParallel().SelectMany<TypeDefinition, MethodDefinition>(t =>
            {
                return t.GetConstructors();
            });
        }

        /// <summary>
        /// Get all instructions within the <paramref name="module"/> which opcode is from the set <paramref name="opcodes"/>.
        /// </summary>
        /// <returns>
        /// List of the instructions within the module with that have opcode from the provided set, 
        /// or null if <paramref name="module"/> or <param name="opcodes"></param> is null
        /// </returns>
        public static IEnumerable<Tuple<MethodDefinition, Instruction>> GetInstructions(this ModuleDefinition module, ICollection<OpCode> opcodes)
        {
            if (module == null || opcodes == null)
            {
                return null;
            }

            return module.Types.AsParallel().SelectMany<TypeDefinition, Tuple<MethodDefinition, Instruction>>(t =>
            {
                List<Tuple<MethodDefinition, Instruction>> methodToInstructions = new List<Tuple<MethodDefinition, Instruction>>();

                if (t.Methods == null)
                {
                    return methodToInstructions;
                }

                foreach (var method in t.Methods)
                {
                    if (method.HasBody)
                    {
                        var newInstructions = method.Body.Instructions.Where<Instruction>(i => opcodes.Contains(i.OpCode));
                        foreach (Instruction instruction in newInstructions)
                        {
                            methodToInstructions.Add(new Tuple<MethodDefinition, Instruction>(method, instruction));
                        }
                    }
                }
                return methodToInstructions;
            });
        }

        /// <summary>
        /// Get all instructions within the <paramref name="module"/> with the opcode=<paramref name="opcode"/>.
        /// </summary>
        /// <returns>
        /// List of the instructions within the <paramref name="module"/> with the opcode=<paramref name="opcode"/>;
        /// null if <paramref name="module"/> or <param name="opcode"></param> is null
        /// </returns>
        public static IEnumerable<Tuple<MethodDefinition, Instruction>> GetInstructions(this ModuleDefinition module, OpCode opcode)
        {
            if (opcode == null)
            {
                return null;
            }

            return module.GetInstructions(new HashSet<OpCode> { opcode });
        }

        /// <summary>
        /// Returns all modules referenced by the provided module.
        /// Note that returned modules are of type <see cref="System.Reflection.Module"/>, not <see cref="Mono.Cecil.ModuleDefinition"/>
        /// </summary>
        /// <param name="module">Module which references are returned</param>
        /// <returns>Collection of referenced modules; or NULL if <paramref name="module"/> is NULL</returns>
        public static IEnumerable<AssemblyDefinition> GetReferencedAssemblies(this ModuleDefinition module)
        {
            // TODO: Clear up what the difference between ModuleDefinition.ModuleReferences and ModuleDefinition.AssemblyReferences is.
            // TODO: Modify this method to accept assembly name and only resolve assembly(ies) that match the name.

            if (module == null)
            {
                return null;
            }

            List<AssemblyDefinition> referencedAssemblies = new List<AssemblyDefinition>();
            foreach (var assemblyNameReference in module.AssemblyReferences)
            {
                try
                {
                    var assemblyResolver = new DefaultAssemblyResolver();
                    assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(module.FullyQualifiedName));
                    referencedAssemblies.Add(assemblyResolver.Resolve(assemblyNameReference));
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
            }
            return referencedAssemblies;
        }

        public static AssemblyDefinition GetReferencedAssembly(this ModuleDefinition module, string assemblyName, HashSet<string> visited = null)
        {
            visited = visited ?? new HashSet<string>();
            if (!visited.Add(module.FullyQualifiedName))
            {
                return null;
            }

            var referencedAssemblies = GetReferencedAssemblies(module);
            if (referencedAssemblies == null)
            {
                return null;
            }

            var result = referencedAssemblies.FirstOrDefault(a =>  a.Name.Name == assemblyName);
            if (result != null)
            {
                return result;
            }

            foreach (var refAsm in referencedAssemblies)
            {
                result = refAsm.MainModule.GetReferencedAssembly(assemblyName, visited);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Returns module with name <paramref name="referencedModuleName"/> referenced by <paramref name="module"/>
        /// or NULL if such module does not exist.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="referencedModuleName"></param>
        /// <returns></returns>
        public static ModuleDefinition GetReferencedModule(this ModuleDefinition module, string assemblyName, string moduleName)
        {
            var assembly = GetReferencedAssembly(module, assemblyName);
            if (assembly == null)
            {
                return null;
            }
            return assembly.Modules.FirstOrDefault(m => m.Name == moduleName);
        }

        #endregion

        #region MemberReference Extensions

        /// <summary>
        /// Returns path to a dependency file of a given test.
        /// </summary>
        /// <param name="test">
        /// Represents a test
        /// instance of <see cref="TypeReference"/> in a case of a test class;
        /// instance of <see cref="MethodReference"/> in a case of a test method;
        /// </param>
        public static string GetDependencyFilePath(this MemberReference test)
        {
            string path = Path.Combine(Paths.GetDependenciesFolderPath(Paths.Direction.Input), $"{test.GetFullName()}.json");
            if (path.IsFilePathTooLong())
            {
                string hashedPath = path.Hashed();
                string newPath = Path.Combine(Paths.GetDependenciesFolderPath(Paths.Direction.Input), $"{hashedPath}.json");
                return newPath;
            }
            return path;
        }

        public static string GetFullName(this MemberReference member)
        {
            if (member is TypeReference)
            {
                return member.FullName;
            }
            else if (member is MethodReference)
            {
                return $"{member.DeclaringType.FullName}.{member.Name}";
            }
            else
            {
                throw new NotSupportedException("Member not supported");
            }
        }

        public static string GetPath(this MemberReference member)
        {
            if (member is TypeReference)
            {
                TypeReference type = member as TypeReference;
                string path = type.Module.FullyQualifiedName.CleanFileName(Util.DefaultPathReplacementChar);
                return path.ToLower();
            }
            else if (member is MethodReference)
            {
                MethodReference method = member as MethodReference;
                string path = method.Module.FullyQualifiedName.CleanFileName(Util.DefaultPathReplacementChar);
                return path.ToLower();
            }
            else
            {
                throw new NotSupportedException("Member not supported");
            }
        }

        public static string GetDependencyName(this MemberReference member)
        {
            return $"{member.GetPath()}.{member.GetFullName()}";
        }


        #endregion

        #region IMemberDefinition Extensions

        /// <summary>
        /// Returns path to a dependency file of a given test.
        /// </summary>
        /// <param name="test">
        /// Represents a test.
        /// instance of <see cref="TypeDefinition"/> in a case of a test class;
        /// instance of <see cref="MethodDefinition"/> in a case of a test method;
        /// </param>
        public static string GetDependencyFilePath(this IMemberDefinition test)
        {
            string dependenciesFolderPath = Paths.GetDependenciesFolderPath(Paths.Direction.Input);
            string path = Path.Combine(dependenciesFolderPath, $"{test.GetFullName()}.json");
            if (path.IsFilePathTooLong())
            {
                string hashedPath = path.Hashed();
                string newPath = Path.Combine(dependenciesFolderPath, $"{hashedPath}.json");
                return newPath;
            }
            return path;
        }

        public static string GetFullName(this IMemberDefinition member)
        {
            if (member is TypeDefinition)
            {
                return member.FullName;
            }
            else if (member is MethodDefinition)
            {
                return $"{member.DeclaringType.FullName}.{member.Name}";
            }
            else
            {
                throw new NotSupportedException("Member not supported");
            }
        }

        public static string GetPath(this IMemberDefinition member)
        {
            if (member is TypeDefinition)
            {
                TypeDefinition type = member as TypeDefinition;
                string path = type.Module.FullyQualifiedName.CleanFileName(Util.DefaultPathReplacementChar);
                return path.ToLower();
            }
            else if (member is MethodDefinition)
            {
                MethodDefinition method = member as MethodDefinition;
                string path = method.Module.FullyQualifiedName.CleanFileName(Util.DefaultPathReplacementChar);
                return path.ToLower();
            }
            else
            {
                throw new NotSupportedException("Member not supported");
            }
        }

        public static string GetDependencyName(this IMemberDefinition member)
        {
            return $"{member.GetPath()}.{member.GetFullName()}";
        }

        #endregion

        #region TypeReference Extensions

        public static TypeReference GetParentType(this TypeReference type)
        {
            if (type == null)
            {
                return null;
            }

            TypeDefinition typeDefinition = type as TypeDefinition;
            if (typeDefinition != null)
            {
                return typeDefinition.BaseType;
            }

            typeDefinition = type.GetTypeDefinition();
            if (typeDefinition != null)
            {
                return typeDefinition.BaseType;
            }

            return null;
        }
        
        public static TypeDefinition GetTypeDefinition(this TypeReference type)
        {
            if (type.Scope is ModuleDefinition)
            {
                // type not imported from the other module
                if (type is TypeDefinition)
                {
                    return type as TypeDefinition;
                }
                ModuleDefinition module = type.Scope as ModuleDefinition;
                // Note that this will not find type definition if type is imported in type.Module.
                // e.g. In RequestReduce project Xunit.Extensions.TheoryAttribute is imported into RequestReduce dll
                return module.GetTypeByFullName(type.FullName);
            }
            else if (type.Scope is AssemblyNameReference)
            {
                // type imported from the other module
                TypeDefinition definition = null;
                try
                {
                    definition = type.Resolve();
                }
                catch (Exception e)
                {
                    // TODO: Consider to put higer log level
                    logger.Debug($"Cannot resolve type: {type.FullName}");
                }
                if (definition != null)
                {
                    return definition;
                }
                AssemblyNameReference assemblyName = type.Scope as AssemblyNameReference;
                var assembly = type.Module.GetReferencedAssembly(assemblyName.Name);
                var module = assembly.MainModule;
                return module.GetTypeByFullName(type.FullName);
            }
            else
            {
                throw new Exception("cannot resolve");
            }
        }

        #endregion

        #region TypeDefinition Extensions

        /// <summary>
        /// Inserts empty method (contains only required Ret instruction).
        /// Requires:
        /// type, and methodName are not null,
        /// </summary>
        /// <param name="methodName">Name of the newly inserted method.</param>
        /// <returns>Newly inserted method, or null if operation not succeded.</returns>
        public static MethodDefinition InsertEmptyMethod(this TypeDefinition type, string methodName)
        {
            if (type == null)
            {
                return null;
            }

            // create new method
            MethodDefinition newMethod = new MethodDefinition(methodName, MethodAttributes.Public, type.Module.TypeSystem.Void);
            // add new method to desired type
            type.Methods.Add(newMethod);
            ILProcessor il = newMethod.Body.GetILProcessor();
            // insert return instruction that has to be at the end of the method
            il.Body.Instructions.Insert(0, il.Create(OpCodes.Ret));
            return newMethod;
        }

        public static FieldDefinition GetFieldByName(this TypeDefinition type, string fieldName)
        {
            if (type == null)
            {
                return null;
            }

            return type.Fields.FirstOrDefault(f => f.Name == fieldName);
        }

        /// <summary>
        /// Checks whether a type contains a method with specified custom attribute.
        /// </summary>
        /// <returns>
        /// True, if <paramref name="type"/> contains a method m that sattisfies <see cref="HasCustomAttribute(ICustomAttributeProvider, string)"/>; otherwise, False.
        /// If eiher <paramref name="type"/> or <paramref name="attributeFullName"/> is null throws <see cref="ArgumentNullException"/>.
        /// </returns>
        public static bool HasMethodWithCustomAttribute(this TypeDefinition type, string attributeFullName, bool includeSubtypes)
        {
            return type.HasMethodWithCustomAttribute(name => name == attributeFullName, includeSubtypes);
        }

        public static bool HasMethodWithCustomAttribute(this TypeDefinition type, Func<string, bool> nameMatch, bool includeSubtypes)
        {
            if (type == null || nameMatch == null)
            {
                throw new ArgumentNullException();
            }

            return type.Methods.FirstOrDefault(m => m.HasCustomAttribute(nameMatch, includeSubtypes)) != null;
        }

        /// <summary>
        /// Gets method by its name
        /// </summary>
        /// <param name="type">Type inside we are searching for a method</param>
        /// <param name="methodName">Name of a method we are looking for</param>
        public static MethodDefinition GetMethodByName(this TypeDefinition type, string methodName)
        {
            if (type == null)
            {
                return null;
            }

            return type.Methods.FirstOrDefault(m => m.Name == methodName);
        }

        public static MethodDefinition GetMethodByFullName(this TypeDefinition type, string name)
        {
            if (type == null)
            {
                return null;
            }

            return type.Methods.FirstOrDefault(m => m.FullName == name);
        }

        public static IEnumerable<MethodDefinition> GetConstructors(this TypeDefinition type)
        {
            if (type == null)
            {
                return null;
            }

            // TODO: Check whether MethodDefinition.IsConstructor Property can be used
            return type.Methods.AsParallel().Where(m => m.Name == CLRConstants.ConstructorName);
        }

        public static MethodDefinition GetDefaultConstructor(this TypeDefinition type)
        {
            IEnumerable<MethodDefinition> constructors = type.GetConstructors();
            foreach (MethodDefinition constructor in constructors)
            {
                if (!constructor.HasCustomAttributes)
                {
                    return constructor;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the static constructor for a type
        /// </summary>
        public static MethodDefinition GetStaticConstructor(this TypeDefinition type)
        {
            if (type == null)
            {
                return null;
            }

            return type.Methods.FirstOrDefault(m => m.Name == CLRConstants.StaticConstructorName);
        }

        /// <summary>
        /// Gets the static constructor, creating one if necessary if force is true
        /// </summary>
        public static MethodDefinition GetStaticConstructor(this TypeDefinition type, bool createIfNecessary)
        {
            var existing = type.GetStaticConstructor();
            if (existing != null || !createIfNecessary) return existing;

            // At this point, existing = null and createIfNecessary = true

            var staticConstructorAttributes =
                    Mono.Cecil.MethodAttributes.Private |
                    Mono.Cecil.MethodAttributes.HideBySig |
                    Mono.Cecil.MethodAttributes.Static |
                    Mono.Cecil.MethodAttributes.SpecialName |
                    Mono.Cecil.MethodAttributes.RTSpecialName;

            MethodDefinition staticConstructor = new MethodDefinition(".cctor", staticConstructorAttributes, type.Module.TypeSystem.Void);
            staticConstructor.IsCompilerControlled = false;
            type.IsBeforeFieldInit = false; // Needed to make sure static constructor runs
            type.Methods.Add(staticConstructor);

            var il = staticConstructor.Body.GetILProcessor();
            il.Emit(OpCodes.Ret);

            return staticConstructor;
        }

        #endregion

        #region MethodReference Extensions

        public static MethodDefinition getMethodDefinition(this MethodReference methodReference)
        {
            if (methodReference == null)
            {
                return null;
            }

            TypeDefinition declaringType = methodReference.Module.Types.SingleOrDefault(t => t.FullName == methodReference.DeclaringType.FullName);
            if (declaringType == null)
            {
                return null;
            }

            var methodDef = declaringType.Methods.SingleOrDefault(m => m.FullName == methodReference.FullName);
            return methodDef;
        }

        #endregion

        #region MethodDefinition Extensions

        /// <summary>
        /// Insert call to a method <paramref name="methodToCall"/> at the beginning of method <paramref name="method"/>
        /// </summary>
        /// <param name="method">Caller method</param>
        /// <param name="methodToCall">Callee method</param>
        /// <param name="ldMethodArgs">Instructions inserted before method call, ordered from first to last, which serve to prepare arguments for the method call.</param>
        public static void InsertMethodCall(this MethodDefinition method, MethodReference methodToCall, IEnumerable<Instruction> ldMethodArgs)
        {
            if (method == null || methodToCall == null)
            {
                return;
            }

            if (!method.HasBody)
            {
                // if method does not have body it is impossible to insert instruction
                // TODO: check when this is the case.
                return;
            }

            if (method.Module != methodToCall.Module)
            {
                methodToCall = method.Module.Import(methodToCall);
            }

            var il = method.Body.GetILProcessor();

            // Creates the CIL instruction for calling our method
            Instruction callInstruction = il.Create(OpCodes.Call, methodToCall);

            if (method.Body.Instructions.Count > 0)
            {
                // Getting the first instruction of the current method
                Instruction firstInstruction = method.Body.Instructions[0];
                // Inserts the callInstruction at the beginning of the method
                il.InsertBefore(firstInstruction, callInstruction);
            }
            else
            {
                // TODO: Check if this ever happens
                il.Append(callInstruction);
            }

            if (ldMethodArgs != null)
            {
                foreach (var methodArg in ldMethodArgs.Reverse())
                {
                    il.InsertBefore(callInstruction, methodArg);
                }
            }
        }

        public static void InsertMethodCall(this MethodDefinition method, MethodReference toCall, Instruction loadArgumentInstruction)
        {
            method.InsertMethodCall(toCall, new List<Instruction> { loadArgumentInstruction });
        }

        /// <summary>
        /// Checks whether <paramref name="caller"/> contains call to <paramref name="callee"/>
        /// </summary>
        public static bool CallsMethod(this MethodDefinition caller, MethodReference callee)
        {
            if (!caller.HasBody)
            {
                return false;
            }

            foreach (Instruction instruction in caller.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call)
                {
                    MethodReference calledMethod = instruction.Operand as MethodReference;
                    if (calledMethod != null && calledMethod.FullName == callee.FullName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region MethodBody Extensions

        public static IEnumerable<Instruction> GetInstructions(this MethodBody body, ICollection<OpCode> opcodes)
        {
            if (body == null || body.Instructions == null)
            {
                return null;
            }

            var instructions = body.Instructions.Where(i => opcodes.Contains(i.OpCode));
            // prevent deferred execution
            // it can cause problems in this case because list of instructions 
            // cannot be modified while iterating through them
            return instructions.ToList();
        }

        public static IEnumerable<Instruction> GetInstructions(this MethodBody body, OpCode opcode)
        {
            return body.GetInstructions(new HashSet<OpCode> { opcode });
        }

        #endregion

        #region MemberReference Collection Extensions

        public static IOrderedEnumerable<T> GetSortedByFullName<T>(this ICollection<T> members) where T : MemberReference
        {
            return from member in members
                   orderby member.FullName
                   select member;
        }

        #endregion
        
        #region OpCodes Extensions

        public static readonly ICollection<OpCode> StaticAccessInstructions = new HashSet<OpCode>
        {
            OpCodes.Ldsfld,
            OpCodes.Ldsflda,
            OpCodes.Stsfld
        };

        #endregion

        #region Textual Representation

        public struct TextualRepresentationOptions
        {
            public bool UseSmartRepresentation;
        }

        private const int ChecksumThreshold = 100;

        public static string GetChecksum(this TypeDefinition type, TextualRepresentationOptions options)
        {
            uint lastHash = 0;
            StringBuilder checksumBuilder = new StringBuilder(2 * ChecksumThreshold);
            Action<string> appendAction = (s) =>
            {
                checksumBuilder.Append(s);
                if (checksumBuilder.Length > ChecksumThreshold)
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(checksumBuilder.ToString());
                    lastHash = Crc32CAlgorithm.Append(lastHash, bytes);
                    checksumBuilder.Clear();
                }
            };
            type.GetTextualRepresentation(appendAction, options);
            // clear if something left in StringBuilder
            if (checksumBuilder.Length > 0)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(checksumBuilder.ToString());
                lastHash = Crc32CAlgorithm.Append(lastHash, bytes);
                checksumBuilder.Clear();
            }
            return lastHash.ToString();
        }

        public static string GetTextualRepresentation(this TypeDefinition type, TextualRepresentationOptions options)
        {
            StringBuilder sb = new StringBuilder();
            Action<string> appendAction = (s) => { sb.Append(s); };
            type.GetTextualRepresentation(appendAction, options);
            return sb.ToString();
        }

        public static void GetTextualRepresentation(this TypeDefinition type, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (type == null)
            {
                return;
            }

            appendAction(type.Attributes.ToString());

            if (type.HasGenericParameters)
            {
                foreach (GenericParameter param in type.GenericParameters)
                {
                    param.GetTextualRepresentation(appendAction, options);
                }
            }
            if (type.HasInterfaces)
            {
                IEnumerable<TypeReference> interfaces = type.Interfaces;
                
                foreach (TypeReference inter in interfaces)
                {
                    inter.GetTextualRepresentation(appendAction, options);
                }
            }
            //if (type.HasNestedTypes)
            //{
            //    foreach (var nested in type.NestedTypes)
            //    {
            //        appendAction(nested.GetTextualRepresentation(options));
            //    }
            //}
            if (type.HasLayoutInfo)
            {
                appendAction(type.ClassSize.ToString());
            }
            if (type.HasFields)
            {
                foreach (var field in type.Fields)
                {
                    field.GetTextualRepresentation(appendAction, options);
                }
            }
            if (type.HasMethods)
            {
                IEnumerable<MethodDefinition> methods = type.Methods;
                foreach (var method in methods)
                {
                    method.GetTextualRepresentation(appendAction, options);
                }
            }
            if (type.HasProperties)
            {
                foreach (var property in type.Properties)
                {
                    property.GetTextualRepresentation(appendAction, options);
                }
            }
            if (type.HasEvents)
            {
                foreach (var ev in type.Events)
                {
                    ev.GetTextualRepresentation(appendAction, options);
                }
            }
            if (type.HasSecurityDeclarations)
            {
                foreach (var sd in type.SecurityDeclarations)
                {
                    sd.GetTextualRepresentation(appendAction, options);
                }
            }
            if (type.HasCustomAttributes)
            {
                foreach (var customAttribute in type.CustomAttributes)
                {
                    customAttribute.GetTextualRepresentation(appendAction, options);
                }
            }
        }

        public static void GetTextualRepresentation(this MethodDefinition method, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (method == null)
            {
                return;
            }

            if (method.HasGenericParameters)
            {
                foreach (GenericParameter param in method.GenericParameters)
                {
                    param.GetTextualRepresentation(appendAction, options);
                }
            }
            if (method.HasParameters)
            {
                foreach (ParameterDefinition param in method.Parameters)
                {
                    param.GetTextualRepresentation(appendAction, options);
                }
            }
            if (method.HasOverrides)
            {
                foreach (var overr in method.Overrides)
                {
                    overr.GetTextualRepresentation(appendAction, options);
                }
            }
            if (method.IsPInvokeImpl)
            {
                method.PInvokeInfo.GetTextualRepresentation(appendAction, options);
            }
            if (method.HasSecurityDeclarations)
            {
                foreach (var securityDeclaration in method.SecurityDeclarations)
                {
                    securityDeclaration.GetTextualRepresentation(appendAction, options);
                }
            }
            if (method.HasCustomAttributes)
            {
                foreach (var attrib in method.CustomAttributes)
                {
                    attrib.GetTextualRepresentation(appendAction, options);
                }
            }
            method.MethodReturnType.GetTextualRepresentation(appendAction, options);
            if (method.HasBody)
            {
                method.Body.GetTextualRepresentation(appendAction, options);
            }
        }

        public static void GetTextualRepresentation(this FieldDefinition field, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (field == null)
            {
                return;
            }

            if (field.HasConstant)
            {
                // TODO: Check if there are cases when this does not work
                if (field.Constant == null)
                {
                    // if constant itself is null value
                    appendAction("null");
                }
                else
                {
                    appendAction(field.Constant.ToString());
                }
            }
            if (field.HasLayoutInfo)
            {
                appendAction(field.Offset.ToString());
            }
            if (field.RVA > 0)
            {
                foreach (var b in field.InitialValue)
                {
                    appendAction(b.ToString());
                }
            }
            if (field.HasMarshalInfo)
            {
                field.MarshalInfo.GetTextualRepresentation(appendAction, options);
            }
            if (field.HasCustomAttributes)
            {
                foreach (CustomAttribute attrib in field.CustomAttributes)
                {
                    attrib.GetTextualRepresentation(appendAction, options);
                }
            }
        }

        public static void GetTextualRepresentation(this PropertyDefinition property, Action<string> appendAction, TextualRepresentationOptions options)
        {
            return;
        }

        public static void GetTextualRepresentation(this EventDefinition ev, Action<string> appendAction, TextualRepresentationOptions options)
        {
            return;
        }

        public static void GetTextualRepresentation(this ParameterDefinition param, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (param == null)
            {
                return;
            }

            if (param.HasConstant)
            {
                // Constant could have a null value
                var constant = param.Constant ?? "";
                appendAction(constant.ToString());
            }
            if (param.HasMarshalInfo)
            {
                param.MarshalInfo.GetTextualRepresentation(appendAction, options);
            }
            if (param.HasCustomAttributes)
            {
                foreach (var attrib in param.CustomAttributes)
                {
                    attrib.GetTextualRepresentation(appendAction, options);
                }
            }
        }

        public static void GetTextualRepresentation(this CustomAttribute customAttribute, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (customAttribute == null)
            {
                return;
            }

            appendAction(Encoding.ASCII.GetString(customAttribute.GetBlob()));
        }

        public static void GetTextualRepresentation(this GenericParameter parameter, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (parameter == null)
            {
                return;
            }

            if (parameter.HasConstraints)
            {
                foreach (TypeReference constraint in parameter.Constraints)
                {
                    constraint.GetTextualRepresentation(appendAction, options);
                }
            }
        }

        public static void GetTextualRepresentation(this MethodReference method, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (method == null)
            {
                return;
            }

            appendAction(method.FullName);
        }

        public static void GetTextualRepresentation(this TypeReference type, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (type == null)
            {
                return;
            }

            appendAction(type.FullName);
        }

        public static void GetTextualRepresentation(this PInvokeInfo invokeInfo, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (invokeInfo == null)
            {
                return;
            }

            appendAction(invokeInfo.IsBestFitDisabled.ToString());
            appendAction(invokeInfo.IsBestFitEnabled.ToString());
            appendAction(invokeInfo.IsCallConvCdecl.ToString());
            appendAction(invokeInfo.IsCallConvFastcall.ToString());
            appendAction(invokeInfo.IsCallConvStdCall.ToString());
            appendAction(invokeInfo.IsCallConvThiscall.ToString());
            appendAction(invokeInfo.IsCallConvWinapi.ToString());
            appendAction(invokeInfo.IsCharSetAnsi.ToString());
            appendAction(invokeInfo.IsCharSetAuto.ToString());
            appendAction(invokeInfo.IsCharSetNotSpec.ToString());
            appendAction(invokeInfo.IsCharSetUnicode.ToString());
            appendAction(invokeInfo.IsNoMangle.ToString());
            appendAction(invokeInfo.IsThrowOnUnmappableCharDisabled.ToString());
            appendAction(invokeInfo.IsThrowOnUnmappableCharEnabled.ToString());
        }

        public static string GetTextualRepresentation(this MarshalInfo marshalInfo, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (marshalInfo == null)
            {
                return string.Empty;
            }

            return marshalInfo.NativeType.ToString();
        }

        public static void GetTextualRepresentation(this SecurityDeclaration securityDeclaration, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (securityDeclaration == null)
            {
                return;
            }

            if (securityDeclaration.HasSecurityAttributes)
            {
                foreach (var attrib in securityDeclaration.SecurityAttributes)
                {
                    attrib.AttributeType.GetTextualRepresentation(appendAction, options);
                }
            }
        }

        public static void GetTextualRepresentation(this MethodReturnType type, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (type == null)
            {
                return;
            }

            if (type.HasConstant)
            {
                appendAction(type.Constant.ToString());
            }
            if (type.HasMarshalInfo)
            {
                type.MarshalInfo.GetTextualRepresentation(appendAction, options);
            }
            if (type.HasCustomAttributes)
            {
                foreach (var attrib in type.CustomAttributes)
                {
                    attrib.GetTextualRepresentation(appendAction, options);
                }
            }
        }

        public static void GetTextualRepresentation(this MethodBody body, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (body == null)
            {
                return;
            }

            appendAction(body.MaxStackSize.ToString());

            foreach (var instr in body.Instructions)
            {
                instr.GetTextualRepresentation(appendAction, options);
            }

            if (body.HasExceptionHandlers)
            {
                foreach (var handler in body.ExceptionHandlers)
                {
                    handler.GetTextualRepresentation(appendAction, options);
                }
            }

            if (body.HasVariables)
            {
                foreach (var variable in body.Variables)
                {
                    variable.GetTextualRepresentation(appendAction, options);
                }
            }
        }

        public static void GetTextualRepresentation(this Instruction instr, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (instr == null)
            {
                return;
            }

            if (!options.UseSmartRepresentation && instr.SequencePoint != null)
            {
                appendAction(instr.SequencePoint.StartLine.ToString());
                appendAction(instr.SequencePoint.StartColumn.ToString());
                appendAction(instr.SequencePoint.EndLine.ToString());
                appendAction(instr.SequencePoint.EndColumn.ToString());
            }
            appendAction(instr.ToString());
        }

        public static void GetTextualRepresentation(this ExceptionHandler handler, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (handler == null)
            {
                return;
            }

            handler.CatchType.GetTextualRepresentation(appendAction, options);

            for (Instruction current = handler.HandlerStart; ; current = current.Next)
            {
                // Add Instructions inside of the handler
                current.GetTextualRepresentation(appendAction, options);
                if (current == handler.HandlerEnd)
                {
                    break;
                }
            }

            appendAction(handler.HandlerType.ToString());

            for (Instruction current = handler.TryStart; ; current = current.Next)
            {
                // Add Instructions inside of the try block
                // TODO: Check if this is already included through MethodBody.Instructions
                current.GetTextualRepresentation(appendAction, options);
                if (current == handler.TryEnd)
                {
                    break;
                }
            }

            // TODO: Check what to do with filters
            // For those who never heard what filter is :)
            // https://lostechies.com/jimmybogard/2015/07/17/c-6-exception-filters-will-improve-your-home-life/
        }

        public static void GetTextualRepresentation(this VariableDefinition variable, Action<string> appendAction, TextualRepresentationOptions options)
        {
            if (variable == null)
            {
                return;
            }

            appendAction(variable.Index.ToString());
            appendAction(variable.IsPinned.ToString());
            appendAction(variable.Name.ToString());
            variable.VariableType.GetTextualRepresentation(appendAction, options);
        }

        #endregion
    }
}
