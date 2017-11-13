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
using System.IO;

namespace EkstaziSharp.Tester
{
    public class DotnetDependencyManager : IDependencyManager
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool FetchDependencies(string solutionFilePath, string projectFilePath)
        {
            if (solutionFilePath == null)
            {
                logger.Error("Failing to fetch dotnet dependencies, solution file should be provided");
                return false;
            }

            // fetching just by specifying project directory sometimes lead to some dependencies missing
            string solutionDir = new FileInfo(solutionFilePath).Directory.FullName;
            return ProcessUtil.ExecuteProcessNoPoppingConsole("dotnet", "restore", solutionDir);
        }
    }
}
