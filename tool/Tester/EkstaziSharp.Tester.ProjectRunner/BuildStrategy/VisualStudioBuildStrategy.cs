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

using System.Management.Automation;

namespace EkstaziSharp.Tester
{
    public class VisualStudioBuildStrategy : IBuildStrategy
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Build(string solutionFilePath, string projectFilePath)
        {
            // TODO: Try to modify execution policy on a level of process, here in c#
            // instead of changing execution policy globally.
            //string powershellPath = @"C:\WINDOWS\SysWOW64\WindowsPowerShell\v1.0\powershell.exe";
            //Process p = ProcessUtils.CreateProcessNoPoppingConsole("cmd.exe", $"/c {powershellPath} -ExecutionPolicy Unrestricted {Paths.BuildProjectScriptPath}");
            //p.Start();
            //p.WaitForExit();
            //p.Close();

            if (solutionFilePath == null)
            {
                logger.Error("solutionFilePath is null. VisualStudio build strategy requires solution path to be provided.");
                return false;
            }

            using (PowerShell powershellInstance = PowerShell.Create())
            {
                // TODO: Try providing commands here, instead of running external script
                powershellInstance.AddCommand(CommonPaths.BuildProjectScriptPath);
                powershellInstance.AddArgument(solutionFilePath);
                powershellInstance.Invoke();
                return !powershellInstance.HadErrors;
            }
        }
    }
}
