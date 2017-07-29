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
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using EkstaziSharp.Util;
using System.Text;

namespace EkstaziSharp.Instrumentation
{
    public class InstrumentatorParameters
    {
        public ModuleDefinition DependencyMonitor
        { get; private set; }
        public TestingFrameworkType TestingFrameworkType
        { get; private set; }
        public InstrumentationStrategy Strategy
        { get; private set; }
        public DependencyMonitor.InstrumentationArguments InstrumentationArgsType;
        public bool InstrumentAtMethodBeginning
        { get; private set; }
        /// <summary>
        /// Represents a list of all modules under instrumentation
        /// </summary>
        public ICollection<ModuleDefinition> ModulesUnderInstrumentation
        { get; set; }

        public InstrumentatorParameters(ModuleDefinition dependencyMonitor, TestingFrameworkType testingFrameworkType, InstrumentationStrategy strategy, DependencyMonitor.InstrumentationArguments instrumentationArgsType, bool instrumentAtMethodBeginning)
        {
            this.DependencyMonitor = dependencyMonitor;
            this.TestingFrameworkType = testingFrameworkType;
            this.Strategy = strategy;
            this.InstrumentationArgsType = instrumentationArgsType;
            this.InstrumentAtMethodBeginning = instrumentAtMethodBeginning;
        }
    }

    public class Instrumentator
    {
        #region Fields

        InstrumentatorParameters parameters;

        #endregion

        #region Properties

        #endregion

        #region Constructor

        public Instrumentator(InstrumentatorParameters parameters)
        {
            this.parameters = parameters;
        }

        #endregion

        #region Public Static Methods

        public static bool AreInstrumented(IEnumerable<string> modulePaths)
        {
            return AreInstrumented(modulePaths.Select<string, ModuleDefinition>((path) => CILTransformer.GetModuleDefinition(path)));
        }

        public static bool AreInstrumented(IEnumerable<ModuleDefinition> modules)
        {
            int instrumentedCount = 0;
            int modulesCount = 0;
            foreach (var module in modules)
            {
                modulesCount++;
                instrumentedCount += IsInstrumented(module) ? 1 : 0;
            }
            if (instrumentedCount == 0)
            {
                return false;
            }
            else if (instrumentedCount == modulesCount)
            {
                return true;
            }
            else
            {
                throw new Exception("Only some modules instrumented. Not Currently Supported!");
            }
        }

        public static bool IsInstrumented(ModuleDefinition module)
        {
            var references = module.GetReferencedAssemblies();
            var depMonitor = references.SingleOrDefault<AssemblyDefinition>(a =>
            {
                return a.FullName.Contains("DependencyMonitor");
            });
            return depMonitor != null;
        }


        #endregion

        #region Private Methods

        private void FlushModules(IEnumerable<ModuleDefinition> modules)
        {
            foreach (var module in modules)
            {
                CILTransformer.WriteModule(module, module.FullyQualifiedName);
            }
        }

        private ProgramInstrumentator GetProgramInstrumentator(ModuleDefinition module)
        {
            switch (parameters.Strategy)
            {
                case InstrumentationStrategy.InstanceConstructor:
                    return new InstanceProgramInstrumentator(module, parameters);
                case InstrumentationStrategy.StaticConstructor:
                    return new StaticConstructorProgramInstrumentator(module, parameters);
                default:
                    throw new Exception("Unsupported instrumentation strategy.");
            }
        }

        private HashSet<ModuleDefinition> LoadModules(IEnumerable<string> paths)
        {
            HashSet<ModuleDefinition> modules = new HashSet<ModuleDefinition>();
            foreach (var path in paths)
            {
                ModuleDefinition programModule = CILTransformer.GetModuleDefinition(path);
                modules.Add(programModule);
            }
            return modules;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Modifies bytecode of tests and programs such that appropriate methods of
        /// <see cref="EkstaziSharp.DependencyMonitor"/> get executed during test execution:
        /// call to <see cref="EkstaziSharp.DependencyMonitor.TestClassStart(string)"/> inserted such that
        /// it is called before a test starts executing. Parameter represents full name of the test.
        /// call to <see cref="EkstaziSharp.DependencyMonitor.TestClassEnd(string)"/> inserted such that
        /// it is called after test finishes. Parameter represents full name of the test.
        /// call to <see cref="EkstaziSharp.DependencyMonitor.T(string)"/> inserted such that it is called
        /// at the beginning of each method of program under test. Parameter represents full name of the method.
        /// </summary>
        public void Instrument(Dictionary<string, ICollection<IMemberDefinition>> moduleToAffectedTests, IEnumerable<string> programModulePaths)
        {
            if (parameters.Strategy == InstrumentationStrategy.None)
            {
                return;
            }

            HashSet<ModuleDefinition> programModules = LoadModules(programModulePaths);
            HashSet<ModuleDefinition> testModules = LoadModules(moduleToAffectedTests.Keys);
            HashSet<ModuleDefinition> allModules = new HashSet<ModuleDefinition>();
            allModules.UnionWith(programModules);
            allModules.UnionWith(testModules);
            parameters.ModulesUnderInstrumentation = allModules;

            if (AreInstrumented(allModules))
            {
                return;
            }

            foreach (var module in allModules)
            {
                ProgramInstrumentator programInstrumentator = GetProgramInstrumentator(module);
                programInstrumentator.Instrument();
            }

            // Since concrete implementation of InstrumentTests can result in adding new types
            // and method to the module, we want to do that step at the end, in order to avoid
            // inserting call to DependencyMonitor in newly generated types or methods.

            foreach (KeyValuePair<string, ICollection<IMemberDefinition>> moduleToTests in moduleToAffectedTests)
            {
                ModuleDefinition testModule = CILTransformer.GetModuleDefinition(moduleToTests.Key);
                TestInstrumentator testInstrumentator = parameters.TestingFrameworkType.GetTestInstrumentator(testModule, parameters);

                foreach (IMemberDefinition test in moduleToTests.Value)
                {
                    if (test is TypeDefinition)
                    {
                        testInstrumentator.InstrumentTestClass((TypeDefinition)test);
                    }
                    else if (test is MethodDefinition)
                    {
                        testInstrumentator.InstrumentTestMethod((MethodDefinition)test);
                    }
                }
            }

            FlushModules(allModules);
        }

        #endregion
    }
}
