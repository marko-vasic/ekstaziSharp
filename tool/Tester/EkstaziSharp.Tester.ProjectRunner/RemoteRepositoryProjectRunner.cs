// Copyright (c) 2017, Marko Vasic and Zuhair Parvez
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
using LibGit2Sharp;

namespace EkstaziSharp.Tester
{
    public class RemoteRepositoryProjectRunner : RepositoryProjectRunner
    {
        #region Protected Fields

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly new RemoteRepositoryProjectRunnerArguments args;

        #endregion

        #region Constructors

        public RemoteRepositoryProjectRunner(ProjectRunnerArguments args) : base(args)
        {
            if (!(args is RemoteRepositoryProjectRunnerArguments))
            {
                throw new ArgumentException("Incorrect type of arguments provided!");
            }
            this.args = args as RemoteRepositoryProjectRunnerArguments;
        }

        #endregion

        #region Protected Properties

        protected override string ProjectPath
        {
            get
            {
                return CommonPaths.GetLocalRepositoryPath(args.RepositoryURL);
            }
        }

        #endregion

        #region Private Methods

        private bool RepositoryFetched()
        {
            // TODO: Add check if repository on the path is the one we are looking for.
            return Repository.IsValid(ProjectPath);
        }

        #endregion

        #region Protected Methods

        protected override Repository InitializeRepo()
        {
            if (!RepositoryFetched())
            {
                // make sure directory does not exist before cloning
                IOUtil.DeleteDirectory(ProjectPath);

                // clone if not already fetched
                // TODO: check if clonedRepoPath always matches ProjectPath
                logger.Debug($"Cloning repository into: {ProjectPath}");
                string clonedRepoPath = Repository.Clone(args.RepositoryURL, ProjectPath);
            }
            else
            {
                logger.Debug($"Repository already fetched into: {ProjectPath}; No need to clone again.");
            }
            return new Repository(ProjectPath);
        }

        #endregion
    }
}
