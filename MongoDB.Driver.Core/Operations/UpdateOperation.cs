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
    /// Represents an Update operation.
    /// </summary>
    public class UpdateOperation : WriteOperation<WriteConcernResult>
    {
        // private fields
        private bool _checkUpdateDocument;
        private UpdateFlags _flags;
        private object _query;
        private object _update;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateOperation" /> class.
        /// </summary>
        public UpdateOperation()
        {
            _checkUpdateDocument = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateOperation" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public UpdateOperation(ISession session)
            : this()
        {
            Session = session;
        }

        // public properties
        /// <summary>
        /// Gets or sets a value indicating whether to check the update document.  What does this mean???
        /// </summary>
        public bool CheckUpdateDocument
        {
            get { return _checkUpdateDocument; }
            set { _checkUpdateDocument = value; }
        }

        /// <summary>
        /// Gets or sets the update flags.
        /// </summary>
        public UpdateFlags Flags
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

        /// <summary>
        /// Gets or sets the update object.
        /// </summary>
        public object Update
        {
            get { return _update; }
            set { _update = value; }
        }

        // public methods
        /// <summary>
        /// Executes the Update operation.
        /// </summary>
        /// <param name="operationBehavior">The operation behavior.</param>
        /// <returns>A WriteConcern result (or null if WriteConcern was not enabled).</returns>
        public override WriteConcernResult Execute(OperationBehavior operationBehavior)
        {
            ValidateRequiredProperties();

            using (var channelProvider = CreateServerChannelProvider(PrimaryServerSelector.Instance, false, operationBehavior))
            using (var channel = channelProvider.GetChannel(Timeout, CancellationToken))
            {
                var readerSettings = GetServerAdjustedReaderSettings(channelProvider.Server);
                var writerSettings = GetServerAdjustedWriterSettings(channelProvider.Server);

                var updateMessage = new UpdateMessage(
                    Collection,
                    _query,
                    _update,
                    _flags,
                    _checkUpdateDocument,
                    writerSettings);

                SendPacketWithWriteConcernResult sendMessageResult;
                using (var packet = new BufferedRequestPacket())
                {
                    packet.AddMessage(updateMessage);
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
            Ensure.IsNotNull("Update", _update);
        }
    }
}