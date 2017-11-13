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

using System.Diagnostics;
using System.Text;
using System.Threading;
using EkstaziSharp.Util;

namespace EkstaziSharp.Tester.Utils
{
    public static class ProcessUtil
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Executes a process while preventing new console to open.
        /// </summary>
        /// <param name="programPath">Path to the program/script that will be executed</param>
        /// <param name="arguments">Arguments to be provided to a program/script</param>
        /// <param name="workingDirectory">Working directory of process to be executed</param>
        /// <returns>True, if process successfully executed; False, otherwise.</returns>
        public static bool ExecuteProcessNoPoppingConsole(string programPath, string arguments, string workingDirectory)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = programPath;
            startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = workingDirectory;
            return ExecuteProcessNoPoppingConsole(startInfo);
        }

        /// <summary>
        /// Executes a process while preventing new console to open.
        /// </summary>
        /// <param name="programPath">Path to the program/script that will be executed</param>
        /// <param name="arguments">Arguments to be provided to a program/script</param>
        /// <returns>True, if process successfully executed; False, otherwise.</returns>
        public static bool ExecuteProcessNoPoppingConsole(string programPath, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = programPath;
            startInfo.Arguments = arguments;
            return ExecuteProcessNoPoppingConsole(startInfo);
        }

        /// <summary>
        /// Executes a process while preventing new console to open.
        /// </summary>
        /// <param name="startInfo">startInfo of a process to be executed.</param>
        /// <returns>True, if process successfully executed; False, otherwise.</returns>
        public static bool ExecuteProcessNoPoppingConsole(ProcessStartInfo startInfo)
        {
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            using (Process p = new Process())
            {
                p.StartInfo = startInfo;

                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();


                // Information why is following needed
                // http://stackoverflow.com/questions/139593/processstartinfo-hanging-on-waitforexit-why
                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false), errorWaitHandle = new AutoResetEvent(false))
                {
                    p.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    p.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    p.Start();
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();

                    if (p.ExitCode != 0)
                    {
                        logger.Warn($"Process exited with ExitCode different than zero, command line: {startInfo.FileName} {startInfo.Arguments}");
                        if (!error.ToString().IsNullOrWhiteSpace())
                        {
                            logger.Debug($"StandardError: {error.ToString()}");
                        }
                        if (!output.ToString().IsNullOrWhiteSpace())
                        {
                            logger.Debug($"StandardOutput: {output.ToString()}");
                        }
                    }

                    return p.ExitCode == 0;
                }
            }
        }
    }
}
