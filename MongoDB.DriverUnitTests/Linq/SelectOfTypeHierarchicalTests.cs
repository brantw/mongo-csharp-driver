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
using System.Text;
using NUnit.Framework;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDB.DriverUnitTests.Linq
{
    [TestFixture]
    public class SelectOfTypeHierarchicalTests
    {
        [BsonDiscriminator(RootClass = true)]
        private class B
        {
            public ObjectId Id;
            public int b;
        }

        private class C : B
        {
            public int c;
        }

        private class D : C
        {
            public int d;
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<B>(Configuration.TestCollectionName);

                collection.Drop();
                collection.Insert(new B { Id = ObjectId.GenerateNewId(), b = 1 });
                collection.Insert(new C { Id = ObjectId.GenerateNewId(), b = 2, c = 2 });
                collection.Insert(new D { Id = ObjectId.GenerateNewId(), b = 3, c = 3, d = 3 });
            }
        }

        [Test]
        public void TestOfTypeB()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<B>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<B>().OfType<B>();

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(B), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(B x) => LinqToMongo.Inject({ \"_t\" : \"B\" })", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.AreEqual(typeof(B), selectQuery.OfType);
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"_t\" : \"B\" }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(3, Consume(query));
            }
        }

        [Test]
        public void TestOfTypeC()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<B>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<B>().OfType<C>();

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(B), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(B x) => LinqToMongo.Inject({ \"_t\" : \"C\" })", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.AreEqual(typeof(C), selectQuery.OfType);
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"_t\" : \"C\" }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(2, Consume(query));
            }
        }

        [Test]
        public void TestOfTypeCWhereCGreaterThan0()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<B>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<B>().OfType<C>().Where(c => c.c > 0);

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(B), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(C c) => (LinqToMongo.Inject({ \"_t\" : \"C\" }) && (c.c > 0))", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.AreEqual(typeof(C), selectQuery.OfType);
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"_t\" : \"C\", \"c\" : { \"$gt\" : 0 } }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(2, Consume(query));
            }
        }

        [Test]
        public void TestOfTypeD()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<B>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<B>().OfType<D>();

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(B), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(B x) => LinqToMongo.Inject({ \"_t\" : \"D\" })", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.AreEqual(typeof(D), selectQuery.OfType);
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"_t\" : \"D\" }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(1, Consume(query));
            }
        }

        [Test]
        public void TestWhereBGreaterThan0OfTypeCWhereCGreaterThan0()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<B>(Configuration.TestCollectionName);

                var query = collection.AsQueryable<B>().Where(b => b.b > 0).OfType<C>().Where(c => c.c > 0);

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(B), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(C c) => (((c.b > 0) && LinqToMongo.Inject({ \"_t\" : \"C\" })) && (c.c > 0))", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.AreEqual(typeof(C), selectQuery.OfType);
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"b\" : { \"$gt\" : 0 }, \"_t\" : \"C\", \"c\" : { \"$gt\" : 0 } }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(2, Consume(query));
            }
        }

        [Test]
        public void TestWhereBIsB()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<B>(Configuration.TestCollectionName);

                var query =
                    from b in collection.AsQueryable<B>()
                    where b is B
                    select b;

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(B), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(B b) => (b is B)", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.AreEqual(null, selectQuery.OfType); // OfType ignored because <T> was the same as <TDocument>
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"_t\" : \"B\" }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(3, Consume(query));
            }
        }

        [Test]
        public void TestWhereBIsC()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<B>(Configuration.TestCollectionName);

                var query =
                    from b in collection.AsQueryable<B>()
                    where b is C
                    select b;

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(B), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(B b) => (b is C)", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.AreEqual(null, selectQuery.OfType);
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"_t\" : \"C\" }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(2, Consume(query));
            }
        }

        [Test]
        public void TestWhereBIsD()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<B>(Configuration.TestCollectionName);

                var query =
                    from b in collection.AsQueryable<B>()
                    where b is D
                    select b;

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(B), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(B b) => (b is D)", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.AreEqual(null, selectQuery.OfType);
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"_t\" : \"D\" }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(1, Consume(query));
            }
        }

        [Test]
        public void TestWhereBTypeEqualsB()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<B>(Configuration.TestCollectionName);

                if (session.ServerInstance.BuildInfo.Version >= new Version(2, 0))
                {
                    var query =
                        from b in collection.AsQueryable<B>()
                        where b.GetType() == typeof(B)
                        select b;

                    var translatedQuery = MongoQueryTranslator.Translate(query);
                    Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                    Assert.AreSame(collection, translatedQuery.Collection);
                    Assert.AreSame(typeof(B), translatedQuery.DocumentType);

                    var selectQuery = (SelectQuery)translatedQuery;
                    Assert.AreEqual("(B b) => (b.GetType() == typeof(B))", ExpressionFormatter.ToString(selectQuery.Where));
                    Assert.AreEqual(null, selectQuery.OfType); // OfType ignored because <T> was the same as <TDocument>
                    Assert.IsNull(selectQuery.OrderBy);
                    Assert.IsNull(selectQuery.Projection);
                    Assert.IsNull(selectQuery.Skip);
                    Assert.IsNull(selectQuery.Take);

                    Assert.AreEqual("{ \"_t.0\" : { \"$exists\" : false }, \"_t\" : \"B\" }", selectQuery.BuildQuery().ToJson());
                    Assert.AreEqual(1, Consume(query));
                }
            }
        }

        [Test]
        public void TestWhereBTypeEqualsC()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<B>(Configuration.TestCollectionName);

                var query =
                     from b in collection.AsQueryable<B>()
                     where b.GetType() == typeof(C)
                     select b;

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(B), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(B b) => (b.GetType() == typeof(C))", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.AreEqual(null, selectQuery.OfType);
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"_t\" : { \"$size\" : 2 }, \"_t.0\" : \"B\", \"_t.1\" : \"C\" }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(1, Consume(query));
            }
        }

        [Test]
        public void TestWhereBTypeEqualsD()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<B>(Configuration.TestCollectionName);

                var query =
                    from b in collection.AsQueryable<B>()
                    where b.GetType() == typeof(D)
                    select b;

                var translatedQuery = MongoQueryTranslator.Translate(query);
                Assert.IsInstanceOf<SelectQuery>(translatedQuery);
                Assert.AreSame(collection, translatedQuery.Collection);
                Assert.AreSame(typeof(B), translatedQuery.DocumentType);

                var selectQuery = (SelectQuery)translatedQuery;
                Assert.AreEqual("(B b) => (b.GetType() == typeof(D))", ExpressionFormatter.ToString(selectQuery.Where));
                Assert.AreEqual(null, selectQuery.OfType);
                Assert.IsNull(selectQuery.OrderBy);
                Assert.IsNull(selectQuery.Projection);
                Assert.IsNull(selectQuery.Skip);
                Assert.IsNull(selectQuery.Take);

                Assert.AreEqual("{ \"_t\" : { \"$size\" : 3 }, \"_t.0\" : \"B\", \"_t.1\" : \"C\", \"_t.2\" : \"D\" }", selectQuery.BuildQuery().ToJson());
                Assert.AreEqual(1, Consume(query));
            }
        }

        private int Consume<T>(IQueryable<T> query)
        {
            var count = 0;
            foreach (var c in query)
            {
                count++;
            }
            return count;
        }
    }
}
