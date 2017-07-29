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

using EkstaziSharp.Analysis;
using EkstaziSharp.Instrumentation;
using Mono.Cecil;
using System;

namespace EkstaziSharp
{
    public enum TestingFrameworkType
    {
        /// <summary>
        /// XUnit version 1
        /// </summary>
        XUnit1 = 1,
        /// <summary>
        /// XUnit version 2
        /// </summary>
        XUnit2,
        /// <summary>
        /// NUnit version 3
        /// </summary>
        NUnit3,
        /// <summary>
        /// NUnit version 2
        /// </summary>
        NUnit2
    }

    public static class TestingFrameworkTypeExtensions
    {
        public static Analyzer GetAnalyzer(this TestingFrameworkType testingFramework, AnalyzerParameters options)
        {
            switch (testingFramework)
            {
                case TestingFrameworkType.NUnit2:
                case TestingFrameworkType.NUnit3:
                    return new NUnitAnalyzer(options);
                case TestingFrameworkType.XUnit1:
                case TestingFrameworkType.XUnit2:
                    return new XUnitAnalyzer(options);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static TestInstrumentator GetTestInstrumentator(this TestingFrameworkType testingFramework, ModuleDefinition testModule, InstrumentatorParameters parameters)
        {
            switch (testingFramework)
            {
                case TestingFrameworkType.NUnit2:
                    return new NUnit2Instrumentator(testModule, parameters);
                case TestingFrameworkType.NUnit3:
                    return new NUnit3Instrumentator(testModule, parameters);
                case TestingFrameworkType.XUnit1:
                    return new XUnit1Instrumentator(testModule, parameters);
                case TestingFrameworkType.XUnit2:
                    return new XUnit2Instrumentator(testModule, parameters);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}