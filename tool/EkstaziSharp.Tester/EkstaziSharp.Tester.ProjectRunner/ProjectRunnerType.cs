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

namespace EkstaziSharp.Tester
{
    public enum ProjectRunnerType
    {
        /// <summary>
        /// Versioned project under test project
        /// </summary>
        TestProject,
        /// <summary>
        /// Local Git repository
        /// </summary>
        LocalRepository,
        /// <summary>
        /// Remote Git repository
        /// </summary>
        RemoteRepository,
        LocalProject
    }

    public static class ProjectRunnerTypeExtensions
    {
        public static ProjectRunner GetProjectRunner(this ProjectRunnerType type, ProjectRunnerArguments args)
        {
            switch (type)
            {
                case ProjectRunnerType.TestProject: return new TestProjectRunner(args);
                case ProjectRunnerType.LocalRepository: return new LocalRepositoryProjectRunner(args);
                case ProjectRunnerType.RemoteRepository: return new RemoteRepositoryProjectRunner(args);
                case ProjectRunnerType.LocalProject: return new LocalProjectRunner(args);
                default: throw new System.Exception($"ProjectRunnerType: {type} Not Supported");
            }
        }

        public static Type GetArgumentsType(this ProjectRunnerType type)
        {
            switch (type)
            {
                case ProjectRunnerType.LocalRepository: return typeof(LocalRepositoryProjectRunnerArguments);
                case ProjectRunnerType.RemoteRepository: return typeof(RemoteRepositoryProjectRunnerArguments);
                case ProjectRunnerType.TestProject: return typeof(TestProjectRunnerArguments);
                case ProjectRunnerType.LocalProject: return typeof(LocalProjectRunnerArguments);
                default: return typeof(ProjectRunnerArguments);
            }
        }
    }
}
