﻿/* Copyright 2010-2013 10gen Inc.
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

using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace MongoDB.DriverUnitTests.CommandResults
{
    [TestFixture]
    public class ValidateCollectionResultTests
    {
        [Test]
        public void Test()
        {
            var node = Configuration.TestServer.GetNode(ReadPreference.Primary);
            if (node.InstanceType != MongoServerInstanceType.ShardRouter)
            {
                var database = node.GetDatabase(Configuration.TestDatabaseName);
                var collection = database.GetCollection<BsonDocument>(Configuration.TestCollectionName);

                // make sure collection exists and has exactly one document
                collection.RemoveAll();
                collection.Insert(new BsonDocument());

                var result = collection.Validate();
                Assert.IsTrue(result.Ok);
                Assert.AreEqual(collection.FullName, result.Namespace);
            }
        }
    }
}
