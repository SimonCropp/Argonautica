// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Collections.ObjectModel;

namespace TestObjects;

public class VersionKeyedCollection :
    KeyedCollection<string, Person>,
    IEnumerable<Person>,
    IJsonOnError
{
    public List<string> Messages { get; set; }

    public VersionKeyedCollection() =>
        Messages = new();

    protected override string GetKeyForItem(Person item) =>
        item.Name;

    public void OnError(object originalObject, object member, string path, Exception error, Action markAsHanded)
    {
        Messages.Add($"{path} - Error message for member {member} = {error.Message}");
        markAsHanded();
    }

    IEnumerator<Person> IEnumerable<Person>.GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
        {
            if (i % 2 == 0)
            {
                throw new($"Index even: {i}");
            }

            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        ((IEnumerable<Person>)this).GetEnumerator();
}