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

using NUnit.Engine;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System;
using Mono.Cecil;
using EkstaziSharp.Util;

namespace EkstaziSharp.Tester
{
    public class NUnit3TestExecutor : NUnitTestExecutor, ITestEventListener
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Methods

        protected override bool IsSupportedTestingFramework(AssemblyNameReference assembly)
        {
            return assembly.Name == TestFrameworkConstants.NUnitAssemblyName && assembly.Version.Major == 3;
        }

        protected override TestExecutionResults ExecuteImpl(string testModule, IEnumerable<IMemberDefinition> testsToRun, string[] args)
        {
            logger.Info(string.Format("Running NUnit3 executor in App domain: {0}", AppDomain.CurrentDomain.FriendlyName));
            result = new TestExecutionResults();

            TestFilterBuilder filterBuilder = new TestFilterBuilder();
            foreach (var t in testsToRun)
            {
                filterBuilder.AddTest(t.GetFullName());
            }

            using (var engine = TestEngineActivator.CreateInstance())
            {
                TestPackage package = new TestPackage(testModule);

                using (var runner = engine.GetRunner(package))
                {
                    // explore all tests (default filter used)
                    XmlNode explorationResults = runner.Explore(new TestFilterBuilder().GetFilter());
                    ParseDiscoveryResults(explorationResults);

                    //foreach (var test in result.TestClasses)
                    //{
                    //    if (!testsToRun.Contains(test))
                    //    {
                    //        result.NotExecutedTestClasses.Add(test);
                    //    }
                    //}
                    //foreach (var test in testsToRun)
                    //{
                    //    if (!result.TestClasses.Contains(test))
                    //    {
                    //        logger.Debug($"Test: {test} found during analyzes phase but not discovered using Runner");
                    //    }
                    //}

                    if (testsToRun.Count() > 0)
                    {
                        XmlNode runResults = runner.Run(this, filterBuilder.GetFilter());
                        ParseExecutionResults(runResults);
                        try
                        {
                            runner.Unload();
                        }
                        catch (Exception e)
                        {
                            logger.Warn("Error while unloading runner", e);
                        }
                    }
                    else
                    {
                        logger.Info("No tests selected for run");
                    }
                }
            }

            var temp = result;
            // clean the local field for the next time
            result = null;
            return temp;
        }

        #endregion

        #region Handlers

        public void OnTestEvent(string report)
        {
            logger.Debug(report);
        }

        #endregion
    }
}
