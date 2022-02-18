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

namespace Argon.Tests.Linq;

public class LinqToJsonAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task CommentsAndReadFromAsync()
    {
        var textReader = new StringReader(@"[
    // hi
    1,
    2,
    3
]");

        var jsonReader = new JsonTextReader(textReader);
        var a = (JArray)await JToken.ReadFromAsync(jsonReader, new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Load
        });

        Xunit.Assert.Equal(4, a.Count);
        Xunit.Assert.Equal(JTokenType.Comment, a[0].Type);
        Xunit.Assert.Equal(" hi", ((JValue)a[0]).Value);
    }

    [Fact]
    public async Task CommentsAndReadFrom_IgnoreCommentsAsync()
    {
        var textReader = new StringReader(@"[
    // hi
    1,
    2,
    3
]");

        var jsonReader = new JsonTextReader(textReader);
        var a = (JArray)await JToken.ReadFromAsync(jsonReader);

        Xunit.Assert.Equal(3, a.Count);
        Xunit.Assert.Equal(JTokenType.Integer, a[0].Type);
        Xunit.Assert.Equal(1L, ((JValue)a[0]).Value);
    }

    [Fact]
    public async Task StartingCommentAndReadFromAsync()
    {
        var textReader = new StringReader(@"
// hi
[
    1,
    2,
    3
]");

        var jsonReader = new JsonTextReader(textReader);
        var v = (JValue)await JToken.ReadFromAsync(jsonReader, new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Load
        });

        Xunit.Assert.Equal(JTokenType.Comment, v.Type);

        IJsonLineInfo lineInfo = v;
        Assert.True( lineInfo.HasLineInfo());
        Xunit.Assert.Equal(2, lineInfo.LineNumber);
        Xunit.Assert.Equal(5, lineInfo.LinePosition);
    }

    [Fact]
    public async Task StartingCommentAndReadFrom_IgnoreCommentsAsync()
    {
        var textReader = new StringReader(@"
// hi
[
    1,
    2,
    3
]");

        var jsonReader = new JsonTextReader(textReader);
        var a = (JArray)await JToken.ReadFromAsync(jsonReader, new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Ignore
        });

        Xunit.Assert.Equal(JTokenType.Array, a.Type);

        IJsonLineInfo lineInfo = a;
        Assert.True( lineInfo.HasLineInfo());
        Xunit.Assert.Equal(3, lineInfo.LineNumber);
        Xunit.Assert.Equal(1, lineInfo.LinePosition);
    }

    [Fact]
    public async Task StartingUndefinedAndReadFromAsync()
    {
        var textReader = new StringReader(@"
undefined
[
    1,
    2,
    3
]");

        var jsonReader = new JsonTextReader(textReader);
        var v = (JValue)await JToken.ReadFromAsync(jsonReader);

        Xunit.Assert.Equal(JTokenType.Undefined, v.Type);

        IJsonLineInfo lineInfo = v;
        Assert.True( lineInfo.HasLineInfo());
        Xunit.Assert.Equal(2, lineInfo.LineNumber);
        Xunit.Assert.Equal(9, lineInfo.LinePosition);
    }

    [Fact]
    public async Task StartingEndArrayAndReadFromAsync()
    {
        var textReader = new StringReader(@"[]");

        var jsonReader = new JsonTextReader(textReader);
        await jsonReader.ReadAsync();
        await jsonReader.ReadAsync();

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await JToken.ReadFromAsync(jsonReader), @"Error reading JToken from JsonReader. Unexpected token: EndArray. Path '', line 1, position 2.");
    }
}