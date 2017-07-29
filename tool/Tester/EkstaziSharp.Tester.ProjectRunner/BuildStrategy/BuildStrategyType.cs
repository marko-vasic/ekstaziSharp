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
    public enum BuildStrategyType
    {
        MSBuild14,
        MSBuild12,
        VisualStudio,
        Microsoft,
        Script,
        None
    }

    public static class BuildStrategyTypeExtensions
    {
        public static IBuildStrategy GetBuildStrategy(this BuildStrategyType type, ProjectRunnerArguments args)
        {
            switch (type)
            {
                case BuildStrategyType.MSBuild14:
                    return new MSBuild14BuildStrategy();
                case BuildStrategyType.MSBuild12:
                    return new MSBuild12BuildStrategy();
                case BuildStrategyType.VisualStudio:
                    return new VisualStudioBuildStrategy();
                case BuildStrategyType.Microsoft:
                    return new MicrosoftBuildStrategy();
                case BuildStrategyType.Script:
                    return new ScriptBuildStrategy(args.BuildScriptPath);
                case BuildStrategyType.None:
                    return new NoneBuildStrategy();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
