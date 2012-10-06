﻿/* Copyright 2010-2012 10gen Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MongoDB.DriverUnitTests.Jira.CSharp265
{
    [TestFixture]
    public class CSharp265Tests
    {
        public class GDA
        {
            public int Id;
            [BsonRepresentation(BsonType.Array)]
            public Dictionary<string, int> Data;
        }

        public class GDD
        {
            public int Id;
            [BsonRepresentation(BsonType.Document)]
            public Dictionary<string, int> Data;
        }

        public class GDX
        {
            public int Id;
            public Dictionary<string, int> Data;
        }

        public class HA
        {
            public int Id;
            [BsonRepresentation(BsonType.Array)]
            public Hashtable Data;
        }

        public class HD
        {
            public int Id;
            [BsonRepresentation(BsonType.Document)]
            public Hashtable Data;
        }

        public class HX
        {
            public int Id;
            public Hashtable Data;
        }

        private MongoServer _server;
        private MongoDatabase _database;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            _server = Configuration.TestServer;
            _database = Configuration.TestDatabase;
            Configuration.TestCollection.Drop();
        }

        [Test]
        public void TestGenericDictionaryArrayRepresentationWithDollar()
        {
            var collection = Configuration.GetTestCollection<GDA>();
            var d = new GDA { Id = 1, Data = new Dictionary<string, int> { { "$a", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : [['$a', 1]] }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            collection.RemoveAll();
            collection.Insert(d);
            var r = collection.FindOne(Query.EQ("_id", d.Id));
            Assert.AreEqual(d.Id, r.Id);
            Assert.AreEqual(1, r.Data.Count);
            Assert.AreEqual(1, r.Data["$a"]);
        }

        [Test]
        public void TestGenericDictionaryArrayRepresentationWithDot()
        {
            var collection = Configuration.GetTestCollection<GDA>();
            var d = new GDA { Id = 1, Data = new Dictionary<string, int> { { "a.b", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : [['a.b', 1]] }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            collection.RemoveAll();
            collection.Insert(d);
            var r = collection.FindOne(Query.EQ("_id", d.Id));
            Assert.AreEqual(d.Id, r.Id);
            Assert.AreEqual(1, r.Data.Count);
            Assert.AreEqual(1, r.Data["a.b"]);
        }

        [Test]
        public void TestGenericDictionaryDocumentRepresentationWithDollar()
        {
            var collection = Configuration.GetTestCollection<GDD>();
            var d = new GDD { Id = 1, Data = new Dictionary<string, int> { { "$a", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : { '$a' : 1 } }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            Assert.Throws<BsonSerializationException>(() => { collection.Insert(d); });
        }

        [Test]
        public void TestGenericDictionaryDocumentRepresentationWithDot()
        {
            var collection = Configuration.GetTestCollection<GDD>();
            var d = new GDD { Id = 1, Data = new Dictionary<string, int> { { "a.b", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : { 'a.b' : 1 } }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            Assert.Throws<BsonSerializationException>(() => { collection.Insert(d); });
        }

        [Test]
        public void TestGenericDictionaryDynamicRepresentationNormal()
        {
            var collection = Configuration.GetTestCollection<GDX>();
            var d = new GDX { Id = 1, Data = new Dictionary<string, int> { { "abc", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : { 'abc' : 1 } }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            collection.RemoveAll();
            collection.Insert(d);
            var r = collection.FindOne(Query.EQ("_id", d.Id));
            Assert.AreEqual(d.Id, r.Id);
            Assert.AreEqual(1, r.Data.Count);
            Assert.AreEqual(1, r.Data["abc"]);
        }

        [Test]
        public void TestGenericDictionaryDynamicRepresentationWithDollar()
        {
            var collection = Configuration.GetTestCollection<GDX>();
            var d = new GDX { Id = 1, Data = new Dictionary<string, int> { { "$a", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : [['$a', 1]] }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            collection.RemoveAll();
            collection.Insert(d);
            var r = collection.FindOne(Query.EQ("_id", d.Id));
            Assert.AreEqual(d.Id, r.Id);
            Assert.AreEqual(1, r.Data.Count);
            Assert.AreEqual(1, r.Data["$a"]);
        }

        [Test]
        public void TestGenericDictionaryDynamicRepresentationWithDot()
        {
            var collection = Configuration.GetTestCollection<GDX>();
            var d = new GDX { Id = 1, Data = new Dictionary<string, int> { { "a.b", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : [['a.b', 1]] }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            collection.RemoveAll();
            collection.Insert(d);
            var r = collection.FindOne(Query.EQ("_id", d.Id));
            Assert.AreEqual(d.Id, r.Id);
            Assert.AreEqual(1, r.Data.Count);
            Assert.AreEqual(1, r.Data["a.b"]);
        }

        [Test]
        public void TestHashtableArrayRepresentationWithDollar()
        {
            var collection = Configuration.GetTestCollection<HA>();
            var d = new HA { Id = 1, Data = new Hashtable { { "$a", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : [['$a', 1]] }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            collection.RemoveAll();
            collection.Insert(d);
            var r = collection.FindOne(Query.EQ("_id", d.Id));
            Assert.AreEqual(d.Id, r.Id);
            Assert.AreEqual(1, r.Data.Count);
            Assert.AreEqual(1, r.Data["$a"]);
        }

        [Test]
        public void TestHashtableArrayRepresentationWithDot()
        {
            var collection = Configuration.GetTestCollection<HA>();
            var d = new HA { Id = 1, Data = new Hashtable { { "a.b", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : [['a.b', 1]] }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            collection.RemoveAll();
            collection.Insert(d);
            var r = collection.FindOne(Query.EQ("_id", d.Id));
            Assert.AreEqual(d.Id, r.Id);
            Assert.AreEqual(1, r.Data.Count);
            Assert.AreEqual(1, r.Data["a.b"]);
        }

        [Test]
        public void TestHashtableDocumentRepresentationWithDollar()
        {
            var collection = Configuration.GetTestCollection<HD>();
            var d = new HD { Id = 1, Data = new Hashtable { { "$a", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : { '$a' : 1 } }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            Assert.Throws<BsonSerializationException>(() => { collection.Insert(d); });
        }

        [Test]
        public void TestHashtableDocumentRepresentationWithDot()
        {
            var collection = Configuration.GetTestCollection<HD>();
            var d = new HD { Id = 1, Data = new Hashtable { { "a.b", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : { 'a.b' : 1 } }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            Assert.Throws<BsonSerializationException>(() => { collection.Insert(d); });
        }

        [Test]
        public void TestHashtableDynamicRepresentationNormal()
        {
            var collection = Configuration.GetTestCollection<HX>();
            var d = new HX { Id = 1, Data = new Hashtable { { "abc", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : { 'abc' : 1 } }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            collection.RemoveAll();
            collection.Insert(d);
            var r = collection.FindOne(Query.EQ("_id", d.Id));
            Assert.AreEqual(d.Id, r.Id);
            Assert.AreEqual(1, r.Data.Count);
            Assert.AreEqual(1, r.Data["abc"]);
        }

        [Test]
        public void TestHashtableDynamicRepresentationWithDollar()
        {
            var collection = Configuration.GetTestCollection<HX>();
            var d = new HX { Id = 1, Data = new Hashtable { { "$a", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : [['$a', 1]] }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            collection.RemoveAll();
            collection.Insert(d);
            var r = collection.FindOne(Query.EQ("_id", d.Id));
            Assert.AreEqual(d.Id, r.Id);
            Assert.AreEqual(1, r.Data.Count);
            Assert.AreEqual(1, r.Data["$a"]);
        }

        [Test]
        public void TestHashtableDynamicRepresentationWithDot()
        {
            var collection = Configuration.GetTestCollection<HX>();
            var d = new HX { Id = 1, Data = new Hashtable { { "a.b", 1 } } };
            var expected = "{ '_id' : 1, 'Data' : [['a.b', 1]] }".Replace("'", "\"");
            var json = d.ToJson();
            Assert.AreEqual(expected, json);

            collection.RemoveAll();
            collection.Insert(d);
            var r = collection.FindOne(Query.EQ("_id", d.Id));
            Assert.AreEqual(d.Id, r.Id);
            Assert.AreEqual(1, r.Data.Count);
            Assert.AreEqual(1, r.Data["a.b"]);
        }
    }
}
