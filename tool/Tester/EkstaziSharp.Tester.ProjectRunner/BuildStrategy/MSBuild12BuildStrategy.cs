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

using EkstaziSharp.Tester.Utils;

namespace EkstaziSharp.Tester
{
    public class MSBuild12BuildStrategy : IBuildStrategy
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Build(string solutionFilePath, string projectFilePath)
        {
            if (solutionFilePath == null && projectFilePath == null)
            {
                logger.Error("Failing to build, both solution file and project file are null");
                return false;
            }

            // TODO: Try storing msbuild.exe in the repository itself
            // so to avoid depending on whether msbuild is installed on a machine.
            string msBuildExePath = "C:\\Program Files (x86)\\MSBuild\\12.0\\Bin\\msbuild.exe";

            string buildArtifact = projectFilePath != null ? projectFilePath : solutionFilePath;
            return ProcessUtil.ExecuteProcessNoPoppingConsole(msBuildExePath, buildArtifact);
        }
    }
}
