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
using MongoDB.Driver;

namespace MongoDB.DriverUnitTests.Jira.CSharp365
{
    [TestFixture]
    public class CSharp365Tests
    {
        [Test]
        public void TestExplainWithFieldsAndCoveredIndex()
        {
            var server = Configuration.TestServer;
            using (var session = server.GetSession())
            {
                if (session.ServerInstance.BuildInfo.Version >= new Version(1, 8, 0))
                {
                    var database = session.GetDatabase(Configuration.TestDatabaseName);
                    var collection = database.GetCollection(Configuration.TestCollectionName);
                    collection.Drop();

                    collection.EnsureIndex("A", "_id");
                    collection.Insert(new BsonDocument { { "_id", 1 }, { "A", 1 } });
                    collection.Insert(new BsonDocument { { "_id", 2 }, { "A", 2 } });
                    collection.Insert(new BsonDocument { { "_id", 3 }, { "A", 3 } });

                    var query = Query.EQ("A", 1);
                    var fields = Fields.Include("_id");
                    var cursor = collection.Find(query).SetFields(fields).SetHint("A_1__id_1"); // make sure it uses the index
                    var explain = cursor.Explain();
                    Assert.IsTrue(explain["indexOnly"].ToBoolean());
                }
            }
        }
    }
}
