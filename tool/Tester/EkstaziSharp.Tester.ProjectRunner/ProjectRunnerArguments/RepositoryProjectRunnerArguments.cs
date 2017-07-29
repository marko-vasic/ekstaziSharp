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

using CommandLine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EkstaziSharp.Tester
{
    public class RepositoryProjectRunnerArguments : ProjectRunnerArguments
    {
        [Option("programModules", 
            Separator = ',', 
            Required = true,
            HelpText = "Specify list of paths to program modules. Path should be specified relatively to the projectPath.")]
        [JsonProperty(Required = Required.Always)]
        public IEnumerable<string> ProgramModules { get; set; }

        [Option("testModules", 
            Separator = ',', 
            Required = true,
            HelpText = "Specify list of paths to test modules. These are the modules which from tests will be run. Path should be specified relatively to the projectPath.")]
        [JsonProperty(Required = Required.Always)]
        public IEnumerable<string> TestModules { get; set; }

        /// <summary>
        /// List of commit SHA's.
        /// Ordered starting from the most recent commit.
        /// </summary>
        [Option("commits", 
            Separator = ',', 
            Required = true,
            HelpText = "Specify list of commits")]
        [JsonProperty(Required = Required.Always)]
        public List<string> Commits { get; set; }
    }
}
