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
using System.Linq;
using System;

namespace EkstaziSharp.Instrumentation
{
    public class XUnit2Instrumentator : XUnitInstrumentator
    {
        #region Properties

        protected override string XUnitAssemblyName
        {
            get
            {
                return TestFrameworkConstants.XUnit2AssemblyName;
            }
        }

        protected override string XUnitModuleName
        {
            get
            {
                return TestFrameworkConstants.XUnit2ModuleName;
            }
        }

        protected override string XUnitClassFixtureTypeName
        {
            get
            {
                return TestFrameworkConstants.XUnit2ClassFixture;
            }
        }

        #endregion

        #region Constructors

        public XUnit2Instrumentator(ModuleDefinition testModule, InstrumentatorParameters parameters) : base(testModule, parameters) { }

        #endregion

        #region Protected Methods

        public override void InstrumentTestMethod(MethodDefinition testMethod)
        {
            // Instrument Test Method to put TestClassStart invocation
            InsertCallToDependencyMonitor(testMethod, TestMethodStartMethod, testMethod);

            TypeDefinition testClass = testMethod.DeclaringType;

            TypeReference disposableInterface = null;
            if (testClass.HasInterfaces)
            {
                disposableInterface = testClass.Interfaces.FirstOrDefault(t => t.FullName.Equals(CLRConstants.SystemIDisposableFullName));
            }
            if (disposableInterface == null)
            {
                var disposableType = ImportDisposableType(testMethod.Module);
                testClass.Interfaces.Add(disposableType);
            }

            // Instrument Dispose Method to put TestClassEnd invocation
            MethodDefinition disposeMethod = testClass.GetMethodByName("Dispose");
            if (disposeMethod == null)
            {
                disposeMethod = AddDisposeMethod(testClass);
            }
            if (!disposeMethod.CallsMethod(TestMethodEndMethod))
            {
                // instrument only if it is not already instrumented
                InsertCallToDependencyMonitor(disposeMethod, TestMethodEndMethod, null);
            }
        }

        #endregion
    }
}
