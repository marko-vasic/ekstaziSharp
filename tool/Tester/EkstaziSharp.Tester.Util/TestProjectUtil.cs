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

namespace EkstaziSharp.Tester.Util
{
    public static class TestProjectUtil
    {
        /// <summary>
        /// Returns the absolute path to the directory that contains test projects.
        /// </summary>
        public static string GetTestsDirectory(TestProjectRunnerArguments args)
        {
            var dirInfo = new DirectoryInfo(Path.Combine(CommonPaths.TesterProjectBinDirectory, args.ProjectPath));
            return dirInfo.FullName;
        }

        /// <summary>
        /// Get absolute path to the specified version of project.
        /// </summary>
        /// <param name="versionIndex">Number of the project version</param>
        public static string GetProjectPath(TestProjectRunnerArguments args, int versionIndex)
        {
            if (versionIndex < 0 || versionIndex >= args.Versions.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return Path.Combine(GetTestsDirectory(args), args.Versions[versionIndex]);
        }

        public static string GetCSProjFilePath(TestProjectRunnerArguments args, int versionIndex)
        {
            string projectPath = GetProjectPath(args, versionIndex);
            return Path.Combine(projectPath, args.Versions[versionIndex] + ".csproj");
        }

        /// <summary>
        /// Get absolute path to the program dll for a specified project version.
        /// </summary>
        /// <param name="versionIndex">Number of the project version</param>
        /// <returns></returns>
        public static string GetDllPath(TestProjectRunnerArguments args, int versionIndex)
        {
            var projectPath = GetProjectPath(args, versionIndex);
            var dllFileName = new DirectoryInfo(projectPath).Name;
            return Path.Combine(projectPath, "bin\\Debug", dllFileName + ".dll");
        }

    }
}
