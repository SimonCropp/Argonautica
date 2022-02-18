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

namespace Argon.Tests.Documentation.Samples.Serializer;

public class DeserializeDateFormatString : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region Usage
        var json = @"[
              '7 December, 2009',
              '1 January, 2010',
              '10 February, 2010'
            ]";

        var dateList = JsonConvert.DeserializeObject<IList<DateTime>>(json, new JsonSerializerSettings
        {
            DateFormatString = "d MMMM, yyyy"
        });

        foreach (var dateTime in dateList)
        {
            Console.WriteLine(dateTime.ToLongDateString());
        }
        // Monday, 07 December 2009
        // Friday, 01 January 2010
        // Wednesday, 10 February 2010
        #endregion

        Xunit.Assert.Equal(new DateTime(2009, 12, 7, 0, 0, 0, DateTimeKind.Utc), dateList[0]);
        Xunit.Assert.Equal(new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc), dateList[1]);
        Xunit.Assert.Equal(new DateTime(2010, 2, 10, 0, 0, 0, DateTimeKind.Utc), dateList[2]);
    }
}