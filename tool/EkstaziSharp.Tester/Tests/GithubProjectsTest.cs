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
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Newtonsoft.Json;

namespace EkstaziSharp.Tester.Tests
{
    [TestFixture]
    [Ignore("Ignore github tests.")]
    public class GithubProjectsTest
    {
        //[Test]
        //public void MonoCecilTest()
        //{
        //    TestCommons.RunWithJson("MonoCecil");
        //}

        //[Test]
        //public void LibGit2SharpTest()
        //{
        //    TestCommons.RunWithJson("LibGit2Sharp");
        //}

        //[Test]
        //public void SignalRTest()
        //{
        //    TestCommons.RunWithJson("SignalR");
        //}

        //[Test]
        //public void LibGit2SharpLocalTest()
        //{
        //    TestCommons.RunWithJson("LibGit2SharpLocal");
        //}

        [Test]
        public void FluentValidation()
        {
            TestCommons.RunWithResource("FluentValidation");
        }

        //[Test]
        //public void AutoMapperTest()
        //{
        //    TestCommons.RunWithJson("AutoMapper");
        //}

        [Test]
        public void NewtonsoftJsonTest()
        {
            TestCommons.RunWithResource("NewtonsoftJson");
        }

        [Test]
        public void RequestReduceTest()
        {
            TestCommons.RunWithResource("RequestReduce");
        }

        [Test]
        public void DynamicExpressoTest()
        {
            TestCommons.RunWithResource("DynamicExpresso");
        }

        [Test]
        public void StatelessTest()
        {
            TestCommons.RunWithResource("Stateless");
        }

        [Test]
        public void LibSodiumTest()
        {
            TestCommons.RunWithResource("LibSodium");
        }

        [Test]
        public void AbotTest()
        {
            TestCommons.RunWithResource("Abot");
        }

        [Test]
        public void MoreLinqTest()
        {
            TestCommons.RunWithResource("MoreLinq");
        }

        [Test]
        public void FluentCommandLineParserTest()
        {
            TestCommons.RunWithResource("FluentCommandLineParser");
        }

        [Test]
        public void JsonFxTest()
        {
            TestCommons.RunWithResource("JsonFx");
        }

        [Test]
        public void OptiKeyTest()
        {
            TestCommons.RunWithResource("OptiKey");
        }

        [Test]
        public void NancyTest()
        {
            TestCommons.RunWithResource("Nancy");
        }

        [Test]
        public void FunScriptTest()
        {
            TestCommons.RunWithResource("FunScript");
        }

        [Test]
        public void ProjectScaffoldTest()
        {
            TestCommons.RunWithResource("ProjectScaffold");
        }
    }
}
