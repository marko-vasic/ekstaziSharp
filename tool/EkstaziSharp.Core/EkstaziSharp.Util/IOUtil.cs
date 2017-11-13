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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EkstaziSharp.Util
{
    public static class IOUtil
    {
        public static void WriteAllText(string path, string contents)
        {
            FileInfo fi = new FileInfo(path);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            File.WriteAllText(path, contents);
        }

        /// <summary>
        /// Deletes a directory and all its contents.
        /// If directory does not exist it does not have effect.
        /// </summary>  
        public static void DeleteDirectory(DirectoryInfo dir)
        {
            if (dir.Exists)
            {
                foreach (var subdirectory in dir.GetDirectories())
                {
                    DeleteDirectory(subdirectory);
                }
                foreach (var file in dir.GetFiles())
                {
                    // Change file attributes to avoid access denied errors
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                Directory.Delete(dir.FullName);
            }
        }

        /// <summary>
        /// Deletes a directory and all its contents.
        /// If directory does not exist it does not have effect.
        /// </summary>  
        public static void DeleteDirectory(string dirPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            DeleteDirectory(dirInfo);
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern bool PathRelativePathTo(
             [Out] StringBuilder pszPath,
             [In] string pszFrom,
             [In] FileAttributes dwAttrFrom,
             [In] string pszTo,
             [In] FileAttributes dwAttrTo
        );

        /// <summary>
        /// Get relative path between two paths.
        /// </summary>
        /// <param name="path1">Source path</param>
        /// <param name="path2">Destination path</param>
        public static string GetRelativePath(FileSystemInfo path1, FileSystemInfo path2)
        {
            if (path1 == null) throw new ArgumentNullException("path1");
            if (path2 == null) throw new ArgumentNullException("path2");

            Func<FileSystemInfo, string> getFullName = delegate (FileSystemInfo path)
            {
                string fullName = path.FullName;

                if (path is DirectoryInfo)
                {
                    if (fullName[fullName.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                    {
                        fullName += System.IO.Path.DirectorySeparatorChar;
                    }
                }
                return fullName;
            };

            string path1FullName = getFullName(path1);
            string path2FullName = getFullName(path2);

            Uri uri1 = new Uri(path1FullName);
            Uri uri2 = new Uri(path2FullName);
            Uri relativeUri = uri1.MakeRelativeUri(uri2);

            return relativeUri.OriginalString;
        }
    }
}
