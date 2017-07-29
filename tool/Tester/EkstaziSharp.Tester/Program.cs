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
using Newtonsoft.Json;
using EkstaziSharp.Tester.Tests;
using EkstaziSharp.Tester.Util;
using System.Linq;

namespace EkstaziSharp.Tester
{
    public class Program
    {
        // example invocation arguments:
        // --testSource LocalProject --projectPath C:\\Users\\vasic\\.ekstaziSharp\\repositories\\https_github.com_JeremySkinner_FluentValidation --buildStrategy MSBuild14 --dependencyManager Dotnet --solutionPath FluentValidation.sln --projectFilePath src\\FluentValidation.Tests\\FluentValidation.Tests.csproj --programModules src\\FluentValidation.Tests\\bin\\Debug\\FluentValidation.dll --testModules src\\FluentValidation.Tests\\bin\\Debug\\FluentValidation.Tests.dll --testingFramework XUnit2 --outputDirectory C:\\Users\\vasic\\.ekstaziSharp\\repositories\\https_github.com_JeremySkinner_FluentValidation --inputDirectory C:\\Users\\vasic\\.ekstaziSharp\\repositories\\https_github.com_JeremySkinner_FluentValidation --debug

        private static string[] cmdLineExample = {
            "--testSource",
            "LocalProject",
            "--projectPath",
            "~/FluentValidation",
            "--programModules",
            "src\\FluentValidation.Tests\\bin\\Debug\\FluentValidation.dll",
            "--testModules",
            "src\\FluentValidation.Tests\\bin\\Debug\\FluentValidation.Tests.dll",
            "--testingFramework",
            "XUnit2",
            "--debug" };

        public static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.Equals("--help"))
                {
                    Console.Write(CommandLineUtil.HelpText(cmdLineExample, typeof(LocalProjectRunnerArguments)));
                    return;
                }
            }

            var cmdOptions = CommandLineUtil.ParseArguments<CommandLineOptions>(args);

            if (cmdOptions != null)
            {
                if (cmdOptions.ConfigurationFilePath != null)
                {
                    TestCommons.RunWithConfiguration(File.ReadAllText(cmdOptions.ConfigurationFilePath));
                }
                else
                {
                    string[] unknownArgs = new string[0];
                    Type runnerArgumentsType = cmdOptions.TestSource.GetArgumentsType();
                    int firstUnknownArg = CommandLineUtil.FindFirstUknownArg(args, runnerArgumentsType);
                    if (firstUnknownArg >= 0)
                    {
                        unknownArgs = args.Skip(firstUnknownArg).ToArray();
                    }

                    ProjectRunnerArguments runnerArgs = CommandLineUtil.ParseArguments(args, runnerArgumentsType) as ProjectRunnerArguments;
                    runnerArgs.TestingFrameworkArguments = unknownArgs;
                    TestCommons.RunWithArgs(runnerArgs);
                }
            }
        }
    }
}
