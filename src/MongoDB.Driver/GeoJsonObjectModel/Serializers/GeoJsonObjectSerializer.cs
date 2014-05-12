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
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel.Serializers
{
    /// <summary>
    /// Represents a serializer for a GeoJson object.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    public class GeoJsonObjectSerializer<TCoordinates> : ClassSerializerBase<GeoJsonObject<TCoordinates>> where TCoordinates : GeoJsonCoordinates
    {
        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <returns>The value.</returns>
        protected override GeoJsonObject<TCoordinates> DeserializeValue(BsonDeserializationContext context)
        {
            var helper = new Helper(Enumerable.Empty<SerializerHelper.Member>());
            return helper.DeserializeValue(context);
        }

        /// <summary>
        /// Gets the actual type.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The actual type.</returns>
        protected override Type GetActualType(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;
            var bookmark = bsonReader.GetBookmark();
            bsonReader.ReadStartDocument();
            if (bsonReader.FindElement("type"))
            {
                var discriminator = bsonReader.ReadString();
                bsonReader.ReturnToBookmark(bookmark);

                switch (discriminator)
                {
                    case "Feature": return typeof(GeoJsonFeature<TCoordinates>);
                    case "FeatureCollection": return typeof(GeoJsonFeatureCollection<TCoordinates>);
                    case "GeometryCollection": return typeof(GeoJsonGeometryCollection<TCoordinates>);
                    case "LineString": return typeof(GeoJsonLineString<TCoordinates>);
                    case "MultiLineString": return typeof(GeoJsonMultiLineString<TCoordinates>);
                    case "MultiPoint": return typeof(GeoJsonMultiPoint<TCoordinates>);
                    case "MultiPolygon": return typeof(GeoJsonMultiPolygon<TCoordinates>);
                    case "Point": return typeof(GeoJsonPoint<TCoordinates>);
                    case "Polygon": return typeof(GeoJsonPolygon<TCoordinates>);
                    default:
                        var message = string.Format("The type field of the GeoJsonObject is not valid: '{0}'.", discriminator);
                        throw new FormatException(message);
                }
            }
            else
            {
                throw new FormatException("GeoJsonObject is missing the type field.");
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, GeoJsonObject<TCoordinates> value)
        {
            var helper = new Helper(Enumerable.Empty<SerializerHelper.Member>());
            helper.SerializeValue(context, value);
        }

        // nested types
        /// <summary>
        /// Represents data being collected during serialization to create an instance of a GeoJsonObject.
        /// </summary>
        internal class Helper
        {
            internal static class Flags
            {
                public const long Type = 1;
                public const long BoundingBox = 2;
                public const long CoordinateReferenceSystem = 4;
                public const long ExtraMember = 8;
                public const long MaxValue = ExtraMember;
            }

            // private fields
            private readonly SerializerHelper _serializerHelper;
            private readonly Type _objectType;
            private readonly string _expectedDiscriminator;
            private readonly GeoJsonObjectArgs<TCoordinates> _args;
            private readonly IBsonSerializer<GeoJsonBoundingBox<TCoordinates>> _boundingBoxSerializer = BsonSerializer.LookupSerializer<GeoJsonBoundingBox<TCoordinates>>();
            private readonly IBsonSerializer<GeoJsonCoordinateReferenceSystem> _coordinateReferenceSystemSerializer = BsonSerializer.LookupSerializer<GeoJsonCoordinateReferenceSystem>();

            // constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="Helper" /> class.
            /// </summary>
            public Helper(IEnumerable<SerializerHelper.Member> derivedMembers)
                : this(typeof(GeoJsonObject<TCoordinates>), null, null, derivedMembers)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Helper" /> class.
            /// </summary>
            /// <param name="objectType">The object type.</param>
            /// <param name="expectedDiscriminator">The expected discriminator.</param>
            /// <param name="args">The args.</param>
            /// <param name="derivedMembers">The derived members.</param>
            protected Helper(Type objectType, string expectedDiscriminator, GeoJsonObjectArgs<TCoordinates> args, IEnumerable<SerializerHelper.Member> derivedMembers)
            {
                _objectType = objectType;
                _expectedDiscriminator = expectedDiscriminator;
                _args = args;

                var members = CreateMembers().Concat(derivedMembers).ToArray();
                ThrowIfDuplicateFlags(members);

                _serializerHelper = new SerializerHelper(members);
            }

            // public properties
            public GeoJsonObjectArgs<TCoordinates> Args
            {
                get { return _args; }
            }

            // private static methods
            private static IEnumerable<SerializerHelper.Member> CreateMembers()
            {
                return new[]
                { 
                    new SerializerHelper.Member("type", Flags.Type),
                    new SerializerHelper.Member("bbox", Flags.BoundingBox, isOptional: true),
                    new SerializerHelper.Member("crs", Flags.CoordinateReferenceSystem, isOptional: true),
                    new SerializerHelper.Member("*", Flags.ExtraMember, isOptional: true)
                };
            }

            private static void ThrowIfDuplicateFlags(ICollection<SerializerHelper.Member> members)
            {
                var distinctFlags = new HashSet<long>(members.Select(m => m.Flag));
                if (distinctFlags.Count != members.Count)
                {
                    throw new BsonInternalException("Duplicate GeoJson flags.");
                }
            }

            // public methods
            public GeoJsonObject<TCoordinates> DeserializeValue(BsonDeserializationContext context)
            {
                var bsonReader = context.Reader;

                _serializerHelper.DeserializeMembers(context, (elementName, flag) =>
                {
                    DeserializeField(context, elementName, flag);
                });

                return CreateObject();
            }

            /// <summary>
            /// Serializes a value.
            /// </summary>
            /// <param name="context">The serialization context.</param>
            /// <param name="value">The value.</param>
            public void SerializeValue(BsonSerializationContext context, GeoJsonObject<TCoordinates> value)
            {
                var bsonWriter = context.Writer;

                bsonWriter.WriteStartDocument();
                SerializeFields(context, value);
                SerializeExtraMembers(context, value.ExtraMembers);
                bsonWriter.WriteEndDocument();
            }

            // protected methods
            /// <summary>
            /// Creates the object.
            /// </summary>
            /// <returns>An instance of a GeoJsonObject.</returns>
            protected virtual GeoJsonObject<TCoordinates> CreateObject()
            {
                throw new NotSupportedException("Cannot create an abstract GeoJsonObject.");
            }

            /// <summary>
            /// Deserializes a field.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="elementName">The element name.</param>
            /// <param name="flag">The member flag.</param>
            protected virtual void DeserializeField(BsonDeserializationContext context, string elementName, long flag)
            {
                switch (flag)
                {
                    case Flags.Type: DeserializeDiscriminator(context, _expectedDiscriminator); break;
                    case Flags.BoundingBox: _args.BoundingBox = DeserializeBoundingBox(context); break;
                    case Flags.CoordinateReferenceSystem: _args.CoordinateReferenceSystem = DeserializeCoordinateReferenceSystem(context); break;
                    case Flags.ExtraMember: DeserializeExtraMember(context, elementName); break;
                }
            }

            /// <summary>
            /// Serializes the fields.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="obj">The GeoJson object.</param>
            protected virtual void SerializeFields(BsonSerializationContext context, GeoJsonObject<TCoordinates> obj)
            {
                SerializeDiscriminator(context, obj.Type);
                SerializeCoordinateReferenceSystem(context, obj.CoordinateReferenceSystem);
                SerializeBoundingBox(context, obj.BoundingBox);
            }

            // private methods
            private GeoJsonBoundingBox<TCoordinates> DeserializeBoundingBox(BsonDeserializationContext context)
            {
                return context.DeserializeWithChildContext(_boundingBoxSerializer);
            }

            private GeoJsonCoordinateReferenceSystem DeserializeCoordinateReferenceSystem(BsonDeserializationContext context)
            {
                return context.DeserializeWithChildContext(_coordinateReferenceSystemSerializer);
            }

            private void DeserializeDiscriminator(BsonDeserializationContext context, string expectedDiscriminator)
            {
                var discriminator = context.Reader.ReadString();
                if (discriminator != expectedDiscriminator)
                {
                    var message = string.Format("Type '{0}' does not match expected type '{1}'.", discriminator, expectedDiscriminator);
                    throw new FormatException(message);
                }
            }

            private void DeserializeExtraMember(BsonDeserializationContext context, string elementName)
            {
                var value = context.DeserializeWithChildContext(BsonValueSerializer.Instance);
                if (_args.ExtraMembers == null)
                {
                    _args.ExtraMembers = new BsonDocument();
                }
                _args.ExtraMembers[elementName] = value;
            }

            private void SerializeBoundingBox(BsonSerializationContext context, GeoJsonBoundingBox<TCoordinates> boundingBox)
            {
                if (boundingBox != null)
                {
                    context.Writer.WriteName("bbox");
                    context.SerializeWithChildContext(_boundingBoxSerializer, boundingBox);
                }
            }

            private void SerializeCoordinateReferenceSystem(BsonSerializationContext context, GeoJsonCoordinateReferenceSystem coordinateReferenceSystem)
            {
                if (coordinateReferenceSystem != null)
                {
                    context.Writer.WriteName("crs");
                    context.SerializeWithChildContext(_coordinateReferenceSystemSerializer, coordinateReferenceSystem);
                }
            }

            private void SerializeDiscriminator(BsonSerializationContext context, GeoJsonObjectType type)
            {
                context.Writer.WriteString("type", type.ToString());
            }

            private void SerializeExtraMembers(BsonSerializationContext context, BsonDocument extraMembers)
            {
                if (extraMembers != null)
                {
                    var bsonWriter = context.Writer;
                    foreach (var element in extraMembers)
                    {
                        bsonWriter.WriteName(element.Name);
                        context.SerializeWithChildContext(BsonValueSerializer.Instance, element.Value);
                    }
                }
            }
        }
    }
}
