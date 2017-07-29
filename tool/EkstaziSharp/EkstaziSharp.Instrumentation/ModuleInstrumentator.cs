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
using System.Collections.Generic;
using System.Linq;

namespace EkstaziSharp.Instrumentation
{
    public abstract class ModuleInstrumentator
    {
        #region Private Fields

        private MethodReference tMethod;

        private MethodReference testClassStartMethod;

        private MethodReference testClassEndMethod;

        private MethodReference testMethodStartMethod;

        private MethodReference testMethodEndMethod;

        private MethodReference getTypeFromHandleMethod;

        private MethodReference getCurrentMethod;

        private HashSet<string> namesOfModulesUnderInstrumentation;

        #endregion

        #region Protected Fields

        protected InstrumentatorParameters parameters;

        protected ModuleDefinition moduleToInstrument;

        #endregion

        #region Properties

        protected MethodReference TMethod
        {
            get
            {
                if (tMethod == null)
                {
                    var dependencyMonitorType = parameters.DependencyMonitor.GetTypeByFullName(DependencyMonitor.ClassFullName);
                    tMethod = dependencyMonitorType.GetMethodByFullName(DependencyMonitor.GetTMethodFullName(parameters.InstrumentationArgsType));
                }
                return tMethod;
            }
        }

        protected MethodReference TestClassStartMethod
        {
            get
            {
                if (testClassStartMethod == null)
                {
                    var dependencyMonitorType = parameters.DependencyMonitor.GetTypeByFullName(DependencyMonitor.ClassFullName);
                    testClassStartMethod = dependencyMonitorType.GetMethodByFullName(DependencyMonitor.GetTestClassStartFullName(parameters.InstrumentationArgsType));
                }
                return testClassStartMethod;
            }
        }

        protected MethodReference TestClassEndMethod
        {
            get
            {
                if (testClassEndMethod == null)
                {
                    var dependencyMonitorType = parameters.DependencyMonitor.GetTypeByFullName(DependencyMonitor.ClassFullName);
                    testClassEndMethod = dependencyMonitorType.GetMethodByFullName(DependencyMonitor.GetTestClassEndFullName(parameters.InstrumentationArgsType));
                }
                return testClassEndMethod;
            }
        }

        protected MethodReference TestMethodStartMethod
        {
            get
            {
                if (testMethodStartMethod == null)
                {
                    var dependencyMonitorType = parameters.DependencyMonitor.GetTypeByFullName(DependencyMonitor.ClassFullName);
                    testMethodStartMethod = dependencyMonitorType.GetMethodByFullName(DependencyMonitor.GetTestMethodStartFullName(parameters.InstrumentationArgsType));
                }
                return testMethodStartMethod;
            }
        }

        protected MethodReference TestMethodEndMethod
        {
            get
            {
                if (testMethodEndMethod == null)
                {
                    var dependencyMonitorType = parameters.DependencyMonitor.GetTypeByFullName(DependencyMonitor.ClassFullName);
                    testMethodEndMethod = dependencyMonitorType.GetMethodByFullName(DependencyMonitor.GetTestMethodEndFullName(parameters.InstrumentationArgsType));
                }
                return testMethodEndMethod;
            }
        }

        protected MethodReference GetTypeFromHandleMethod
        {
            get
            {
                if (getTypeFromHandleMethod == null)
                {
                    var mscorlibModule = moduleToInstrument.GetReferencedModule(CLRConstants.CoreLibAssemblyName, CLRConstants.CLRModuleName);
                    TypeDefinition systemType = mscorlibModule.GetTypeByFullName(CLRConstants.SystemType);
                    MethodDefinition methodDef = systemType.Methods.First(m => m.FullName == CLRConstants.GetTypeFromHandleMethod);
                    getTypeFromHandleMethod = moduleToInstrument.Import(methodDef);
                }
                return getTypeFromHandleMethod;
            }
        }

        protected MethodReference GetCurrentMethod
        {
            get
            {
                if (getCurrentMethod == null)
                {
                    var mscorlibModule = moduleToInstrument.GetReferencedModule(CLRConstants.CoreLibAssemblyName, CLRConstants.CLRModuleName);
                    TypeDefinition methodBaseType = mscorlibModule.GetTypeByFullName(CLRConstants.MethodBase);
                    MethodDefinition methodDef = methodBaseType.Methods.First(m => m.FullName == CLRConstants.GetCurrentMethod);
                    getCurrentMethod = moduleToInstrument.Import(methodDef);
                }
                return getCurrentMethod;
            }
        }

