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

using System.Collections.Generic;
using Mono.Cecil;
using EkstaziSharp.Util;
using System.Linq;
using System;

namespace EkstaziSharp.Analysis
{
    public class XUnitAnalyzer : Analyzer
    {
        #region Constants

        private const string FactAttribute = "Xunit.FactAttribute";
        private const string TheoryAttribute = "Xunit.TheoryAttribute";
        // Used only in Xunit1
        // https://xunit.github.io/docs/upgrade-extensions.html
        private const string XunitExtensionsTheoryAttribute = "Xunit.Extensions.TheoryAttribute";

        private readonly HashSet<string> TestAttributes = new HashSet<string> { FactAttribute, TheoryAttribute, XunitExtensionsTheoryAttribute };

        #endregion

        #region Constructors

        public XUnitAnalyzer(AnalyzerParameters parameters) : base(parameters) { }

        #endregion

        #region Protected Methods

        protected override IEnumerable<IMemberDefinition> FindAllTestClasses(string modulePath)
        {
            ModuleDefinition module = CILTransformer.GetModuleDefinition(modulePath);

            // XUnit detects only tests inside of Public classes
            // that is why we filter types that have public flag
            // http://stackoverflow.com/questions/16214684/why-is-the-xunit-runner-not-finding-my-tests
            return module
                .Types
                .Where(type => (type.IsPublic || type.IsNestedPublic) && !type.IsAbstract && IsTestClass(type))
                .ToList();
        }

        private bool IsTestClass(TypeDefinition type)
        {
            if (type == null)
            {
                return false;
            }

            bool hasTestMethod = HasXunitTestMethod(type);
            if (hasTestMethod)
            {
                return true;
            }

            bool isChildOfTestClass = IsTestClass(type.GetParentType()?.GetTypeDefinition());
            return isChildOfTestClass;
        }

        private bool HasXunitTestMethod(TypeDefinition type)
        {
            return type != null && type.HasMethodWithCustomAttribute(name => TestAttributes.Contains(name), true);
        }

        protected override IEnumerable<IMemberDefinition> FindAllTestMethods(string modulePath)
        {
            List<IMemberDefinition> testMethods = new List<IMemberDefinition>();

            ModuleDefinition module = CILTransformer.GetModuleDefinition(modulePath);

            foreach (TypeDefinition type in module.Types)
            {
                if (!type.IsPublic)
                {
                    // XUnit detects only tests inside of Public classes
                    // that is why we filter types that have public flag
                    continue;
                }

                if (type.HasMethods)
                {
                    foreach (MethodDefinition method in type.Methods)
                    {
                        if (method.HasCustomAttribute(name => TestAttributes.Contains(name), true))
                        {
                            testMethods.Add(method);
                        }
                    }
                }
            }
            return testMethods;
        }

        #endregion
    }
}
