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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Diagnostics;
using MongoDB.Driver.Core.Operations.Serializers;
using MongoDB.Driver.Core.Support;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Operation to execute an aggregation framework command.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class AggregateOperation<TDocument> : CommandOperationBase<ICursor<TDocument>>, IEnumerable<TDocument>
    {
        // private fields
        private int _batchSize;
        private CollectionNamespace _collection;
        private BsonDocument[] _pipeline;
        private ReadPreference _readPreference;
        private IBsonSerializer<TDocument> _documentSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateOperation{TDocument}" /> class.
        /// </summary>
        public AggregateOperation()
        {
            _readPreference = ReadPreference.Primary;
        }

        // public properties
        /// <summary>
        /// Gets or sets the size of the batch.
        /// </summary>
        public int BatchSize
        {
            get { return _batchSize; }
            set { _batchSize = value; }
        }

        /// <summary>
        /// Gets or sets the collection.
        /// </summary>
        public CollectionNamespace Collection
        {
            get { return _collection; }
            set { _collection = value; }
        }

        /// <summary>
        /// Gets or sets the document serializer.
        /// </summary>
        public IBsonSerializer<TDocument> DocumentSerializer
        {
            get { return _documentSerializer; }
            set { _documentSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the pipeline.
        /// </summary>
        public BsonDocument[] Pipeline
        {
            get { return _pipeline; }
            set { _pipeline = value; }
        }

        /// <summary>
        /// Gets or sets the read preference.
        /// </summary>
        public ReadPreference ReadPreference
        {
            get { return _readPreference; }
            set { _readPreference = value; }
        }

        // public methods
        /// <summary>
        /// Executes the specified operation behavior.
        /// </summary>
        /// <returns></returns>
        public override ICursor<TDocument> Execute()
        {
            EnsureRequiredProperties();

            var aggregateCommand = new BsonDocument
            {
                { "aggregate", _collection.CollectionName },
                { "pipeline", new BsonArray(_pipeline.Select(op => op.ToBsonDocument(op.GetType()))) }
            };

            // don't dispose of channelProvider. The cursor will do that for us.
            var activity = __trace.TraceActivity("AggregationOperation");
            var channelProvider = CreateServerChannelProvider(new ReadPreferenceServerSelector(_readPreference), true);
            try
            {
                if (__trace.Switch.ShouldTrace(TraceEventType.Verbose))
                {
                    __trace.TraceVerbose("aggregating on collection {0} with pipeline {1} at {2}.", _collection.FullName, aggregateCommand.ToJson(), channelProvider.Server.DnsEndPoint);
                }
                if (channelProvider.Server.BuildInfo.Version >= new Version(2, 5, 1))
                {
                    aggregateCommand["cursor"] = new BsonDocument();
                    if (_batchSize > 0)
                    {
                        aggregateCommand["cursor"]["batchSize"] = _batchSize;
                    }
                }

                var aggregateCommandResultSerializer = new AggregateCommandResultSerializer<TDocument>(_documentSerializer);

                var args = new ExecuteCommandProtocolArgs<AggregateCommandResult<TDocument>>
                {
                    Command = aggregateCommand,
                    CommandResultSerializer = aggregateCommandResultSerializer,
                    Database = new DatabaseNamespace(_collection.DatabaseName),
                    ReadPreference = _readPreference
                };

                var result = ExecuteCommandProtocol(channelProvider, args);

                return new Cursor<TDocument>(
                    activity,
                    channelProvider: channelProvider,
                    cursorId: result.CursorId,
                    collection: _collection,
                    limit: 0,
                    numberToReturn: _batchSize,
                    firstBatch: result.FirstBatch,
                    serializer: _documentSerializer,
                    timeout: Timeout,
                    cancellationToken: CancellationToken,
                    readerSettings: GetServerAdjustedReaderSettings(channelProvider.Server));
            }
            catch
            {
                channelProvider.Dispose();
                activity.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Gets the enumerator. This implicitly calls Execute.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<TDocument> GetEnumerator()
        {
            return Execute();
        }

        // explicit interface implementations
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // protected methods
        /// <summary>
        /// Validates the required properties.
        /// </summary>
        protected override void EnsureRequiredProperties()
        {
            base.EnsureRequiredProperties();
            Ensure.IsNotNull("Collection", _collection);
            Ensure.IsGreaterThanOrEqualTo("BatchSize", _batchSize, 0);
            Ensure.IsNotNull("Pipeline", _pipeline);
            Ensure.IsNotNull("ReadPreference", _readPreference);
            if (_documentSerializer == null)
            {
                _documentSerializer = BsonSerializer.LookupSerializer<TDocument>();
            }
        }
    }
}