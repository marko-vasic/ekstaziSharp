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
using System.Linq;
using LibGit2Sharp;
using System.IO;
using EkstaziSharp.Tester.Utils;
using EkstaziSharp.Util;

namespace EkstaziSharp.Tester
{
    public abstract class RepositoryProjectRunner : ProjectRunner
    {
        #region Fields

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly new RepositoryProjectRunnerArguments args;

        protected int currentCommitIndex;

        protected Repository repo;

        #endregion

        #region Protected Properties

        protected override IEnumerable<string> ProgramModules
        {
            get
            {
                return args.ProgramModules.Select(modulePath => Path.Combine(ProjectPath, modulePath));
            }
        }

        protected override IEnumerable<string> TestModules
        {
            get
            {
                return args.TestModules.Select(modulePath => Path.Combine(ProjectPath, modulePath));
            }
        }

        #endregion

        #region Constructors

        public RepositoryProjectRunner(ProjectRunnerArguments args) : base(args)
        {
            if (!(args is RepositoryProjectRunnerArguments))
            {
                throw new ArgumentException("Incorrect type of arguments provided!");
            }
            this.args = args as RepositoryProjectRunnerArguments;
        }

        #endregion

        #region Private Methods

        private void CommitLogs()
        {
            using (Repository repo = new Repository(Paths.GetEkstaziInformationFolderPath(Paths.Direction.Output)))
            {
                CommitOptions co = new CommitOptions();
                Commands.Stage(repo, "*");
                Signature mySignature = new Signature("Marko Vasic", "marko.z.vasic@gmail.com", DateTimeOffset.Now);
                repo.Commit(GetSHA(currentCommitIndex), mySignature, mySignature);
            }
        }

        private void CleanRepo()
        {
            ProcessUtil.ExecuteProcessNoPoppingConsole("git", "clean -xfd", ProjectPath);
            ProcessUtil.ExecuteProcessNoPoppingConsole("git", "checkout .", ProjectPath);
        }

        private void Checkout()
        {
            CleanRepo();
            Commands.Checkout(repo, GetSHA(currentCommitIndex));
        }

        #endregion

        #region Protected Methods

        protected int NumberOfCommits()
        {
            return args.Commits.Count;
        }

        protected string GetSHA(int commitIndex)
        {
            if (commitIndex < 0 || commitIndex >= args.Commits.Count)
            {
                throw new ArgumentException("Invalid commit index");
            }
            return args.Commits[args.Commits.Count - 1 - commitIndex];
        }

        protected void InitializeLogsRepository()
        {
            Repository.Init(Paths.GetEkstaziInformationFolderPath(Paths.Direction.Output));
        }

        protected override bool MoveToNextRevision()
        {
            CommitLogs();

            logger.InfoFormat("Processed commit {0}. Total number of commits {1}", currentCommitIndex + 1, NumberOfCommits());

            currentCommitIndex++;
            if (currentCommitIndex > 0 && currentCommitIndex < NumberOfCommits())
            {
                IOUtil.DeleteDirectory(Paths.GetEkstaziLogsFolderPath(Paths.Direction.Output));
                logger.Info($"Checking out commit {GetSHA(currentCommitIndex)}");
                Checkout();
                return true;
            }
            return false;
        }

        protected abstract Repository InitializeRepo();

        protected override sealed void InitializeProject()
        {
            repo = InitializeRepo();
            currentCommitIndex = 0;
            Checkout();
            InitializeLogsRepository();
        }

        #endregion
    }
}
