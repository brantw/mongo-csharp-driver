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

namespace MongoDB.DriverUnitTests.Jira.CSharp538
{
    [TestFixture]
    public class CSharp538Tests
    {
        [BsonKnownTypes(typeof(B))]
        public abstract class A
        {

        }

        public class B : A
        {

        }

        [Test]
        public void Test()
        {
            using (var session = Configuration.TestServer.GetSession())
            {
                var db = session.GetDatabase(Configuration.TestDatabaseName);
                var collection = db.GetCollection<A>("csharp_538");

                var count = collection.AsQueryable().OfType<B>().Count();
                Assert.AreEqual(0, count);
            }
        }
    }
}