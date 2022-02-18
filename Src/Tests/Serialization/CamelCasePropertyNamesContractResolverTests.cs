#region License
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
using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;

namespace Argon.Tests.Serialization;

public class CamelCasePropertyNamesContractResolverTests : TestFixtureBase
{
    [Fact]
    public void EnsureContractsShared()
    {
        var resolver1 = new CamelCasePropertyNamesContractResolver();
        var contract1 = (JsonObjectContract)resolver1.ResolveContract(typeof(CamelCasePropertyNamesContractResolverTests));

        var resolver2 = new CamelCasePropertyNamesContractResolver();
        var contract2 = (JsonObjectContract)resolver2.ResolveContract(typeof(CamelCasePropertyNamesContractResolverTests));

        Xunit.Assert.True(ReferenceEquals(contract1, contract2));

        var nt1 = resolver1.GetNameTable();
        var nt2 = resolver2.GetNameTable();

        Xunit.Assert.True(ReferenceEquals(nt1, nt2));
    }

    [Fact]
    public void JsonConvertSerializerSettings()
    {
        var person = new Person
        {
            BirthDate = new DateTime(2000, 11, 20, 23, 55, 44, DateTimeKind.Utc),
            LastModified = new DateTime(2000, 11, 20, 23, 55, 44, DateTimeKind.Utc),
            Name = "Name!"
        };

        var json = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        StringAssert.AreEqual(@"{
  ""name"": ""Name!"",
  ""birthDate"": ""2000-11-20T23:55:44Z"",
  ""lastModified"": ""2000-11-20T23:55:44Z""
}", json);

        var deserializedPerson = JsonConvert.DeserializeObject<Person>(json, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        Assert.AreEqual(person.BirthDate, deserializedPerson.BirthDate);
        Assert.AreEqual(person.LastModified, deserializedPerson.LastModified);
        Assert.AreEqual(person.Name, deserializedPerson.Name);

        json = JsonConvert.SerializeObject(person, Formatting.Indented);
        StringAssert.AreEqual(@"{
  ""Name"": ""Name!"",
  ""BirthDate"": ""2000-11-20T23:55:44Z"",
  ""LastModified"": ""2000-11-20T23:55:44Z""
}", json);
    }

    [Fact]
    public void JTokenWriter()
    {
        var ignoreAttributeOnClassTestClass = new JsonIgnoreAttributeOnClassTestClass
        {
            Field = int.MinValue
        };

        var serializer = new JsonSerializer
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        var writer = new JTokenWriter();

        serializer.Serialize(writer, ignoreAttributeOnClassTestClass);

        var o = (JObject)writer.Token;
        var p = o.Property("theField");

        Assert.IsNotNull(p);
        Assert.AreEqual(int.MinValue, (int)p.Value);

        var json = o.ToString();
    }

#pragma warning disable 618
    [Fact]
    public void MemberSearchFlags()
    {
        var privateMembersClass = new PrivateMembersClass("PrivateString!", "InternalString!");

        var json = JsonConvert.SerializeObject(privateMembersClass, Formatting.Indented, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver { DefaultMembersSearchFlags = BindingFlags.NonPublic | BindingFlags.Instance }
        });

        StringAssert.AreEqual(@"{
  ""_privateString"": ""PrivateString!"",
  ""i"": 0,
  ""_internalString"": ""InternalString!""
}", json);

        var deserializedPrivateMembersClass = JsonConvert.DeserializeObject<PrivateMembersClass>(@"{
  ""_privateString"": ""Private!"",
  ""i"": -2,
  ""_internalString"": ""Internal!""
}", new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver { DefaultMembersSearchFlags = BindingFlags.NonPublic | BindingFlags.Instance }
        });

        Assert.AreEqual("Private!", ReflectionUtils.GetMemberValue(typeof(PrivateMembersClass).GetField("_privateString", BindingFlags.Instance | BindingFlags.NonPublic), deserializedPrivateMembersClass));
        Assert.AreEqual("Internal!", ReflectionUtils.GetMemberValue(typeof(PrivateMembersClass).GetField("_internalString", BindingFlags.Instance | BindingFlags.NonPublic), deserializedPrivateMembersClass));

        // readonly
        Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(PrivateMembersClass).GetField("i", BindingFlags.Instance | BindingFlags.NonPublic), deserializedPrivateMembersClass));
    }
#pragma warning restore 618

    [Fact]
    public void BlogPostExample()
    {
        var product = new Product
        {
            ExpiryDate = new DateTime(2010, 12, 20, 18, 1, 0, DateTimeKind.Utc),
            Name = "Widget",
            Price = 9.99m,
            Sizes = new[] { "Small", "Medium", "Large" }
        };

        var json =
            JsonConvert.SerializeObject(
                product,
                Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
            );

        //{
        //  "name": "Widget",
        //  "expiryDate": "\/Date(1292868060000)\/",
        //  "price": 9.99,
        //  "sizes": [
        //    "Small",
        //    "Medium",
        //    "Large"
        //  ]
        //}

        StringAssert.AreEqual(@"{
  ""name"": ""Widget"",
  ""expiryDate"": ""2010-12-20T18:01:00Z"",
  ""price"": 9.99,
  ""sizes"": [
    ""Small"",
    ""Medium"",
    ""Large""
  ]
}", json);
    }

    [Fact]
    public void DynamicCamelCasePropertyNames()
    {
        dynamic o = new TestDynamicObject();
        o.Text = "Text!";
        o.Integer = int.MaxValue;

        string json = JsonConvert.SerializeObject(o, Formatting.Indented,
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

        StringAssert.AreEqual(@"{
  ""explicit"": false,
  ""text"": ""Text!"",
  ""integer"": 2147483647,
  ""int"": 0,
  ""childObject"": null
}", json);
    }

    [Fact]
    public void DictionaryCamelCasePropertyNames()
    {
        var values = new Dictionary<string, string>
        {
            { "First", "Value1!" },
            { "Second", "Value2!" }
        };

        var json = JsonConvert.SerializeObject(values, Formatting.Indented,
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

        StringAssert.AreEqual(@"{
  ""first"": ""Value1!"",
  ""second"": ""Value2!""
}", json);
    }
}