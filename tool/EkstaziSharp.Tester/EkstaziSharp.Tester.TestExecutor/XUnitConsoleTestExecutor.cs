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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using EkstaziSharp.Tester.Utils;
using System.IO;
using EkstaziSharp.Util;
using System.Xml;

namespace EkstaziSharp.Tester
{
    public class XUnitConsoleTestExecutor : TestExecutor
    {
        #region Fields

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Constants

        private const string ExecutedTestCaseCountAttribute = "total";
        private const string PassedTestCaseCountAttribute = "passed";
        private const string FailedTestCaseCountAttribute = "failed";
        private const string SkippedTestCaseCountAttribute = "skipped";
        private const string TimeAttribute = "time";

        private const int ArgumentsLengthLimit = 30000;

        #endregion

        #region Private Methods

        public string GetResultsFilePath(int index)
        {
            return Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), $"xunit_console_runner_log{index}.xml");
        }

        private void EnsureResultsDirExists()
        {
            string resultsDir = Path.GetDirectoryName(GetResultsFilePath(1));
            if (!Directory.Exists(resultsDir))
            {
                Directory.CreateDirectory(resultsDir);
            }
        }

        private TestExecutionResults ProcessExecutionResults(XmlNode xml)
        {
            TestExecutionResults result = new TestExecutionResults();

            var assemblyNodes = xml.GetChildren("assembly");
            if (assemblyNodes.Count != 1)
            {
                logger.Error("There should be results from exactly one assembly since we are running a single assembly.");
                return null;
            }
            XmlNode assemblyNode = assemblyNodes[0];
            result.PassedTestMethodsCount = assemblyNode.ParseAttribute<int>(PassedTestCaseCountAttribute);
            result.FailedTestMethodsCount = assemblyNode.ParseAttribute<int>(FailedTestCaseCountAttribute);
            result.SkippedTestMethodsCount = assemblyNode.ParseAttribute<int>(SkippedTestCaseCountAttribute);
            result.ExecutionTime = assemblyNode.ParseAttribute<decimal>(TimeAttribute);

            var tests = assemblyNode.GetChildren("test");
            foreach (var test in tests)
            {
                string testClassName = test.ParseAttribute<string>("type");
                string testMethodName = test.ParseAttribute<string>("name");
                if (testMethodName.Contains('('))
                {
                    // convert trait name into test method name
                    testMethodName = testMethodName.CutTillFirstOccurence('(');
                }

                result.ExecutedTestClasses.Add(testClassName);
                result.ExecutedTestMethods.Add(testMethodName);
            }
            return result;
        }

        private IEnumerable<string> FilterRunListBasedOnUserArguments(IEnumerable<string> testsToRun, string[] arguments)
        {
            if (arguments == null || testsToRun == null)
            {
                return testsToRun;
            }

            List<string> userChosenClasses = new List<string>();
            if (arguments != null && arguments.Length > 0)
            {
                int i = 0;
                while (i < arguments.Length)
                {
                    string arg = arguments[i];
                    if (arg == "-class")
                    {
                        // skip the next argument as well
                        i++;
                        userChosenClasses.Add(arguments[i]);
                    }
                    i++;
                }
            }
            return userChosenClasses.Count == 0 ? testsToRun : testsToRun.Intersect(userChosenClasses);
        }

        private string ParseUserArguments(string[] arguments)
        {
            StringBuilder builder = new StringBuilder();
            if (arguments != null && arguments.Length > 0)
            {
                int i = 0;
                while (i < arguments.Length)
                {
                    string arg = arguments[i];
                    if (arg == "-class")
                    {
                        // skip the next argument as well
                        i++;
                    }
                    else if (arg == "-parallel")
                    {
                        // now ignore -parallel since we are adding it explicitly
                        // skip the next argument as well (type of parallel)
                        i++;
                    }
                    else
                    {
                        builder.Append(arg + " ");
                    }
                    i++;
                }
            }
            return builder.ToString();
        }

        private List<string> ConstructXUnitArguments(IEnumerable<IMemberDefinition> testsToRun, string[] arguments)
        {
            List<StringBuilder> builders = new List<StringBuilder>();
            StringBuilder currentBuilder = new StringBuilder();
            builders.Add(currentBuilder);

            string userArguments = ParseUserArguments(arguments);

            foreach (IMemberDefinition test in testsToRun)
            {
                if (currentBuilder.Length > ArgumentsLengthLimit)
                {
                    // we exceeded the command line length limit
                    currentBuilder = new StringBuilder();
                    builders.Add(currentBuilder);
                }

                if (test is TypeDefinition)
                {
                    currentBuilder.Append($"-class {test.GetFullName()} ");
                }
                else if (test is MethodDefinition)
                {
                    currentBuilder.Append($"-method {test.GetFullName()} ");
                }
            }

            List<string> generatedArguments = new List<string>();
            for (int i = 0; i < builders.Count; i++)
            {
                StringBuilder builder = builders[i];
                string resultsFilePath = GetResultsFilePath(i);
                string insertedArguments = $"-parallel none -xml {resultsFilePath}";
                generatedArguments.Add($"{userArguments} {builder.ToString()} {insertedArguments}");
            }

            return generatedArguments;
        }

        #endregion

        #region Protected Methods

        protected override bool IsSupportedTestingFramework(AssemblyNameReference assembly)
        {
            return (assembly.Name == TestFrameworkConstants.XUnit1AssemblyName && assembly.Version.Major == 1)
                || (assembly.Name == TestFrameworkConstants.XUnit2AssemblyName && assembly.Version.Major == 2);
        }

        protected override TestExecutionResults ExecuteImpl(string testModule, IEnumerable<IMemberDefinition> testsToRun, string[] arguments)
        {
            // TODO: do discovery of all tests inside the module
            if (!testsToRun.Any())
            {
                return new TestExecutionResults();
            }

            EnsureResultsDirExists();

//            IEnumerable<string> toRun = FilterRunListBasedOnUserArguments(testsToRun, arguments);
            List<string> xunitArgs = ConstructXUnitArguments(testsToRun, arguments);
            TestExecutionResults results = new TestExecutionResults();
            for (int i = 0; i < xunitArgs.Count; i++) 
            {
                string commandLineArgs = xunitArgs[i];
                ProcessUtil.ExecuteProcessNoPoppingConsole(CommonPaths.XUnitConsoleExePath, $"{testModule} {commandLineArgs}");
                string xmlResults = File.ReadAllText(GetResultsFilePath(i));
                results = results + ProcessExecutionResults(xmlResults.ToXmlDocument());
            }
            return results;
        }

        #endregion
    }
}
