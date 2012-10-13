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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDB.DriverUnitTests.Linq
{
    [TestFixture]
    public class SkipAndTakeTests
    {
        private class C
        {
            public ObjectId Id { get; set; }
            [BsonElement("x")]
            public int? X { get; set; }
        }

        [Test]
        public void TestSkip()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var s = new List<string> { "one", "two", "three" };

                var list = s.Take(3).Take(5).ToList();

                var query = collection.AsQueryable<C>().Skip(5);

                var selectQuery = (SelectQuery)MongoQueryTranslator.Translate(query);
                Assert.AreEqual(5, selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);
            }
        }

        [Test]
        public void TestSkipThenSkip()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<C>().Skip(5).Skip(15);

                var selectQuery = (SelectQuery)MongoQueryTranslator.Translate(query);
                Assert.AreEqual(20, selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);
            }
        }

        [Test]
        public void TestSkipThenTake()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<C>().Skip(5).Take(20);

                var selectQuery = (SelectQuery)MongoQueryTranslator.Translate(query);
                Assert.AreEqual(5, selectQuery.Skip);
                Assert.AreEqual(20, selectQuery.Take);
            }
        }

        [Test]
        public void TestSkipThenTakeThenSkip()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<C>().Skip(5).Take(20).Skip(10);

                var selectQuery = (SelectQuery)MongoQueryTranslator.Translate(query);
                Assert.AreEqual(15, selectQuery.Skip);
                Assert.AreEqual(10, selectQuery.Take);
            }
        }

        [Test]
        public void TestSkipThenTakeThenSkipWithTooMany()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<C>().Skip(5).Take(20).Skip(30);

                var selectQuery = (SelectQuery)MongoQueryTranslator.Translate(query);
                Assert.IsNull(selectQuery.Skip);
                Assert.AreEqual(0, selectQuery.Take);
            }
        }

        [Test]
        public void TestSkipThenWhereThenTake()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<C>().Skip(20).Where(c => c.X == 10).Take(30);

                Assert.Throws(typeof(MongoQueryException), () => MongoQueryTranslator.Translate(query));
            }
        }

        [Test]
        public void TestTake()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<C>().Take(5);

                var selectQuery = (SelectQuery)MongoQueryTranslator.Translate(query);
                Assert.IsNull(selectQuery.Skip);
                Assert.AreEqual(5, selectQuery.Take);
            }
        }

        [Test]
        public void TestTakeThenSkip()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<C>().Take(20).Skip(10);

                var selectQuery = (SelectQuery)MongoQueryTranslator.Translate(query);
                Assert.AreEqual(10, selectQuery.Skip);
                Assert.AreEqual(10, selectQuery.Take);
            }
        }

        [Test]
        public void TestTakeThenSkipThenTake()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<C>().Take(20).Skip(10).Take(5);

                var selectQuery = (SelectQuery)MongoQueryTranslator.Translate(query);
                Assert.AreEqual(10, selectQuery.Skip);
                Assert.AreEqual(5, selectQuery.Take);
            }
        }

        [Test]
        public void TestTakeThenSkipThenTakeWithTooMany()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<C>().Take(20).Skip(10).Take(15);

                var selectQuery = (SelectQuery)MongoQueryTranslator.Translate(query);
                Assert.AreEqual(10, selectQuery.Skip);
                Assert.AreEqual(10, selectQuery.Take);
            }
        }

        [Test]
        public void TestTakeThenTake()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<C>().Take(20).Take(5);

                var selectQuery = (SelectQuery)MongoQueryTranslator.Translate(query);
                Assert.IsNull(selectQuery.Skip);
                Assert.AreEqual(5, selectQuery.Take);
            }
        }

        [Test]
        public void TestTakeThenWhereThenSkip()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<C>().Take(20).Where(c => c.X == 10).Skip(30);

                Assert.Throws(typeof(MongoQueryException), () => MongoQueryTranslator.Translate(query));
            }
        }

        [Test]
        public void TestWhereThenSkipThenTake()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<C>().Where(c => c.X == 10).Skip(10).Take(5);

                var selectQuery = (SelectQuery)MongoQueryTranslator.Translate(query);
                Assert.AreEqual(10, selectQuery.Skip);
                Assert.AreEqual(5, selectQuery.Take);
            }
        }

        [Test]
        public void Test0Take()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<C>().Take(0).ToList();
                Assert.AreEqual(0, query.Count);
            }
        }

        [Test]
        public void TestOfTypeCWith0Take()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<Uri>().OfType<C>().Take(0).ToList();
                Assert.AreEqual(0, query.Count);
            }
        }
    }
}