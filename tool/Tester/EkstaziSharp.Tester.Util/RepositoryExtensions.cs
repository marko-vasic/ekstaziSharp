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
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace EkstaziSharp.Tester.Util
{
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Get <paramref name="n"/> most recent commits of the repository.
        /// </summary>
        /// <param name="n"></param>
        /// <returns>
        /// The <paramref name="n"/> most recent commits of the <paramref name="repo"/>,
        /// ordered starting from the least recent commit to the most recent one;
        /// Returns null if repo is null or n is nonpositive.
        /// </returns>
        public static LinkedList<Commit> GetMostRecentCommits(this Repository repo, int n)
        {
            if (repo == null || n <= 0)
            {
                return null;
            }

            LinkedList<Commit> commits = new LinkedList<Commit>();
            int num = n;
            foreach (Commit c in repo.Commits)
            {
                commits.AddFirst(c);
                if (--n == 0)
                {
                    break;
                }
            }
            return commits;
        }
    }
}