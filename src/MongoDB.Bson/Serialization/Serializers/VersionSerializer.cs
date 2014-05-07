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

using System;
using System.IO;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Versions.
    /// </summary>
    public class VersionSerializer : SealedClassSerializerBase<Version>, IRepresentationConfigurable<VersionSerializer>
    {
        // private static fields
        private static readonly ClassDefinition<Version> __classDefinition;

        // private fields
        private readonly BsonType _representation;

        // static constructor
        static VersionSerializer()
        {
            var int32Serializer = new Int32Serializer();
            __classDefinition = new ClassDefinition<Version, int, int, int, int>
            (
                new MemberDefinition<int>(0, "Major", int32Serializer),
                new MemberDefinition<int>(1, "Minor", int32Serializer),
                new MemberDefinition<int>(2, "Build", int32Serializer, isRequired: false),
                new MemberDefinition<int>(3, "Revision", int32Serializer, isRequired: false),
                (major, minor, build, revision, missingMemberFlags) =>
                {
                    switch (missingMemberFlags)
                    {
                        case 0x00: return new Version(major, minor, build, revision);
                        case 0x08: return new Version(major, minor, build);
                        case 0x0c: return new Version(major, minor);
                        default: throw new BsonInternalException(); // should never happen
                    }
                }
            );
        }

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionSerializer"/> class.
        /// </summary>
        public VersionSerializer()
            : this(BsonType.String)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionSerializer"/> class.
        /// </summary>
        /// <param name="representation">The representation.</param>
        public VersionSerializer(BsonType representation)
        {
            switch (representation)
            {
                case BsonType.Document:
                case BsonType.String:
                    break;

                default:
                    var message = string.Format("{0} is not a valid representation for a VersionSerializer.", representation);
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
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <returns>An object.</returns>
        protected override Version DeserializeValue(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;

            BsonType bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Document:
                    return __classDefinition.Deserialize(context);

                case BsonType.String:
                    return new Version(bsonReader.ReadString());

                default:
                    throw CreateCannotDeserializeFromBsonTypeException(bsonType);
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The object.</param>
        protected override void SerializeValue(BsonSerializationContext context, Version value)
        {
            var bsonWriter = context.Writer;

            switch (_representation)
            {
                case BsonType.Document:
                    bsonWriter.WriteStartDocument();
                    bsonWriter.WriteInt32("Major", value.Major);
                    bsonWriter.WriteInt32("Minor", value.Minor);
                    if (value.Build != -1)
                    {
                        bsonWriter.WriteInt32("Build", value.Build);
                        if (value.Revision != -1)
                        {
                            bsonWriter.WriteInt32("Revision", value.Revision);
                        }
                    }
                    bsonWriter.WriteEndDocument();
                    break;

                case BsonType.String:
                    bsonWriter.WriteString(value.ToString());
                    break;

                default:
                    var message = string.Format("'{0}' is not a valid Version representation.", _representation);
                    throw new BsonSerializationException(message);
            }
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified representation.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        public VersionSerializer WithRepresentation(BsonType representation)
        {
            if (representation == _representation)
            {
                return this;
            }
            else
            {
                return new VersionSerializer(representation);
            }
        }

        // explicit interface implementations
        IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
        {
            return WithRepresentation(representation);
        }
    }
}
