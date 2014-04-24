/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NUnit.Framework;

namespace MongoDB.DriverUnitTests.Jira
{
    [TestFixture]
    public class CSharp908Tests
    {
        private class C
        {
            public int Id { get; set; }
            [BsonRepresentation(BsonType.Int32, AllowOverflow = true)]
            public uint N { get; set; }
        }

        [Test]
        public void TestIncUnsigned(
            [Values(-2, -1, 0, 1, 2)] int delta)
        {
            var value = (uint)(int.MaxValue + delta);
            var expected = value + 1;

            var collection = Configuration.GetTestCollection<C>();
            collection.Drop();
            collection.Insert(new C { Id = 1, N = value });

            var query = Query.EQ("_id", 1);
            var update = Update<C>.Inc(c => c.N, 1);
            var result = collection.Update(query, update);

            var document = collection.FindOne(query);
            Assert.AreEqual(expected, document.N);
        }
    }
}