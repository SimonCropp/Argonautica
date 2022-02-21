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

namespace Argon.Tests.Documentation.Samples.Serializer;

public class ReferenceLoopHandlingIgnore : TestFixtureBase
{
    #region ReferenceLoopHandlingIgnoreTypes
    public class Employee
    {
        public string Name { get; set; }
        public Employee Manager { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region ReferenceLoopHandlingIgnoreUsage
        var joe = new Employee { Name = "Joe User" };
        var mike = new Employee { Name = "Mike Manager" };
        joe.Manager = mike;
        mike.Manager = mike;

        var json = JsonConvert.SerializeObject(joe, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        Console.WriteLine(json);
        // {
        //   "Name": "Joe User",
        //   "Manager": {
        //     "Name": "Mike Manager"
        //   }
        // }
        #endregion

        XUnitAssert.AreEqualNormalized(@"{
  ""Name"": ""Joe User"",
  ""Manager"": {
    ""Name"": ""Mike Manager""
  }
}", json);
    }
}