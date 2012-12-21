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

using MongoDB.Driver.Internal;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a binding to a source of connections.
    /// </summary>
    public class MongoConnectionBinding : IMongoBinding
    {
        // private fields
        private readonly MongoInternalConnection _internalConnection;

        // constructors
        public MongoConnectionBinding(MongoInternalConnection internalConnection)
        {
            _internalConnection = internalConnection;
        }

        // public properties
        // properties
        /// <summary>
        /// Gets the server this binding is bound to.
        /// </summary>
        public MongoServer Server
        {
            get { return _internalConnection.ServerInstance.Server; }
        }

        /// <summary>
        /// Gets the server instance this binding is bound to (returns null if not bound to a server instance).
        /// </summary>
        public MongoServerInstance ServerInstance
        {
            get { return _internalConnection.ServerInstance; }
        }

        // internal properties
        internal MongoInternalConnection InternalConnection
        {
            get { return _internalConnection; }
        }

        // public methods
        public MongoConnection AcquireConnection(MongoDatabase database, ReadPreference readPreference)
        {
            EnsureReadPreferenceIsCompatible(readPreference);
            return new MongoConnection(this, _internalConnection);
        }

        public void ReleaseConnection(MongoConnection connection)
        {
            // do nothing (we don't own the internal connection)
        }

        // private methods
        private void EnsureReadPreferenceIsCompatible(ReadPreference readPreference)
        {
            // TODO: implement EnsureReadPreferenceIsCompatible
        }
    }
}
