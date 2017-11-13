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

using System.ComponentModel;
using System;
using EkstaziSharp.Instrumentation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using CommandLine;
using System.Collections.Generic;

namespace EkstaziSharp.Tester
{
    public class ProjectRunnerArguments
    {
        #region Properties

        /// <summary>
        /// Whether project is in remote repository, or in local folder.
        /// </summary>
        [Option("testSource", 
            Required = true,
            HelpText = @"Specify the source of the project under test. 
                              Allowed Options:
                              LocalProject:     Use for projects located on local machine.
                              LocalRepository:  Use for local projects that use git version control. You can use LocalProject instead in case you do not want to specify revisions/commits for testing.
                              RemoteRepository: Use for projects that use git version control and that are on remote.
                              TestProject:      Do not use this option. It is used for EkstaziSharp tests")]
        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ProjectRunnerType TestSource { get; set; }

        [Option("buildStrategy", 
            Required = false, 
            Default = BuildStrategyType.None,
            HelpText = @"Specify the build strategy that will be used for building the project under test.
                                Allowed Options:
                                MSBuild14:    Use MSBuild14 to build the project.
                                MSBuild12:    Use MSBuild12 to build the project.
                                VisualStudio: Use Visual studio tools to build the project.
                                Microsoft:    Use Microsoft Build Framework to build the project.
                                Script:       Use custom script to build the project. Provide path to the script via buildScriptPath option
                                None:         Use if you do not wont tool to build the project. Note that project should already be built when running the tool in that case.")]
        [DefaultValue(BuildStrategyType.None)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate, Required = Required.Default)]
        [JsonConverter(typeof(StringEnumConverter))]
        public BuildStrategyType BuildStrategyType { get; set; }

        [Option("buildScriptPath",
            Required = false,
            Default = null,
            HelpText = @"Specify path to script to be used for building a project. Use this in combination with buildStrategy: Script")]
        [JsonProperty(Required = Required.Default)]
        public string BuildScriptPath { get; set; }

        [Option("dependencyManager", 
            Required = false, 
            Default = DependencyManagerType.None,
            HelpText = @"Specify the dependency manager that will be used to fetch necessary packages in order to build the project under test. 
                                Allowed Options:
                                Nuget: Use Nuget package manager to fetch packages.
                                Dotnet: Use Dotnet package manager to fetch packages.
                                None: Do not fetch packages. Typically use this on parallel with BuildStrategy.None option.")]
        [DefaultValue(DependencyManagerType.None)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate, Required = Required.Default)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DependencyManagerType DependencyManagerType { get; set; }

        /// <summary>
        /// Path to a solution (.sln) file.
        /// Path is relative to the project root folder.
        /// </summary>
        [Option("solutionPath", 
            Required = false,
            HelpText = "Specify path to a solution (.sln) file. This has to be specified if you want the tool to build your project. If buildStrategy is set to None does not have effect.")]
        [JsonProperty(Required = Required.Default)]
        public string SolutionPath { get; set; }

        /// <summary>
        /// Path to a project (.csproj) file.
        /// Path is relative to the project root folder.
        /// </summary>
        [Option("projectFilePath",
            Required = false,
            HelpText = @"Specify path to a project (.csproj) file.
                                This option should always be used in combination with solutionPath;
                                while it is not required, this option can optimize build and fetching dependencies stages by concentrating only on the project instead of the whole solution.
                                If buildStrategy is set to None does not have effect.")]
        [JsonProperty(Required = Required.Default)]
        public string ProjectFilePath { get; set; }
        
        [Option("testingFramework", 
            Required = true,
            HelpText = @"Specify Test Framework used by your project. 
                                Supported Frameworks:
                                XUnit2
                                XUnit1
                                NUnit3
                                NUnit2")]
        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public TestingFrameworkType TestingFramework { get; set; }

        /// <summary>
        /// Custom arguments to be provided when running tests using testing framework.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string[] TestingFrameworkArguments { get; set; }

        [Option("instrumentationStrategy", 
            Required = false, 
            Default = InstrumentationStrategy.InstanceConstructor,
            HelpText = @"Specify a strategy Ekstazi will use beneath.
                                Allowed Options:
                                InstanceConstructor: This is a default strategy where Ekstazi inserts calls into instance constructors to collect selection information. 
                                StaticConstructor:   Under Development. This is a strategy where Ekstazi inserts calls into static constructors to collect selection information.
                                None:                When this is used Ekstazi behaves like RetestAll approach, where all tests are run.")]
        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public InstrumentationStrategy InstrumentationStrategy { get; set; }

        [Option("instrumentAtBeginningOfMethods", 
            Required = false, 
            Default = false,
            HelpText = @"If true, insert all instrumentation at beginning of methods; if false, instrument where needed.
                            There is a trade-off here: reduction of calls to dependency method, and selection precision.")]
        [JsonProperty(Required = Required.Default)]
        public bool InstrumentAtMethodBeginning { get; set; }

        [Option("dependencyGranularity",
            Required = false,
            Default = DependencyCollectionGranularity.Class,
            HelpText = @"Define granularity on which dependencies will be collected.")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate, Required = Required.Default)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DependencyCollectionGranularity DependencyCollectionGranularity { get; set; }

        [DefaultValue(DependencyMonitor.InstrumentationArguments.Strings)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate, Required = Required.Default)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DependencyMonitor.InstrumentationArguments InstrumentationArgumentsType { get; set; }

        [Option("noSmartChecksums", 
            Required = false,
            Default = false,
            HelpText = "If false, smart checksums used; otherwise, regular checksums.")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.Default)]
        public bool NoSmartChecksums { get; set; }

        [Option("runInIsolation", Required = false, Default = false)]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool RunInIsolation { get; set; }

        [Option("outputDirectory", 
            Required = false,
            HelpText = "Specify the output directory for ekstazi files.")]
        [JsonProperty(Required = Required.Default)]
        public string OutputDirectory { get; set; }

        [Option("inputDirectory", 
            Required = false,
            HelpText = "Specify the input directory for ekstazi files")]
        [JsonProperty(Required = Required.Default)]
        public string InputDirectory { get; set; }

        [Option("debug", 
            Required = false, 
            Default = false,
            HelpText = "Specify to get debug logs.")]
        [DefaultValue(false)]
        [JsonProperty(Required = Required.Default)]
        public bool Debug { get; set; }

        [Option("help", Required = false, Default = false)]
        public bool Help { get; set; }

        #endregion

        #region Virtual Properties

        public virtual bool CleanEkstaziFiles { get { return true; } }

        #endregion
    }
}
