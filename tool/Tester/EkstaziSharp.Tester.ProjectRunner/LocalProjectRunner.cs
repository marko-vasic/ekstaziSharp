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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkstaziSharp.Tester
{
    public class LocalProjectRunner : ProjectRunner
    {
        #region Protected Fields

        protected readonly new LocalProjectRunnerArguments args;

        #endregion

        public LocalProjectRunner(ProjectRunnerArguments args) : base(args)
        {
            if (!(args is LocalProjectRunnerArguments))
            {
                throw new ArgumentException("Incorrect type of arguments provided!");
            }
            this.args = args as LocalProjectRunnerArguments;
        }

        protected override IEnumerable<string> ProgramModules
        {
            get
            {
                return args.ProgramModules.Select(modulePath => Path.Combine(ProjectPath, modulePath));
            }
        }

        protected override string ProjectPath
        {
            get
            {
                return args.ProjectPath;
            }
        }

        protected override IEnumerable<string> TestModules
        {
            get
            {
                return args.TestModules.Select(modulePath => Path.Combine(ProjectPath, modulePath));
            }
        }

        protected override void InitializeProject()
        {
            return;
        }

        protected override bool MoveToNextRevision()
        {
            return false;
        }
    }
}
