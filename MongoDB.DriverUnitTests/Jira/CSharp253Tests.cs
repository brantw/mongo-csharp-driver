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

namespace MongoDB.DriverUnitTests.Jira.CSharp253
{
    [TestFixture]
    public class CSharp253Tests
    {
        public class C
        {
            public ObjectId Id { get; set; }
            public MongoDBRef DBRef { get; set; }
            public BsonNull BsonNull { get; set; }
        }

        [Test]
        public void TestInsertClass()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection(Configuration.TestCollectionName);
                var c = new C
                {
                    DBRef = new MongoDBRef("database", "collection", ObjectId.GenerateNewId()),
                    BsonNull = null
                };
                collection.Insert(c);
            }
        }

        [Test]
        public void TestInsertDollar()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection(Configuration.TestCollectionName);
                Assert.Throws<BsonSerializationException>(() => { collection.Insert(new BsonDocument("$x", 1)); });
            }

            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection(Configuration.TestCollectionName);
                Assert.Throws<BsonSerializationException>(() => { collection.Insert(new BsonDocument("x", new BsonDocument("$x", 1))); });
            }
        }

        [Test]
        public void TestInsertPeriod()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection(Configuration.TestCollectionName);
                Assert.Throws<BsonSerializationException>(() => { collection.Insert(new BsonDocument("a.b", 1)); });
            }

            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection(Configuration.TestCollectionName);
                Assert.Throws<BsonSerializationException>(() => { collection.Insert(new BsonDocument("a", new BsonDocument("b.c", 1))); });
            }
        }

        [Test]
        public void TestLegacyDollar()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection(Configuration.TestCollectionName);
                var document = new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "BsonNull", new BsonDocument("_csharpnull", true) },
                { "Code", new BsonDocument
                    {
                        { "$code", "code" },
                        { "$scope", "scope" }
                    }
                },
                { "DBRef", new BsonDocument
                    {
                        { "$db", "db" },
                        { "$id", "id" },
                        { "$ref", "ref" }
                    }
                }
            };
                collection.Insert(document);
            }
        }

        [Test]
        public void TestCreateIndexOnNestedElement()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection(Configuration.TestCollectionName);
                collection.CreateIndex("a.b");
            }
        }
    }
}
