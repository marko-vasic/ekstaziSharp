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
using System.IO;
using System.Diagnostics;
using Mono.Cecil;
using EkstaziSharp.Util;
using EkstaziSharp.Analysis;
using EkstaziSharp.Instrumentation;
using Newtonsoft.Json;

namespace EkstaziSharp.Tester
{
    /// <summary>
    /// <see cref="ProjectRunner"/> provides basic functionality for running tests on a target project.
    /// Note: The project in this context denotes a software project that can contain multiple subprojects,
    /// it does not denote a single csproj file, to be on a same page.
    /// </summary>
    public abstract class ProjectRunner
    {
        #region Fields

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string BackupModuleName = "{0}_backup";

        protected readonly ProjectRunnerArguments args;

        private Stopwatch analysisStopwatch;

        private Stopwatch instrumentationStopwatch;

        #endregion

        #region Protected Properties

        protected abstract IEnumerable<string> ProgramModules { get; }

        protected abstract IEnumerable<string> TestModules { get; }

        /// <summary>
        /// Absolute path to the root of a project folder
        /// </summary>
        protected abstract string ProjectPath { get; }

        #endregion

        #region Constructors

        public ProjectRunner(ProjectRunnerArguments args)
        {
            this.args = args;
        }

        #endregion

        #region Private Methods

        private void BackupModules()
        {
            foreach (var module in TestModules.Union(ProgramModules))
            {
                File.Copy(module, string.Format(BackupModuleName, module), true);
            }
        }

        private void RestoreModules()
        {
            foreach (var module in TestModules.Union(ProgramModules))
            {
                File.Copy(string.Format(BackupModuleName, module), module, true);
                File.Delete(string.Format(BackupModuleName, module));
            }
        }

        /// <summary>
        /// Builds a project
        /// </summary>
        /// <returns>True, if build succeeds; False, otherwise</returns>
        private bool BuildProject()
        {
            string solutionFilePath = GetSolutionFilePath();
            string projectFilePath = GetProjectFilePath();
            logger.Debug("Fetching Dependencies");
            bool dependeciesFetched = args.DependencyManagerType.GetDependencyManager().FetchDependencies(solutionFilePath, projectFilePath);
            if (!dependeciesFetched)
            {
                logger.Warn("Dependencies fetching failed");
            }
            logger.Debug("Building Project");
            return args.BuildStrategyType.GetBuildStrategy(args).Build(solutionFilePath, projectFilePath);
        }

        private Dictionary<string, ICollection<IMemberDefinition>> InstrumentAndSelect(IEnumerable<string> programModules, IEnumerable<string> testModules)
        {
            AnalyzerParameters analyzerParameters = new AnalyzerParameters(args.DependencyCollectionGranularity, !args.NoSmartChecksums);
            Analyzer analyzer = args.TestingFramework.GetAnalyzer(analyzerParameters);
            analysisStopwatch = new Stopwatch();

            analysisStopwatch.Start();
            Dictionary<string, ICollection<IMemberDefinition>> moduleToAffectedTests = analyzer.Analyze(testModules, programModules);
            analysisStopwatch.Stop();

            ModuleDefinition dependencyMonitorModule = CILTransformer.GetModuleDefinition(CommonPaths.DependencyMonitorDLLPath);
            InstrumentatorParameters instrumentatorParameters = new InstrumentatorParameters(dependencyMonitorModule, args.TestingFramework, args.InstrumentationStrategy, args.InstrumentationArgumentsType, args.InstrumentAtMethodBeginning);
            Instrumentator instrumentator = new Instrumentator(instrumentatorParameters);

            foreach (var modulePath in testModules)
            {
                string destination = Path.Combine(Path.GetDirectoryName(modulePath), "DependencyMonitor.dll");
                File.Copy(CommonPaths.DependencyMonitorDLLPath, destination, true);
                string configuration = Path.Combine(Path.GetDirectoryName(modulePath), Paths.DependencyMonitorConfigurationFileName);
                File.WriteAllText(configuration, Paths.OutputDirectory);
            }

            instrumentationStopwatch = new Stopwatch();
            instrumentationStopwatch.Start();
            bool instrument = false;
            foreach (var tests in moduleToAffectedTests.Values)
            {
                if (tests.Any())
                {
                    // instruments if there are some tests to be run
                    instrument = true;
                    break;
                }
            }
            if (instrument)
            {
                instrumentator.Instrument(moduleToAffectedTests, programModules);
            }
            instrumentationStopwatch.Stop();

            return moduleToAffectedTests;
        }

