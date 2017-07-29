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

using EkstaziSharp.Util;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkstaziSharp.Instrumentation
{
    public abstract class TestInstrumentator : ModuleInstrumentator
    {
        #region Constructors

        /// <param name="testModule">Represents module which tests to instrument</param>
        /// <param name="parameters">Parameters of instrumentation</param>
        public TestInstrumentator(ModuleDefinition testModule, InstrumentatorParameters parameters)
            : base(testModule, parameters)
        {
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Modifies bytecode of the test such that appropriate methods of 
        /// <see cref="EkstaziSharp.DependencyMonitor"/> get executed during test execution.
        /// </summary>
        public abstract void InstrumentTestClass(TypeDefinition testClass);

        public abstract void InstrumentTestMethod(MethodDefinition testMethod);

        #endregion
    }
}
