﻿/* Copyright 2010-2014 MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using NUnit.Framework;

namespace MongoDB.Bson.Tests.Serialization.Conventions
{
    [TestFixture]
    public class ExtraElementsMemberConventionsTests
    {
        private class TestClassA
        {
            public Guid Id { get; set; }
            public BsonDocument ExtraElements { get; set; }
        }

        private class TestClassB
        {
            public Guid Id { get; set; }
        }

        [Test]
        public void TestNamedExtraElementsMemberConventionWithTestClassA()
        {
            var convention = new NamedExtraElementsMemberConvention("ExtraElements");
            var classMap = new BsonClassMap<TestClassA>();
            convention.Apply(classMap);
            Assert.IsNotNull(classMap.ExtraElementsMemberMap);
            Assert.AreEqual("ExtraElements", classMap.ExtraElementsMemberMap.MemberName);
        }

        [Test]
        public void TestNamedExtraElementsMemberConventionWithTestClassB()
        {
            var convention = new NamedExtraElementsMemberConvention("ExtraElements");
            var classMap = new BsonClassMap<TestClassB>();
            convention.Apply(classMap);
            Assert.IsNull(classMap.ExtraElementsMemberMap);
        }
    }
}
