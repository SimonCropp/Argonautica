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

#if !NET5_0_OR_GREATER
using System.Data;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Serialization;

public class EntitiesSerializationTests : TestFixtureBase
{
    [Fact]
    public void SerializeEntity()
    {
        var rootFolder = CreateEntitiesTestData();

        var json = JsonConvert.SerializeObject(rootFolder, Formatting.Indented, new IsoDateTimeConverter());

        var expected = @"{
  ""$id"": ""1"",
  ""FolderId"": ""a4e8ba80-eb24-4591-bb1c-62d3ad83701e"",
  ""Name"": ""Root folder"",
  ""Description"": ""Description!"",
  ""CreatedDate"": ""2000-12-10T10:50:00Z"",
  ""Files"": [],
  ""ChildFolders"": [
    {
      ""$id"": ""2"",
      ""FolderId"": ""484936e2-7cbb-4592-93ff-b2103e5705e4"",
      ""Name"": ""Child folder"",
      ""Description"": ""Description!"",
      ""CreatedDate"": ""2001-11-20T10:50:00Z"",
      ""Files"": [
        {
          ""$id"": ""3"",
          ""FileId"": ""cc76d734-49f1-4616-bb38-41514228ac6c"",
          ""Name"": ""File 1"",
          ""Description"": ""Description!"",
          ""CreatedDate"": ""2002-10-30T10:50:00Z"",
          ""Folder"": {
            ""$ref"": ""2""
          },
          ""EntityKey"": {
            ""$id"": ""4"",
            ""EntitySetName"": ""File"",
            ""EntityContainerName"": ""DataServicesTestDatabaseEntities"",
            ""EntityKeyValues"": [
              {
                ""Key"": ""FileId"",
                ""Type"": ""System.Guid"",
                ""Value"": ""cc76d734-49f1-4616-bb38-41514228ac6c""
              }
            ]
          }
        }
      ],
      ""ChildFolders"": [],
      ""ParentFolder"": {
        ""$ref"": ""1""
      },
      ""EntityKey"": {
        ""$id"": ""5"",
        ""EntitySetName"": ""Folder"",
        ""EntityContainerName"": ""DataServicesTestDatabaseEntities"",
        ""EntityKeyValues"": [
          {
            ""Key"": ""FolderId"",
            ""Type"": ""System.Guid"",
            ""Value"": ""484936e2-7cbb-4592-93ff-b2103e5705e4""
          }
        ]
      }
    }
  ],
  ""ParentFolder"": null,
  ""EntityKey"": {
    ""$id"": ""6"",
    ""EntitySetName"": ""Folder"",
    ""EntityContainerName"": ""DataServicesTestDatabaseEntities"",
    ""EntityKeyValues"": [
      {
        ""Key"": ""FolderId"",
        ""Type"": ""System.Guid"",
        ""Value"": ""a4e8ba80-eb24-4591-bb1c-62d3ad83701e""
      }
    ]
  }
}";

