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

using Mono.Cecil;
using EkstaziSharp.Util;
using Mono.Cecil.Cil;

namespace EkstaziSharp.Instrumentation
{
    /// <summary>
    /// This class notifies DependencyMonitor about each class used during program execution
    /// by adding calls to DependencyMonitor in each static constructor.
    /// Reasoning is that in order for any class to be used its static constructor has to be invoked.
    /// One potential drawback of this approach is that program has to be run in a separate AppDomain
    /// to collect correct list of dependencies, if that is not done than some classes could already be
    /// loaded in memory and their static constructor would not be triggered.
    /// </summary>
    public class StaticConstructorProgramInstrumentator : ProgramInstrumentator
    {
        #region Constructors

        public StaticConstructorProgramInstrumentator(ModuleDefinition moduleToInstrument, InstrumentatorParameters parameters)
            : base(moduleToInstrument, parameters) { }

        #endregion

        #region Private Methods

        /// <summary>
        /// Instruments every type and nested type with a static constructor 
        /// </summary>
        private void InstrumentStaticConstructors(ModuleDefinition module)
        {
            foreach (var type in module.Types)
            {
                InstrumentStaticConstructors(type);
                foreach (var nestedType in type.NestedTypes)
                {
                    InstrumentStaticConstructors(nestedType);
                }
            }
        }

        private void InstrumentStaticConstructors(TypeDefinition type)
        {
            if (type.IsEnum) return;
            if (type.Name == "<Module>") return;

            MethodDefinition staticConstructor = type.GetStaticConstructor(true);
            InsertCallToDependencyMonitor(staticConstructor, TMethod, staticConstructor.DeclaringType);
        }

        #endregion

        #region Public Methods

        public override void Instrument()
        {
            InstrumentStaticConstructors(moduleToInstrument);
        }

        #endregion
    }
}
