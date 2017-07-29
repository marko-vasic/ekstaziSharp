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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkstaziSharp.Util
{
    /// <summary>
    /// This class defines various constants used inside Common Language Runtime
    /// </summary>
    public static class CLRConstants
    {
        public const string ConstructorName = ".ctor";

        public const string StaticConstructorName = ".cctor";

        public const string CoreLibAssemblyName = "mscorlib";

        public const string CLRModuleName = "CommonLanguageRuntimeLibrary";

        public const string SystemObjectFullName = "System.Object";

        public const string SystemIDisposableFullName = "System.IDisposable";

        public const string MethodInfo = "System.Reflection.MethodInfo";

        public const string SystemType = "System.Type";

        public const string GetTypeFromHandleMethod = "System.Type System.Type::GetTypeFromHandle(System.RuntimeTypeHandle)";

        public const string MethodBase = "System.Reflection.MethodBase";

        public const string GetCurrentMethod = "System.Reflection.MethodBase System.Reflection.MethodBase::GetCurrentMethod()";
    }
}
