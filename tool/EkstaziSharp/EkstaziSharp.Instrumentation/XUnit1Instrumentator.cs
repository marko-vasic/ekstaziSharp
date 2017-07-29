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
using EkstaziSharp.Util;
using System;

namespace EkstaziSharp.Instrumentation
{
    public class XUnit1Instrumentator : XUnitInstrumentator
    {
        #region Constants

        private const string NamespaceForGeneratedTypes = "$EkstaziGenerated$";

        private const string GeneratedTraceAttributeName = "TraceAttribute";

        #endregion

        #region Fields

        private TypeReference methodInfo;

        #endregion

        #region Properties

        protected override string XUnitAssemblyName
        {
            get
            {
                return TestFrameworkConstants.XUnit1AssemblyName;
            }
        }

        protected override string XUnitModuleName
        {
            get
            {
                return TestFrameworkConstants.XUnit1ModuleName;
            }
        }

        protected override string XUnitClassFixtureTypeName
        {
            get
            {
                return TestFrameworkConstants.XUnit1ClassFixture;
            }
        }

        #endregion

        #region Constructors

        public XUnit1Instrumentator(ModuleDefinition testModule, InstrumentatorParameters parameters) : base(testModule, parameters) { }

        #endregion

        #region Private Methods

        private void InjectFixture(TypeDefinition testToRun, TypeDefinition fixture)
        {
            MethodDefinition setFixture = testToRun.InsertEmptyMethod("SetFixture");
            setFixture.IsPublic = true;
            setFixture.IsHideBySig = true;
            setFixture.IsVirtual = true;
            setFixture.IsNewSlot = true;
            setFixture.IsFinal = true;

            ParameterDefinition fixtureParam = new ParameterDefinition(fixture);
            fixtureParam.Name = "data";
            setFixture.Parameters.Add(fixtureParam);
        }

        private TypeReference GetImportedBeforeAfterTestAttribute(ModuleDefinition testModule)
        {
            ModuleDefinition xunitModule = GetXUnitModule(testModule);
            var beforeAfterTestAttribute = xunitModule.GetTypeByFullName(TestFrameworkConstants.XUnit1BeforeAfterTestAttribute);
            return testModule.Import(beforeAfterTestAttribute);
        }

        private TypeDefinition GetTraceAttribute(ModuleDefinition testModule)
        {
            return testModule.GetTypeByFullName($"{NamespaceForGeneratedTypes}.{GeneratedTraceAttributeName}");
        }

        #endregion

        #region Protected Methods

        protected override void RegisterClassFixture(TypeDefinition test, TypeDefinition fixtureClass)
        {
            base.RegisterClassFixture(test, fixtureClass);
            InjectFixture(test, fixtureClass);
        }

        private TypeReference ImportMethodInfo(ModuleDefinition module)
        {
            if (methodInfo == null)
            {
                var mscorlibModule = module.GetReferencedModule(CLRConstants.CoreLibAssemblyName, CLRConstants.CLRModuleName);
                TypeDefinition typeDefinition = mscorlibModule.GetTypeByFullName(CLRConstants.MethodInfo);
                methodInfo = module.Import(typeDefinition);
            }
            return methodInfo;
        }

        public override void InstrumentTestMethod(MethodDefinition testMethod)
        {
            InsertCallToDependencyMonitor(testMethod, TestMethodStartMethod, testMethod);

            ModuleDefinition testModule = testMethod.Module;
            TypeDefinition traceAttribute = GetTraceAttribute(testModule);

            if (traceAttribute == null)
            {
                TypeReference beforeAfterTestAttribute = GetImportedBeforeAfterTestAttribute(testModule);

                string newTraceAttribute = "TraceAttribute";
                traceAttribute = new TypeDefinition(
                    @namespace: "$EkstaziGenerated$",
                    name: newTraceAttribute,
                    attributes: TypeAttributes.Public | TypeAttributes.BeforeFieldInit,
                    baseType: beforeAfterTestAttribute);

                testModule.Types.Add(traceAttribute);

                AddConstructor(traceAttribute, traceAttribute.BaseType.GetTypeDefinition());

                MethodDefinition afterMethod = traceAttribute.InsertEmptyMethod("After");
                afterMethod.Attributes = MethodAttributes.Public
                    | MethodAttributes.Virtual
                    | MethodAttributes.HideBySig;
                TypeReference methodInfoType = ImportMethodInfo(testModule);
                ParameterDefinition methodInfoParameter = new ParameterDefinition(methodInfoType);
                afterMethod.Parameters.Add(methodInfoParameter);
                InsertCallToDependencyMonitor(afterMethod, TestMethodEndMethod, null);
            }

            var customAttribute = new CustomAttribute(traceAttribute.GetDefaultConstructor());
            testMethod.CustomAttributes.Add(customAttribute);
        }

        #endregion
    }
}
