﻿// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using TestObjects;

[UsesVerify]
public class SerializationEventAttributeTests : TestFixtureBase
{
    [Fact]
    public void ObjectEvents()
    {
        var objs = new[] {new SerializationEventTestObject(), new DerivedSerializationEventTestObject()};

        foreach (var current in objs)
        {
            var obj = current;

            Assert.Equal(11, obj.Member1);
            Assert.Equal("Hello World!", obj.Member2);
            Assert.Equal("This is a nonserialized value", obj.Member3);
            Assert.Equal(null, obj.Member4);
            Assert.Equal(null, obj.Member5);

            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            XUnitAssert.AreEqualNormalized("""
                {
                  "Member1": 11,
                  "Member2": "This value went into the data file during serialization.",
                  "Member4": null
                }
                """, json);

            Assert.Equal(11, obj.Member1);
            Assert.Equal("This value was reset after serialization.", obj.Member2);
            Assert.Equal("This is a nonserialized value", obj.Member3);
            Assert.Equal(null, obj.Member4);

            var expectedError = $"Error message for member Member6 = Error getting value from 'Member6' on '{obj.GetType().FullName}'.";
            Assert.Equal(expectedError, obj.Member5);

            var o = JObject.Parse("""
                {
                  "Member1": 11,
                  "Member2": "This value went into the data file during serialization.",
                  "Member4": null
                }
                """);
            o["Member6"] = "Dummy text for error";

            obj = (SerializationEventTestObject) JsonConvert.DeserializeObject(o.ToString(), obj.GetType());

            Assert.Equal(11, obj.Member1);
            Assert.Equal("This value went into the data file during serialization.", obj.Member2);
            Assert.Equal("This value was set during deserialization", obj.Member3);
            Assert.Equal("This value was set after deserialization.", obj.Member4);

            expectedError = $"Error message for member Member6 = Error setting value to 'Member6' on '{obj.GetType()}'.";
            Assert.Equal(expectedError, obj.Member5);

            var derivedObj = obj as DerivedSerializationEventTestObject;
            if (derivedObj != null)
            {
                Assert.Equal("This value was set after deserialization.", derivedObj.Member7);
            }
        }
    }

