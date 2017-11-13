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

using System.Linq;
using LibGit2Sharp;

namespace EkstaziSharp.Tester.Util
{
    public static class CommitExtensions
    {
        /// <summary>
        /// Gets the contents of the file inside of the project,
        /// only files in the root of the project can be fetched.
        /// </summary>
        /// <param name="commit">Represents revision of the project, which from file is read</param>
        /// <param name="name">Name of the file</param>
        public static string GetFileContents(this Commit commit, string name)
        {
            if (commit == null || commit.Tree == null || name == null)
            {
                return null;
            }

            TreeEntry te = commit.Tree.FirstOrDefault<TreeEntry>(t => t.Name == name);

            if (te != null && te.TargetType == TreeEntryTargetType.Blob)
            {
                var blob = (Blob)te.Target;
                return blob.GetContentText();
            }

            return null;
        }
    }
}
