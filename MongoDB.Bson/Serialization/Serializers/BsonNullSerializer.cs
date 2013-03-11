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
using System.IO;
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BsonNulls.
    /// </summary>
    public class BsonNullSerializer : BsonBaseSerializer
    {
        // private static fields
        private static BsonNullSerializer __instance = new BsonNullSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonNullSerializer class.
        /// </summary>
        public BsonNullSerializer()
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the BsonNullSerializer class.
        /// </summary>
        public static BsonNullSerializer Instance
        {
            get { return __instance; }
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
            VerifyTypes(nominalType, actualType, typeof(BsonNull));

            var bsonType = bsonReader.GetCurrentBsonType();
            string message;
            switch (bsonType)
            {
                case BsonType.Null:
                    bsonReader.ReadNull();
                    return BsonNull.Value;
                case BsonType.Document:
                    bsonReader.ReadStartDocument();
                    var name = bsonReader.ReadName();
                    if (name == "_csharpnull" || name == "$csharpnull")
                    {
                        var csharpNull = bsonReader.ReadBoolean();
                        bsonReader.ReadEndDocument();
                        return csharpNull ? null : BsonNull.Value;
                    }
                    else
                    {
                        message = string.Format("Unexpected element name while deserializing a BsonNull: {0}.", name);
                        throw new FileFormatException(message);
                    }
                default:
                    message = string.Format("Cannot deserialize BsonNull from BsonType {0}.", bsonType);
                    throw new FileFormatException(message);
            }
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
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteBoolean("_csharpnull", true);
                bsonWriter.WriteEndDocument();
            }
            else
            {
                bsonWriter.WriteNull();
            }
        }
    }
}
