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
using System.Threading;
using Xunit.Runners;
using Xunit.Abstractions;
using Mono.Cecil;
using EkstaziSharp.Util;

namespace EkstaziSharp.Tester
{
    public class XUnitTestExecutor : TestExecutor
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Fields

        /// <summary>
        /// Syncronization variable
        /// </summary>
        private ManualResetEvent finished = new ManualResetEvent(false);

        /// <summary>
        /// Helper variable for handlers
        /// </summary>
        private AssemblyRunner runner;

        /// <summary>
        /// The tests to run with this executor
        /// </summary>
        private IEnumerable<IMemberDefinition> testsToRun;

        /// <summary>
        /// Stores the result of the execution
        /// </summary>
        private TestExecutionResults result;

        /// <summary>
        /// Tracks which tests got executed.
        /// </summary>
        private HashSet<string> executedTests = new HashSet<string>();

        private bool executionCanceled;

        #endregion

        #region Private Methods

        private bool InTheRunList(string testClassName, string testMethodName)
        {
            foreach (var test in testsToRun)
            {
                if (test is TypeDefinition)
                {
                    if (test.FullName == testClassName)
                    {
                        return true;
                    }
                }
                else if (test is MethodDefinition)
                {
                    if (test.DeclaringType.FullName == testClassName && test.Name == testMethodName)
                    {
                        return true;
                    }
                }
                else
                {
                    logger.Error("Unsupported test type");
                }
            }
            return false;
        }

        #endregion

        #region Protected Methods

        protected override bool IsSupportedTestingFramework(AssemblyNameReference assembly)
        {
            return (assembly.Name == TestFrameworkConstants.XUnit1AssemblyName && assembly.Version.Major == 1)
                || (assembly.Name == TestFrameworkConstants.XUnit2AssemblyName && assembly.Version.Major == 2);
        }

        protected override TestExecutionResults ExecuteImpl(string testModule, IEnumerable<IMemberDefinition> testsToRun, string[] args)
        {
            if (!testsToRun.Any())
            {
                // discovery cannot be done
                // When running with empty list of tests XUnit hangs
                return new TestExecutionResults();
            }

            executedTests.Clear();

            this.testsToRun = testsToRun;
            executionCanceled = false;
            result = new TestExecutionResults();

            runner = AssemblyRunner.WithAppDomain(testModule);
            logger.Debug($"Running XUnit executor in App domain: {AppDomain.CurrentDomain.FriendlyName}");

            runner.TestCaseFilter = TestCaseFilterHandler;
            runner.OnExecutionComplete = ExecutionCompleteHandler;
            runner.OnTestFailed = TestFailedHandler;
            runner.OnTestFinished += TestFinishedHandler;
            runner.OnDiscoveryComplete += DiscoveryCompleteHandler;
            runner.OnErrorMessage += ErrorMessageHandler;

            logger.Debug("Starting tests");
            runner.Start(maxParallelThreads: 1);

            finished.WaitOne();
            finished.Dispose();

            while (runner.Status != AssemblyRunnerStatus.Idle && !executionCanceled)
            {
                // spin until runner finishes
            }

            if (executionCanceled)
            {
                try
                {
                    runner.Dispose();
                }
                catch (InvalidOperationException e)
                {
                    // for now ignore this exception
                    // due to XUnit bug of not resetting Status to Idle when cancelling run
                }
            }
            else
            {
                runner.Dispose();
            }

            var temp = result;
            // reset result variable
            result = null;
            return temp;
        }

        #endregion

        #region Handlers

        private void ErrorMessageHandler(ErrorMessageInfo info)
        {
            logger.Info("### TEST ERROR ###");
            logger.Info(info.ExceptionMessage);
            logger.Info(info.ExceptionStackTrace);
        }

        private bool TestCaseFilterHandler(ITestCase item)
        {
            string testMethodName = item.TestMethod.Method.Name;
            string testClassName = item.TestMethod.TestClass.Class.Name;

            result.TestMethods.Add(testMethodName);
            result.TestClasses.Add(testClassName);

            if (InTheRunList(testClassName, testMethodName))
            {
                result.ExecutedTestMethods.Add($"{testClassName}.{testMethodName}");
                result.ExecutedTestClasses.Add(testClassName);
                return true;
            }

            return false;
        }

        private void DiscoveryCompleteHandler(DiscoveryCompleteInfo info)
        {
            logger.Info("### DISCOVERY COMPLETE ###");
            logger.InfoFormat("Total number of tests (methods): {0}", info.TestCasesDiscovered);
            logger.InfoFormat("Number of selected tests (methods): {0}", info.TestCasesToRun);
            if (info.TestCasesToRun == 0)
            {
                // I ensured at beginning of Execute method that testsToRun is not empty
                // thus this case shouldn't occur
                // I tried implementing by using just cancellation here,
                // but it has some issues
                logger.Info("No tests to run");
                // Note that ExecutionCompleteHandler does not get triggered
                // when there are no tests to be run unless if Cancel() is called
                runner.Cancel();
                // Note that runner Status is not set to Idle if execution is canceled.
                executionCanceled = true;
            }
        }

        private void TestFailedHandler(TestFailedInfo info)
        {
            // Commented out in order not to affect execution time
            //logger.DebugFormat("### TEST METHOD FINISHED: {0} ###", info.TestDisplayName);
            //logger.InfoFormat("### TEST METHOD FAILED: {0} ###", info.TestDisplayName);
            //logger.Debug("Exception Message: " + info.ExceptionMessage);
            //logger.Debug("Exception Type: " + info.ExceptionType);
            //logger.Debug("Stack Trace: " + info.ExceptionStackTrace);
        }

        private void TestFinishedHandler(TestFinishedInfo info)
        {
            // Commented out in order not to affect execution time
            //logger.DebugFormat("### TEST METHOD FINISHED: {0} ###", info.TestDisplayName);
            string testName = info.TestDisplayName;
            int parenthesisIndex = testName.IndexOf('(');
            if (parenthesisIndex != -1)
            {
                // this is a theory, extract the test name
                testName = testName.Substring(0, parenthesisIndex);
            }

            executedTests.Add($"{testName}");
        }

        private void ExecutionCompleteHandler(ExecutionCompleteInfo info)
        {
            logger.Info("### Test Execution Complete");
            logger.InfoFormat("Finished {0} tests in {1} seconds, {2} failed and {3} skipped",
                info.TotalTests, info.ExecutionTime, info.TestsFailed, info.TestsSkipped);

            if (executedTests.Count != result.ExecutedTestMethods.Count)
            {
                logger.Error("Number of total tests run not equal to number of tests selected to run!");
            }

            result.FailedTestMethodsCount = info.TestsFailed;
            result.SkippedTestMethodsCount = info.TestsSkipped;
            result.PassedTestMethodsCount = info.TotalTests - (info.TestsFailed + info.TestsSkipped);
            result.ExecutionTime = info.ExecutionTime;

            finished.Set();
        }

        #endregion
    }
}