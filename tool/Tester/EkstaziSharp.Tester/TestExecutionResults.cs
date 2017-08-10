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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkstaziSharp.Tester
{
    [Serializable]
    public class TestExecutionResults
    {
        #region Fields

        private HashSet<string> testMethods = new HashSet<string>();

        private HashSet<string> executedTestMethods = new HashSet<string>();

        private HashSet<string> notExecutedTestMethods = new HashSet<string>();

        private HashSet<string> failedTestMethods = new HashSet<string>();

        private HashSet<string> testClasses = new HashSet<string>();

        private HashSet<string> executedTestClasses = new HashSet<string>();

        private HashSet<string> notExecutedTestClasses = new HashSet<string>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Duration of a test execution.
        /// </summary>
        public decimal ExecutionTime { get; set; }

        /// <summary>
        /// Number of executed Test Classes.
        /// </summary>
        public int ExecutedTestClassesCount
        {
            get
            {
                return ExecutedTestClasses.Count;
            }
        }

        /// <summary>
        /// Number of Test Classes in a test suite
        /// </summary>
        public int TotalNumberOfTestClasses
        {
            get
            {
                return TestClasses.Count;
            }
        }

        public double TestClassSelectionRation
        {
            get
            {
                return (double)ExecutedTestClassesCount / TotalNumberOfTestClasses;
            }
        }

        /// <summary>
        /// Number of executed Test Methods
        /// </summary>
        public int ExecutedTestMethodsCount
        {
            get
            {
                return ExecutedTestMethods.Count;
            }
        }

        /// <summary>
        /// Number of Test Methods in a test suite
        /// </summary>
        public int TotalNumberOfTestMethods
        {
            get
            {
                return TestMethods.Count;
            }
        }

        public double TestMethodsSelectionRatio
        {
            get
            {
                return (double)ExecutedTestMethodsCount / TotalNumberOfTestMethods;
            }
        }

        /// <summary>
        /// Number of test methods that passed execution.
        /// </summary>
        public int PassedTestMethodsCount { get; set; }

        /// <summary>
        /// Number of test methods that failed execution.
        /// </summary>
        public int FailedTestMethodsCount { get; set; }

        /// <summary>
        /// Number of test methods that were skipped.
        /// </summary>
        public int SkippedTestMethodsCount { get; set; }

        /// <summary>
        /// A list of executed test classes.
        /// TODO: This information may not be useful
        /// in case we do not execute each test from a test class.
        /// In that case we should have a list of executed test classes,
        /// and mapping from a test class to list of its executed test methods,
        /// additionally we could also have a list of all test methods inside of a
        /// test class.
        /// </summary>
        public HashSet<string> ExecutedTestClasses
        {
            get
            {
                return executedTestClasses;
            }
            set
            {
                executedTestClasses = value;
            }
        }

        /// <summary>
        /// A list of executed test methods.
        /// </summary>
        public HashSet<string> ExecutedTestMethods
        {
            get
            {
                return executedTestMethods;
            }
            set
            {
                executedTestMethods = value;
            }
        }

        public HashSet<string> FailedTestMethods
        {
            get
            {
                return failedTestMethods;
            }
            set
            {
                failedTestMethods = value;
            }
        }

        // TODO: Mark differently in case we don't have this information,
        // i.e. it is not populated during run
        // then in case this list is empty because all test classes were run.
        public HashSet<string> NotExecutedTestClasses
        {
            get
            {
                return notExecutedTestClasses;
            }
        }

        // TODO: Mark differently in case we don't have this information,
        // i.e. it is not populated during run
        // then in case this list is empty because all test methods were run.
        public HashSet<string> NotExecutedTestMethods
        {
            get
            {
                return notExecutedTestMethods;
            }
        }

        /// <summary>
        /// A list of all test classes in a suite
        /// </summary>
        public HashSet<string> TestClasses
        {
            get
            {
                return testClasses;
            }
            set
            {
                testClasses = value;
            }
        }

        /// <summary>
        /// A list of all test methods in a suite
        /// </summary>
        public HashSet<string> TestMethods
        {
            get
            {
                return testMethods;
            }
            set
            {
                testMethods = value;
            }
        }

        #endregion

        // TODO: Check if we can avoid using operator
        public static TestExecutionResults operator +(TestExecutionResults one, TestExecutionResults two)
        {
            TestExecutionResults cumalative = new TestExecutionResults();
            cumalative.PassedTestMethodsCount = one.PassedTestMethodsCount + two.PassedTestMethodsCount;
            cumalative.FailedTestMethodsCount = one.FailedTestMethodsCount + two.FailedTestMethodsCount;
            cumalative.SkippedTestMethodsCount = one.SkippedTestMethodsCount + two.SkippedTestMethodsCount;
            cumalative.ExecutionTime = one.ExecutionTime + two.ExecutionTime;

            cumalative.TestClasses.UnionWith(one.TestClasses);
            cumalative.TestClasses.UnionWith(two.TestClasses);

            cumalative.ExecutedTestClasses.UnionWith(one.ExecutedTestClasses);
            cumalative.ExecutedTestClasses.UnionWith(two.ExecutedTestClasses);

            cumalative.NotExecutedTestClasses.UnionWith(one.NotExecutedTestClasses);
            cumalative.NotExecutedTestClasses.UnionWith(two.NotExecutedTestClasses);

            cumalative.TestMethods.UnionWith(one.TestMethods);
            cumalative.TestMethods.UnionWith(two.TestMethods);

            cumalative.ExecutedTestMethods.UnionWith(one.ExecutedTestMethods);
            cumalative.ExecutedTestMethods.UnionWith(two.ExecutedTestMethods);

            cumalative.NotExecutedTestMethods.UnionWith(one.NotExecutedTestMethods);
            cumalative.NotExecutedTestMethods.UnionWith(two.NotExecutedTestMethods);

            cumalative.FailedTestMethods.UnionWith(one.FailedTestMethods);
            cumalative.FailedTestMethods.UnionWith(two.FailedTestMethods);

            return cumalative;
        }

        public override string ToString()
        {
            // TODO: Add information about number of passed and failed tests once when you fix
            // counting of test traits vs methods
            string template = "Executed {0} tests in {1} seconds";
            return string.Format(template, executedTestMethods.Count(), ExecutionTime);
        }
    }
}