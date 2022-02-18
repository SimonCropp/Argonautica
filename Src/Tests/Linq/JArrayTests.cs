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

using System.ComponentModel;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

// ReSharper disable UseObjectOrCollectionInitializer

namespace Argon.Tests.Linq;

public class JArrayTests : TestFixtureBase
{
    [Fact]
    public void RemoveSpecificAndRemoveSelf()
    {
        var o = new JObject
        {
            { "results", new JArray(1, 2, 3, 4) }
        };

        var a = (JArray)o["results"];

        var last = a.Last();

        Xunit.Assert.True(a.Remove(last));

        last = a.Last();
        last.Remove();

        Xunit.Assert.Equal(2, a.Count);
    }

    [Fact]
    public void Clear()
    {
        var a = new JArray { 1 };
        Xunit.Assert.Equal(1, a.Count);

        a.Clear();
        Xunit.Assert.Equal(0, a.Count);
    }

    [Fact]
    public void AddToSelf()
    {
        var a = new JArray();
        a.Add(a);

        Xunit.Assert.False(ReferenceEquals(a[0], a));
    }

    [Fact]
    public void Contains()
    {
        var v = new JValue(1);

        var a = new JArray { v };

        Assert.False( a.Contains(new JValue(2)));
        Assert.False( a.Contains(new JValue(1)));
        Assert.False( a.Contains(null));
        Assert.True( a.Contains(v));
    }

    [Fact]
    public void GenericCollectionCopyTo()
    {
        var j = new JArray
        {
            new JValue(1),
            new JValue(2),
            new JValue(3)
        };
        Xunit.Assert.Equal(3, j.Count);

        var a = new JToken[5];

        ((ICollection<JToken>)j).CopyTo(a, 1);

        Xunit.Assert.Equal(null, a[0]);

        Xunit.Assert.Equal(1, (int)a[1]);

        Xunit.Assert.Equal(2, (int)a[2]);

        Xunit.Assert.Equal(3, (int)a[3]);

        Xunit.Assert.Equal(null, a[4]);
    }

    [Fact]
    public void GenericCollectionCopyToNullArrayShouldThrow()
    {
        var j = new JArray();

        ExceptionAssert.Throws<ArgumentNullException>(() => { ((ICollection<JToken>)j).CopyTo(null, 0); },
            @"Value cannot be null.
Parameter name: array",
            "Value cannot be null. (Parameter 'array')");
    }

