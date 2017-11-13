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

using Mono.Cecil;
using Mono.Cecil.Cil;
using EkstaziSharp.Util;
using System;
using System.Linq;

namespace EkstaziSharp.Instrumentation
{

    /// xunit 1 related doc: http://www.bricelam.net/2012/04/xunitnet-extensibility.html
    /// xunit 2 related doc: https://xunit.github.io/docs/shared-context.html
    public abstract class XUnitInstrumentator : TestInstrumentator
    {
        #region Fields

        private TypeReference importedSystemObject;

        private TypeDefinition systemObjectDefinition;

        private TypeDefinition classFixture;

        private ModuleDefinition xunitModule;

        private TypeReference disposableType;

        #endregion

        #region Properties

        private TypeReference ImportedSystemObject
        {
            get
            {
                if (importedSystemObject == null)
                {
                    var mscorlibModule = moduleToInstrument.GetReferencedModule(CLRConstants.CoreLibAssemblyName, CLRConstants.CLRModuleName);
                    TypeDefinition objectDefinition = mscorlibModule.GetTypeByFullName(CLRConstants.SystemObjectFullName);
                    importedSystemObject = moduleToInstrument.Import(objectDefinition);
                }
                return importedSystemObject;
            }
        }

        private TypeDefinition SystemObjectDefinition
        {
            get
            {
                if (systemObjectDefinition == null)
                {
                    systemObjectDefinition = ImportedSystemObject.GetTypeDefinition();
                }
                return systemObjectDefinition;
            }
        }

        protected abstract string XUnitAssemblyName { get; }

        protected abstract string XUnitModuleName { get; }

        protected abstract string XUnitClassFixtureTypeName { get; }

        #endregion

        #region Constructors

        public XUnitInstrumentator(ModuleDefinition testModule, InstrumentatorParameters parameters) : base(testModule, parameters) { }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create a Class Fixture for a given <paramref name="testClass"/> and add it to the test module.
        /// If test is called TestAdd, generated fixture class will look like: class GeneratedTestAddFixture implements System.IDisposable
        /// https://xunit.github.io/docs/shared-context.html
        /// Insert constructor to the Class Fixture with call to <see cref="EkstaziSharp.DependencyMonitor"/> when <paramref name="testClass"/> is about to start executing.
        /// Insert dispose method to the Class Fixture with call to <see cref="EkstaziSharp.DependencyMonitor"/> when <paramref name="testClass"/> is finished executing.
        /// </summary>
        /// <param name="testClass"></param>
        /// <returns>Returns generated fixture class</returns>
        private TypeDefinition CreateClassFixture(TypeDefinition testClass)
        {
            string generatedFixtureName = string.Format("$Generated{0}Fixture", testClass.Name);
            TypeReference objectType = ImportedSystemObject;
            var classFixture = new TypeDefinition(
                @namespace: testClass.Namespace,
                name: generatedFixtureName,
                attributes: TypeAttributes.Public | TypeAttributes.BeforeFieldInit,
                baseType: objectType);

            var disposableType = ImportDisposableType(testClass.Module);
            classFixture.Interfaces.Add(disposableType);
            testClass.Module.Types.Add(classFixture);

            MethodDefinition constructor = AddConstructor(classFixture, SystemObjectDefinition);
            InsertCallToDependencyMonitor(constructor, TestClassStartMethod, testClass);
            MethodDefinition disposeMethod = AddDisposeMethod(classFixture);
            InsertCallToDependencyMonitor(disposeMethod, TestClassEndMethod, testClass);

            return classFixture;
        }

        private TypeDefinition GetClassFixture(ModuleDefinition module)
        {
            if (classFixture == null)
            {
                ModuleDefinition xunitModule = GetXUnitModule(module);
                classFixture = xunitModule.GetTypeByFullName(XUnitClassFixtureTypeName);
            }
            return classFixture;
        }

        /// <summary>
        /// Modifies the <paramref name="test"/> <see cref="TypeDefinition"/>
        /// by adding IClassFixture&lt;<paramref name="fixtureClass"/>&gt; interface to list of interfaces that the test implements.
        /// </summary>
        /// <param name="test">TypeDefinition of the test</param>
        /// <param name="fixtureClass">TypeDefinition of the fixtureClass</param>
        private void AddFixtureInterface(TypeDefinition test, TypeDefinition fixtureClass)
        {
            var testModule = test.Module;
            TypeDefinition iClassFixture = GetClassFixture(testModule);
            // Import IClassFixture type in the test module.
            var importedIClassFixture = testModule.Import(iClassFixture);
            // Create a generic type IClassFixture<T>
            GenericInstanceType iClassFixtureType = new GenericInstanceType(importedIClassFixture);
            // Specify type of generics, now IClassFixture<fixtureClass>
            iClassFixtureType.GenericArguments.Add(fixtureClass);
            // Add generated type to implements section of the test class.
            test.Interfaces.Add(iClassFixtureType);
        }

        #endregion Private Methods

        #region Protected Methods

        /// <summary>
        /// Get xunitModule referenced from the test module.
        /// </summary>
        protected ModuleDefinition GetXUnitModule(ModuleDefinition testModule)
        {
            if (xunitModule == null)
            {
                xunitModule = testModule.GetReferencedModule(XUnitAssemblyName, XUnitModuleName);
                if (xunitModule == null)
                {
                    throw new ArgumentException("Could not find xunit module in " + testModule.FullyQualifiedName);
                }
            }
            return xunitModule;
        }

        /// <summary>
        /// Adds a constructor into <paramref name="type"/> class.
        /// </summary>
        protected MethodDefinition AddConstructor(TypeDefinition type, TypeDefinition baseType)
        {
            // TODO: Clearly define when to use Name and when FullName of method, type, etc.
            // Add information about that to CILTransformer class.
            MethodDefinition constructor = type.InsertEmptyMethod(CLRConstants.ConstructorName);
            constructor.Attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;

            MethodDefinition baseConstructor = baseType.GetMethodByName(CLRConstants.ConstructorName);
            Instruction callBaseInstruction = CILTransformer.InsertMethodCallAtBeginning(constructor, baseConstructor);
            ILProcessor il = constructor.Body.GetILProcessor();
            il.InsertBefore(callBaseInstruction, il.Create(OpCodes.Ldarg_0));

            return constructor;
        }

        protected TypeReference ImportDisposableType(ModuleDefinition module)
        {
            if (disposableType == null)
            {
                var mscorlibModule = module.GetReferencedModule(CLRConstants.CoreLibAssemblyName, CLRConstants.CLRModuleName);
                TypeDefinition disposableDefinition = mscorlibModule.GetTypeByFullName(CLRConstants.SystemIDisposableFullName);
                disposableType = module.Import(disposableDefinition);
            }
            return disposableType;
        }

        /// <summary>
        /// Adds a dispose method into <paramref name="type"/> class.
        /// </summary>
        protected MethodDefinition AddDisposeMethod(TypeDefinition type)
        {
            MethodDefinition disposeMethod = type.InsertEmptyMethod("Dispose");
            disposeMethod.Attributes = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
            return disposeMethod;
        }

        protected virtual void RegisterClassFixture(TypeDefinition test, TypeDefinition fixtureClass)
        {
            AddFixtureInterface(test, fixtureClass);
        }

        public override void InstrumentTestClass(TypeDefinition testClass)
        {
            // Create a Class Fixture for the test class
            TypeDefinition fixtureClass = CreateClassFixture(testClass);
            // Add interface with Class Fixture attribute to the test
            RegisterClassFixture(testClass, fixtureClass);
        }

        #endregion
    }
}
