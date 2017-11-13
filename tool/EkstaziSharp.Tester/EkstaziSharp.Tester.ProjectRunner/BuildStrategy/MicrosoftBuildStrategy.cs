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

using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace EkstaziSharp.Tester
{
    public class MicrosoftBuildStrategy : IBuildStrategy
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Build(string solutionFilePath, string projectFilePath)
        {
            ProjectCollection collection = new ProjectCollection();

            Dictionary<string, string> globalProperties = new Dictionary<string, string>();

            /**
            We do not know configuration name of the target project.
            If we leave bellow properties unspecified then default configuration will be used.
            globalProperties.Add("Configuration", "Debug");
            globalProperties.Add("Platform", "Any CPU");
            e.g. Mono.Cecil project does not have configuration with the above name.
            **/
            // TODO: See what to do with signing problem
            // In a case of Mono.Cecil disabling signing process will lead to 
            // exception when trying to connect to a Test friend assembly using its strong name.
            if (!solutionFilePath.Contains("Mono.Cecil.sln"))
            {
                // Disable signing of assembly, otherwise modifying assembly will throw an exception.
                // e.g. LibGit2Sharp project uses signing.
                globalProperties.Add("SignAssembly", "false");
            }

            BuildParameters buildParameters = new BuildParameters(collection);
            MSBuildLogger customLogger = new MSBuildLogger();
            buildParameters.Loggers = new List<ILogger>() { customLogger };
            BuildRequestData buildRequest = new BuildRequestData(solutionFilePath,
                globalProperties, null, new string[] { "Build" }, null);

            BuildResult buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequest);
            if (buildResult.OverallResult == BuildResultCode.Failure)
            {
                logger.Error("BuildError: " + customLogger.BuildErrors);
            }
            return buildResult.OverallResult == BuildResultCode.Success;
        }
    }
}
