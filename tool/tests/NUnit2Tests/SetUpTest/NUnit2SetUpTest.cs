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
using System;

/// <summary>
/// This test classes should demonstrate whether or not 
/// multiple SetUp and TearDown methods can be used in one test class.
/// </summary>
[TestFixture]
public class NUnit2SetUpTest
{
    int setUpCount = 0;
    int tearDownCount = 0;

    [TestFixtureSetUp]
    public void SetUp1()
    {
        setUpCount++;
        Console.WriteLine("SetUp1");
    }

    [TestFixtureSetUp]
    public void SetUp2()
    {
        setUpCount++;
        Console.WriteLine("SetUp2");
    }

    [TestFixtureTearDown]
    public void TearDown1()
    {
        tearDownCount++;
        Console.WriteLine("TearDown1");
    }

    [TestFixtureTearDown]
    public void TearDown2()
    {
        tearDownCount++;
        Console.WriteLine("TearDown2");
    }

    [Test]
    public void Test1()
    {
        Assert.AreEqual(setUpCount, 2);
        Assert.AreEqual(tearDownCount, 0);
        Console.WriteLine("Test1");
    }

    [Test]
    public void Test2()
    {
        Assert.AreEqual(setUpCount, 2);
        Assert.AreEqual(tearDownCount, 0);
        Console.WriteLine("Test2");
    }
}