    [Fact]
    public void GenericCollectionCopyToNegativeArrayIndexShouldThrow()
    {
        var j = new JArray();

        ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => { ((ICollection<JToken>)j).CopyTo(new JToken[1], -1); },
            @"arrayIndex is less than 0.
Parameter name: arrayIndex",
            "arrayIndex is less than 0. (Parameter 'arrayIndex')");
    }

    [Fact]
    public void GenericCollectionCopyToArrayIndexEqualGreaterToArrayLengthShouldThrow()
    {
        var j = new JArray();

        ExceptionAssert.Throws<ArgumentException>(() => { ((ICollection<JToken>)j).CopyTo(new JToken[1], 1); }, @"arrayIndex is equal to or greater than the length of array.");
    }

    [Fact]
    public void GenericCollectionCopyToInsufficientArrayCapacity()
    {
        var j = new JArray
        {
            new JValue(1),
            new JValue(2),
            new JValue(3)
        };

        ExceptionAssert.Throws<ArgumentException>(() => { ((ICollection<JToken>)j).CopyTo(new JToken[3], 1); }, @"The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
    }

    [Fact]
    public void Remove()
    {
        var v = new JValue(1);
        var j = new JArray {v};

        Xunit.Assert.Equal(1, j.Count);

        Assert.False( j.Remove(new JValue(1)));
        Assert.False( j.Remove(null));
        Assert.True( j.Remove(v));
        Assert.False( j.Remove(v));

        Xunit.Assert.Equal(0, j.Count);
    }

    [Fact]
    public void IndexOf()
    {
        var v1 = new JValue(1);
        var v2 = new JValue(1);
        var v3 = new JValue(1);

        var j = new JArray {v1};

        Xunit.Assert.Equal(0, j.IndexOf(v1));

        j.Add(v2);
        Xunit.Assert.Equal(0, j.IndexOf(v1));
        Xunit.Assert.Equal(1, j.IndexOf(v2));

        j.AddFirst(v3);
        Xunit.Assert.Equal(1, j.IndexOf(v1));
        Xunit.Assert.Equal(2, j.IndexOf(v2));
        Xunit.Assert.Equal(0, j.IndexOf(v3));

        v3.Remove();
        Xunit.Assert.Equal(0, j.IndexOf(v1));
        Xunit.Assert.Equal(1, j.IndexOf(v2));
        Xunit.Assert.Equal(-1, j.IndexOf(v3));
    }

    [Fact]
    public void RemoveAt()
    {
        var v1 = new JValue(1);
        var v2 = new JValue(1);
        var v3 = new JValue(1);

        var j = new JArray
        {
            v1,
            v2,
            v3
        };

        Assert.True( j.Contains(v1));
        j.RemoveAt(0);
        Assert.False( j.Contains(v1));

        Assert.True( j.Contains(v3));
        j.RemoveAt(1);
        Assert.False( j.Contains(v3));

        Xunit.Assert.Equal(1, j.Count);
    }

    [Fact]
    public void RemoveAtOutOfRangeIndexShouldError()
    {
        var j = new JArray();

        ExceptionAssert.Throws<ArgumentOutOfRangeException>(
            () => { j.RemoveAt(0); },
            @"Index is equal to or greater than Count.
Parameter name: index",
            "Index is equal to or greater than Count. (Parameter 'index')");
    }

    [Fact]
    public void RemoveAtNegativeIndexShouldError()
    {
        var j = new JArray();

        ExceptionAssert.Throws<ArgumentOutOfRangeException>(
            () => { j.RemoveAt(-1); },
            @"Index is less than 0.
Parameter name: index",
            "Index is less than 0. (Parameter 'index')");
    }

    [Fact]
    public void Insert()
    {
        var v1 = new JValue(1);
        var v2 = new JValue(2);
        var v3 = new JValue(3);
        var v4 = new JValue(4);

        var j = new JArray
        {
            v1,
            v2,
            v3
        };

        j.Insert(1, v4);

        Xunit.Assert.Equal(0, j.IndexOf(v1));
        Xunit.Assert.Equal(1, j.IndexOf(v4));
        Xunit.Assert.Equal(2, j.IndexOf(v2));
        Xunit.Assert.Equal(3, j.IndexOf(v3));
    }

    [Fact]
    public void AddFirstAddedTokenShouldBeFirst()
    {
        var v1 = new JValue(1);
        var v2 = new JValue(2);
        var v3 = new JValue(3);

        var j = new JArray();
        Xunit.Assert.Equal(null, j.First);
        Xunit.Assert.Equal(null, j.Last);

        j.AddFirst(v1);
        Xunit.Assert.Equal(v1, j.First);
        Xunit.Assert.Equal(v1, j.Last);

        j.AddFirst(v2);
        Xunit.Assert.Equal(v2, j.First);
        Xunit.Assert.Equal(v1, j.Last);

        j.AddFirst(v3);
        Xunit.Assert.Equal(v3, j.First);
        Xunit.Assert.Equal(v1, j.Last);
    }

    [Fact]
    public void InsertShouldInsertAtZeroIndex()
    {
        var v1 = new JValue(1);
        var v2 = new JValue(2);

        var j = new JArray();

        j.Insert(0, v1);
        Xunit.Assert.Equal(0, j.IndexOf(v1));

        j.Insert(0, v2);
        Xunit.Assert.Equal(1, j.IndexOf(v1));
        Xunit.Assert.Equal(0, j.IndexOf(v2));
    }

    [Fact]
    public void InsertNull()
    {
        var j = new JArray();
        j.Insert(0, null);

        Xunit.Assert.Equal(null, ((JValue)j[0]).Value);
    }

    [Fact]
    public void InsertNegativeIndexShouldThrow()
    {
        var j = new JArray();

        ExceptionAssert.Throws<ArgumentOutOfRangeException>(
            () => { j.Insert(-1, new JValue(1)); },
            @"Index was out of range. Must be non-negative and less than the size of the collection.
Parameter name: index",
            "Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'index')");
    }

    [Fact]
    public void InsertOutOfRangeIndexShouldThrow()
    {
        var j = new JArray();

        ExceptionAssert.Throws<ArgumentOutOfRangeException>(
            () => { j.Insert(2, new JValue(1)); },
            @"Index must be within the bounds of the List.
Parameter name: index",
            "Index must be within the bounds of the List. (Parameter 'index')");
    }

    [Fact]
    public void Item()
    {
        var v1 = new JValue(1);
        var v2 = new JValue(2);
        var v3 = new JValue(3);
        var v4 = new JValue(4);

        var j = new JArray
        {
            v1,
            v2,
            v3
        };

        j[1] = v4;

        Xunit.Assert.Equal(null, v2.Parent);
        Xunit.Assert.Equal(-1, j.IndexOf(v2));
        Xunit.Assert.Equal(j, v4.Parent);
        Xunit.Assert.Equal(1, j.IndexOf(v4));
    }

    [Fact]
    public void Parse_ShouldThrowOnUnexpectedToken()
    {
        var json = @"{""prop"":""value""}";

        ExceptionAssert.Throws<JsonReaderException>(() => { JArray.Parse(json); }, "Error reading JArray from JsonReader. Current JsonReader item is not an array: StartObject. Path '', line 1, position 1.");
    }

    public class ListItemFields
    {
        public string ListItemText { get; set; }
        public object ListItemValue { get; set; }
    }

    [Fact]
    public void ArrayOrder()
    {
        var itemZeroText = "Zero text";

        IEnumerable<ListItemFields> t = new List<ListItemFields>
        {
            new() { ListItemText = "First", ListItemValue = 1 },
            new() { ListItemText = "Second", ListItemValue = 2 },
            new() { ListItemText = "Third", ListItemValue = 3 }
        };

        var optionValues =
            new JObject(
                new JProperty("options",
                    new JArray(
                        new JObject(
                            new JProperty("text", itemZeroText),
                            new JProperty("value", "0")),
                        from r in t
                        orderby r.ListItemValue
                        select new JObject(
                            new JProperty("text", r.ListItemText),
                            new JProperty("value", r.ListItemValue.ToString())))));

        var result = "myOptions = " + optionValues.ToString();

        StringAssert.AreEqual(@"myOptions = {
  ""options"": [
    {
      ""text"": ""Zero text"",
      ""value"": ""0""
    },
    {
      ""text"": ""First"",
      ""value"": ""1""
    },
    {
      ""text"": ""Second"",
      ""value"": ""2""
    },
    {
      ""text"": ""Third"",
      ""value"": ""3""
    }
  ]
}", result);
    }

    [Fact]
    public void Iterate()
    {
        var a = new JArray(1, 2, 3, 4, 5);

        var i = 1;
        foreach (var token in a)
        {
            Xunit.Assert.Equal(i, (int)token);
            i++;
        }
    }

    [Fact]
    public void ITypedListGetItemProperties()
    {
        var p1 = new JProperty("Test1", 1);
        var p2 = new JProperty("Test2", "Two");
        ITypedList a = new JArray(new JObject(p1, p2));

        var propertyDescriptors = a.GetItemProperties(null);
        Xunit.Assert.NotNull(propertyDescriptors);
        Xunit.Assert.Equal(2, propertyDescriptors.Count);
        Xunit.Assert.Equal("Test1", propertyDescriptors[0].Name);
        Xunit.Assert.Equal("Test2", propertyDescriptors[1].Name);
    }

    [Fact]
    public void AddArrayToSelf()
    {
        var a = new JArray(1, 2);
        a.Add(a);

        Xunit.Assert.Equal(3, a.Count);
        Xunit.Assert.Equal(1, (int)a[0]);
        Xunit.Assert.Equal(2, (int)a[1]);
        Xunit.Assert.NotSame(a, a[2]);
    }

    [Fact]
    public void SetValueWithInvalidIndex()
    {
        ExceptionAssert.Throws<ArgumentException>(() =>
        {
            var a = new JArray
            {
                ["badvalue"] = new JValue(3)
            };
        }, @"Set JArray values with invalid key value: ""badvalue"". Int32 array index expected.");
    }

    [Fact]
    public void SetValue()
    {
        object key = 0;

        var a = new JArray((object)null)
        {
            [key] = new JValue(3)
        };

        Xunit.Assert.Equal(3, (int)a[key]);
    }

    [Fact]
    public void ReplaceAll()
    {
        var a = new JArray(new[] { 1, 2, 3 });
        Xunit.Assert.Equal(3, a.Count);
        Xunit.Assert.Equal(1, (int)a[0]);
        Xunit.Assert.Equal(2, (int)a[1]);
        Xunit.Assert.Equal(3, (int)a[2]);

        a.ReplaceAll(1);
        Xunit.Assert.Equal(1, a.Count);
        Xunit.Assert.Equal(1, (int)a[0]);
    }

    [Fact]
    public void ParseIncomplete()
    {
        ExceptionAssert.Throws<JsonReaderException>(() => { JArray.Parse("[1"); }, "Unexpected end of content while loading JArray. Path '[0]', line 1, position 2.");
    }

    [Fact]
    public void InsertAddEnd()
    {
        var array = new JArray();
        array.Insert(0, 123);
        array.Insert(1, 456);

        Xunit.Assert.Equal(2, array.Count);
        Xunit.Assert.Equal(123, (int)array[0]);
        Xunit.Assert.Equal(456, (int)array[1]);
    }

    [Fact]
    public void ParseAdditionalContent()
    {
        var json = @"[
""Small"",
""Medium"",
""Large""
], 987987";

        ExceptionAssert.Throws<JsonReaderException>(() => { JArray.Parse(json); }, "Additional text encountered after finished reading JSON content: ,. Path '', line 5, position 1.");
    }

    [Fact]
    public void ToListOnEmptyArray()
    {
        var json = @"{""decks"":[]}";

        var decks = (JArray)JObject.Parse(json)["decks"];
        IList<JToken> l = decks.ToList();
        Xunit.Assert.Equal(0, l.Count);

        json = @"{""decks"":[1]}";

        decks = (JArray)JObject.Parse(json)["decks"];
        l = decks.ToList();
        Xunit.Assert.Equal(1, l.Count);
    }

    [Fact]
    public void Parse_NoComments()
    {
        var json = "[1,2/*comment*/,3]";

        var a = JArray.Parse(json, new JsonLoadSettings());

        Xunit.Assert.Equal(3, a.Count);
        Xunit.Assert.Equal(1, (int)a[0]);
        Xunit.Assert.Equal(2, (int)a[1]);
        Xunit.Assert.Equal(3, (int)a[2]);

        a = JArray.Parse(json, new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Ignore
        });

        Xunit.Assert.Equal(3, a.Count);
        Xunit.Assert.Equal(1, (int)a[0]);
        Xunit.Assert.Equal(2, (int)a[1]);
        Xunit.Assert.Equal(3, (int)a[2]);

        a = JArray.Parse(json, new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Load
        });

        Xunit.Assert.Equal(4, a.Count);
        Xunit.Assert.Equal(1, (int)a[0]);
        Xunit.Assert.Equal(2, (int)a[1]);
        Xunit.Assert.Equal(JTokenType.Comment, a[2].Type);
        Xunit.Assert.Equal(3, (int)a[3]);
    }

    [Fact]
    public void Parse_ExcessiveContentJustComments()
    {
        var json = @"[1,2,3]/*comment*/
//Another comment.";

        var a = JArray.Parse(json);

        Xunit.Assert.Equal(3, a.Count);
        Xunit.Assert.Equal(1, (int)a[0]);
        Xunit.Assert.Equal(2, (int)a[1]);
        Xunit.Assert.Equal(3, (int)a[2]);
    }

    [Fact]
    public void Parse_ExcessiveContent()
    {
        var json = @"[1,2,3]/*comment*/
//Another comment.
[]";

        ExceptionAssert.Throws<JsonReaderException>(() => JArray.Parse(json),
            "Additional text encountered after finished reading JSON content: [. Path '', line 3, position 0.");
    }

    [Fact]
    public void Parse_LineInfo()
    {
        var json = "[1,2,3]";

        var a = JArray.Parse(json, new JsonLoadSettings());

        Assert.True( ((IJsonLineInfo)a).HasLineInfo());
        Assert.True( ((IJsonLineInfo)a[0]).HasLineInfo());
        Assert.True( ((IJsonLineInfo)a[1]).HasLineInfo());
        Assert.True( ((IJsonLineInfo)a[2]).HasLineInfo());

        a = JArray.Parse(json, new JsonLoadSettings
        {
            LineInfoHandling = LineInfoHandling.Ignore
        });

        Assert.False( ((IJsonLineInfo)a).HasLineInfo());
        Assert.False( ((IJsonLineInfo)a[0]).HasLineInfo());
        Assert.False( ((IJsonLineInfo)a[1]).HasLineInfo());
        Assert.False( ((IJsonLineInfo)a[2]).HasLineInfo());

        a = JArray.Parse(json, new JsonLoadSettings
        {
            LineInfoHandling = LineInfoHandling.Load
        });

        Assert.True( ((IJsonLineInfo)a).HasLineInfo());
        Assert.True( ((IJsonLineInfo)a[0]).HasLineInfo());
        Assert.True( ((IJsonLineInfo)a[1]).HasLineInfo());
        Assert.True( ((IJsonLineInfo)a[2]).HasLineInfo());
    }
}