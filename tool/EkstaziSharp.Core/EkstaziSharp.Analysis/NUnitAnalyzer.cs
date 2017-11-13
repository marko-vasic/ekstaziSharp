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
using Mono.Cecil;
using EkstaziSharp.Util;

namespace EkstaziSharp.Analysis
{
    public class NUnitAnalyzer : Analyzer
    {
        #region Constants

        private const string TestFixtureAttribute = "NUnit.Framework.TestFixtureAttribute";
        private const string TestAttribute = "NUnit.Framework.TestAttribute";

        #endregion

        #region Constructors

        public NUnitAnalyzer(AnalyzerParameters parameters) : base(parameters) { }

        #endregion

        protected override IEnumerable<IMemberDefinition> FindAllTestClasses(string modulePath)
        {
            List<IMemberDefinition> testClasses = new List<IMemberDefinition>();

            ModuleDefinition module = CILTransformer.GetModuleDefinition(modulePath);
            // TODO: Check if performance improvements can be gained if running loop in parallel
            foreach (TypeDefinition type in module.Types)
            {
                bool isTestFixture = type.HasCustomAttribute(TestFixtureAttribute, false);
                if (isTestFixture)
                {
                    testClasses.Add(type);
                }
                else
                {
                    // if type is not TestFixture check whether it contains a method with Test attribute
                    if (type.HasMethodWithCustomAttribute(TestAttribute, false))
                    {
                        testClasses.Add(type);
                    }
                }
            }
            return testClasses;
        }

        protected override IEnumerable<IMemberDefinition> FindAllTestMethods(string modulePath)
        {
            List<IMemberDefinition> testMethods = new List<IMemberDefinition>();

            ModuleDefinition module = CILTransformer.GetModuleDefinition(modulePath);
            // TODO: Check if performance improvements can be gained if running loop in parallel
            foreach (TypeDefinition type in module.Types)
            {
                if (type.HasMethods)
                {
                    foreach (MethodDefinition method in type.Methods)
                    {
                        if (method.HasCustomAttribute(TestAttribute, false))
                        {
                            testMethods.Add(method);
                        }
                    }
                }
            }
            return testMethods;
        }
    }
}