        protected HashSet<string> NamesOfModulesUnderInstrumentation
        {
            get
            {
                if (namesOfModulesUnderInstrumentation == null && parameters.ModulesUnderInstrumentation != null)
                {
                    namesOfModulesUnderInstrumentation = new HashSet<string>();
                    foreach (var module in parameters.ModulesUnderInstrumentation)
                    {
                        namesOfModulesUnderInstrumentation.Add(module.Name);
                    }
                }
                return namesOfModulesUnderInstrumentation;
            }
        }

        #endregion

        public ModuleInstrumentator(ModuleDefinition moduleToInstrument, InstrumentatorParameters parameters)
        {
            this.moduleToInstrument = moduleToInstrument;
            this.parameters = parameters;
        }

        #region Methods

        protected void InsertCallToDependencyMonitor(MethodDefinition methodToInstrument, Instruction insertCallBefore, MethodReference dependencyMonitorMethodToCall, MemberReference argumentToDependencyMonitor)
        {
            MethodReference methodToCall = dependencyMonitorMethodToCall;
            if (methodToInstrument.Module.FullyQualifiedName != dependencyMonitorMethodToCall.Module.FullyQualifiedName)
            {
                methodToCall = methodToInstrument.Module.Import(dependencyMonitorMethodToCall);
            }
            ILProcessor il = methodToInstrument.Body.GetILProcessor();
            Instruction callInstruction = il.Create(OpCodes.Call, methodToCall);
            il.InsertBefore(insertCallBefore, callInstruction);

            if (parameters.InstrumentationArgsType == DependencyMonitor.InstrumentationArguments.ReflectionObjects)
            {
                if (argumentToDependencyMonitor != null)
                {
                    if (argumentToDependencyMonitor is TypeDefinition)
                    {
                        TypeDefinition type = argumentToDependencyMonitor as TypeDefinition;
                        Instruction ldToken = il.Create(OpCodes.Ldtoken, type);
                        Instruction callGetType = il.Create(OpCodes.Call, GetTypeFromHandleMethod);

                        il.InsertBefore(callInstruction, callGetType);
                        il.InsertBefore(callGetType, ldToken);
                    }
                    else
                    {
                        MethodDefinition method = argumentToDependencyMonitor as MethodDefinition;
                        Instruction callGetMethod = il.Create(OpCodes.Call, GetCurrentMethod);
                        il.InsertBefore(callInstruction, callGetMethod);
                    }
                }
                else
                {
                    Instruction ldNull = il.Create(OpCodes.Ldnull);
                    il.InsertBefore(callInstruction, ldNull);
                }
            }
            else if (parameters.InstrumentationArgsType == DependencyMonitor.InstrumentationArguments.Strings)
            {
                string callSiteName = null;
                if (dependencyMonitorMethodToCall == TestClassStartMethod || dependencyMonitorMethodToCall == TestMethodStartMethod)
                {
                    string path = argumentToDependencyMonitor != null ? argumentToDependencyMonitor.GetPath() : string.Empty;
                    Instruction ldPathInstruction = il.Create(OpCodes.Ldstr, path);
                    il.InsertBefore(callInstruction, ldPathInstruction);

                    callSiteName = argumentToDependencyMonitor != null ? argumentToDependencyMonitor.GetFullName() : string.Empty;                 
                }
                else
                {
                    callSiteName = argumentToDependencyMonitor != null ? argumentToDependencyMonitor.GetDependencyName() : string.Empty;
                }

                Instruction ldStrInstruction = il.Create(OpCodes.Ldstr, callSiteName);
                il.InsertBefore(callInstruction, ldStrInstruction);
            }
        }

        protected void InsertCallToDependencyMonitor(MethodDefinition methodToInstrument, MethodReference dependencyMonitorMethodToCall, MemberReference argumentToDependencyMonitor)
        {
            Instruction firstInstruction = methodToInstrument.Body.Instructions[0];
            InsertCallToDependencyMonitor(methodToInstrument, firstInstruction, dependencyMonitorMethodToCall, argumentToDependencyMonitor);
        }

        #endregion
    }
}
