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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;

using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.DriverUnitTests.Jira.CSharp258
{
    [TestFixture]
    public class CSharp258Tests
    {
        public class C
        {
            public ObjectId Id { get; set; }
            public DateTime DateTime { get; set; }
        }

        [Test]
        public void TestDateTimePropertyWithNewMaxDateTimeRepresentation()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);
                collection.RemoveAll();
                collection.Insert(
                    new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "DateTime", new BsonDateTime(253402300799999) }
                });

                var c = collection.FindOne();
                Assert.AreEqual(DateTime.MaxValue, c.DateTime);
            }
        }

        [Test]
        public void TestDateTimePropertyWithOldMaxDateTimeRepresentation()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);
                collection.RemoveAll();
                collection.Insert(
                    new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "DateTime", new BsonDateTime(253402300800000) }
                });

                var c = collection.FindOne();
                Assert.AreEqual(DateTime.MaxValue, c.DateTime);
            }
        }

        [Test]
        public void TestDocumentWithNewMaxDateTimeRepresentation()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);
                collection.RemoveAll();
                collection.Insert(
                    new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "DateTime", new BsonDateTime(253402300799999) }
                });

                var document = collection.FindOneAs<BsonDocument>();
                Assert.AreEqual(DateTime.MaxValue, document["DateTime"].AsDateTime);
                Assert.AreEqual(253402300799999, document["DateTime"].AsBsonDateTime.MillisecondsSinceEpoch);
            }
        }

        [Test]
        public void TestDocumentWithOldMaxDateTimeRepresentation()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);
                collection.RemoveAll();
                collection.Insert(
                    new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "DateTime", new BsonDateTime(253402300800000) }
                });

                var document = collection.FindOneAs<BsonDocument>();
                Assert.AreEqual(DateTime.MaxValue, document["DateTime"].AsDateTime);
                Assert.AreEqual(253402300799999, document["DateTime"].AsBsonDateTime.MillisecondsSinceEpoch);
            }
        }
    }
}
