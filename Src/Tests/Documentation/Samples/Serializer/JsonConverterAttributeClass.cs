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

namespace Argon.Tests.Documentation.Samples.Serializer;

public class JsonConverterAttributeClass : TestFixtureBase
{
    #region Types
    public class UserConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var user = (User)value;

            writer.WriteValue(user.UserName);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var user = new User
            {
                UserName = (string)reader.Value
            };

            return user;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(User);
        }
    }

    [JsonConverter(typeof(UserConverter))]
    public class User
    {
        public string UserName { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region Usage
        var user = new User
        {
            UserName = @"domain\username"
        };

        var json = JsonConvert.SerializeObject(user, Formatting.Indented);

        Console.WriteLine(json);
        // "domain\\username"
        #endregion

        Assert.AreEqual(@"""domain\\username""", json);
    }
}