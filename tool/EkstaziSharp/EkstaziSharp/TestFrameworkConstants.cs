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

namespace EkstaziSharp
{
    public static class TestFrameworkConstants
    {
        #region XUnit

        public const string XUnit1AssemblyName = "xunit";
        public const string XUnit1ModuleName = "xunit.dll";
        public const string XUnit1ClassFixture = "Xunit.IUseFixture`1";
        public const string XUnit1BeforeAfterTestAttribute = "Xunit.BeforeAfterTestAttribute";

        public const string XUnit2AssemblyName = "xunit.core";
        public const string XUnit2ModuleName = "xunit.core.dll";
        public const string XUnit2ClassFixture = "Xunit.IClassFixture`1";

        #endregion

        #region NUnit

        public const string NUnitAssemblyName = "nunit.framework";
        public const string NUnitModuleName = "nunit.framework.dll";

        public const string NUnitTestMethodSetUpAttributeName = "NUnit.Framework.SetUpAttribute";
        public const string NUnitTestMethodTearDownAttributeName = "NUnit.Framework.TearDownAttribute";

        public const string NUnit2TestClassSetUpAttributeName = "NUnit.Framework.TestFixtureSetUpAttribute";
        public const string NUnit2TestClassTearDownAttributeName = "NUnit.Framework.TestFixtureTearDownAttribute";

        public const string NUnit3TestClassSetUpAttributeName = "NUnit.Framework.OneTimeSetUpAttribute";
        public const string NUnit3TestClassTearDownAttributeName = "NUnit.Framework.OneTimeTearDownAttribute";

        #endregion
    }
}