        StringAssert.AreEqual(expected, json);
    }

    [Fact]
    public void SerializeEntityCamelCase()
    {
        var rootFolder = CreateEntitiesTestData();

        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = {new IsoDateTimeConverter()}
        };

        var json = JsonConvert.SerializeObject(rootFolder, settings);

        Console.WriteLine(json);

        var expected = @"{
  ""$id"": ""1"",
  ""folderId"": ""a4e8ba80-eb24-4591-bb1c-62d3ad83701e"",
  ""name"": ""Root folder"",
  ""description"": ""Description!"",
  ""createdDate"": ""2000-12-10T10:50:00Z"",
  ""files"": [],
  ""childFolders"": [
    {
      ""$id"": ""2"",
      ""folderId"": ""484936e2-7cbb-4592-93ff-b2103e5705e4"",
      ""name"": ""Child folder"",
      ""description"": ""Description!"",
      ""createdDate"": ""2001-11-20T10:50:00Z"",
      ""files"": [
        {
          ""$id"": ""3"",
          ""fileId"": ""cc76d734-49f1-4616-bb38-41514228ac6c"",
          ""name"": ""File 1"",
          ""description"": ""Description!"",
          ""createdDate"": ""2002-10-30T10:50:00Z"",
          ""folder"": {
            ""$ref"": ""2""
          },
          ""entityKey"": {
            ""$id"": ""4"",
            ""entitySetName"": ""File"",
            ""entityContainerName"": ""DataServicesTestDatabaseEntities"",
            ""entityKeyValues"": [
              {
                ""key"": ""FileId"",
                ""type"": ""System.Guid"",
                ""value"": ""cc76d734-49f1-4616-bb38-41514228ac6c""
              }
            ]
          }
        }
      ],
      ""childFolders"": [],
      ""parentFolder"": {
        ""$ref"": ""1""
      },
      ""entityKey"": {
        ""$id"": ""5"",
        ""entitySetName"": ""Folder"",
        ""entityContainerName"": ""DataServicesTestDatabaseEntities"",
        ""entityKeyValues"": [
          {
            ""key"": ""FolderId"",
            ""type"": ""System.Guid"",
            ""value"": ""484936e2-7cbb-4592-93ff-b2103e5705e4""
          }
        ]
      }
    }
  ],
  ""parentFolder"": null,
  ""entityKey"": {
    ""$id"": ""6"",
    ""entitySetName"": ""Folder"",
    ""entityContainerName"": ""DataServicesTestDatabaseEntities"",
    ""entityKeyValues"": [
      {
        ""key"": ""FolderId"",
        ""type"": ""System.Guid"",
        ""value"": ""a4e8ba80-eb24-4591-bb1c-62d3ad83701e""
      }
    ]
  }
}";

        StringAssert.AreEqual(expected, json);
    }

    [Fact]
    public void DeserializeEntity()
    {
        var json = @"{
  ""$id"": ""1"",
  ""FolderId"": ""a4e8ba80-eb24-4591-bb1c-62d3ad83701e"",
  ""Name"": ""Root folder"",
  ""Description"": ""Description!"",
  ""CreatedDate"": ""2000-12-10T10:50:00Z"",
  ""Files"": [],
  ""ChildFolders"": [
    {
      ""$id"": ""2"",
      ""FolderId"": ""484936e2-7cbb-4592-93ff-b2103e5705e4"",
      ""Name"": ""Child folder"",
      ""Description"": ""Description!"",
      ""CreatedDate"": ""2001-11-20T10:50:00Z"",
      ""Files"": [
        {
          ""$id"": ""3"",
          ""FileId"": ""cc76d734-49f1-4616-bb38-41514228ac6c"",
          ""Name"": ""File 1"",
          ""Description"": ""Description!"",
          ""CreatedDate"": ""2002-10-30T10:50:00Z"",
          ""Folder"": {
            ""$ref"": ""2""
          },
          ""EntityKey"": {
            ""$id"": ""4"",
            ""EntitySetName"": ""File"",
            ""EntityContainerName"": ""DataServicesTestDatabaseEntities"",
            ""EntityKeyValues"": [
              {
                ""Key"": ""FileId"",
                ""Type"": ""System.Guid"",
                ""Value"": ""cc76d734-49f1-4616-bb38-41514228ac6c""
              }
            ]
          }
        }
      ],
      ""ChildFolders"": [],
      ""ParentFolder"": {
        ""$ref"": ""1""
      },
      ""EntityKey"": {
        ""$id"": ""5"",
        ""EntitySetName"": ""Folder"",
        ""EntityContainerName"": ""DataServicesTestDatabaseEntities"",
        ""EntityKeyValues"": [
          {
            ""Key"": ""FolderId"",
            ""Type"": ""System.Guid"",
            ""Value"": ""484936e2-7cbb-4592-93ff-b2103e5705e4""
          }
        ]
      }
    }
  ],
  ""ParentFolder"": null,
  ""EntityKey"": {
    ""$id"": ""6"",
    ""EntitySetName"": ""Folder"",
    ""EntityContainerName"": ""DataServicesTestDatabaseEntities"",
    ""EntityKeyValues"": [
      {
        ""Key"": ""FolderId"",
        ""Type"": ""System.Guid"",
        ""Value"": ""a4e8ba80-eb24-4591-bb1c-62d3ad83701e""
      }
    ]
  }
}";

        var f = JsonConvert.DeserializeObject<Folder>(json, new IsoDateTimeConverter());

        Xunit.Assert.NotNull(f);
        Xunit.Assert.Equal(new Guid("A4E8BA80-EB24-4591-BB1C-62D3AD83701E"), f.FolderId);
        Xunit.Assert.Equal("Folder", f.EntityKey.EntitySetName);
        Xunit.Assert.Equal("DataServicesTestDatabaseEntities", f.EntityKey.EntityContainerName);
        Xunit.Assert.Equal("Folder", f.EntityKey.EntitySetName);
        Assert.False( f.EntityKey.IsTemporary);
        Xunit.Assert.Equal(1, f.EntityKey.EntityKeyValues.Length);
        Xunit.Assert.Equal("FolderId", f.EntityKey.EntityKeyValues[0].Key);
        Xunit.Assert.Equal(new Guid("A4E8BA80-EB24-4591-BB1C-62D3AD83701E"), f.EntityKey.EntityKeyValues[0].Value);
        Xunit.Assert.Equal("Root folder", f.Name);
        Xunit.Assert.Equal(new DateTime(2000, 12, 10, 10, 50, 0, DateTimeKind.Utc), f.CreatedDate);
        Xunit.Assert.Equal(null, f.ParentFolder);
        Xunit.Assert.Equal(1, f.ChildFolders.Count);

        var childFolder = f.ChildFolders.ElementAt(0);

        Xunit.Assert.Equal("Child folder", childFolder.Name);
        Xunit.Assert.Equal("Description!", childFolder.Description);
        Xunit.Assert.Equal(f, childFolder.ParentFolder);
        Xunit.Assert.Equal(f, childFolder.ParentFolderReference.Value);
        // is this a problem?
        Xunit.Assert.Equal(null, childFolder.ParentFolderReference.EntityKey);
    }

    [Fact]
    public void SerializeMultiValueEntityKey()
    {
        var e = new EntityKey("DataServicesTestDatabaseEntities.Folder",
            new List<EntityKeyMember>
            {
                new("GuidId", new Guid("A4E8BA80-EB24-4591-BB1C-62D3AD83701E")),
                new("IntId", int.MaxValue),
                new("LongId", long.MaxValue),
                new("StringId", "String!"),
                new("DateTimeId", new DateTime(2000, 12, 10, 10, 50, 0, DateTimeKind.Utc))
            });

        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        var json = JsonConvert.SerializeObject(e, settings);

        StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""entitySetName"": ""Folder"",
  ""entityContainerName"": ""DataServicesTestDatabaseEntities"",
  ""entityKeyValues"": [
    {
      ""key"": ""GuidId"",
      ""type"": ""System.Guid"",
      ""value"": ""a4e8ba80-eb24-4591-bb1c-62d3ad83701e""
    },
    {
      ""key"": ""IntId"",
      ""type"": ""System.Int32"",
      ""value"": ""2147483647""
    },
    {
      ""key"": ""LongId"",
      ""type"": ""System.Int64"",
      ""value"": ""9223372036854775807""
    },
    {
      ""key"": ""StringId"",
      ""type"": ""System.String"",
      ""value"": ""String!""
    },
    {
      ""key"": ""DateTimeId"",
      ""type"": ""System.DateTime"",
      ""value"": ""12/10/2000 10:50:00""
    }
  ]
}", json);

        var newKey = JsonConvert.DeserializeObject<EntityKey>(json);
        Xunit.Assert.False(ReferenceEquals(e, newKey));

        Xunit.Assert.Equal(5, newKey.EntityKeyValues.Length);
        Xunit.Assert.Equal("GuidId", newKey.EntityKeyValues[0].Key);
        Xunit.Assert.Equal(new Guid("A4E8BA80-EB24-4591-BB1C-62D3AD83701E"), newKey.EntityKeyValues[0].Value);
        Xunit.Assert.Equal("IntId", newKey.EntityKeyValues[1].Key);
        Xunit.Assert.Equal(int.MaxValue, newKey.EntityKeyValues[1].Value);
        Xunit.Assert.Equal("LongId", newKey.EntityKeyValues[2].Key);
        Xunit.Assert.Equal(long.MaxValue, newKey.EntityKeyValues[2].Value);
        Xunit.Assert.Equal("StringId", newKey.EntityKeyValues[3].Key);
        Xunit.Assert.Equal("String!", newKey.EntityKeyValues[3].Value);
        Xunit.Assert.Equal("DateTimeId", newKey.EntityKeyValues[4].Key);
        Xunit.Assert.Equal(new DateTime(2000, 12, 10, 10, 50, 0, DateTimeKind.Utc), newKey.EntityKeyValues[4].Value);
    }

    [Fact]
    public void SerializeMultiValueEntityKeyCameCase()
    {
        var e = new EntityKey("DataServicesTestDatabaseEntities.Folder",
            new List<EntityKeyMember>
            {
                new("GuidId", new Guid("A4E8BA80-EB24-4591-BB1C-62D3AD83701E")),
                new("IntId", int.MaxValue),
                new("LongId", long.MaxValue),
                new("StringId", "String!"),
                new("DateTimeId", new DateTime(2000, 12, 10, 10, 50, 0, DateTimeKind.Utc))
            });

        var json = JsonConvert.SerializeObject(e, Formatting.Indented);

        StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""EntitySetName"": ""Folder"",
  ""EntityContainerName"": ""DataServicesTestDatabaseEntities"",
  ""EntityKeyValues"": [
    {
      ""Key"": ""GuidId"",
      ""Type"": ""System.Guid"",
      ""Value"": ""a4e8ba80-eb24-4591-bb1c-62d3ad83701e""
    },
    {
      ""Key"": ""IntId"",
      ""Type"": ""System.Int32"",
      ""Value"": ""2147483647""
    },
    {
      ""Key"": ""LongId"",
      ""Type"": ""System.Int64"",
      ""Value"": ""9223372036854775807""
    },
    {
      ""Key"": ""StringId"",
      ""Type"": ""System.String"",
      ""Value"": ""String!""
    },
    {
      ""Key"": ""DateTimeId"",
      ""Type"": ""System.DateTime"",
      ""Value"": ""12/10/2000 10:50:00""
    }
  ]
}", json);

        var newKey = JsonConvert.DeserializeObject<EntityKey>(json);
        Xunit.Assert.False(ReferenceEquals(e, newKey));

        Xunit.Assert.Equal(5, newKey.EntityKeyValues.Length);
        Xunit.Assert.Equal("GuidId", newKey.EntityKeyValues[0].Key);
        Xunit.Assert.Equal(new Guid("A4E8BA80-EB24-4591-BB1C-62D3AD83701E"), newKey.EntityKeyValues[0].Value);
        Xunit.Assert.Equal("IntId", newKey.EntityKeyValues[1].Key);
        Xunit.Assert.Equal(int.MaxValue, newKey.EntityKeyValues[1].Value);
        Xunit.Assert.Equal("LongId", newKey.EntityKeyValues[2].Key);
        Xunit.Assert.Equal(long.MaxValue, newKey.EntityKeyValues[2].Value);
        Xunit.Assert.Equal("StringId", newKey.EntityKeyValues[3].Key);
        Xunit.Assert.Equal("String!", newKey.EntityKeyValues[3].Value);
        Xunit.Assert.Equal("DateTimeId", newKey.EntityKeyValues[4].Key);
        Xunit.Assert.Equal(new DateTime(2000, 12, 10, 10, 50, 0, DateTimeKind.Utc), newKey.EntityKeyValues[4].Value);
    }

    Folder CreateEntitiesTestData()
    {
        var folder = new Folder
        {
            FolderId = new Guid("A4E8BA80-EB24-4591-BB1C-62D3AD83701E")
        };
        folder.EntityKey = new EntityKey("DataServicesTestDatabaseEntities.Folder", "FolderId", folder.FolderId);
        folder.Name = "Root folder";
        folder.Description = "Description!";
        folder.CreatedDate = new DateTime(2000, 12, 10, 10, 50, 0, DateTimeKind.Utc);

        var childFolder = new Folder
        {
            FolderId = new Guid("484936E2-7CBB-4592-93FF-B2103E5705E4")
        };
        childFolder.EntityKey = new EntityKey("DataServicesTestDatabaseEntities.Folder", "FolderId", childFolder.FolderId);
        childFolder.Name = "Child folder";
        childFolder.Description = "Description!";
        childFolder.CreatedDate = new DateTime(2001, 11, 20, 10, 50, 0, DateTimeKind.Utc);

        folder.ChildFolders.Add(childFolder);

        var file1 = new File
        {
            FileId = new Guid("CC76D734-49F1-4616-BB38-41514228AC6C")
        };
        file1.EntityKey = new EntityKey("DataServicesTestDatabaseEntities.File", "FileId", file1.FileId);
        file1.Name = "File 1";
        file1.Description = "Description!";
        file1.CreatedDate = new DateTime(2002, 10, 30, 10, 50, 0, DateTimeKind.Utc);

        childFolder.Files.Add(file1);
        return folder;
    }
}

#endif