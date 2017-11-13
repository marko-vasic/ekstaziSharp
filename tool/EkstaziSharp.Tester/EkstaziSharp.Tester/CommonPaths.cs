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
using System.IO;
using EkstaziSharp.Util;

namespace EkstaziSharp.Tester
{
    public static class CommonPaths
    {
        #region Fields

        private static readonly string LocalRepositoriesDirName = Path.Combine(Paths.GetEkstaziOutputDirectory(Paths.Direction.Output), "repositories");

        #endregion

        #region Public Properties

        public static string GetLocalRepositoryPath(string repoURL)
        {
            string folderName = repoURL.Replace(@"://", "_");
            folderName = folderName.Replace(@"/", "_");
            folderName = folderName.Replace(".git", "");
            return Path.Combine(LocalRepositories.FullName, folderName);
        }

        /// <summary>
        /// Directory where remote repositories are fetched to.
        /// </summary>
        public static DirectoryInfo LocalRepositories
        {
            get
            {
                var path = Path.Combine(Path.GetTempPath(), LocalRepositoriesDirName);
                DirectoryInfo localRepositoryDir = new DirectoryInfo(path);
                EnsureDirectoryExists(path, d => localRepositoryDir = d);
                return localRepositoryDir;
            }
        }

        public static string TesterProjectBinDirectory
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public static string SolutionDirectory
        {
            get
            {
                DirectoryInfo di = new DirectoryInfo(Path.Combine(TesterProjectBinDirectory, "..", "..", "tool"));
                return di.FullName;
            }
        }

        public static string TestProjectsDirectory
        {
            get
            {
                return Path.Combine(SolutionDirectory, "tests");
            }
        }

        public static string PackagesDirectory
        {
            get
            {
                return Path.Combine(SolutionDirectory, "packages");
            }
        }

        public static string XUnitConsoleExePath
        {
            get
            {
                return Path.Combine(PackagesDirectory, "xunit.runner.console.2.1.0", "tools", "xunit.console.exe");
            }
        }

        public static string BuildProjectScriptPath
        {
            get
            {
                return Path.Combine(TestProjectsDirectory, "buildProject.ps1");
            }
        }

        public static string XUnit2TestsDirectory
        {
            get
            {
                return Path.Combine(TestProjectsDirectory, "XUnit2Tests");
            }
        }

        public static string XUnit1TestsDirectory
        {
            get
            {
                return Path.Combine(TestProjectsDirectory, "XUnit1Tests");
            }
        }

        public static string NUnit2TestsDirectory
        {
            get
            {
                return Path.Combine(TestProjectsDirectory, "NUnit2Tests");
            }
        }

        public static string NUnit3TestsDirectory
        {
            get
            {
                return Path.Combine(TestProjectsDirectory, "NUnit3Tests");
            }
        }

        public static string NugetExePath
        {
            get
            {
                return Path.Combine(TesterProjectBinDirectory, "nuget.exe");
            }
        }

        public static string DependencyMonitorDLLPath
        {
            get
            {
                return Path.Combine(TesterProjectBinDirectory, "DependencyMonitor.dll");
            }
        }

        public static string NewtonsoftJSONDLL
        {
            get
            {
                return Path.Combine(TesterProjectBinDirectory, "Newtonsoft.Json.dll");
            }
        }

        #region Log Files

        public static string TestResultsFilePath
        {
            get
            {
                return Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), "test_results.txt");
            }
        }

        public static string AnalysisTimeFilePath
        {
            get
            {
                return Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), "analysis_time.txt");
            }
        }

        public static string InstrumentationTimeFilePath
        {
            get
            {
                return Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), "instrumentation_time.txt");
            }
        }

        public static string ExecutionTimeFilePath
        {
            get
            {
                return Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), "execution_time.txt");
            }
        }

        public static string TotalTimeFilePath
        {
            get
            {
                return Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), "total_time.txt");
            }
        }

        public static string NumberOfExecutedTestClassesFilePath
        {
            get
            {
                return Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), "executed_test_classes_count.txt");
            }
        }

        public static string TotalNumberOfTestClassesFilePath
        {
            get
            {
                return Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), "total_test_classes_count.txt");
            }
        }

        public static string NumberOfExecutedTestMethodsFilePath
        {
            get
            {
                return Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), "executed_test_methods_count.txt");
            }
        }

        public static string TotalNumberOfTestMethodsFilePath
        {
            get
            {
                return Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), "total_test_methods_count.txt");
            }
        }

        public static string NumberOfPassedTestMethodsFilePath
        {
            get
            {
                return Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), "passed_test_methods_count.txt");
            }
        }

        public static string NumberOfFailedTestMethodsFilePath
        {
            get
            {
                return Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), "failed_test_methods_count.txt");
            }
        }

        public static string NumberOfSkippedTestMethodsFilePath
        {
            get
            {
                return Path.Combine(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output), "skipped_test_methods_count.txt");
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private static void EnsureDirectoryExists(string path, Action<DirectoryInfo> directorySetter)
        {
            var directory = new DirectoryInfo(path);
            if (!directory.Exists)
            {
                directory.Create();
            }
            directorySetter(directory);
        }

        #endregion
    }
}
