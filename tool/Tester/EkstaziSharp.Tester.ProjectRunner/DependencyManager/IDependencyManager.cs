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

namespace EkstaziSharp.Tester
{
    public interface IDependencyManager
    {
        /// <summary>
        /// Fetches a project dependencies using underlying dependency fetching strategy.
        /// If <paramref name="projectFilePath"/> is not null 
        /// fetches dependencies only for that single project.
        /// If <paramref name="projectFilePath"/> is null, 
        /// or if it is not possible to fetch dependencies for that single project,
        /// tries fetching dependencies for the whole solution.
        /// </summary>
        /// <param name="solutionFilePath">Path to a solution (.sln) file</param>
        /// <param name="projectFilePath">Path to a project (.csproj) file</param>
        /// <returns>True, if dependencies are succeessfully fetched; false, otherwise</returns>
        bool FetchDependencies(string solutionFilePath, string projectFilePath);
    }
}