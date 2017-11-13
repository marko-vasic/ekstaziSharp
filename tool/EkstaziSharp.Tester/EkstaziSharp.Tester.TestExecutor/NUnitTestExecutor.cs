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
using System.Xml;
using EkstaziSharp.Util;

namespace EkstaziSharp.Tester
{
    public abstract class NUnitTestExecutor : TestExecutor
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constants

        private const string DurationAttribute = "duration";
        private const string TestCaseCountAttribute = "testcasecount";
        private const string ExecutedTestCaseCountAttribute = "total";
        private const string PassedTestCaseCountAttribute = "passed";
        private const string FailedTestCaseCountAttribute = "failed";
        private const string SkippedTestCaseCountAttribute = "skipped";
        private const string ResultAttribute = "result";

        private const string TypeAttribute = "type";
        private const string AssemblyType = "Assembly";
        private const string NamespaceType = "Namespace";
        private const string TestFixtureType = "TestFixture";

        private const string ClassnameAttribute = "classname";
        private const string FullnameAttribute = "fullname";

        private const string TestSuiteName = "test-suite";
        private const string TestCaseName = "test-case";

        #endregion

        /// <summary>
        /// Helper field for the handlers
        /// </summary>
        protected TestExecutionResults result;

        #region Protected Methods

        protected T ParseAttribute<T>(XmlNode node, string attributeName)
        {
            try
            {
                return node.ParseAttribute<T>(attributeName);
            }
            catch (NotSupportedException e)
            {
                logger.Error("Cannot convert attribute: " + attributeName);
                logger.Error(e);
                return default(T);
            }
        }

        /// <summary>
        /// If type null ignores type attribute
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected List<XmlNode> GetChildrenOfTypeAndName(XmlNode node, string name, string type = null)
        {
            List<XmlNode> nodes = new List<XmlNode>();

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == name)
                {
                    if (type == null)
                    {
                        nodes.Add(childNode);
                    }
                    else
                    {
                        string typeAttribute = ParseAttribute<string>(childNode, TypeAttribute);
                        if (typeAttribute == type)
                        {
                            nodes.Add(childNode);
                        }
                    }
                    List<XmlNode> childNodes = GetChildrenOfTypeAndName(childNode, name, type);
                    if (childNodes != null && childNodes.Count > 0)
                    {
                        nodes.AddRange(childNodes);
                    }
                }
            }

            return nodes;
        }

        protected void ParseDiscoveryResults(XmlNode xmlDocument)
        {
            XmlNode discoveryResults = null;
            foreach (XmlNode childNode in xmlDocument.ChildNodes)
            {
                string typeAttribute = ParseAttribute<string>(childNode, TypeAttribute);
                if (typeAttribute == AssemblyType)
                {
                    // TODO: Check if there can be multiple nodes of Assembly Type
                    discoveryResults = childNode;
                    break;
                }
            }

            List<XmlNode> testFixtures = GetChildrenOfTypeAndName(discoveryResults, TestSuiteName, TestFixtureType);
            testFixtures.ForEach(testFixtureNode =>
            {
                string testFixtureFullname = ParseAttribute<string>(testFixtureNode, FullnameAttribute);
                result.TestClasses.Add(testFixtureFullname);

                List<XmlNode> testCases = GetChildrenOfTypeAndName(testFixtureNode, TestCaseName, null);
                testCases.ForEach(testCaseNode =>
                {
                    string testCaseFullname = ParseAttribute<string>(testCaseNode, FullnameAttribute);
                    result.TestMethods.Add(testCaseFullname);
                });
            });
        }

        protected void ParseExecutionResults(XmlNode xmlDocument)
        {
            XmlNode executionResults = null;
            foreach (XmlNode childNode in xmlDocument.ChildNodes)
            {
                if (childNode.Attributes.GetNamedItem(TypeAttribute) == null)
                {
                    continue;
                }
                string typeAttribute = ParseAttribute<string>(childNode, TypeAttribute);
                if (typeAttribute == AssemblyType)
                {
                    // TODO: Check if there can be multiple nodes of Assembly Type
                    executionResults = childNode;
                    break;
                }
            }

            // assembly
            List<XmlNode> testFixtures = GetChildrenOfTypeAndName(executionResults, TestSuiteName, TestFixtureType);
            testFixtures.ForEach(testFixtureNode =>
            {
                string testFixtureFullname = ParseAttribute<string>(testFixtureNode, FullnameAttribute);
                result.ExecutedTestClasses.Add(testFixtureFullname);

                List<XmlNode> testCases = GetChildrenOfTypeAndName(testFixtureNode, TestCaseName, null);
                testCases.ForEach(testCaseNode =>
                {
                    string testCaseFullname = ParseAttribute<string>(testCaseNode, FullnameAttribute);
                    result.ExecutedTestMethods.Add(testCaseFullname);
                    string testExecutionResult = ParseAttribute<string>(testCaseNode, ResultAttribute);
                    if (string.Equals(testExecutionResult, "passed", StringComparison.OrdinalIgnoreCase))
                    {
                        result.PassedTestMethodsCount++;
                    }
                    else if (string.Equals(testExecutionResult, "failed", StringComparison.OrdinalIgnoreCase))
                    {
                        result.FailedTestMethods.Add(testCaseFullname);
                        result.FailedTestMethodsCount++;
                    }
                    else if (string.Equals(testExecutionResult, "skipped", StringComparison.OrdinalIgnoreCase))
                    {
                        result.SkippedTestMethodsCount++;
                    }
                    else
                    {
                        logger.Debug("Unrecognized test result: " + testExecutionResult);
                    }
                });
            });

            result.ExecutionTime = ParseAttribute<decimal>(executionResults, DurationAttribute);

            int executedTestCaseCount = ParseAttribute<int>(executionResults, ExecutedTestCaseCountAttribute);
            if (executedTestCaseCount != result.ExecutedTestMethodsCount)
            {
                logger.Debug("Count of executed test cases counted through handler is not the same as in the execution results report");
            }
        }

        #endregion
    }
}
