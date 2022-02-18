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
using Argon.Tests.TestObjects;
using ErrorEventArgs = Argon.Serialization.ErrorEventArgs;
using OriginalStreamWriter = System.IO.StreamWriter;

namespace Argon.Tests.Documentation;

public class SerializationTests : TestFixtureBase
{
    public class StreamWriter : OriginalStreamWriter
    {
        public StreamWriter(string path) : base(Path.GetFileName(path))
        {
        }
    }

    [Fact]
    public void SerializeObject()
    {
        #region SerializeObject
        var product = new Product
        {
            Name = "Apple",
            ExpiryDate = new DateTime(2008, 12, 28),
            Price = 3.99M,
            Sizes = new[] { "Small", "Medium", "Large" }
        };

        var output = JsonConvert.SerializeObject(product);
        //{
        //  "Name": "Apple",
        //  "ExpiryDate": "2008-12-28T00:00:00",
        //  "Price": 3.99,
        //  "Sizes": [
        //    "Small",
        //    "Medium",
        //    "Large"
        //  ]
        //}

        var deserializedProduct = JsonConvert.DeserializeObject<Product>(output);
        #endregion

        Xunit.Assert.Equal("Apple", deserializedProduct.Name);
    }

    [Fact]
    public void JsonSerializerToStream()
    {
        #region JsonSerializerToStream
        var product = new Product
        {
            ExpiryDate = new DateTime(2008, 12, 28)
        };

        var serializer = new JsonSerializer();
        serializer.Converters.Add(new JavaScriptDateTimeConverter());
        serializer.NullValueHandling = NullValueHandling.Ignore;

        using (var sw = new StreamWriter(@"c:\json.txt"))
        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            serializer.Serialize(writer, product);
            // {"ExpiryDate":new Date(1230375600000),"Price":0}
        }
        #endregion
    }

    #region SerializationAttributes
    [JsonObject(MemberSerialization.OptIn)]
    public class Person
    {
        // "John Smith"
        [JsonProperty]
        public string Name { get; set; }

        // "2000-12-15T22:11:03"
        [JsonProperty]
        public DateTime BirthDate { get; set; }

        // new Date(976918263055)
        [JsonProperty]
        public DateTime LastModified { get; set; }

        // not serialized because mode is opt-in
        public string Department { get; set; }
    }
    #endregion

    #region SerializationCallbacksObject
    public class SerializationEventTestObject
    {
        // 2222
        // This member is serialized and deserialized with no change.
        public int Member1 { get; set; }

        // The value of this field is set and reset during and 
        // after serialization.
        public string Member2 { get; set; }

        // This field is not serialized. The OnDeserializedAttribute 
        // is used to set the member value after serialization.
        [JsonIgnore]
        public string Member3 { get; set; }

        // This field is set to null, but populated after deserialization.
        public string Member4 { get; set; }

        public SerializationEventTestObject()
        {
            Member1 = 11;
            Member2 = "Hello World!";
            Member3 = "This is a nonserialized value";
            Member4 = null;
        }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            Member2 = "This value went into the data file during serialization.";
        }

        [OnSerialized]
        internal void OnSerializedMethod(StreamingContext context)
        {
            Member2 = "This value was reset after serialization.";
        }

        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            Member3 = "This value was set during deserialization";
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Member4 = "This value was set after deserialization.";
        }
    }
    #endregion

    [Fact]
    public void SerializationCallbacksExample()
    {
        #region SerializationCallbacksExample
        var obj = new SerializationEventTestObject();

        Console.WriteLine(obj.Member1);
        // 11
        Console.WriteLine(obj.Member2);
        // Hello World!
        Console.WriteLine(obj.Member3);
        // This is a nonserialized value
        Console.WriteLine(obj.Member4);
        // null

        var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
        // {
        //   "Member1": 11,
        //   "Member2": "This value went into the data file during serialization.",
        //   "Member4": null
        // }

        Console.WriteLine(obj.Member1);
        // 11
        Console.WriteLine(obj.Member2);
        // This value was reset after serialization.
        Console.WriteLine(obj.Member3);
        // This is a nonserialized value
        Console.WriteLine(obj.Member4);
        // null

        obj = JsonConvert.DeserializeObject<SerializationEventTestObject>(json);

        Console.WriteLine(obj.Member1);
        // 11
        Console.WriteLine(obj.Member2);
        // This value went into the data file during serialization.
        Console.WriteLine(obj.Member3);
        // This value was set during deserialization
        Console.WriteLine(obj.Member4);
        // This value was set after deserialization.
        #endregion

        Xunit.Assert.Equal(11, obj.Member1);
    }

    [Fact]
    public void SerializationErrorHandling()
    {
        #region SerializationErrorHandling
        var errors = new List<string>();

        var c = JsonConvert.DeserializeObject<List<DateTime>>(@"[
                  '2009-09-09T00:00:00Z',
                  'I am not a date and will error!',
                  [
                    1
                  ],
                  '1977-02-20T00:00:00Z',
                  null,
                  '2000-12-01T00:00:00Z'
                ]",
            new JsonSerializerSettings
            {
                Error = delegate(object _, ErrorEventArgs args)
                {
                    errors.Add(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                },
                Converters = { new IsoDateTimeConverter() }
            });

        // 2009-09-09T00:00:00Z
        // 1977-02-20T00:00:00Z
        // 2000-12-01T00:00:00Z

        // The string was not recognized as a valid DateTime. There is a unknown word starting at index 0.
        // Unexpected token parsing date. Expected String, got StartArray.
        // Cannot convert null value to System.DateTime.
        #endregion

        Xunit.Assert.Equal(new DateTime(2009, 9, 9, 0, 0, 0, DateTimeKind.Utc), c[0]);
    }

    [Fact]
    public void SerializationErrorHandlingWithParent()
    {
        #region SerializationErrorHandlingWithParent
        var errors = new List<string>();

        var serializer = new JsonSerializer();
        serializer.Error += delegate(object _, ErrorEventArgs args)
        {
            // only log an error once
            if (args.CurrentObject == args.ErrorContext.OriginalObject)
            {
                errors.Add(args.ErrorContext.Error.Message);
            }
        };
        #endregion
    }

    #region SerializationErrorHandlingAttributeObject
    public class PersonError
    {
        List<string> _roles;

        public string Name { get; set; }
        public int Age { get; set; }

        public List<string> Roles
        {
            get
            {
                if (_roles == null)
                {
                    throw new Exception("Roles not loaded!");
                }

                return _roles;
            }
            set => _roles = value;
        }

        public string Title { get; set; }

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            errorContext.Handled = true;
        }
    }
    #endregion

    [Fact]
    public void SerializationErrorHandlingAttributeExample()
    {
        #region SerializationErrorHandlingAttributeExample
        var person = new PersonError
        {
            Name = "George Michael Bluth",
            Age = 16,
            Roles = null,
            Title = "Mister Manager"
        };

        var json = JsonConvert.SerializeObject(person, Formatting.Indented);

        Console.WriteLine(json);
        //{
        //  "Name": "George Michael Bluth",
        //  "Age": 16,
        //  "Title": "Mister Manager"
        //}
        #endregion

        StringAssert.AreEqual(@"{
  ""Name"": ""George Michael Bluth"",
  ""Age"": 16,
  ""Title"": ""Mister Manager""
}", json);
    }

    [Fact]
    public void PreservingObjectReferencesOff()
    {
        #region PreservingObjectReferencesOff
        var p = new Person
        {
            BirthDate = new DateTime(1980, 12, 23, 0, 0, 0, DateTimeKind.Utc),
            LastModified = new DateTime(2009, 2, 20, 12, 59, 21, DateTimeKind.Utc),
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
        //    "BirthDate": "1980-12-23T00:00:00Z",
        //    "LastModified": "2009-02-20T12:59:21Z"
        //  },
        //  {
        //    "Name": "James",
        //    "BirthDate": "1980-12-23T00:00:00Z",
        //    "LastModified": "2009-02-20T12:59:21Z"
        //  }
        //]
        #endregion

        StringAssert.AreEqual(@"[
  {
    ""Name"": ""James"",
    ""BirthDate"": ""1980-12-23T00:00:00Z"",
    ""LastModified"": ""2009-02-20T12:59:21Z""
  },
  {
    ""Name"": ""James"",
    ""BirthDate"": ""1980-12-23T00:00:00Z"",
    ""LastModified"": ""2009-02-20T12:59:21Z""
  }
]", json);
    }

    [Fact]
    public void PreservingObjectReferencesOn()
    {
        var p = new Person
        {
            Name = "James"
        };
        var people = new List<Person>
        {
            p,
            p
        };

        #region PreservingObjectReferencesOn
        var json = JsonConvert.SerializeObject(people, Formatting.Indented,
            new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });

        //[
        //  {
        //    "$id": "1",
        //    "Name": "James",
        //    "BirthDate": "1983-03-08T00:00Z",
        //    "LastModified": "2012-03-21T05:40Z"
        //  },
        //  {
        //    "$ref": "1"
        //  }
        //]

        var deserializedPeople = JsonConvert.DeserializeObject<List<Person>>(json,
            new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });

        Console.WriteLine(deserializedPeople.Count);
        // 2

        var p1 = deserializedPeople[0];
        var p2 = deserializedPeople[1];

        Console.WriteLine(p1.Name);
        // James
        Console.WriteLine(p2.Name);
        // James

        var equal = Object.ReferenceEquals(p1, p2);
        // true
        #endregion

        Assert.True( equal);
    }

    #region PreservingObjectReferencesAttribute
    [JsonObject(IsReference = true)]
    public class EmployeeReference
    {
        public string Name { get; set; }
        public EmployeeReference Manager { get; set; }
    }
    #endregion

    #region CustomCreationConverterObject
    public interface IPerson
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        DateTime BirthDate { get; set; }
    }

    public class Employee : IPerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }

        public string Department { get; set; }
        public string JobTitle { get; set; }
    }

    public class PersonConverter : CustomCreationConverter<IPerson>
    {
        public override IPerson Create(Type objectType)
        {
            return new Employee();
        }
    }
    #endregion

    [Fact]
    public void CustomCreationConverterExample()
    {
        var json = @"[
  {
    ""FirstName"": ""Maurice"",
    ""LastName"": ""Moss"",
    ""BirthDate"": ""1981-03-08T00:00Z"",
    ""Department"": ""IT"",
    ""JobTitle"": ""Support""
  },
  {
    ""FirstName"": ""Jen"",
    ""LastName"": ""Barber"",
    ""BirthDate"": ""1985-12-10T00:00Z"",
    ""Department"": ""IT"",
    ""JobTitle"": ""Manager""
  }
]";

        #region CustomCreationConverterExample
        //[
        //  {
        //    "FirstName": "Maurice",
        //    "LastName": "Moss",
        //    "BirthDate": "1981-03-08T00:00Z",
        //    "Department": "IT",
        //    "JobTitle": "Support"
        //  },
        //  {
        //    "FirstName": "Jen",
        //    "LastName": "Barber",
        //    "BirthDate": "1985-12-10T00:00Z",
        //    "Department": "IT",
        //    "JobTitle": "Manager"
        //  }
        //]

        var people = JsonConvert.DeserializeObject<List<IPerson>>(json, new PersonConverter());

        var person = people[0];

        Console.WriteLine(person.GetType());
        // Argon.Tests.Employee

        Console.WriteLine(person.FirstName);
        // Maurice

        var employee = (Employee)person;

        Console.WriteLine(employee.JobTitle);
        // Support
        #endregion

        Xunit.Assert.Equal("Support", employee.JobTitle);
    }

    [Fact]
    public void ContractResolver()
    {
        #region ContractResolver
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
        //  "expiryDate": "2010-12-20T18:01:00Z",
        //  "price": 9.99,
        //  "sizes": [
        //    "Small",
        //    "Medium",
        //    "Large"
        //  ]
        //}
        #endregion

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
    public void SerializingCollectionsSerializing()
    {
        #region SerializingCollectionsSerializing
        var p1 = new Product
        {
            Name = "Product 1",
            Price = 99.95m,
            ExpiryDate = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc),
        };
        var p2 = new Product
        {
            Name = "Product 2",
            Price = 12.50m,
            ExpiryDate = new DateTime(2009, 7, 31, 0, 0, 0, DateTimeKind.Utc),
        };

        var products = new List<Product>
        {
            p1,
            p2
        };

        var json = JsonConvert.SerializeObject(products, Formatting.Indented);
        //[
        //  {
        //    "Name": "Product 1",
        //    "ExpiryDate": "2000-12-29T00:00:00Z",
        //    "Price": 99.95,
        //    "Sizes": null
        //  },
        //  {
        //    "Name": "Product 2",
        //    "ExpiryDate": "2009-07-31T00:00:00Z",
        //    "Price": 12.50,
        //    "Sizes": null
        //  }
        //]
        #endregion

        StringAssert.AreEqual(@"[
  {
    ""Name"": ""Product 1"",
    ""ExpiryDate"": ""2000-12-29T00:00:00Z"",
    ""Price"": 99.95,
    ""Sizes"": null
  },
  {
    ""Name"": ""Product 2"",
    ""ExpiryDate"": ""2009-07-31T00:00:00Z"",
    ""Price"": 12.50,
    ""Sizes"": null
  }
]", json);
    }

    [Fact]
    public void SerializingCollectionsDeserializing()
    {
        #region SerializingCollectionsDeserializing
        var json = @"[
              {
                'Name': 'Product 1',
                'ExpiryDate': '2000-12-29T00:00Z',
                'Price': 99.95,
                'Sizes': null
              },
              {
                'Name': 'Product 2',
                'ExpiryDate': '2009-07-31T00:00Z',
                'Price': 12.50,
                'Sizes': null
              }
            ]";

        var products = JsonConvert.DeserializeObject<List<Product>>(json);

        Console.WriteLine(products.Count);
        // 2

        var p1 = products[0];

        Console.WriteLine(p1.Name);
        // Product 1
        #endregion

        Xunit.Assert.Equal("Product 1", p1.Name);
    }

    [Fact]
    public void SerializingCollectionsDeserializingDictionaries()
    {
        #region SerializingCollectionsDeserializingDictionaries
        var json = @"{""key1"":""value1"",""key2"":""value2""}";

        var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        Console.WriteLine(values.Count);
        // 2

        Console.WriteLine(values["key1"]);
        // value1
        #endregion

        Xunit.Assert.Equal("value1", values["key1"]);
    }

    #region SerializingDatesInJson
    public class LogEntry
    {
        public string Details { get; set; }
        public DateTime LogDate { get; set; }
    }

    [Fact]
    public void WriteJsonDates()
    {
        var entry = new LogEntry
        {
            LogDate = new DateTime(2009, 2, 15, 0, 0, 0, DateTimeKind.Utc),
            Details = "Application started."
        };

        // default as of Json.NET 4.5
        var isoJson = JsonConvert.SerializeObject(entry);
        // {"Details":"Application started.","LogDate":"2009-02-15T00:00:00Z"}

        var microsoftDateFormatSettings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
        };
        var microsoftJson = JsonConvert.SerializeObject(entry, microsoftDateFormatSettings);
        // {"Details":"Application started.","LogDate":"\/Date(1234656000000)\/"}

        var javascriptJson = JsonConvert.SerializeObject(entry, new JavaScriptDateTimeConverter());
        // {"Details":"Application started.","LogDate":new Date(1234656000000)}
    }
    #endregion

    #region ReducingSerializedJsonSizeOptOut
    public class Car
    {
        // included in JSON
        public string Model { get; set; }
        public DateTime Year { get; set; }
        public List<string> Features { get; set; }

        // ignored
        [JsonIgnore]
        public DateTime LastModified { get; set; }
    }
    #endregion

    #region ReducingSerializedJsonSizeOptIn
    [DataContract]
    public class Computer
    {
        // included in JSON
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public decimal SalePrice { get; set; }

        // ignored
        public string Manufacture { get; set; }
        public int StockCount { get; set; }
        public decimal WholeSalePrice { get; set; }
        public DateTime NextShipmentDate { get; set; }
    }
    #endregion

    #region ReducingSerializedJsonSizeNullValueHandlingObject
    public class Movie
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Classification { get; set; }
        public string Studio { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<string> ReleaseCountries { get; set; }
    }
    #endregion

    [Fact]
    public void ReducingSerializedJsonSizeNullValueHandlingExample()
    {
        #region ReducingSerializedJsonSizeNullValueHandlingExample
        var movie = new Movie
        {
            Name = "Bad Boys III",
            Description = "It's no Bad Boys"
        };

        var included = JsonConvert.SerializeObject(movie,
            Formatting.Indented,
            new JsonSerializerSettings { });

        // {
        //   "Name": "Bad Boys III",
        //   "Description": "It's no Bad Boys",
        //   "Classification": null,
        //   "Studio": null,
        //   "ReleaseDate": null,
        //   "ReleaseCountries": null
        // }

        var ignored = JsonConvert.SerializeObject(movie,
            Formatting.Indented,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        // {
        //   "Name": "Bad Boys III",
        //   "Description": "It's no Bad Boys"
        // }
        #endregion

        StringAssert.AreEqual(@"{
  ""Name"": ""Bad Boys III"",
  ""Description"": ""It's no Bad Boys"",
  ""Classification"": null,
  ""Studio"": null,
  ""ReleaseDate"": null,
  ""ReleaseCountries"": null
}", included);

        StringAssert.AreEqual(@"{
  ""Name"": ""Bad Boys III"",
  ""Description"": ""It's no Bad Boys""
}", ignored);
    }

    #region ReducingSerializedJsonSizeDefaultValueHandlingObject
    public class Invoice
    {
        public string Company { get; set; }
        public decimal Amount { get; set; }

        // false is default value of bool
        public bool Paid { get; set; }
        // null is default value of nullable
        public DateTime? PaidDate { get; set; }

        // customize default values
        [DefaultValue(30)]
        public int FollowUpDays { get; set; }

        [DefaultValue("")]
        public string FollowUpEmailAddress { get; set; }
    }
    #endregion

    [Fact]
    public void ReducingSerializedJsonSizeDefaultValueHandlingExample()
    {
        #region ReducingSerializedJsonSizeDefaultValueHandlingExample
        var invoice = new Invoice
        {
            Company = "Acme Ltd.",
            Amount = 50.0m,
            Paid = false,
            FollowUpDays = 30,
            FollowUpEmailAddress = string.Empty,
            PaidDate = null
        };

        var included = JsonConvert.SerializeObject(invoice,
            Formatting.Indented,
            new JsonSerializerSettings { });

        // {
        //   "Company": "Acme Ltd.",
        //   "Amount": 50.0,
        //   "Paid": false,
        //   "PaidDate": null,
        //   "FollowUpDays": 30,
        //   "FollowUpEmailAddress": ""
        // }

        var ignored = JsonConvert.SerializeObject(invoice,
            Formatting.Indented,
            new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

        // {
        //   "Company": "Acme Ltd.",
        //   "Amount": 50.0
        // }
        #endregion

        StringAssert.AreEqual(@"{
  ""Company"": ""Acme Ltd."",
  ""Amount"": 50.0,
  ""Paid"": false,
  ""PaidDate"": null,
  ""FollowUpDays"": 30,
  ""FollowUpEmailAddress"": """"
}", included);

        StringAssert.AreEqual(@"{
  ""Company"": ""Acme Ltd."",
  ""Amount"": 50.0
}", ignored);
    }

    #region ReducingSerializedJsonSizeContractResolverObject
    public class DynamicContractResolver : DefaultContractResolver
    {
        readonly char _startingWithChar;

        public DynamicContractResolver(char startingWithChar)
        {
            _startingWithChar = startingWithChar;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            // only serializer properties that start with the specified character
            properties =
                properties.Where(p => p.PropertyName.StartsWith(_startingWithChar.ToString())).ToList();

            return properties;
        }
    }

    public class Book
    {
        public string BookName { get; set; }
        public decimal BookPrice { get; set; }
        public string AuthorName { get; set; }
        public int AuthorAge { get; set; }
        public string AuthorCountry { get; set; }
    }
    #endregion

    [Fact]
    public void ReducingSerializedJsonSizeContractResolverExample()
    {
        #region ReducingSerializedJsonSizeContractResolverExample
        var book = new Book
        {
            BookName = "The Gathering Storm",
            BookPrice = 16.19m,
            AuthorName = "Brandon Sanderson",
            AuthorAge = 34,
            AuthorCountry = "United States of America"
        };

        var startingWithA = JsonConvert.SerializeObject(book, Formatting.Indented,
            new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('A') });

        // {
        //   "AuthorName": "Brandon Sanderson",
        //   "AuthorAge": 34,
        //   "AuthorCountry": "United States of America"
        // }

        var startingWithB = JsonConvert.SerializeObject(book, Formatting.Indented,
            new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('B') });

        // {
        //   "BookName": "The Gathering Storm",
        //   "BookPrice": 16.19
        // }
        #endregion

        StringAssert.AreEqual(@"{
  ""AuthorName"": ""Brandon Sanderson"",
  ""AuthorAge"": 34,
  ""AuthorCountry"": ""United States of America""
}", startingWithA);

        StringAssert.AreEqual(@"{
  ""BookName"": ""The Gathering Storm"",
  ""BookPrice"": 16.19
}", startingWithB);
    }

    #region SerializingPartialJsonFragmentsObject
    public class SearchResult
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
    }
    #endregion

    [Fact]
    public void SerializingPartialJsonFragmentsExample()
    {
        #region SerializingPartialJsonFragmentsExample
        var googleSearchText = @"{
              'responseData': {
                'results': [
                  {
                    'GsearchResultClass': 'GwebSearch',
                    'unescapedUrl': 'http://en.wikipedia.org/wiki/Paris_Hilton',
                    'url': 'http://en.wikipedia.org/wiki/Paris_Hilton',
                    'visibleUrl': 'en.wikipedia.org',
                    'cacheUrl': 'http://www.google.com/search?q=cache:TwrPfhd22hYJ:en.wikipedia.org',
                    'title': '<b>Paris Hilton</b> - Wikipedia, the free encyclopedia',
                    'titleNoFormatting': 'Paris Hilton - Wikipedia, the free encyclopedia',
                    'content': '[1] In 2006, she released her debut album...'
                  },
                  {
                    'GsearchResultClass': 'GwebSearch',
                    'unescapedUrl': 'http://www.imdb.com/name/nm0385296/',
                    'url': 'http://www.imdb.com/name/nm0385296/',
                    'visibleUrl': 'www.imdb.com',
                    'cacheUrl': 'http://www.google.com/search?q=cache:1i34KkqnsooJ:www.imdb.com',
                    'title': '<b>Paris Hilton</b>',
                    'titleNoFormatting': 'Paris Hilton',
                    'content': 'Self: Zoolander. Socialite <b>Paris Hilton</b>...'
                  }
                ],
                'cursor': {
                  'pages': [
                    {
                      'start': '0',
                      'label': 1
                    },
                    {
                      'start': '4',
                      'label': 2
                    },
                    {
                      'start': '8',
                      'label': 3
                    },
                    {
                      'start': '12',
                      'label': 4
                    }
                  ],
                  'estimatedResultCount': '59600000',
                  'currentPageIndex': 0,
                  'moreResultsUrl': 'http://www.google.com/search?oe=utf8&ie=utf8...'
                }
              },
              'responseDetails': null,
              'responseStatus': 200
            }";

        var googleSearch = JObject.Parse(googleSearchText);

        // get JSON result objects into a list
        IList<JToken> results = googleSearch["responseData"]["results"].Children().ToList();

        // serialize JSON results into .NET objects
        IList<SearchResult> searchResults = new List<SearchResult>();
        foreach (var result in results)
        {
            // JToken.ToObject is a helper method that uses JsonSerializer internally
            var searchResult = result.ToObject<SearchResult>();
            searchResults.Add(searchResult);
        }

        // Title = <b>Paris Hilton</b> - Wikipedia, the free encyclopedia
        // Content = [1] In 2006, she released her debut album...
        // Url = http://en.wikipedia.org/wiki/Paris_Hilton

        // Title = <b>Paris Hilton</b>
        // Content = Self: Zoolander. Socialite <b>Paris Hilton</b>...
        // Url = http://www.imdb.com/name/nm0385296/
        #endregion

        Xunit.Assert.Equal("<b>Paris Hilton</b> - Wikipedia, the free encyclopedia", searchResults[0].Title);
    }

    [Fact]
    public void SerializeMultidimensionalArrayExample()
    {
        var famousCouples = new[,]
        {
            { "Adam", "Eve" },
            { "Bonnie", "Clyde" },
            { "Donald", "Daisy" },
            { "Han", "Leia" }
        };

        var json = JsonConvert.SerializeObject(famousCouples, Formatting.Indented);
        // [
        //   ["Adam", "Eve"],
        //   ["Bonnie", "Clyde"],
        //   ["Donald", "Daisy"],
        //   ["Han", "Leia"]
        // ]

        var deserialized = JsonConvert.DeserializeObject<string[,]>(json);

        Console.WriteLine(deserialized[3, 0] + ", " + deserialized[3, 1]);
        // Han, Leia

        Xunit.Assert.Equal("Han", deserialized[3, 0]);
    }
}