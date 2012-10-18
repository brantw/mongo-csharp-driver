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
    /// Represents a serializer for BsonValues.
    /// </summary>
    public class BsonValueSerializer : BsonBaseSerializer
    {
        // private static fields
        private static BsonValueSerializer __instance = new BsonValueSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonValueSerializer class.
        /// </summary>
        public BsonValueSerializer()
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the BsonValueSerializer class.
        /// </summary>
        public static BsonValueSerializer Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="serializationConfig">The serialization config.</param>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public override object Deserialize(
            SerializationConfig serializationConfig,
            BsonReader bsonReader,
            Type nominalType,
            Type actualType, // ignored
            IBsonSerializationOptions options)
        {
            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Array: return (BsonValue)BsonArraySerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonArray), options);
                case BsonType.Binary: return (BsonValue)BsonBinaryDataSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonBinaryData), options);
                case BsonType.Boolean: return (BsonValue)BsonBooleanSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonBoolean), options);
                case BsonType.DateTime: return (BsonValue)BsonDateTimeSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonDateTime), options);
                case BsonType.Document: return (BsonValue)BsonDocumentSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonDocument), options);
                case BsonType.Double: return (BsonValue)BsonDoubleSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonDouble), options);
                case BsonType.Int32: return (BsonValue)BsonInt32Serializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonInt32), options);
                case BsonType.Int64: return (BsonValue)BsonInt64Serializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonInt64), options);
                case BsonType.JavaScript: return (BsonValue)BsonJavaScriptSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonJavaScript), options);
                case BsonType.JavaScriptWithScope: return (BsonValue)BsonJavaScriptWithScopeSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonJavaScriptWithScope), options);
                case BsonType.MaxKey: return (BsonValue)BsonMaxKeySerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonMaxKey), options);
                case BsonType.MinKey: return (BsonValue)BsonMinKeySerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonMinKey), options);
                case BsonType.Null: return (BsonValue)BsonNullSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonNull), options);
                case BsonType.ObjectId: return (BsonValue)BsonObjectIdSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonObjectId), options);
                case BsonType.RegularExpression: return (BsonValue)BsonRegularExpressionSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonRegularExpression), options);
                case BsonType.String: return (BsonValue)BsonStringSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonString), options);
                case BsonType.Symbol: return (BsonValue)BsonSymbolSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonSymbol), options);
                case BsonType.Timestamp: return (BsonValue)BsonTimestampSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonTimestamp), options);
                case BsonType.Undefined: return (BsonValue)BsonUndefinedSerializer.Instance.Deserialize(serializationConfig, bsonReader, typeof(BsonUndefined), options);
                default:
                    var message = string.Format("Invalid BsonType {0}.", bsonType);
                    throw new BsonInternalException(message);
            }
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="serializationConfig">The serialization config.</param>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public override void Serialize(
            SerializationConfig serializationConfig,
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var bsonValue = (BsonValue)value;
            switch (bsonValue.BsonType)
            {
                case BsonType.Array: BsonArraySerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonArray), bsonValue, options); break;
                case BsonType.Binary: BsonBinaryDataSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonBinaryData), bsonValue, options); break;
                case BsonType.Boolean: BsonBooleanSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonBoolean), bsonValue, options); break;
                case BsonType.DateTime: BsonDateTimeSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonDateTime), bsonValue, options); break;
                case BsonType.Document: BsonDocumentSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonDocument), bsonValue, options); break;
                case BsonType.Double: BsonDoubleSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonDouble), bsonValue, options); break;
                case BsonType.Int32: BsonInt32Serializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonInt32), bsonValue, options); break;
                case BsonType.Int64: BsonInt64Serializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonInt64), bsonValue, options); break;
                case BsonType.JavaScript: BsonJavaScriptSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonJavaScript), bsonValue, options); break;
                case BsonType.JavaScriptWithScope: BsonJavaScriptWithScopeSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonJavaScriptWithScope), bsonValue, options); break;
                case BsonType.MaxKey: BsonMaxKeySerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonMaxKey), bsonValue, options); break;
                case BsonType.MinKey: BsonMinKeySerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonMinKey), bsonValue, options); break;
                case BsonType.Null: BsonNullSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonNull), bsonValue, options); break;
                case BsonType.ObjectId: BsonObjectIdSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonObjectId), bsonValue, options); break;
                case BsonType.RegularExpression: BsonRegularExpressionSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonRegularExpression), bsonValue, options); break;
                case BsonType.String: BsonStringSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonString), bsonValue, options); break;
                case BsonType.Symbol: BsonSymbolSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonSymbol), bsonValue, options); break;
                case BsonType.Timestamp: BsonTimestampSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonTimestamp), bsonValue, options); break;
                case BsonType.Undefined: BsonUndefinedSerializer.Instance.Serialize(serializationConfig, bsonWriter, typeof(BsonUndefined), bsonValue, options); break;
                default: throw new BsonInternalException("Invalid BsonType.");
            }
        }
    }
}
