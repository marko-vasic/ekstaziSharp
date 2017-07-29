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
using System.IO;
using System.Collections.Generic;
using EkstaziSharp.Tester.Util;
using EkstaziSharp.Util;

namespace EkstaziSharp.Tester
{
    /// <summary>
    /// Runner customized for our short test examples.
    /// </summary>
    public class TestProjectRunner : ProjectRunner
    {
        #region Private Fields

        private int currentVersionIndex;

        #endregion

        #region Protected Fields

        protected new TestProjectRunnerArguments args;

        #endregion

        #region Protected Properties

        protected override IEnumerable<string> ProgramModules
        {
            get
            {
                string dllPath = TestProjectUtil.GetDllPath(args, currentVersionIndex);
                var modulesUnderTest = new List<string> { dllPath };
                return modulesUnderTest;
            }
        }

        protected override IEnumerable<string> TestModules
        {
            get
            {
                return ProgramModules;
            }
        }

        protected override string ProjectPath
        {
            get
            {
                return TestProjectUtil.GetTestsDirectory(args);
            }
        }

        #endregion

        #region Constructors

        public TestProjectRunner(ProjectRunnerArguments args) : base(args)
        {
            if (!(args is TestProjectRunnerArguments))
            {
                throw new ArgumentException("Incorrect type of arguments provided!");
            }
            this.args = args as TestProjectRunnerArguments;
        }

        #endregion

        #region Private Methods

        private void FakeResultsForNextRevision()
        {
            string checksumsPath = Paths.GetChecksumsFilePath(Paths.Direction.Output);
            string checksums = File.ReadAllText(checksumsPath);
            string modifiedChecksums = checksums.Replace($"v{currentVersionIndex}", $"v{currentVersionIndex + 1}");
            File.WriteAllText(checksumsPath, modifiedChecksums);

            string dependenciesDirPath = Paths.GetDependenciesFolderPath(Paths.Direction.Output);
            DirectoryInfo dependenciesDir = new DirectoryInfo(dependenciesDirPath);
            if (dependenciesDir.Exists)
            {
                foreach (var file in dependenciesDir.GetFiles())
                {
                    string dependencies = File.ReadAllText(file.FullName);
                    string modifiedDependencies = dependencies.Replace($"v{currentVersionIndex}", $"v{currentVersionIndex + 1}");
                    File.WriteAllText(file.FullName, modifiedDependencies);
                }
            }
        }

        #endregion

        #region Protected Methods

        protected override string GetProjectFilePath()
        {
            return Path.Combine(TestProjectUtil.GetTestsDirectory(args), args.Versions[currentVersionIndex], args.Versions[currentVersionIndex] + ".csproj");
        }

        protected override void InitializeProject()
        {
            currentVersionIndex = 0;
        }

        protected override bool MoveToNextRevision()
        {
            FakeResultsForNextRevision();

            currentVersionIndex++;

            if (currentVersionIndex >= args.Versions.Count)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion
    }
}
