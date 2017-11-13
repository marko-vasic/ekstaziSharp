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
using LibGit2Sharp;

namespace EkstaziSharp.Tester
{
    public class LocalRepositoryProjectRunner : RepositoryProjectRunner
    {
        #region Protected Fields

        protected readonly new LocalRepositoryProjectRunnerArguments args;

        #endregion

        #region Protected Properties

        protected override string ProjectPath
        {
            get
            {
                return args.ProjectPath;
            }
        }

        #endregion

        #region Constructors

        public LocalRepositoryProjectRunner(ProjectRunnerArguments args) : base(args)
        {
            if (!(args is LocalRepositoryProjectRunnerArguments))
            {
                throw new ArgumentException("Incorrect type of arguments provided!");
            }
            this.args = args as LocalRepositoryProjectRunnerArguments;
        }

        #endregion
        
        #region Protected Methods

        protected override Repository InitializeRepo()
        {
            return new Repository(ProjectPath);
        }

        #endregion
    }
}
