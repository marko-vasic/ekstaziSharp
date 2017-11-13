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

using NUnit.Framework;

namespace CatchBlock
{
    [TestFixture]
    [Ignore("Ignore test. It is supposed to be run only as a part of EkstaziSharp tests.")]
    public class Test
    {
        [Test]
        public void M1()
        {
            Assert.AreEqual(new C().M(), 2);
        }
    }
}
