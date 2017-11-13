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
using System;
using System.Collections.Generic;
using System.Linq;

namespace EkstaziSharp.Tester
{
    // NOTE: Running in isolation is still in experimental phase
    public class IsolationTestExecutor : ITestExecutor
    {
        #region Fields

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private TestExecutor referencedExecutor;

        #endregion

        #region Constructors

        public IsolationTestExecutor(TestExecutor referencedExecutor)
        {
            this.referencedExecutor = referencedExecutor;
        }

        #endregion

        #region Private Methods

        public TestExecutionResults ExecuteOne(string testModule, IMemberDefinition testToRun, string[] args)
        {
            AppDomain appDomain = null;
            try
            {
                appDomain = AppDomain.CreateDomain(testToRun.FullName, null, AppDomain.CurrentDomain.SetupInformation);
                Type referencedExecutorType = referencedExecutor.GetType();
                string assemblyName = referencedExecutorType.Assembly.FullName;
                string referencedExecutorTypeName = referencedExecutorType.FullName;
                TestExecutor executor = (TestExecutor)appDomain.CreateInstanceAndUnwrap(assemblyName, referencedExecutorTypeName);

                List<IMemberDefinition> toRun = new List<IMemberDefinition> { testToRun }; // List is serializable
                return executor.Execute(testModule, toRun, args);
            }
            catch (Exception e)
            {
                logger.Error(e);
                return new TestExecutionResults(); // Empty
            }
            finally
            {
                if (appDomain != null)
                {
                    AppDomain.Unload(appDomain);
                }
            }
        }

        #endregion

        #region Public Methods

        public TestExecutionResults Execute(string testModule, IEnumerable<IMemberDefinition> testsToRun, string[] arguments)
        {
            // TODO: Check if testsToRun.AsParallel give performance improvements
            var results = testsToRun.Select(test => ExecuteOne(testModule, test, arguments));
            return results.Aggregate(new TestExecutionResults(), (current, next) => current + next);
        }

        #endregion
    }
}
