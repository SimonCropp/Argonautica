﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Issues;

public class Issue1752 : TestFixtureBase
{
    [Fact]
    public void Test_EmptyString()
    {
        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        var s1 = JsonConvert.SerializeObject(new TestObject { Obj = new byte[] { } }, settings);

        var t1 = JsonConvert.DeserializeObject<TestObject>(s1, settings);
        Xunit.Assert.NotNull(t1.Obj);

        var data = (byte[])t1.Obj;
        Assert.AreEqual(0, data.Length);
    }

    [Fact]
    public void Test_Null()
    {
        var t1 = JsonConvert.DeserializeObject<TestObject1>("{'Obj':null}");
        Xunit.Assert.Null(t1.Obj);
    }

    class TestObject
    {
        public object Obj { get; set; }
    }

    class TestObject1
    {
        public byte[] Obj { get; set; }
    }
}