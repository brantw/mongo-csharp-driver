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

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NUnit.Framework;

namespace MongoDB.Bson.Tests.Jira
{
    [TestFixture]
    public class CSharp966Tests
    {
        public class C
        {
            public int Id { get; set; }
            public Dictionary<string, dynamic> Values { get; set; }
        }

        [Test]
        public void TestArray()
        {
            var c = new C
            {
                Id = 1,
                Values = new Dictionary<string, dynamic> { { "xyz", new [] { "abc", "def" } } } // Array
            };
            var json = c.ToJson();
            var expected = "{ '_id' : 1, 'Values' : { 'xyz' : { '_t' : 'System.String[]', '_v' : ['abc', 'def'] } } }".Replace("'", "\"");
            Assert.AreEqual(expected, json);

            var rehydrated = BsonSerializer.Deserialize<C>(json);
            Assert.AreEqual(1, rehydrated.Id);
            Assert.AreEqual(1, rehydrated.Values.Count);
            Assert.IsInstanceOf<string[]>(rehydrated.Values["xyz"]);
            var array = (string[])rehydrated.Values["xyz"];
            Assert.AreEqual(2, array.Length);
            Assert.AreEqual("abc", array[0]);
            Assert.AreEqual("def", array[1]);
        }

        [Test]
        public void TestArrayMissingDiscriminator()
        {
            var json = "{ '_id' : 1, 'Values' : { 'xyz' : ['abc', 'def'] } } ".Replace("'", "\"");

            var rehydrated = BsonSerializer.Deserialize<C>(json);
            Assert.AreEqual(1, rehydrated.Id);
            Assert.AreEqual(1, rehydrated.Values.Count);
            Assert.IsInstanceOf<List<object>>(rehydrated.Values["xyz"]);
            var array = (List<object>)rehydrated.Values["xyz"];
            Assert.AreEqual(2, array.Count);
            Assert.AreEqual("abc", array[0]);
            Assert.AreEqual("def", array[1]);
        }

        [Test]
        public void TestNumber()
        {
            var c = new C
            {
                Id = 1,
                Values = new Dictionary<string, dynamic> { { "xyz", 2 } } // Number
            };
            var json = c.ToJson();
            var expected = "{ '_id' : 1, 'Values' : { 'xyz' : 2 } }".Replace("'", "\"");
            Assert.AreEqual(expected, json);

            var rehydrated = BsonSerializer.Deserialize<C>(json);
            Assert.AreEqual(1, rehydrated.Id);
            Assert.AreEqual(1, rehydrated.Values.Count);
            Assert.AreEqual(2, rehydrated.Values["xyz"]);
        }
    }
}
