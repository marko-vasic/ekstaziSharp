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

//#define Debug

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using EkstaziSharp.Util;

namespace EkstaziSharp
{
    public static class DependencyMonitor
    {
        public enum InstrumentationArguments
        {
            Strings,
            ReflectionObjects
        }

        #region Fields

        public const string ClassFullName = "EkstaziSharp.DependencyMonitor";

        private static MethodBase CurrentTestMethod = null;

        private static string CurrentTestMethodName = string.Empty;

        private static string CurrentTestClassName = string.Empty;

        /// <summary>
        /// Set of dependencies for current Test Class.
        /// </summary>
        private static HashSet<string> Dependencies = new HashSet<string>();

        #endregion

        #region Introspection Methods

        public static string GetTMethodFullName(InstrumentationArguments argsType)
        {
            switch (argsType)
            {
                case InstrumentationArguments.ReflectionObjects:
                    return "System.Void EkstaziSharp.DependencyMonitor::T(System.Type)";
                case InstrumentationArguments.Strings:
                    return "System.Void EkstaziSharp.DependencyMonitor::T(System.String)";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetTestClassStartFullName(InstrumentationArguments args)
        {
            switch (args)
            {
                case InstrumentationArguments.ReflectionObjects:
                    return "System.Void EkstaziSharp.DependencyMonitor::TestClassStart(System.Type)";
                case InstrumentationArguments.Strings:
                    return "System.Void EkstaziSharp.DependencyMonitor::TestClassStart(System.String,System.String)";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetTestClassEndFullName(InstrumentationArguments args)
        {
            switch (args)
            {
                case InstrumentationArguments.ReflectionObjects:
                    return "System.Void EkstaziSharp.DependencyMonitor::TestClassEnd(System.Type)";
                case InstrumentationArguments.Strings:
                    return "System.Void EkstaziSharp.DependencyMonitor::TestClassEnd(System.String)";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetTestMethodStartFullName(InstrumentationArguments args)
        {
            switch (args)
            {
                case InstrumentationArguments.ReflectionObjects:
                    return "System.Void EkstaziSharp.DependencyMonitor::TestMethodStart(System.Reflection.MethodBase)";
                case InstrumentationArguments.Strings:
                    return "System.Void EkstaziSharp.DependencyMonitor::TestMethodStart(System.String,System.String)";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetTestMethodEndFullName(InstrumentationArguments args)
        {
            switch (args)
            {
                case InstrumentationArguments.ReflectionObjects:
                    return "System.Void EkstaziSharp.DependencyMonitor::TestMethodEnd(System.Reflection.MethodBase)";
                case InstrumentationArguments.Strings:
                    return "System.Void EkstaziSharp.DependencyMonitor::TestMethodEnd(System.String)";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        static DependencyMonitor()
        {
            if (File.Exists(Paths.DependencyMonitorConfigurationFileName))
            {
                var outputDirectory = File.ReadAllText(Paths.DependencyMonitorConfigurationFileName);
                Paths.OutputDirectory = outputDirectory;
            }
            if (!Directory.Exists(Paths.GetDependenciesFolderPath(Paths.Direction.Output)))
            {
                Directory.CreateDirectory(Paths.GetDependenciesFolderPath(Paths.Direction.Output));
            }
        }

        #region Private Methods

        private static string DependenciesToJson(string testName)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            sb.AppendLine("{");

            sb.AppendLine($"  \"TestName\":\"{testName}\",");

            sb.AppendLine("  \"Dependencies\":");
            sb.AppendLine("  [");
            foreach (var dependency in Dependencies)
            {
                if (!first)
                {
                    sb.AppendLine(",");
                }
                else
                {
                    first = false;
                }
                sb.Append($"    \"{dependency}\"");
            }
            if (Dependencies.Count > 0)
            {
                sb.AppendLine(",");
            }
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// Returns a path to dependency file for a given test.
        /// </summary>
        private static string GetDependencyFilePath(string testName)
        {
            string dependenciesFolderPath = Paths.GetDependenciesFolderPath(Paths.Direction.Output);
            string filePath = Path.Combine(dependenciesFolderPath, $"{testName}.json");
            if (!filePath.IsFilePathTooLong())
            {
                return filePath;
            }
            else
            {
                string hashedPath = filePath.Hashed();
                return Path.Combine(dependenciesFolderPath, $"{hashedPath}.json");
            }
        }

        private static void WriteDependenciesToFile(string testName)
        {
            string filePath = GetDependencyFilePath(testName);
            File.WriteAllText(filePath, DependenciesToJson(testName));
        }

        /// <summary>
        /// Returns test class name in ECMA335 specification format.
        /// NOTE: ECMA335 format is used for type names by Mono.Cecil,
        /// that is why conversion is needed.
        /// </summary>
        private static string GetTestClassName(Type type)
        {
            return type.FullName.GetECMA335Name();
        }

        /// <summary>
        /// Returns test method name in ECMA335 specification format.
        /// NOTE: ECMA335 format is used for type names by Mono.Cecil,
        /// that is why conversion is needed.
        /// </summary>
        private static string GetTestMethodName(MethodBase method)
        {
            return $"{method.DeclaringType.FullName.GetECMA335Name()}.{method.Name}";
        }

        /// <summary>
        /// Returns a dependency name as used for writing to a dependency file.
        /// </summary>
        private static string GetDependencyName(Type type)
        {
            string codeBase = type.Assembly.CodeBase;
            string escapedFileString = codeBase.Substring(8);
            string path = escapedFileString.CleanFileName(Util.Util.DefaultPathReplacementChar);
            path = path.ToLower();
            string typeName = GetTestClassName(type);
            return $"{path}.{typeName}";
        }

        #endregion

        #region Public Methods (Using Type and MethodBase)

        public static void T(Type type)
        {
            #if Debug
            Logger.Instance.Log("T: " + type.FullName);
            #endif

            Dependencies.Add(GetDependencyName(type));
        }

        public static void TestClassStart(Type type)
        {
            #if Debug
            Logger.Instance.Log("TestClassStart: " + type.FullName);
            #endif

            Dependencies.Clear();
            // ensure test class is added to dependencies
            // needed in cases when constructor of a test class
            // gets invoked before its TestClassStart
            Dependencies.Add(GetDependencyName(type));
        }

        public static void TestClassEnd(Type type)
        {
            #if Debug
            Logger.Instance.Log("TestClassEnd: " + type.FullName);
            #endif

            WriteDependenciesToFile(GetTestClassName(type));
        }

        public static void TestMethodStart(MethodBase method)
        {
            #if Debug
            Logger.Instance.Log("TestMethodStart: " + (method != null ? method.Name : "null"));
            #endif

            Dependencies.Clear();
            if (method != null)
            {
                CurrentTestMethod = method;
                // always add test class into dependency list
                Dependencies.Add(GetDependencyName(method.DeclaringType));
            }
        }

        public static void TestMethodEnd(MethodBase method)
        {
            #if Debug
            Logger.Instance.Log("TestMethodEnd: " + (method != null ? method.Name : "null"));
            #endif

            // TestMethodEnd can be called with null value for a method argument
            // that occurs in testing frameworks where TestMethodEnd is invoked at the same point for multiple test methods.
            // in such a case we should use the value set in previous invocation of TestMethodStart method

            MethodBase currentMethod = method != null ? method : CurrentTestMethod;

            if (currentMethod != null)
            {
                WriteDependenciesToFile(GetTestMethodName(currentMethod));
            }

            CurrentTestMethod = null;
        }

        #endregion

        #region Public Methods (Using string arguments)

        public static void T(string typeWithFullPath)
        {
            #if Debug
            Logger.Instance.Log("T: " + typeWithFullPath);
            #endif

            Dependencies.Add(typeWithFullPath);
        }

        public static void TestClassStart(string path, string testClass)
        {
            #if Debug
            Logger.Instance.Log($"TestClassStart: {testClass} path: {path}");
            #endif

            CurrentTestClassName = testClass;
            Dependencies.Clear();
            // ensure test class is added to dependencies
            // needed in cases when constructor of a test class
            // gets invoked before its TestClassStart
            Dependencies.Add($"{path}.{testClass}");
        }

        public static void TestClassEnd(string testClassWithFullPath)
        {
            #if Debug
            Logger.Instance.Log("TestClassEnd: " + testClassWithFullPath);
            #endif

            WriteDependenciesToFile(CurrentTestClassName);

            CurrentTestClassName = string.Empty;
        }

        public static void TestMethodStart(string path, string testMethod)
        {
            #if Debug
            Logger.Instance.Log("TestMethodStart: " + testMethod);
            #endif

            CurrentTestMethodName = testMethod;
            Dependencies.Clear();
            if (testMethod != null)
            {
                int dotIndex = testMethod.LastIndexOf('.');

                string className = testMethod.Substring(0, dotIndex);
                // always add test class into dependency list
                Dependencies.Add($"{path}.{className}");
            }
        }

        public static void TestMethodEnd(string testMethodWithFullPath)
        {
            #if Debug
            Logger.Instance.Log("TestMethodEnd: " + testMethodWithFullPath);
            #endif

            // TestMethodEnd can be called with null value for a method argument
            // that occurs in testing frameworks where TestMethodEnd is invoked at the same point for multiple test methods.
            // in such a case we should use the value set in previous invocation of TestMethodStart method

            if (CurrentTestMethodName != string.Empty)
            {
                WriteDependenciesToFile(CurrentTestMethodName);
            }

            CurrentTestMethodName = string.Empty;
        }

        #endregion
    }
}
