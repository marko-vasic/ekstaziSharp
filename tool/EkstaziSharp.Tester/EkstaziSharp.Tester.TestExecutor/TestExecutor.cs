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
using Mono.Cecil;
using EkstaziSharp.Util;

namespace EkstaziSharp.Tester
{
    // TODO: Check why we put this class to inherit MarshalByRefObject
    public abstract class TestExecutor : MarshalByRefObject, ITestExecutor
    {
        #region Private Methods

        // TODO: if we switch to autmatic detection of testing framework used,
        // there will be no need to manually specify testing framework
        // which may remove need for this Assert
        private void AssertTestingFrameworkReferenced(string testModule)
        {
            ModuleDefinition module = CILTransformer.GetModuleDefinition(testModule);
            bool frameworkFound = false;
            foreach (var reference in module.AssemblyReferences)
            {
                if (IsSupportedTestingFramework(reference))
                {
                    frameworkFound = true;
                    break;
                }
            }
            if (!frameworkFound)
            {
                throw new Exception($"Appropriate Testing Framework not referenced from the module: {testModule}. Cannot run current test executor on this module!");
            }
        }

        #endregion

        #region NonVirtual Methods

        public TestExecutionResults Execute(string testModule, IEnumerable<IMemberDefinition> testsToRun, string[] arguments)
        {
            AssertTestingFrameworkReferenced(testModule);
            return ExecuteImpl(testModule, testsToRun, arguments);
        }

        #endregion

        #region Abstract

        protected abstract bool IsSupportedTestingFramework(AssemblyNameReference assembly);

        protected abstract TestExecutionResults ExecuteImpl(string testModule, IEnumerable<IMemberDefinition> testsToRun, string[] arguments);

        #endregion
    }
}
