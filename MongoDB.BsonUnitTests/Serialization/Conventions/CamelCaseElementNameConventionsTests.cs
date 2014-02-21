/* Copyright 2010-2014 MongoDB Inc.
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

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using NUnit.Framework;

namespace MongoDB.BsonUnitTests.Serialization.Conventions
{
    [TestFixture]
    public class CamelCaseElementNameConventionsTests
    {
        private class TestClass
        {
            public string FirstName { get; set; }
            public int Age { get; set; }
            public string _DumbName { get; set; }
            public string lowerCase { get; set; }
            public int X { get; set; }
            public int XValue { get; set; }
            public int XY { get; set; }
            public int XYValue { get; set; }
            public int IOStatus { get; set; }
            public int TCPStatus { get; set; }
            public int HTMLStatus { get; set; }
            public int TCPIOStatus { get; set; }
        }

        [Test]
        [TestCase("FirstName", "firstName")]
        [TestCase("Age", "age")]
        [TestCase("_DumbName", "_DumbName")]
        [TestCase("lowerCase", "lowerCase")]
        [TestCase("X", "x")]
        [TestCase("XValue", "xValue")]
        [TestCase("XY", "xY")]
        [TestCase("XYValue", "xYValue")]
        [TestCase("IOStatus", "iOStatus")]
        [TestCase("TCPStatus", "tCPStatus")]
        [TestCase("HTMLStatus", "hTMLStatus")]
        [TestCase("TCPIOStatus", "tCPIOStatus")]
        public void TestCamelCaseElementNameConvention(string memberName, string expected)
        {
            var convention = new CamelCaseElementNameConvention();
            var classMap = new BsonClassMap<TestClass>();
            var memberMap = classMap.MapProperty(memberName);
            convention.Apply(memberMap);
            Assert.AreEqual(expected, memberMap.ElementName);
        }

        [Test]
        [TestCase("FirstName", "firstName")]
        [TestCase("Age", "age")]
        [TestCase("_DumbName", "_DumbName")]
        [TestCase("lowerCase", "lowerCase")]
        [TestCase("X", "x")]
        [TestCase("XValue", "xValue")]
        [TestCase("XY", "xy")]
        [TestCase("XYValue", "xyValue")]
        [TestCase("IOStatus", "ioStatus")]
        [TestCase("TCPStatus", "tcpStatus")]
        [TestCase("HTMLStatus", "htmlStatus")]
        [TestCase("TCPIOStatus", "tcpioStatus")]
        public void TestCamelCaseElementNameConventionHandlingPrefixes(string memberName, string expected)
        {
            var convention = new CamelCaseElementNameConvention(true);
            var classMap = new BsonClassMap<TestClass>();
            var memberMap = classMap.MapProperty(memberName);
            convention.Apply(memberMap);
            Assert.AreEqual(expected, memberMap.ElementName);
        }
    }
}
