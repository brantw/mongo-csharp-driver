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
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MongoDB.DriverUnitTests.Jira.CSharp215
{
    [TestFixture]
    public class CSharp215Tests
    {
        public class C
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id;
            public int X;
        }

        [Test]
        public void TestSave()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<C>(Configuration.TestCollectionName);

                collection.RemoveAll();

                var doc = new C { X = 1 };
                collection.Save(doc);
                var id = doc.Id;

                Assert.AreEqual(1, collection.Count());
                var fetched = collection.FindOne();
                Assert.AreEqual(id, fetched.Id);
                Assert.AreEqual(1, fetched.X);

                doc.X = 2;
                collection.Save(doc);

                Assert.AreEqual(1, collection.Count());
                fetched = collection.FindOne();
                Assert.AreEqual(id, fetched.Id);
                Assert.AreEqual(2, fetched.X);
            }
        }
    }
}
