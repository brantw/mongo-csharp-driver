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
using System.Collections;
using System.IO;
using System.Text;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BitArrays.
    /// </summary>
    public class BitArraySerializer : SealedClassSerializerBase<BitArray>, IRepresentationConfigurable<BitArraySerializer>
    {
        // private fields
        private readonly BsonType _representation;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BitArraySerializer"/> class.
        /// </summary>
        public BitArraySerializer()
            : this(BsonType.Binary)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BitArraySerializer"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        public BitArraySerializer(BsonType representation)
        {
            switch (representation)
            {
                case BsonType.Binary:
                case BsonType.String:
                    break;

                default:
                    var message = string.Format("{0} is not a valid representation for a BitArraySerializer.", representation);
                    throw new ArgumentException(message);
            }

            _representation = representation;
        }

        // public properties
        /// <summary>
        /// Gets the representation.
        /// </summary>
        /// <value>
        /// The representation.
        /// </value>
        public BsonType Representation
        {
            get { return _representation; }
        }

        // public methods
#pragma warning disable 618 // about obsolete BsonBinarySubType.OldBinary
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <returns>An object.</returns>
        protected override BitArray DeserializeValue(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;
            BitArray bitArray;

            BsonType bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Null:
                    bsonReader.ReadNull();
                    return null;

                case BsonType.Binary:
                    return new BitArray(bsonReader.ReadBytes());

                case BsonType.Document:
                    bsonReader.ReadStartDocument();
                    var length = bsonReader.ReadInt32("Length");
                    var bytes = bsonReader.ReadBytes("Bytes");
                    bsonReader.ReadEndDocument();
                    bitArray = new BitArray(bytes);
                    bitArray.Length = length;
                    return bitArray;

                case BsonType.String:
                    var s = bsonReader.ReadString();
                    bitArray = new BitArray(s.Length);
                    for (int i = 0; i < s.Length; i++)
                    {
                        var c = s[i];
                        switch (c)
                        {
                            case '0':
                                break;
                            case '1':
                                bitArray[i] = true;
                                break;
                            default:
                                throw new FileFormatException("String value is not a valid BitArray.");
                        }
                    }
                    return bitArray;

                default:
                    throw CreateCannotDeserializeFromBsonTypeException(bsonType);
            }
        }
#pragma warning restore 618

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, BitArray value)
        {
            var bsonWriter = context.Writer;

            switch (_representation)
            {
                case BsonType.Binary:
                    if ((value.Length % 8) == 0)
                    {
                        bsonWriter.WriteBytes(GetBytes(value));
                    }
                    else
                    {
                        bsonWriter.WriteStartDocument();
                        bsonWriter.WriteInt32("Length", value.Length);
                        bsonWriter.WriteBytes("Bytes", GetBytes(value));
                        bsonWriter.WriteEndDocument();
                    }
                    break;

                case BsonType.String:
                    var sb = new StringBuilder(value.Length);
                    for (int i = 0; i < value.Length; i++)
                    {
                        sb.Append(value[i] ? '1' : '0');
                    }
                    bsonWriter.WriteString(sb.ToString());
                    break;

                default:
                    var message = string.Format("'{0}' is not a valid BitArray representation.", _representation);
                    throw new BsonSerializationException(message);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified representation.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        public BitArraySerializer WithRepresentation(BsonType representation)
        {
            if (representation == _representation)
            {
                return this;
            }
            else
            {
                return new BitArraySerializer(representation);
            }
        }

        // private methods
        private byte[] GetBytes(BitArray bitArray)
        {
            // TODO: is there a more efficient way to do this?
            var bytes = new byte[(bitArray.Length + 7) / 8];
            var i = 0;
            foreach (bool value in bitArray)
            {
                if (value)
                {
                    var index = i / 8;
                    var bit = i % 8;
                    bytes[index] |= (byte)(1 << bit);
                }
                i++;
            }
            return bytes;
        }

        // explicit interface implementations
        IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
        {
            return WithRepresentation(representation);
        }
    }
}
