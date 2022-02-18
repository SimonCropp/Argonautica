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

#pragma warning disable 618
using Xunit;

namespace Argon.Tests.Converters;

public class ObjectIdConverterTests : TestFixtureBase
{
    public class ObjectIdTestClass
    {
        [JsonProperty("_id")]
        public BsonObjectId Id { get; set; }

        [JsonProperty("test")]
        public string Test { get; set; }
    }

    [Fact]
    public void Serialize()
    {
        var c = new ObjectIdTestClass
        {
            Id = new BsonObjectId(HexToBytes("4ABBED9D1D8B0F0218000001")),
            Test = "1234£56"
        };

        var ms = new MemoryStream();
        var serializer = new JsonSerializer();

        // serialize product to BSON
        var writer = new BsonWriter(ms);
        serializer.Serialize(writer, c);

        var expected = HexToBytes("29000000075F6964004ABBED9D1D8B0F02180000010274657374000900000031323334C2A335360000");

        Assert.Equal(expected, ms.ToArray());
    }

    [Fact]
    public void Deserialize()
    {
        var bson = HexToBytes("29000000075F6964004ABBED9D1D8B0F02180000010274657374000900000031323334C2A335360000");

        var serializer = new JsonSerializer();

        var reader = new BsonReader(new MemoryStream(bson));
        var c = serializer.Deserialize<ObjectIdTestClass>(reader);

        Assert.Equal(c.Id.Value, HexToBytes("4ABBED9D1D8B0F0218000001"));
        Assert.Equal(c.Test, "1234£56");
    }
}
#pragma warning restore 618