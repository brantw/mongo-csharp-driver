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
using System.Configuration;
using System.Linq;
using System.Text;

using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.DriverUnitTests
{
    /// <summary>
    /// A static class to handle online test configuration.
    /// </summary>
    public static class Configuration
    {
        // private static fields
        private static MongoServer __testServer;
        private static MongoDatabase __testDatabase;
        private static MongoCollectionBsonDocument __testCollection;

        // static constructor
        static Configuration()
        {
            var connectionString = Environment.GetEnvironmentVariable("CSharpDriverTestsConnectionString")
                ?? "mongodb://localhost/?safe=true";

            var mongoUrlBuilder = new MongoUrlBuilder(connectionString);
            var serverSettings = mongoUrlBuilder.ToServerSettings();
            if (!serverSettings.SafeMode.Enabled)
            {
                serverSettings.SafeMode = SafeMode.True;
            }

            __testServer = MongoServer.Create(serverSettings);
            __testDatabase = __testServer["csharpdriverunittests"];
            __testCollection = __testDatabase["testcollection"];
        }

        // public static methods
        /// <summary>
        /// Gets the test collection.
        /// </summary>
        public static MongoCollectionBsonDocument TestCollection
        {
            get { return __testCollection; }
        }

        /// <summary>
        /// Gets the test database.
        /// </summary>
        public static MongoDatabase TestDatabase
        {
            get { return __testDatabase; }
        }

        /// <summary>
        /// Gets the test server.
        /// </summary>
        public static MongoServer TestServer
        {
            get { return __testServer; }
        }

        // public static methods
        /// <summary>
        /// Gets the test collection with a document type of T.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <returns>The collection.</returns>
        public static MongoCollection<TDocument> GetTestCollection<TDocument>()
        {
            return __testDatabase.GetCollection<TDocument>(__testCollection.Name);
        }

        /// <summary>
        /// Gets the test collection with a document type of documentType.
        /// </summary>
        /// <param name="documentType">The document type..</param>
        /// <returns>The collection.</returns>
        public static MongoCollectionTyped GetTestCollection(Type documentType)
        {
            return __testDatabase.GetCollection(documentType, __testCollection.Name);
        }
    }
}
