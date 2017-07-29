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
using NUnit.Engine.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Mono.Cecil;
using EkstaziSharp.Util;

namespace EkstaziSharp.Tester
{
    public class NUnit2TestExecutor : NUnitTestExecutor, ITestEventListener
    {
        #region Fields

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Methods

        protected override TestExecutionResults ExecuteImpl(string testModule, IEnumerable<IMemberDefinition> testsToRun, string[] arguments)
        {
            logger.Info(string.Format("Running NUnit2 executor in App domain: {0}", AppDomain.CurrentDomain.FriendlyName));
            result = new TestExecutionResults();

            var appDomain = AppDomain.CreateDomain("nunit2-test", null, testModule, null, false);
            NUnit2FrameworkDriver driver = new NUnit2FrameworkDriver(appDomain) { ID = "1" }; // Not sure why "1" but necessary
            var settings = new Dictionary<string, object>();
            string x = driver.Load(testModule, settings);

            TestFilterBuilder filterBuilder = new TestFilterBuilder();
            foreach (var t in testsToRun)
            {
                filterBuilder.AddTest(t.GetFullName());
            }

            string filter = filterBuilder.GetFilter().Text;

            string discoveryXmlString = driver.Explore(filter);
            // TODO: Check if discovery information can be otained from
            // execution results. Not sure if execution results contain all
            // test methods and classes or only executed ones
            ParseDiscoveryResults(discoveryXmlString.ToXmlDocument());
            if (testsToRun.Count() > 0)
            {
                string xmlString = driver.Run(this, filter);
                ParseExecutionResults(xmlString.ToXmlDocument());
            }
            else
            {
                logger.Info("No tests selected for run");
            }

            AppDomain.Unload(appDomain);

            var temp = result;
            // clean the local field for the next time
            result = null;
            return temp;
        }

        protected override bool IsSupportedTestingFramework(AssemblyNameReference assembly)
        {
            return assembly.Name == TestFrameworkConstants.NUnitAssemblyName && assembly.Version.Major == 2;
        }

        #endregion

        #region Handlers

        public void OnTestEvent(string report)
        {
            //var doc = new XmlDocument();
            //doc.LoadXml(report);
            //var testCaseNode = doc.FirstChild;
            //if (testCaseNode.Name == TestCaseName)
            //{
            //    // can it also be skipped?
            //    string status = testCaseNode.Attributes["result"].InnerText;
            //    if (status == "Passed")
            //    {
            //        result.PassedTestMethodsCount++;
            //    }
            //    else
            //    {
            //        result.FailedTestMethodsCount++;
            //    }

            //    result.ExecutedTestMethods.Add(testCaseNode.Attributes["fullname"].InnerText);
            //    result.ExecutedTestClasses.Add(testCaseNode.Attributes["classname"].InnerText);
            //}
            logger.Debug(report);
        }

        #endregion
    }
}
