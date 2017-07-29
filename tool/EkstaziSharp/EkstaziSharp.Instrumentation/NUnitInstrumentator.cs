// Copyright (c) 2017, Marko Vasic
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
    public abstract class NUnitInstrumentator : TestInstrumentator
    {
        #region Constants

        private const string OneTimeSetUpMethodName = "$OneTimeSetUp";
        private const string OneTimeTearDownMethodName = "$OneTimeTearDown";
        private const string TearDownMethodName = "$TearDown";

        #endregion

        #region Fields

        private ModuleDefinition nunitModule;

        #endregion

        #region Properties

        protected abstract string OneTimeSetUpAttribute { get; }
        protected abstract string OneTimeTearDownAttribute { get; }

        #endregion

        #region Constructors

        public NUnitInstrumentator(ModuleDefinition testModule, InstrumentatorParameters parameters) : base(testModule, parameters) { }

        #endregion

        #region Private Methods

        // Assumption, all tests from same module
        private ModuleDefinition GetNUnitModule(ModuleDefinition module)
        {
            if (nunitModule == null)
            {
                nunitModule = module.GetReferencedModule(TestFrameworkConstants.NUnitAssemblyName, TestFrameworkConstants.NUnitModuleName);
            }
            return nunitModule;
        }

        #endregion

        #region Public Methods

        public override void InstrumentTestClass(TypeDefinition testClass)
        {
            ModuleDefinition testModule = testClass.Module;

            var nunitModule = GetNUnitModule(testModule);

            // TODO: Reason whether to reuse existing SetUp method if there is one
            // I tested whether multiple SetUp and TearDown methods are supported
            // and it seems like they are; so this shouldn't c
            MethodDefinition setUpMethod = testClass.InsertEmptyMethod(OneTimeSetUpMethodName);
            CILTransformer.AddCustomAttribute(setUpMethod, nunitModule, OneTimeSetUpAttribute);
            InsertCallToDependencyMonitor(setUpMethod, TestClassStartMethod, testClass);

            MethodDefinition tearDownMethod = testClass.InsertEmptyMethod(OneTimeTearDownMethodName);
            CILTransformer.AddCustomAttribute(tearDownMethod, nunitModule, OneTimeTearDownAttribute);
            InsertCallToDependencyMonitor(tearDownMethod, TestClassEndMethod, testClass);
        }

        public override void InstrumentTestMethod(MethodDefinition testMethod)
        {
            ModuleDefinition testModule = testMethod.Module;
            TypeDefinition testClass = testMethod.DeclaringType;

            var nunitModule = GetNUnitModule(testModule);
            InsertCallToDependencyMonitor(testMethod, TestMethodStartMethod, testMethod);

            MethodDefinition tearDownMethod = testClass.GetMethodByName(TearDownMethodName);
            if (tearDownMethod == null)
            {
                tearDownMethod = testClass.InsertEmptyMethod(TearDownMethodName);
                CILTransformer.AddCustomAttribute(tearDownMethod, nunitModule, TestFrameworkConstants.NUnitTestMethodTearDownAttributeName);
            }
            if (!tearDownMethod.CallsMethod(TestMethodEndMethod))
            {
                InsertCallToDependencyMonitor(tearDownMethod, TestMethodEndMethod, null);
            }
        }

        #endregion
    }
}
