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
using Newtonsoft.Json;
using CommandLine;
using System;

namespace EkstaziSharp.Tester
{
    public class LocalProjectRunnerArguments : ProjectRunnerArguments
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
        /// Path to the project located on a local disk.
        /// This path is relative to the directory of exe file.
        /// </summary>
        [Option("projectPath", 
            Required = true,
            HelpText = "Specify path to the project")]
        [JsonProperty(Required = Required.Always)]
        public string ProjectPath { get; set; }

        public override bool CleanEkstaziFiles
        {
            get
            {
                return false;
            }
        }
    }
}
