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

using System;
using System.IO;

namespace EkstaziSharp.Util
{
    public static class Paths
    {
        public enum Direction
        {
            Input,
            Output
        }

        #region Fields

        public static readonly string DefaultInOutDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        private static string outputDirectory = DefaultInOutDir;

        private static string inputDirectory = DefaultInOutDir;

        public const string DependencyMonitorConfigurationFileName = "DependencyMonitorConfiguration.txt";

        #endregion

        #region Properties

        public static string OutputDirectory
        {
            get
            {
                return outputDirectory;
            }
            set
            {
                outputDirectory = value;
            }
        }

        public static string InputDirectory
        {
            get
            {
                return inputDirectory;
            }
            set
            {
                inputDirectory = value;
            }
        }

        #endregion

        #region Methods

        public static string GetEkstaziOutputDirectory(Direction direction)
        {
            switch (direction)
            {
                case Direction.Input: return Path.Combine(InputDirectory, ".ekstaziSharp");
                case Direction.Output: return Path.Combine(OutputDirectory, ".ekstaziSharp");
                default: throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Path to a folder where ekstazi tracking information will be written.
        /// </summary>
        public static string GetEkstaziInformationFolderPath(Direction direction)
        {
            return Path.Combine(GetEkstaziOutputDirectory(direction), "ekstaziInformation");
        }

        /// <summary>
        /// Path to a folder containing execution logs.
        /// Some code assumes that this is inside of the EkstaziInformationFolderPath, so be careful if changing.
        /// Also logger configuration file should point to the same folder.
        /// </summary>    
        public static string GetEkstaziLogsFolderPath(Direction direction)
        {
            return Path.Combine(GetEkstaziInformationFolderPath(direction), "executionLogs");
        }

        ///// <summary>
        ///// Path to a file that contains checksums.
        ///// </summary>
        public static string GetChecksumsFilePath(Direction direction)
        {
            return Path.Combine(GetEkstaziInformationFolderPath(direction), "checksums.txt");
        }

        ///// <summary>
        ///// Name of a file that contains list of affected tests file names.
        ///// </summary>
        public static string GetAffectedTestsFilePath(Direction direction)
        {
            return Path.Combine(GetEkstaziInformationFolderPath(direction), "affected.json");
        }

        public static string GetDependenciesFolderPath(Direction direction)
        {
            return Path.Combine(GetEkstaziInformationFolderPath(direction), "dependencies");
        }

        #endregion
    }
}
