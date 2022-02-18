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

using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;
using Xunit;
// ReSharper disable UseObjectOrCollectionInitializer


namespace Argon.Tests.Serialization;

public class PreserveReferencesHandlingTests : TestFixtureBase
{
    public class ContentB
    {
        public bool SomeValue { get; set; }
    }

    [JsonConverter(typeof(ListConverter))]
    public class ContentA : List<object>
    {
        public ContentB B { get; set; }

        public ContentA()
        {
            B = new ContentB();
        }
    }

    public class ListConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new ContentA { B = serializer.Deserialize<ContentB>(reader) }; // Construct my data back.
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var b = ((ContentA)value).B;
            serializer.Serialize(writer, b); // My Content.B contains all useful data.
        }
    }

    public class Container
    {
        public List<ContentA> ListA { get; set; }
        public List<ContentA> ListB { get; set; }

        public Container()
        {
            ListA = new List<ContentA>();
            ListB = new List<ContentA>();
        }
    }

    [Fact]
    public void SerializeReferenceInConvert()
    {
        var settings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        };

        var c1 = new Container();
        var content = new ContentA
        {
            B =
            {
                SomeValue = true
            }
        };
        c1.ListA.Add(content);
        c1.ListB.Add(content);

        var s = JsonConvert.SerializeObject(c1, settings);

        XUnitAssert.AreEqualNormalized(@"{
  ""$id"": ""1"",
  ""$type"": ""Argon.Tests.Serialization.PreserveReferencesHandlingTests+Container, Tests"",
  ""ListA"": {
    ""$id"": ""2"",
    ""$type"": """ + ReflectionUtils.GetTypeName(typeof(List<ContentA>), 0, DefaultSerializationBinder.Instance) + @""",
    ""$values"": [
      {
        ""$id"": ""3"",
        ""$type"": ""Argon.Tests.Serialization.PreserveReferencesHandlingTests+ContentB, Tests"",
        ""SomeValue"": true
      }
    ]
  },
  ""ListB"": {
    ""$id"": ""4"",
    ""$type"": """ + ReflectionUtils.GetTypeName(typeof(List<ContentA>), 0, DefaultSerializationBinder.Instance) + @""",
    ""$values"": [
      {
        ""$ref"": ""3""
      }
    ]
  }
}", s);

        var c2 = JsonConvert.DeserializeObject<Container>(s, settings);

        Assert.Equal(c2.ListA[0], c2.ListB[0]);
        XUnitAssert.True(c2.ListA[0].B.SomeValue);
    }

    public class Parent
    {
        public Child ReadOnlyChild => Child1;

        public Child Child1 { get; set; }
        public Child Child2 { get; set; }

        public IList<string> ReadOnlyList => List1;

        public IList<string> List1 { get; set; }
        public IList<string> List2 { get; set; }
    }

    public class Child
    {
        public string PropertyName { get; set; }
    }

    [Fact]
    public void SerializeReadOnlyProperty()
    {
        var c = new Child
        {
            PropertyName = "value?"
        };
        IList<string> l = new List<string>
        {
            "value!"
        };
        var p = new Parent
        {
            Child1 = c,
            Child2 = c,
            List1 = l,
            List2 = l
        };

        var json = JsonConvert.SerializeObject(p, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.All
        });

        XUnitAssert.AreEqualNormalized(@"{
  ""$id"": ""1"",
  ""ReadOnlyChild"": {
    ""PropertyName"": ""value?""
  },
  ""Child1"": {
    ""$id"": ""2"",
    ""PropertyName"": ""value?""
  },
  ""Child2"": {
    ""$ref"": ""2""
  },
  ""ReadOnlyList"": [
    ""value!""
  ],
  ""List1"": {
    ""$id"": ""3"",
    ""$values"": [
      ""value!""
    ]
  },
  ""List2"": {
    ""$ref"": ""3""
  }
}", json);

        var newP = JsonConvert.DeserializeObject<Parent>(json, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.All
        });

        Assert.Equal("value?", newP.Child1.PropertyName);
        Assert.Equal(newP.Child1, newP.Child2);
        Assert.Equal(newP.Child1, newP.ReadOnlyChild);

        Assert.Equal("value!", newP.List1[0]);
        Assert.Equal(newP.List1, newP.List2);
        Assert.Equal(newP.List1, newP.ReadOnlyList);
    }

    [Fact]
    public void SerializeDictionarysWithPreserveObjectReferences()
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var circularDictionary = new CircularDictionary();
        circularDictionary.Add("other", new CircularDictionary { { "blah", null } });
        circularDictionary.Add("self", circularDictionary);

        var json = JsonConvert.SerializeObject(circularDictionary, Formatting.Indented,
            new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });

        XUnitAssert.AreEqualNormalized(@"{
  ""$id"": ""1"",
  ""other"": {
    ""$id"": ""2"",
    ""blah"": null
  },
  ""self"": {
    ""$ref"": ""1""
  }
}", json);
    }

    [Fact]
    public void DeserializeDictionarysWithPreserveObjectReferences()
    {
        var json = @"{
  ""$id"": ""1"",
  ""other"": {
    ""$id"": ""2"",
    ""blah"": null
  },
  ""self"": {
    ""$ref"": ""1""
  }
}";

        var circularDictionary = JsonConvert.DeserializeObject<CircularDictionary>(json,
            new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });

        Assert.Equal(2, circularDictionary.Count);
        Assert.Equal(1, circularDictionary["other"].Count);
        Assert.True(ReferenceEquals(circularDictionary, circularDictionary["self"]));
    }

    public class CircularList : List<CircularList>
    {
    }

    [Fact]
    public void SerializeCircularListsError()
    {
        var classRef = typeof(CircularList).FullName;

        var circularList = new CircularList();
        circularList.Add(null);
        circularList.Add(new CircularList { null });
        circularList.Add(new CircularList { new() { circularList } });

        ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.SerializeObject(circularList, Formatting.Indented); }, "Self referencing loop detected with type '" + classRef + "'. Path '[2][0]'.");
    }

    [Fact]
    public void SerializeCircularListsIgnore()
    {
        var circularList = new CircularList();
        circularList.Add(null);
        circularList.Add(new CircularList { null });
        circularList.Add(new CircularList { new() { circularList } });

        var json = JsonConvert.SerializeObject(circularList,
            Formatting.Indented,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

        XUnitAssert.AreEqualNormalized(@"[
  null,
  [
    null
  ],
  [
    []
  ]
]", json);
    }

    [Fact]
    public void SerializeListsWithPreserveObjectReferences()
    {
        var circularList = new CircularList();
        circularList.Add(null);
        circularList.Add(new CircularList { null });
        circularList.Add(new CircularList { new() { circularList } });

        var json = JsonConvert.SerializeObject(circularList, Formatting.Indented,
            new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });

        XUnitAssert.AreEqualNormalized(@"{
  ""$id"": ""1"",
  ""$values"": [
    null,
    {
      ""$id"": ""2"",
      ""$values"": [
        null
      ]
    },
    {
      ""$id"": ""3"",
      ""$values"": [
        {
          ""$id"": ""4"",
          ""$values"": [
            {
              ""$ref"": ""1""
            }
          ]
        }
      ]
    }
  ]
}", json);
    }

    [Fact]
    public void DeserializeListsWithPreserveObjectReferences()
    {
        var json = @"{
  ""$id"": ""1"",
  ""$values"": [
    null,
    {
      ""$id"": ""2"",
      ""$values"": [
        null
      ]
    },
    {
      ""$id"": ""3"",
      ""$values"": [
        {
          ""$id"": ""4"",
          ""$values"": [
            {
              ""$ref"": ""1""
            }
          ]
        }
      ]
    }
  ]
}";

        var circularList = JsonConvert.DeserializeObject<CircularList>(json,
            new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });

        Assert.Equal(3, circularList.Count);
        Assert.Equal(null, circularList[0]);
        Assert.Equal(1, circularList[1].Count);
        Assert.Equal(1, circularList[2].Count);
        Assert.Equal(1, circularList[2][0].Count);
        Assert.True(ReferenceEquals(circularList, circularList[2][0][0]));
    }

    [Fact]
    public void DeserializeArraysWithPreserveObjectReferences()
    {
        var json = @"{
  ""$id"": ""1"",
  ""$values"": [
    null,
    {
      ""$id"": ""2"",
      ""$values"": [
        null
      ]
    },
    {
      ""$id"": ""3"",
      ""$values"": [
        {
          ""$id"": ""4"",
          ""$values"": [
            {
              ""$ref"": ""1""
            }
          ]
        }
      ]
    }
  ]
}";

        ExceptionAssert.Throws<JsonSerializationException>(() =>
        {
            JsonConvert.DeserializeObject<string[][]>(json,
                new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });
        }, @"Cannot preserve reference to array or readonly list, or list created from a non-default constructor: System.String[][]. Path '$values', line 3, position 14.");
    }

    public class CircularDictionary : Dictionary<string, CircularDictionary>
    {
    }

    [Fact]
    public void SerializeCircularDictionarysError()
    {
        var classRef = typeof(CircularDictionary).FullName;

        var circularDictionary = new CircularDictionary();
        circularDictionary.Add("other", new CircularDictionary { { "blah", null } });
        circularDictionary.Add("self", circularDictionary);

        ExceptionAssert.Throws<JsonSerializationException>(() => { JsonConvert.SerializeObject(circularDictionary, Formatting.Indented); }, @"Self referencing loop detected with type '" + classRef + "'. Path ''.");
    }

    [Fact]
    public void SerializeCircularDictionarysIgnore()
    {
        var circularDictionary = new CircularDictionary();
        circularDictionary.Add("other", new CircularDictionary { { "blah", null } });
        circularDictionary.Add("self", circularDictionary);

        var json = JsonConvert.SerializeObject(circularDictionary, Formatting.Indented,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

        XUnitAssert.AreEqualNormalized(@"{
  ""other"": {
    ""blah"": null
  }
}", json);
    }

    [Fact]
    public void UnexpectedEnd()
    {
        var json = @"{
  ""$id"":";

        ExceptionAssert.Throws<JsonSerializationException>(() =>
        {
            JsonConvert.DeserializeObject<string[][]>(json,
                new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    MetadataPropertyHandling = MetadataPropertyHandling.Default
                });
        }, @"Unexpected end when reading JSON. Path '$id', line 2, position 8.");
    }

    public class CircularReferenceClassConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var circularReferenceClass = (CircularReferenceClass)value;

            var reference = serializer.ReferenceResolver.GetReference(serializer, circularReferenceClass);

            var me = new JObject
            {
                ["$id"] = new JValue(reference),
                ["$type"] = new JValue(value.GetType().Name),
                ["Name"] = new JValue(circularReferenceClass.Name)
            };

            var o = JObject.FromObject(circularReferenceClass.Child, serializer);
            me["Child"] = o;

            me.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = JObject.Load(reader);
            var id = (string)o["$id"];
            if (id != null)
            {
                var circularReferenceClass = new CircularReferenceClass();
                serializer.Populate(o.CreateReader(), circularReferenceClass);
                return circularReferenceClass;
            }
            else
            {
                var reference = (string)o["$ref"];
                return serializer.ReferenceResolver.ResolveReference(serializer, reference);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CircularReferenceClass);
        }
    }

    [Fact]
    public void SerializeCircularReferencesWithConverter()
    {
        var c1 = new CircularReferenceClass { Name = "c1" };
        var c2 = new CircularReferenceClass { Name = "c2" };
        var c3 = new CircularReferenceClass { Name = "c3" };

        c1.Child = c2;
        c2.Child = c3;
        c3.Child = c1;

        var json = JsonConvert.SerializeObject(c1, Formatting.Indented, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Converters = new List<JsonConverter> { new CircularReferenceClassConverter() }
        });

        XUnitAssert.AreEqualNormalized(@"{
  ""$id"": ""1"",
  ""$type"": ""CircularReferenceClass"",
  ""Name"": ""c1"",
  ""Child"": {
    ""$id"": ""2"",
    ""$type"": ""CircularReferenceClass"",
    ""Name"": ""c2"",
    ""Child"": {
      ""$id"": ""3"",
      ""$type"": ""CircularReferenceClass"",
      ""Name"": ""c3"",
      ""Child"": {
        ""$ref"": ""1""
      }
    }
  }
}", json);
    }

    [Fact]
    public void DeserializeCircularReferencesWithConverter()
    {
        var json = @"{
  ""$id"": ""1"",
  ""$type"": ""CircularReferenceClass"",
  ""Name"": ""c1"",
  ""Child"": {
    ""$id"": ""2"",
    ""$type"": ""CircularReferenceClass"",
    ""Name"": ""c2"",
    ""Child"": {
      ""$id"": ""3"",
      ""$type"": ""CircularReferenceClass"",
      ""Name"": ""c3"",
      ""Child"": {
        ""$ref"": ""1""
      }
    }
  }
}";

        var c1 = JsonConvert.DeserializeObject<CircularReferenceClass>(json, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Converters = new List<JsonConverter> { new CircularReferenceClassConverter() }
        });

        Assert.Equal("c1", c1.Name);
        Assert.Equal("c2", c1.Child.Name);
        Assert.Equal("c3", c1.Child.Child.Name);
        Assert.Equal("c1", c1.Child.Child.Child.Name);
    }

    [Fact]
    public void SerializeEmployeeReference()
    {
        var mikeManager = new EmployeeReference
        {
            Name = "Mike Manager"
        };
        var joeUser = new EmployeeReference
        {
            Name = "Joe User",
            Manager = mikeManager
        };

        var employees = new List<EmployeeReference>
        {
            mikeManager,
            joeUser
        };

        var json = JsonConvert.SerializeObject(employees, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  {
    ""$id"": ""1"",
    ""Name"": ""Mike Manager"",
    ""Manager"": null
  },
  {
    ""$id"": ""2"",
    ""Name"": ""Joe User"",
    ""Manager"": {
      ""$ref"": ""1""
    }
  }
]", json);
    }

    [Fact]
    public void DeserializeEmployeeReference()
    {
        var json = @"[
  {
    ""$id"": ""1"",
    ""Name"": ""Mike Manager"",
    ""Manager"": null
  },
  {
    ""$id"": ""2"",
    ""Name"": ""Joe User"",
    ""Manager"": {
      ""$ref"": ""1""
    }
  }
]";

        var employees = JsonConvert.DeserializeObject<List<EmployeeReference>>(json);

        Assert.Equal(2, employees.Count);
        Assert.Equal("Mike Manager", employees[0].Name);
        Assert.Equal("Joe User", employees[1].Name);
        Assert.Equal(employees[0], employees[1].Manager);
    }

    [JsonObject(IsReference = true)]
    class Condition
    {
        public int Value { get; }

        public Condition(int value)
        {
            Value = value;
        }
    }

    class ClassWithConditions
    {
        public Condition Condition1 { get; }

        public Condition Condition2 { get; }

        public ClassWithConditions(Condition condition1, Condition condition2)
        {
            Condition1 = condition1;
            Condition2 = condition2;
        }
    }

    [Fact]
    public void SerializeIsReferenceReadonlyProperty()
    {
        var condition = new Condition(1);
        var value = new ClassWithConditions(condition, condition);

        var json = JsonConvert.SerializeObject(value, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""Condition1"": {
    ""$id"": ""1"",
    ""Value"": 1
  },
  ""Condition2"": {
    ""$ref"": ""1""
  }
}", json);
    }

    [Fact]
    public void DeserializeIsReferenceReadonlyProperty()
    {
        var json = @"{
  ""Condition1"": {
    ""$id"": ""1"",
    ""Value"": 1
  },
  ""Condition2"": {
    ""$ref"": ""1""
  }
}";

        var value = JsonConvert.DeserializeObject<ClassWithConditions>(json);
        Assert.Equal(value.Condition1.Value, 1);
        Assert.Equal(value.Condition1, value.Condition2);
    }

    [Fact]
    public void SerializeCircularReference()
    {
        var c1 = new CircularReferenceClass { Name = "c1" };
        var c2 = new CircularReferenceClass { Name = "c2" };
        var c3 = new CircularReferenceClass { Name = "c3" };

        c1.Child = c2;
        c2.Child = c3;
        c3.Child = c1;

        var json = JsonConvert.SerializeObject(c1, Formatting.Indented, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        });

        XUnitAssert.AreEqualNormalized(@"{
  ""$id"": ""1"",
  ""Name"": ""c1"",
  ""Child"": {
    ""$id"": ""2"",
    ""Name"": ""c2"",
    ""Child"": {
      ""$id"": ""3"",
      ""Name"": ""c3"",
      ""Child"": {
        ""$ref"": ""1""
      }
    }
  }
}", json);
    }

    [Fact]
    public void DeserializeCircularReference()
    {
        var json = @"{
  ""$id"": ""1"",
  ""Name"": ""c1"",
  ""Child"": {
    ""$id"": ""2"",
    ""Name"": ""c2"",
    ""Child"": {
      ""$id"": ""3"",
      ""Name"": ""c3"",
      ""Child"": {
        ""$ref"": ""1""
      }
    }
  }
}";

        var c1 =
            JsonConvert.DeserializeObject<CircularReferenceClass>(json, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });

        Assert.Equal("c1", c1.Name);
        Assert.Equal("c2", c1.Child.Name);
        Assert.Equal("c3", c1.Child.Child.Name);
        Assert.Equal("c1", c1.Child.Child.Child.Name);
    }

    [Fact]
    public void SerializeReferenceInList()
    {
        var e1 = new EmployeeReference { Name = "e1" };
        var e2 = new EmployeeReference { Name = "e2" };

        var employees = new List<EmployeeReference> { e1, e2, e1, e2 };

        var json = JsonConvert.SerializeObject(employees, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"[
  {
    ""$id"": ""1"",
    ""Name"": ""e1"",
    ""Manager"": null
  },
  {
    ""$id"": ""2"",
    ""Name"": ""e2"",
    ""Manager"": null
  },
  {
    ""$ref"": ""1""
  },
  {
    ""$ref"": ""2""
  }
]", json);
    }

    [Fact]
    public void DeserializeReferenceInList()
    {
        var json = @"[
  {
    ""$id"": ""1"",
    ""Name"": ""e1"",
    ""Manager"": null
  },
  {
    ""$id"": ""2"",
    ""Name"": ""e2"",
    ""Manager"": null
  },
  {
    ""$ref"": ""1""
  },
  {
    ""$ref"": ""2""
  }
]";

        var employees = JsonConvert.DeserializeObject<List<EmployeeReference>>(json);
        Assert.Equal(4, employees.Count);

        Assert.Equal("e1", employees[0].Name);
        Assert.Equal("e2", employees[1].Name);
        Assert.Equal("e1", employees[2].Name);
        Assert.Equal("e2", employees[3].Name);

        Assert.Equal(employees[0], employees[2]);
        Assert.Equal(employees[1], employees[3]);
    }

    [Fact]
    public void SerializeReferenceInDictionary()
    {
        var e1 = new EmployeeReference { Name = "e1" };
        var e2 = new EmployeeReference { Name = "e2" };

        var employees = new Dictionary<string, EmployeeReference>
        {
            { "One", e1 },
            { "Two", e2 },
            { "Three", e1 },
            { "Four", e2 }
        };

        var json = JsonConvert.SerializeObject(employees, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"{
  ""One"": {
    ""$id"": ""1"",
    ""Name"": ""e1"",
    ""Manager"": null
  },
  ""Two"": {
    ""$id"": ""2"",
    ""Name"": ""e2"",
    ""Manager"": null
  },
  ""Three"": {
    ""$ref"": ""1""
  },
  ""Four"": {
    ""$ref"": ""2""
  }
}", json);
    }

    [Fact]
    public void DeserializeReferenceInDictionary()
    {
        var json = @"{
  ""One"": {
    ""$id"": ""1"",
    ""Name"": ""e1"",
    ""Manager"": null
  },
  ""Two"": {
    ""$id"": ""2"",
    ""Name"": ""e2"",
    ""Manager"": null
  },
  ""Three"": {
    ""$ref"": ""1""
  },
  ""Four"": {
    ""$ref"": ""2""
  }
}";

        var employees = JsonConvert.DeserializeObject<Dictionary<string, EmployeeReference>>(json);
        Assert.Equal(4, employees.Count);

        var e1 = employees["One"];
        var e2 = employees["Two"];

        Assert.Equal("e1", e1.Name);
        Assert.Equal("e2", e2.Name);

        Assert.Equal(e1, employees["Three"]);
        Assert.Equal(e2, employees["Four"]);
    }

    [Fact]
    public void ExampleWithout()
    {
        var p = new Person
        {
            BirthDate = new DateTime(1980, 12, 23, 0, 0, 0, DateTimeKind.Utc),
            LastModified = new DateTime(2009, 2, 20, 12, 59, 21, DateTimeKind.Utc),
            Department = "IT",
            Name = "James"
        };

        var people = new List<Person>
        {
            p,
            p
        };

        var json = JsonConvert.SerializeObject(people, Formatting.Indented);
        //[
        //  {
        //    "Name": "James",
        //    "BirthDate": "\/Date(346377600000)\/",
        //    "LastModified": "\/Date(1235134761000)\/"
        //  },
        //  {
        //    "Name": "James",
        //    "BirthDate": "\/Date(346377600000)\/",
        //    "LastModified": "\/Date(1235134761000)\/"
        //  }
        //]
    }

    [Fact]
    public void ExampleWith()
    {
        var p = new Person
        {
            BirthDate = new DateTime(1980, 12, 23, 0, 0, 0, DateTimeKind.Utc),
            LastModified = new DateTime(2009, 2, 20, 12, 59, 21, DateTimeKind.Utc),
            Department = "IT",
            Name = "James"
        };

        var people = new List<Person>
        {
            p,
            p
        };

        var json = JsonConvert.SerializeObject(people, Formatting.Indented,
            new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
        //[
        //  {
        //    "$id": "1",
        //    "Name": "James",
        //    "BirthDate": "\/Date(346377600000)\/",
        //    "LastModified": "\/Date(1235134761000)\/"
        //  },
        //  {
        //    "$ref": "1"
        //  }
        //]

        var deserializedPeople = JsonConvert.DeserializeObject<List<Person>>(json,
            new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });

        Assert.Equal(2, deserializedPeople.Count);

        var p1 = deserializedPeople[0];
        var p2 = deserializedPeople[1];

        Assert.Equal("James", p1.Name);
        Assert.Equal("James", p2.Name);

        var equal = ReferenceEquals(p1, p2);
        XUnitAssert.True(equal);
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class User
    {
        #region properties
        [JsonProperty(Required = Required.Always, PropertyName = "SecretType")]
        string secretType;

        [JsonProperty(Required = Required.Always)]
        public string Login { get; set; }

        public Type SecretType
        {
            get => Type.GetType(secretType);
            set => secretType = value.AssemblyQualifiedName;
        }

        [JsonProperty]
        public User Friend { get; set; }
        #endregion

        #region constructors
        public User()
        {
        }

        public User(string login, Type secretType)
            : this()
        {
            Login = login;
            SecretType = secretType;
        }
        #endregion

        #region methods
        public override int GetHashCode()
        {
            return SecretType.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("SecretType: {0}, Login: {1}", secretType, Login);
        }
        #endregion
    }

    [Fact]
    public void DeserializeTypeWithDubiousGetHashcode()
    {
        var user1 = new User("Peter", typeof(Version));
        var user2 = new User("Michael", typeof(Version));

        user1.Friend = user2;

        var serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        var json = JsonConvert.SerializeObject(user1, Formatting.Indented, serializerSettings);

        var deserializedUser = JsonConvert.DeserializeObject<User>(json, serializerSettings);
        Assert.NotNull(deserializedUser);
    }

    [Fact]
    public void PreserveReferencesHandlingWithReusedJsonSerializer()
    {
        var c = new MyClass();

        IList<MyClass> myClasses1 = new List<MyClass>
        {
            c,
            c
        };

        var ser = new JsonSerializer
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All
        };

        var ms = new MemoryStream();

        using (var sw = new StreamWriter(ms))
        using (var writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
        {
            ser.Serialize(writer, myClasses1);
        }

        var data = ms.ToArray();
        var json = Encoding.UTF8.GetString(data, 0, data.Length);

        XUnitAssert.AreEqualNormalized(@"{
  ""$id"": ""1"",
  ""$values"": [
    {
      ""$id"": ""2"",
      ""PreProperty"": 0,
      ""PostProperty"": 0
    },
    {
      ""$ref"": ""2""
    }
  ]
}", json);

        ms = new MemoryStream(data);
        IList<MyClass> myClasses2;

        using (var sr = new StreamReader(ms))
        using (var reader = new JsonTextReader(sr))
        {
            myClasses2 = ser.Deserialize<IList<MyClass>>(reader);
        }

        Assert.Equal(2, myClasses2.Count);
        Assert.Equal(myClasses2[0], myClasses2[1]);

        Assert.NotEqual(myClasses1[0], myClasses2[0]);
    }

    [Fact]
    public void ReferencedIntList()
    {
        var l = new ReferencedList<int>
        {
            1,
            2,
            3
        };

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  1,
  2,
  3
]", json);
    }

    [Fact]
    public void ReferencedComponentList()
    {
        var c1 = new TestComponentSimple();

        var l = new ReferencedList<TestComponentSimple>
        {
            c1,
            new(),
            c1
        };

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  {
    ""$id"": ""1"",
    ""MyProperty"": 0
  },
  {
    ""$id"": ""2"",
    ""MyProperty"": 0
  },
  {
    ""$ref"": ""1""
  }
]", json);
    }

    [Fact]
    public void ReferencedIntDictionary()
    {
        var l = new ReferencedDictionary<int>
        {
            {"First", 1},
            {"Second", 2},
            {"Third", 3}
        };

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""First"": 1,
  ""Second"": 2,
  ""Third"": 3
}", json);
    }

    [Fact]
    public void ReferencedComponentDictionary()
    {
        var c1 = new TestComponentSimple();

        var l = new ReferencedDictionary<TestComponentSimple>
        {
            {"First", c1},
            {"Second", new TestComponentSimple()},
            {"Third", c1}
        };

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""First"": {
    ""$id"": ""1"",
    ""MyProperty"": 0
  },
  ""Second"": {
    ""$id"": ""2"",
    ""MyProperty"": 0
  },
  ""Third"": {
    ""$ref"": ""1""
  }
}", json);

        var d = JsonConvert.DeserializeObject<ReferencedDictionary<TestComponentSimple>>(json);
        Assert.Equal(3, d.Count);
        Assert.True(ReferenceEquals(d["First"], d["Third"]));
    }

    [Fact]
    public void ReferencedObjectItems()
    {
        var o1 = new ReferenceObject
        {
            Component1 = new TestComponentSimple { MyProperty = 1 }
        };

        o1.Component2 = o1.Component1;
        o1.ComponentNotReference = new TestComponentSimple();
        o1.String = "String!";
        o1.Integer = int.MaxValue;

        var json = JsonConvert.SerializeObject(o1, Formatting.Indented);
        var expected = @"{
  ""Component1"": {
    ""$id"": ""1"",
    ""MyProperty"": 1
  },
  ""Component2"": {
    ""$ref"": ""1""
  },
  ""ComponentNotReference"": {
    ""MyProperty"": 0
  },
  ""String"": ""String!"",
  ""Integer"": 2147483647
}";
        XUnitAssert.AreEqualNormalized(expected, json);

        var referenceObject = JsonConvert.DeserializeObject<ReferenceObject>(json);
        Assert.NotNull(referenceObject);

        Assert.True(ReferenceEquals(referenceObject.Component1, referenceObject.Component2));
    }

    [Fact]
    public void PropertyItemIsReferenceObject()
    {
        var c1 = new TestComponentSimple();

        var o1 = new PropertyItemIsReferenceObject
        {
            Data = new PropertyItemIsReferenceBody
            {
                Prop1 = c1,
                Prop2 = c1,
                Data = new List<TestComponentSimple>
                {
                    c1
                }
            }
        };

        var json = JsonConvert.SerializeObject(o1, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""Data"": {
    ""Prop1"": {
      ""$id"": ""1"",
      ""MyProperty"": 0
    },
    ""Prop2"": {
      ""$ref"": ""1""
    },
    ""Data"": {
      ""$id"": ""2"",
      ""$values"": [
        {
          ""MyProperty"": 0
        }
      ]
    }
  }
}", json);

        var o2 = JsonConvert.DeserializeObject<PropertyItemIsReferenceObject>(json);

        var c2 = o2.Data.Prop1;
        var c3 = o2.Data.Prop2;
        var c4 = o2.Data.Data[0];

        Assert.True(ReferenceEquals(c2, c3));
        Assert.False(ReferenceEquals(c2, c4));
    }

    [Fact]
    public void DuplicateId()
    {
        var json = @"{
  ""Data"": {
    ""Prop1"": {
      ""$id"": ""1"",
      ""MyProperty"": 0
    },
    ""Prop2"": {
      ""$id"": ""1"",
      ""MyProperty"": 0
    }
  }
}";

        ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<PropertyItemIsReferenceObject>(json, new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Default
        }), "Error reading object reference '1'. Path 'Data.Prop2.MyProperty', line 9, position 19.");
    }
}

public class PropertyItemIsReferenceBody
{
    public TestComponentSimple Prop1 { get; set; }
    public TestComponentSimple Prop2 { get; set; }
    public IList<TestComponentSimple> Data { get; set; }
}

public class PropertyItemIsReferenceObject
{
    [JsonProperty(ItemIsReference = true)]
    public PropertyItemIsReferenceBody Data { get; set; }
}

public class PropertyItemIsReferenceList
{
    [JsonProperty(ItemIsReference = true)]
    public IList<IList<object>> Data { get; set; }
}

[JsonArray(ItemIsReference = true)]
public class ReferencedList<T> : List<T>
{
}

[JsonDictionary(ItemIsReference = true)]
public class ReferencedDictionary<T> : Dictionary<string, T>
{
}

[JsonObject(ItemIsReference = true)]
public class ReferenceObject
{
    public TestComponentSimple Component1 { get; set; }
    public TestComponentSimple Component2 { get; set; }

    [JsonProperty(IsReference = false)]
    public TestComponentSimple ComponentNotReference { get; set; }

    public string String { get; set; }
    public int Integer { get; set; }
}