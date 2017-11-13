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
using System.Collections.Generic;
using Mono.Cecil;
using System.Linq;
using Newtonsoft.Json;
using EkstaziSharp.Util;
using EkstaziSharp.Instrumentation;

namespace EkstaziSharp.Analysis
{
    public class AnalyzerParameters
    {
        public DependencyCollectionGranularity DependencyCollectionGranularity
        { get; private set; }
        public bool UseSmartChecksums
        { get; private set; }

        public AnalyzerParameters(DependencyCollectionGranularity granularity, bool useSmartChecksums)
        {
            DependencyCollectionGranularity = granularity;
            UseSmartChecksums = useSmartChecksums;
        }
    }

    public class TestDependencies
    {
        public string TestName { get; set; }
        public List<string> Dependencies { get; set; }
    }

    public abstract class Analyzer
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        AnalyzerParameters parameters;

        #region Constructors

        public Analyzer(AnalyzerParameters parameters)
        {
            this.parameters = parameters;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Detects and returns collection of Test Classes that are affected by recent program changes.
        /// It does so by comparing checksums of the old code revision with newly calculated checksums.
        /// Guarantees: There will be no duplicates in the returned collection.
        /// </summary>
        /// <param name="testModulePaths">List of paths to test modules (modules containing test cases).</param>
        /// <param name="programModulePaths">List of paths to program modules (modules containing program under test).</param>
        /// <returns>TypeDefinition of affected test classes.</returns>
        public Dictionary<string, ICollection<IMemberDefinition>> Analyze(IEnumerable<string> testModulePaths, IEnumerable<string> programModulePaths)
        {
            Dictionary<string, ICollection<IMemberDefinition>> moduleToAffectedTests = new Dictionary<string, ICollection<IMemberDefinition>>();

            IEnumerable<string> allModules = Enumerable.Union(programModulePaths, testModulePaths);
            if (Instrumentator.AreInstrumented(allModules))
            {
                // if modules are already instrumented that means nothing changed from the last run
                logger.Info("Modules already instrumented. Do not select any test for the run.");
                return moduleToAffectedTests;
            }

            // read old checksums
            Dictionary<string, string> oldChecksums = LoadChecksums();
            // compute new checksums
            Dictionary<string, string> newChecksums = ComputeChecksums(allModules);
            // store new checksums to a file
            StoreChecksums(newChecksums);
            // find all tests
            foreach (string testModulePath in testModulePaths)
            {
                IEnumerable<IMemberDefinition> tests = null;
                switch (parameters.DependencyCollectionGranularity)
                {
                    case DependencyCollectionGranularity.Class:
                        tests = FindAllTestClasses(testModulePath);
                        break;
                    case DependencyCollectionGranularity.Method:
                        tests = FindAllTestMethods(testModulePath);
                        break;
                    default:
                        throw new Exception("Unsupported Collection Granularity");
                }

                // find tests that are affected by recent program changes
                ICollection<IMemberDefinition> affectedTests = FindAffectedTests(tests, oldChecksums, newChecksums);
                moduleToAffectedTests.Add(testModulePath, affectedTests);
            }

            return moduleToAffectedTests;
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Returns a collection of <see cref="TypeDefinition"/> representing TestClasses found in provided modules.
        /// Guarantees: There will be no duplicates in returned collection.
        /// </summary>
        /// <param name="modulePaths">List of paths to modules.</param>
        protected abstract IEnumerable<IMemberDefinition> FindAllTestClasses(string modulePath);

        protected abstract IEnumerable<IMemberDefinition> FindAllTestMethods(string modulePath);

        #endregion Protected Methods

        #region Private Methods

        private Dictionary<string, string> LoadChecksums()
        {
            Dictionary<string, string> checksums = new Dictionary<string, string>();
            if (File.Exists(Paths.GetChecksumsFilePath(Paths.Direction.Input)))
            {
                try
                {
                    var checksumFileContent = File.ReadAllText(Paths.GetChecksumsFilePath(Paths.Direction.Input));
                    checksums = JsonConvert.DeserializeObject<Dictionary<string, string>>(checksumFileContent);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return checksums;
        }

        /// <summary>
        /// Computes checksums for all modules with specified paths.
        /// </summary>
        /// <param name="modulePaths"></param>
        /// <returns>
        /// Dictionary of checksums:
        /// Key - Full Name of a Type
        /// Value - Corresponding checksum
        /// </returns>
        private Dictionary<string, string> ComputeChecksums(IEnumerable<String> modulePaths)
        {
            Dictionary<string, string> checksums = new Dictionary<string, string>();

            if (modulePaths == null)
            {
                return checksums;
            }
            foreach (var modulePath in modulePaths)
            {
                ModuleDefinition moduleDefinition = CILTransformer.GetModuleDefinition(modulePath);
                if (moduleDefinition.HasTypes)
                {
                    foreach (var type in moduleDefinition.GetTypes(includeNestedTypes:true))
                    {
                        // TODO: Exclude <Module> type, and see what to do if you encounter
                        // the type with the same full name in different modules
                        var options = new CecilExtensions.TextualRepresentationOptions() { UseSmartRepresentation = parameters.UseSmartChecksums };
                        checksums[((IMemberDefinition)type).GetDependencyName()] = type.GetChecksum(options);
                    }
                }
            }

            return checksums;
        }

        private void StoreChecksums(Dictionary<string, string> checksums)
        {
            var checksumsDirPath = Path.GetDirectoryName(Paths.GetChecksumsFilePath(Paths.Direction.Output));
            if (!Directory.Exists(checksumsDirPath))
            {
                // ensure that directory exists
                Directory.CreateDirectory(checksumsDirPath);
            }

            // clear checksums file
            // TODO: See whether clearing is necessary since the method should overwrite existing file
            File.WriteAllText(Paths.GetChecksumsFilePath(Paths.Direction.Output), "");
            File.WriteAllText(Paths.GetChecksumsFilePath(Paths.Direction.Output), JsonConvert.SerializeObject(checksums, Formatting.Indented));
        }

        private ICollection<IMemberDefinition> FindAffectedTests(IEnumerable<IMemberDefinition> allTests, Dictionary<string, string> oldChecksums, Dictionary<string, string> newChecksums)
        {
            List<IMemberDefinition> affected = new List<IMemberDefinition>();
            foreach (var test in allTests)
            {
                string dependencyPath = test.GetDependencyFilePath();
                
                if (File.Exists(dependencyPath))
                {
                    var testDependencies = JsonConvert.DeserializeObject<TestDependencies>(File.ReadAllText(dependencyPath));
                    if (IsAffected(testDependencies.Dependencies, oldChecksums, newChecksums))
                    {
                        affected.Add(test);
                    }
                }
                else
                {
                    affected.Add(test);
                }
            }
            var affectedTestNames = affected.Select(t => t.FullName);
            // TODO: Check if affected should be written in Input or Output
            File.WriteAllText(Paths.GetAffectedTestsFilePath(Paths.Direction.Output), JsonConvert.SerializeObject(affectedTestNames));
            return affected;
        }

        private bool IsAffected(List<string> dependencies, Dictionary<string, string> oldChecksums, Dictionary<string, string> newChecksums)
        {
            foreach (var dependency in dependencies)
            {
                string oldChecksum;
                string newChecksum;
                oldChecksums.TryGetValue(dependency, out oldChecksum);
                newChecksums.TryGetValue(dependency, out newChecksum);
                
                if (oldChecksum != null && newChecksum != null)
                {
                    // both checksums exist
                    if (oldChecksum != newChecksum)
                    {
                        // dependency changed
                        return true;
                    }
                }
                else if (oldChecksum != null && newChecksum == null)
                {
                    // old checksum exists but new does not
                    // dependency deleted
                    return true;
                }
                else if (oldChecksum == null && newChecksum != null)
                {
                    // old checksum does not exist but new does
                    // new dependency created
                    return true;
                }
                else
                {
                    // neither old nor new checksum exist
                    continue;
                }
            }
            return false;
        }

        #endregion Private Methods
    }
}
