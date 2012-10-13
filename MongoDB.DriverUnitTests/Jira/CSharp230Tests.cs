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

namespace MongoDB.DriverUnitTests.Jira.CSharp230
{
    [TestFixture]
    public class CSharp230Tests
    {
        [Test]
        public void TestEnsureIndexAfterDropCollection()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var database = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection(Configuration.TestCollectionName);

                if (collection.Exists())
                {
                    collection.Drop();
                }
                session.Server.ResetIndexCache();

                Assert.IsFalse(collection.IndexExists("x"));
                collection.EnsureIndex("x");
                Assert.IsTrue(collection.IndexExists("x"));

                collection.Drop();
                Assert.IsFalse(collection.IndexExists("x"));
                collection.EnsureIndex("x");
                Assert.IsTrue(collection.IndexExists("x"));
            }
        }
    }
}