    [Fact]
    public void ObjectWithConstructorEvents()
    {
        var obj = new SerializationEventTestObjectWithConstructor(11, "Hello World!", null);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("Hello World!", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);

        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        XUnitAssert.AreEqualNormalized("""
            {
              "Member1": 11,
              "Member2": "This value went into the data file during serialization.",
              "Member4": null
            }
            """, json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("This value was reset after serialization.", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);

        obj = JsonConvert.DeserializeObject<SerializationEventTestObjectWithConstructor>(json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("This value went into the data file during serialization.", obj.Member2);
        Assert.Equal("This value was set during deserialization", obj.Member3);
        Assert.Equal("This value was set after deserialization.", obj.Member4);
    }

    [Fact]
    public void ListEvents()
    {
        var obj = new SerializationEventTestList
        {
            1.1m,
            2.222222222m,
            int.MaxValue,
            Convert.ToDecimal(Math.PI)
        };

        Assert.Equal(11, obj.Member1);
        Assert.Equal("Hello World!", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);

        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  -1.0,
  1.1,
  2.222222222,
  2147483647.0,
  3.14159265358979
]", json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("This value was reset after serialization.", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);

        obj = JsonConvert.DeserializeObject<SerializationEventTestList>(json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("Hello World!", obj.Member2);
        Assert.Equal("This value was set during deserialization", obj.Member3);
        Assert.Equal("This value was set after deserialization.", obj.Member4);
    }

    [Fact]
    public void DictionaryEvents()
    {
        var obj = new SerializationEventTestDictionary
        {
            {1.1m, "first"},
            {2.222222222m, "second"},
            {int.MaxValue, "third"},
            {Convert.ToDecimal(Math.PI), "fourth"}
        };

        Assert.Equal(11, obj.Member1);
        Assert.Equal("Hello World!", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);

        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        XUnitAssert.AreEqualNormalized("""
            {
              "1.1": "first",
              "2.222222222": "second",
              "2147483647": "third",
              "3.14159265358979": "fourth",
              "79228162514264337593543950335": "Inserted on serializing"
            }
            """, json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("This value was reset after serialization.", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);

        obj = JsonConvert.DeserializeObject<SerializationEventTestDictionary>(json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("Hello World!", obj.Member2);
        Assert.Equal("This value was set during deserialization", obj.Member3);
        Assert.Equal("This value was set after deserialization.", obj.Member4);
    }

    [Fact]
    public void ObjectEventsDocumentationExample()
    {
        var obj = new SerializationEventTestObject();

        Assert.Equal(11, obj.Member1);
        Assert.Equal("Hello World!", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);
        Assert.Equal(null, obj.Member5);

        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        XUnitAssert.AreEqualNormalized("""
            {
              "Member1": 11,
              "Member2": "This value went into the data file during serialization.",
              "Member4": null
            }
            """, json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("This value was reset after serialization.", obj.Member2);
        Assert.Equal("This is a nonserialized value", obj.Member3);
        Assert.Equal(null, obj.Member4);
        Assert.Equal("Error message for member Member6 = Error getting value from 'Member6' on 'TestObjects.SerializationEventTestObject'.", obj.Member5);

        obj = JsonConvert.DeserializeObject<SerializationEventTestObject>(json);

        Assert.Equal(11, obj.Member1);
        Assert.Equal("This value went into the data file during serialization.", obj.Member2);
        Assert.Equal("This value was set during deserialization", obj.Member3);
        Assert.Equal("This value was set after deserialization.", obj.Member4);
        Assert.Equal(null, obj.Member5);
    }

    public class SerializationEventBaseTestObject :
        IJsonOnSerializing
    {
        public string TestMember { get; set; }

        public void OnSerializing() =>
            TestMember = "Set!";
    }

    public class SerializationEventContextSubClassTestObject : SerializationEventBaseTestObject
    {
    }

    [Fact]
    public void SerializationEventContextTestObjectSubClassTest()
    {
        var obj = new SerializationEventContextSubClassTestObject();

        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        XUnitAssert.AreEqualNormalized("""
            {
              "TestMember": "Set!"
            }
            """, json);
    }

    [Fact]
    public void WhenSerializationErrorDetectedBySerializer_ThenCallbackIsCalled()
    {
        // Verify contract is properly finding our callback
        var resolver = new DefaultContractResolver().ResolveContract(typeof(FooEvent));

        Assert.Equal(resolver.OnErrorCallbacks.Count, 1);

        var serializer = JsonSerializer.Create(new()
        {
            // If I don't specify Error here, the callback isn't called
            // either, but no exception is thrown.
            MissingMemberHandling = MissingMemberHandling.Error
        });

        // This throws with missing member exception, rather than calling my callback.
        var foo = serializer.Deserialize<FooEvent>(new JsonTextReader(new StringReader("{ Id: 25 }")));

        // When fixed, this would pass.
        Assert.Equal(25, foo.Identifier);
    }

    public class FooEvent:
        IJsonOnError
    {
        public int Identifier { get; set; }

        public void OnError(object currentObject, ErrorContext context)
        {
            Identifier = 25;

            // Here we could for example manually copy the
            // persisted "Id" value into the renamed "Identifier"
            // property, etc.
            context.Handled = true;
        }
    }

    [Fact]
    public void DerivedSerializationEvents()
    {
        var c = JsonConvert.DeserializeObject<DerivedSerializationEventOrderTestObject>("{}");

        JsonConvert.SerializeObject(c, Formatting.Indented);

        var e = c.GetEvents();

        XUnitAssert.AreEqualNormalized(@"OnDeserializing
OnDeserializing_Derived
OnDeserialized
OnDeserialized_Derived
OnSerializing
OnSerializing_Derived
OnSerialized
OnSerialized_Derived", string.Join(Environment.NewLine, e.ToArray()));
    }

    [Fact]
    public Task DerivedDerivedSerializationEvents()
    {
        var c = JsonConvert.DeserializeObject<DerivedDerivedSerializationEventOrderTestObject>("{}");

        JsonConvert.SerializeObject(c, Formatting.Indented);

        var e = c.GetEvents();

        return Verify(e);
    }

    public class SerializationEventOrderTestObject :
        IJsonOnSerializing,
        IJsonOnSerialized,
        IJsonOnDeserializing,
        IJsonOnDeserialized
    {
        protected IList<string> Events { get; }

        public SerializationEventOrderTestObject() =>
            Events = new List<string>();

        public IList<string> GetEvents() =>
            Events;

        public virtual void OnSerializing() =>
            Events.Add("OnSerializing");

        public virtual void OnSerialized() =>
            Events.Add("OnSerialized");

        public virtual void OnDeserializing() =>
            Events.Add("OnDeserializing");

        public virtual void OnDeserialized() =>
            Events.Add("OnDeserialized");
    }

    public class DerivedSerializationEventOrderTestObject : SerializationEventOrderTestObject
    {
        public override void OnSerializing()
        {
            base.OnSerializing();
            Events.Add("OnSerializing_Derived");
        }

        public override void OnSerialized()
        {
            base.OnSerialized();
            Events.Add("OnSerialized_Derived");
        }

        public override void OnDeserializing()
        {
            base.OnDeserializing();
            Events.Add("OnDeserializing_Derived");
        }

        public override void OnDeserialized()
        {
            base.OnDeserialized();
            Events.Add("OnDeserialized_Derived");
        }
    }

    public class DerivedDerivedSerializationEventOrderTestObject : DerivedSerializationEventOrderTestObject
    {
        public override void OnSerializing()
        {
            base.OnSerializing();
            Events.Add("OnSerializing_Derived_Derived");
        }

        public override void OnSerialized()
        {
            base.OnSerialized();
            Events.Add("OnSerialized_Derived_Derived");
        }

        public override void OnDeserializing()
        {
            base.OnDeserializing();
            Events.Add("OnDeserializing_Derived_Derived");
        }

        public override void OnDeserialized()
        {
            base.OnDeserialized();
            Events.Add("OnDeserialized_Derived_Derived");
        }
    }
}