        private TestExecutionResults ExecuteTests(string testModule, IEnumerable<IMemberDefinition> testsToRun)
        {
            if (!args.RunInIsolation && args.InstrumentationStrategy == InstrumentationStrategy.StaticConstructor)
            {
                throw new Exception("When StaticConstructor instrumentation strategy is used tests should be run in Isolation.");
            }

            ITestExecutor executor = null;

            if (args.RunInIsolation)
            {
                executor = new IsolationTestExecutor(args.TestingFramework.GetExecutor());
            }
            else
            {
                executor = args.TestingFramework.GetExecutor();
            }

            TestExecutionResults results = executor.Execute(testModule, testsToRun, args.TestingFrameworkArguments);

            int numOfExecuted = args.DependencyCollectionGranularity == DependencyCollectionGranularity.Class ?
                results.ExecutedTestClassesCount : results.ExecutedTestMethodsCount;
            if (testsToRun.Count() != numOfExecuted)
            {
                logger.Warn("Number of tests scheduled for run does not match number of actually run tests");
            }

            return results;
        }

        private void LogResults(TestExecutionResults result)
        {
            IOUtil.WriteAllText(CommonPaths.TestResultsFilePath, JsonConvert.SerializeObject(result, Formatting.Indented));
            IOUtil.WriteAllText(CommonPaths.TotalNumberOfTestClassesFilePath, result.TotalNumberOfTestClasses.ToString());
            IOUtil.WriteAllText(CommonPaths.NumberOfExecutedTestClassesFilePath, result.ExecutedTestClassesCount.ToString());
            IOUtil.WriteAllText(CommonPaths.TotalNumberOfTestMethodsFilePath, result.TotalNumberOfTestMethods.ToString());
            IOUtil.WriteAllText(CommonPaths.NumberOfExecutedTestMethodsFilePath, result.ExecutedTestMethodsCount.ToString());
            IOUtil.WriteAllText(CommonPaths.NumberOfPassedTestMethodsFilePath, result.PassedTestMethodsCount.ToString());
            IOUtil.WriteAllText(CommonPaths.NumberOfFailedTestMethodsFilePath, result.FailedTestMethodsCount.ToString());
            IOUtil.WriteAllText(CommonPaths.NumberOfSkippedTestMethodsFilePath, result.SkippedTestMethodsCount.ToString());
            decimal executionTime = result.ExecutionTime;
            IOUtil.WriteAllText(CommonPaths.ExecutionTimeFilePath, executionTime.FormattedNumber());
            double analysisTime = analysisStopwatch.Elapsed.TotalSeconds;
            IOUtil.WriteAllText(CommonPaths.AnalysisTimeFilePath, analysisTime.FormattedNumber());
            double instrumentationTime = instrumentationStopwatch.Elapsed.TotalSeconds;
            IOUtil.WriteAllText(CommonPaths.InstrumentationTimeFilePath, instrumentationTime.FormattedNumber());
            double totalTime = (double)executionTime + analysisTime + instrumentationTime;
            IOUtil.WriteAllText(CommonPaths.TotalTimeFilePath, totalTime.FormattedNumber());
        }

        private void CleanFiles()
        {
            if (args.CleanEkstaziFiles)
            {
                IOUtil.DeleteDirectory(Paths.GetEkstaziInformationFolderPath(Paths.Direction.Input));
            }

            string logsDirPath = Paths.GetEkstaziLogsFolderPath(Paths.Direction.Input);
            IOUtil.DeleteDirectory(logsDirPath);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Get absolute path of a project's solution (.sln) file
        /// </summary>
        protected string GetSolutionFilePath()
        {
            if (args.SolutionPath == null)
            {
                return null;
            }

            return Path.Combine(ProjectPath, args.SolutionPath);
        }

        /// <summary>
        /// Get absolute path of a project (.csproj) file;
        /// the project represents our testing target.
        /// </summary>
        protected virtual string GetProjectFilePath()
        {
            if (args.ProjectFilePath == null)
            {
                return null;
            }

            return Path.Combine(ProjectPath, args.ProjectFilePath);
        }

        protected abstract void InitializeProject();

        /// <summary>
        /// True if there is next revision, false otherwise.
        /// </summary>
        protected abstract bool MoveToNextRevision();

        #endregion

        #region Public Methods

        public List<TestExecutionResults> Run()
        {
            CleanFiles();
            InitializeProject();
            List<TestExecutionResults> results = new List<TestExecutionResults>();

            do
            {
                try
                {
                    logger.Info("Building project");
                    if (!BuildProject())
                    {
                        throw new Exception("Build failed");
                    }

                    BackupModules();

                    Dictionary<string, ICollection<IMemberDefinition>> moduleToAffectedTests = InstrumentAndSelect(ProgramModules, TestModules);

                    logger.Info("Executing tests");
                    TestExecutionResults result = new TestExecutionResults();
                    foreach (KeyValuePair<string, ICollection<IMemberDefinition>> moduleToTests in moduleToAffectedTests)
                    {
                        result += ExecuteTests(moduleToTests.Key, moduleToTests.Value);
                    }
                    logger.Info("Test execution summary: " + result.ToString());

                    if (args.Debug)
                    {
                        LogResults(result);
                    }

                    results.Add(result);

                    RestoreModules();
                }
                catch (Exception e)
                {
                    logger.Error("Exception thrown", e);
                }
            }
            while (MoveToNextRevision());

            return results;
        }

        #endregion
    }
}
