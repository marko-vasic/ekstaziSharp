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

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using EkstaziSharp.Util;
using System.IO;

namespace EkstaziSharp.Tester.Tests
{
    [TestFixture]
    public class XUnit1Tests
    {
        private static readonly string ProjectRelativePath =
                                    IOUtil.GetRelativePath(new DirectoryInfo(CommonPaths.TesterProjectBinDirectory), new DirectoryInfo(CommonPaths.XUnit1TestsDirectory));


        private static readonly string ClassLevelConfiguration =
            @"{{
                ""TestSource"": ""TestProject"",
                ""BuildStrategyType"": ""MSBuild14"",
                ""DependencyManagerType"": ""Nuget"",
                ""ProjectPath"": """ + ProjectRelativePath + @""",
                ""Versions"":  [{0}],
                ""TestingFramework"": ""XUnit1"",
                ""InstrumentationStrategy"": ""InstanceConstructor"",
                ""DependencyCollectionGranularity"": ""Class"",
                ""RunInIsolation"": false,
                ""Debug"": true
             }}";

        private static readonly string MethodLevelConfiguration = 
            @"{{
                ""TestSource"": ""TestProject"",
                ""BuildStrategyType"": ""MSBuild14"",
                ""DependencyManagerType"": ""Nuget"",
                ""ProjectPath"": """ + ProjectRelativePath + @""",
                ""Versions"":  [{0}],
                ""TestingFramework"": ""XUnit1"",
                ""InstrumentationStrategy"": ""InstanceConstructor"",
                ""DependencyCollectionGranularity"": ""Method"",
                ""RunInIsolation"": false,
                ""Debug"": true
             }}";

        [Test, TestCaseSource(typeof(XUnit1Tests), "ClassLevelTestConfigurations")]
        public void ClassLevelTests(string configuration)
        {
            TestCommons.RunWithConfiguration(configuration);
        }

        [Test, TestCaseSource(typeof(XUnit1Tests), "MethodLevelTestConfigurations")]
        public void MethodLevelTests(string configuration)
        {
            TestCommons.RunWithConfiguration(configuration);
        }

        public static IEnumerable ClassLevelTestConfigurations
        {
            get
            {
                List<TestCaseData> testDataList = TestCommons.GetTestData(CommonPaths.XUnit1TestsDirectory, ClassLevelConfiguration, "ClassLevel");

                foreach (var testData in testDataList)
                {
                    yield return testData;
                }
            }
        }

        public static IEnumerable MethodLevelTestConfigurations
        {
            get
            {
                List<TestCaseData> testDataList = TestCommons.GetTestData(CommonPaths.XUnit1TestsDirectory, MethodLevelConfiguration, "MethodLevel");

                foreach (var testData in testDataList)
                {
                    yield return testData;
                }
            }
        }
    }
}
