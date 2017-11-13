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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkstaziSharp.Tester
{
    public class ScriptBuildStrategy : IBuildStrategy
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string scriptPath;

        public ScriptBuildStrategy(string scriptPath)
        {
            this.scriptPath = scriptPath;
        }

        public bool Build(string solutionFilePath, string projectFilePath)
        {
            if (scriptPath == null)
            {
                logger.Error("Failing to build, build script has to be provided");
                return false;
            }

            var dir = Path.GetDirectoryName(scriptPath);
            var file = Path.GetFileName(scriptPath);

            return ProcessUtil.ExecuteProcessNoPoppingConsole("cmd", $"/C {file}", dir);
        }
    }
}
