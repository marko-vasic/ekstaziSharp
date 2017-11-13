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
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using EkstaziSharp.Tester.Util;
using NUnit.Framework;
using Newtonsoft.Json;
using EkstaziSharp.Tester.Utils;
using EkstaziSharp.Util;

namespace EkstaziSharp.Tester.Tests
{
    public class TestCommons
    {
        /// <summary>
        /// Regex string representing TestProjects Name Format
        /// </summary>
        private const string TestDirectoryNameFormat = @"([A-Za-z]+)_v([0-9]+)";

        public class ToRun
        {
            [JsonProperty("toRun", Required = Required.Always)]
            public List<string> TestsToRun { get; set; }

            [JsonProperty("methodsToRun", Required = Required.Always)]
            public List<string> MethodsToRun { get; set; }
        }

        public static void RunWithResource(string resourceName)
        {
            try
            {
                string json = Resource.ResourceManager.GetString(resourceName);
                RunWithConfiguration(json);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        public static void RunWithConfiguration(string json)
        {
            ProjectRunnerArguments programArgs = JsonConvert.DeserializeObject<ProjectRunnerArguments>(json);
            var argumentsType = programArgs.TestSource.GetArgumentsType();
            programArgs = JsonConvert.DeserializeObject(json, argumentsType) as ProjectRunnerArguments;
            RunWithArgs(programArgs);
        }

        public static void RunWithArgs(ProjectRunnerArguments programArgs)
        {
            if (programArgs.OutputDirectory != null)
            {
                Paths.OutputDirectory = programArgs.OutputDirectory;
            }
            if (programArgs.InputDirectory != null)
            {
                Paths.InputDirectory = programArgs.InputDirectory;
            }
            Log4NetUtil.InitializeLoggers(programArgs.Debug);

            var runner = programArgs.TestSource.GetProjectRunner(programArgs);
            List<TestExecutionResults> results = runner.Run();
            if (programArgs.TestSource == ProjectRunnerType.TestProject)
            {
                CheckResults(results, programArgs as TestProjectRunnerArguments);
            }
        }

        public static void CheckResults(List<TestExecutionResults> results, TestProjectRunnerArguments args)
        {
            Assert.That(results.Count, Is.EqualTo(args.Versions.Count));

            for (int versionIndex = 0; versionIndex < args.Versions.Count; versionIndex++)
            {
                string projectPath = TestProjectUtil.GetProjectPath(args, versionIndex);
                string testsToRunFilePath = Path.Combine(projectPath, "toRun.json");
                string testsToRunFileContent = File.ReadAllText(testsToRunFilePath);
                ToRun runInfo = JsonConvert.DeserializeObject<ToRun>(testsToRunFileContent);

                List<string> testsToRun = args.DependencyCollectionGranularity == DependencyCollectionGranularity.Class ?
                    runInfo.TestsToRun : runInfo.MethodsToRun;
                ISet<string> runTests = args.DependencyCollectionGranularity == DependencyCollectionGranularity.Class ?
                    results[versionIndex].ExecutedTestClasses : results[versionIndex].ExecutedTestMethods;

                Assert.That(runTests.Count, Is.EqualTo(testsToRun.Count), $"Number of tests run not equal in version {versionIndex}");

                for (int testIndex = 0; testIndex < testsToRun.Count; testIndex++)
                {
                    Assert.That(runTests.Contains(testsToRun[testIndex]), $"Test not run: {testsToRun[testIndex]} in version: {versionIndex}");
                }

                if (results[versionIndex].FailedTestMethodsCount > 0)
                {
                    // if there is some test that failed
                    StringBuilder failedTests = new StringBuilder();
                    foreach (string failedTest in results[versionIndex].FailedTestMethods)
                    {
                        failedTests.Append($"{failedTest}\n");
                    }
                    Assert.Fail($"There were failing test cases:\n{failedTests.ToString()}");
                }
            }
        }

        public static string GetTestName(string directoryName)
        {
            Match match = Regex.Match(directoryName, TestDirectoryNameFormat);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }

        public static List<string> GetTestConfigurations(string dirPath, string configurationTemplate)
        {
            DirectoryInfo di = new DirectoryInfo(dirPath);
            Dictionary<string, int> testToMaxVersion = new Dictionary<string, int>();

            foreach (DirectoryInfo testDirectory in di.GetDirectories())
            {
                string dirName = testDirectory.Name;
                Match match = Regex.Match(testDirectory.Name, TestDirectoryNameFormat);
                if (match.Success)
                {
                    string testName = match.Groups[1].Value;
                    string versionString = match.Groups[2].Value;
                    int version = Convert.ToInt32(versionString);
                    int maxVersion = -1;
                    bool entryFound = testToMaxVersion.TryGetValue(testName, out maxVersion);
                    if (!entryFound || version > maxVersion)
                    {
                        testToMaxVersion[testName] = version;
                    }
                }
            }

            List<string> testConfigurations = new List<string>();
            foreach (KeyValuePair<string, int> pair in testToMaxVersion)
            {
                StringBuilder versions = new StringBuilder();
                int maxVersion = pair.Value;
                string testName = pair.Key;
                for (int i = 0; i <= maxVersion; i++)
                {
                    versions.Append($"\"{testName}_v{i}\"");
                    if (i < maxVersion)
                    {
                        versions.Append(",");
                    }
                }
                string configuration = string.Format(configurationTemplate, versions.ToString());
                testConfigurations.Add(configuration);
            }
            return testConfigurations;
        }

        public static List<TestCaseData> GetTestData(string testDirPath, string configurationTemplate, string testNameSuffix)
        {
            List<string> configurations = GetTestConfigurations(testDirPath, configurationTemplate);
            List<TestCaseData> testDataList = new List<TestCaseData>();

            foreach (string configuration in configurations)
            {
                var programArgs = JsonConvert.DeserializeObject<TestProjectRunnerArguments>(configuration);
                var testName = $"{GetTestName(programArgs.Versions[0])}_{testNameSuffix}";
                var testData = new TestCaseData(configuration);
                testData.SetName(testName);
                testDataList.Add(testData);
            }

            return testDataList;
        }
    }
}
