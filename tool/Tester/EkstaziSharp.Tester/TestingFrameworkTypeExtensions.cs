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

namespace EkstaziSharp.Tester
{
    public static class TestingFrameworkTypeExtensions
    {
        public static TestExecutor GetExecutor(this TestingFrameworkType testingFramework)
        {
            switch (testingFramework)
            {
                case TestingFrameworkType.NUnit2:
                    return new NUnit2TestExecutor();
                case TestingFrameworkType.NUnit3:
                    return new NUnit3TestExecutor();
                case TestingFrameworkType.XUnit1:
                    return new XUnitTestExecutor();
                case TestingFrameworkType.XUnit2:
                    return new XUnitConsoleTestExecutor();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Type GetArgumentsType(this TestingFrameworkType type)
        {
            switch (type)
            {
                case TestingFrameworkType.XUnit2:
                case TestingFrameworkType.XUnit1:
                    return typeof(XUnitArguments);
                default:
                    return typeof(TestFrameworkArguments);
            }
        }
    }
}
