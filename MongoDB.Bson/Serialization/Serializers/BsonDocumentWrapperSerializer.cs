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

using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BsonDocumentWrappers.
    /// </summary>
    public class BsonDocumentWrapperSerializer : BsonBaseSerializer
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDocumentWrapperSerializer class.
        /// </summary>
        public BsonDocumentWrapperSerializer(SerializationContext serializationContext)
            : base(serializationContext)
        {
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public override object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public override void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var wrapper = (BsonDocumentWrapper)value;
            if (wrapper.IsUpdateDocument)
            {
                var savedCheckElementNames = bsonWriter.CheckElementNames;
                var savedCheckUpdateDocument = bsonWriter.CheckUpdateDocument;
                try
                {
                    bsonWriter.CheckElementNames = false;
                    bsonWriter.CheckUpdateDocument = true;
                    SerializationContext.Serialize(bsonWriter, wrapper.WrappedNominalType, wrapper.WrappedObject, null); // TODO: wrap options also?
                }
                finally
                {
                    bsonWriter.CheckElementNames = savedCheckElementNames;
                    bsonWriter.CheckUpdateDocument = savedCheckUpdateDocument;
                }
            }
            else
            {
                SerializationContext.Serialize(bsonWriter, wrapper.WrappedNominalType, wrapper.WrappedObject, null); // TODO: wrap options also?
            }
        }
    }
}
