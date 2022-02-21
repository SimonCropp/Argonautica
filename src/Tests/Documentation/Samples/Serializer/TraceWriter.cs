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

public class TraceWriter : TestFixtureBase
{
    #region TraceWriterTypes
    public class Account
    {
        public string FullName { get; set; }
        public bool Deleted { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region TraceWriterUsage
        var json = @"{
              'FullName': 'Dan Deleted',
              'Deleted': true,
              'DeletedDate': '2013-01-20T00:00:00'
            }";

        var traceWriter = new MemoryTraceWriter();

        var account = JsonConvert.DeserializeObject<Account>(json, new JsonSerializerSettings
        {
            TraceWriter = traceWriter
        });

        Console.WriteLine(traceWriter.ToString());
        // 2013-01-21T01:36:24.422 Info Started deserializing Argon.Tests.Documentation.Examples.TraceWriter+Account. Path 'FullName', line 2, position 20.
        // 2013-01-21T01:36:24.442 Verbose Could not find member 'DeletedDate' on Tests.Documentation.Examples.TraceWriter+Account. Path 'DeletedDate', line 4, position 23.
        // 2013-01-21T01:36:24.447 Info Finished deserializing Argon.Tests.Documentation.Examples.TraceWriter+Account. Path '', line 5, position 8.
        // 2013-01-21T01:36:24.450 Verbose Deserialized JSON:
        // {
        //   "FullName": "Dan Deleted",
        //   "Deleted": true,
        //   "DeletedDate": "2013-01-20T00:00:00"
        // }
        #endregion

        Assert.Equal(4, traceWriter.GetTraceMessages().Count());
    }
}