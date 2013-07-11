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

using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Protocol;
using MongoDB.Driver.Core.Sessions;
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a Remove operation.
    /// </summary>
    public class RemoveOperation : WriteOperation<WriteConcernResult>
    {
        // private fields
        private DeleteFlags _flags;
        private object _query;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveOperation" /> class.
        /// </summary>
        public RemoveOperation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveOperation" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public RemoveOperation(ISession session)
        {
            Session = session;
        }

        // public properties
        /// <summary>
        /// Gets or sets the delete flags.
        /// </summary>
        public DeleteFlags Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        /// <summary>
        /// Gets or sets the query object.
        /// </summary>
        public object Query
        {
            get { return _query; }
            set { _query = value; }
        }

        // public methods
        /// <summary>
        /// Executes the Remove operation.
        /// </summary>
        /// <param name="operationBehavior">The operation behavior.</param>
        /// <returns>A WriteConcern result (or null if WriteConcern was not enabled).</returns>
        public override WriteConcernResult Execute(OperationBehavior operationBehavior)
        {
            ValidateRequiredProperties();

            var options = new CreateSessionChannelProviderOptions(
                serverSelector: new ReadPreferenceServerSelector(ReadPreference.Primary),
                isQuery: false)
            {
                DisposeSession = operationBehavior == OperationBehavior.CloseSession,
            };

            using(var channelProvider = Session.CreateSessionChannelProvider(options))
            using (var channel = channelProvider.GetChannel())
            {
                var readerSettings = GetServerAdjustedReaderSettings(channel.Server);
                var writerSettings = GetServerAdjustedWriterSettings(channel.Server);

                var deleteMessage = new DeleteMessage(Collection, _query, _flags, writerSettings);

                SendPacketWithWriteConcernResult sendMessageResult;
                using (var packet = new BufferedRequestPacket())
                {
                    packet.AddMessage(deleteMessage);
                    sendMessageResult = SendPacketWithWriteConcern(channel, packet, WriteConcern, writerSettings);
                }

                return ReadWriteConcernResult(channel, sendMessageResult, readerSettings);
            }
        }

        // protected methods
        /// <summary>
        /// Validates the required properties.
        /// </summary>
        protected override void ValidateRequiredProperties()
        {
            base.ValidateRequiredProperties();
            Ensure.IsNotNull("Query", _query);
        }
    }
}