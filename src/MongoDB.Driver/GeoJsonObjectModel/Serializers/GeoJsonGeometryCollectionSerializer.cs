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

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel.Serializers
{
    /// <summary>
    /// Represents a serializer for a GeoJsonGeometryCollection value.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    public class GeoJsonGeometryCollectionSerializer<TCoordinates> : ClassSerializerBase<GeoJsonGeometryCollection<TCoordinates>> where TCoordinates : GeoJsonCoordinates
    {
        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <returns>The value.</returns>
        protected override GeoJsonGeometryCollection<TCoordinates> DeserializeValue(BsonDeserializationContext context)
        {
            var helper = new Helper();
            return (GeoJsonGeometryCollection<TCoordinates>)helper.DeserializeValue(context);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, GeoJsonGeometryCollection<TCoordinates> value)
        {
            var helper = new Helper();
            helper.SerializeValue(context, value);
        }

        // nested classes
        internal class Helper : GeoJsonGeometrySerializer<TCoordinates>.Helper
        {
            // internal constants
            new internal static class Flags
            {
                public const long BaseMaxValue = GeoJsonGeometrySerializer<TCoordinates>.Helper.Flags.MaxValue;
                public const long Geometries = BaseMaxValue << 1;
            }

            // private fields
            private readonly IBsonSerializer<GeoJsonGeometry<TCoordinates>> _geometrySerializer = BsonSerializer.LookupSerializer<GeoJsonGeometry<TCoordinates>>();
            private List<GeoJsonGeometry<TCoordinates>> _geometries;

            // constructors
            public Helper()
                : base(typeof(GeoJsonGeometryCollection<TCoordinates>), "GeometryCollection", new GeoJsonObjectArgs<TCoordinates>(), CreateMembers())
            {
            }

            // public properties
            public List<GeoJsonGeometry<TCoordinates>> Geometries
            {
                get { return _geometries; }
                set { _geometries = value; }
            }

            protected override GeoJsonObject<TCoordinates> CreateObject()
            {
                return new GeoJsonGeometryCollection<TCoordinates>(Args, _geometries);
            }

            // private static methods
            private static IEnumerable<SerializerHelper.Member> CreateMembers()
            {
                return new[]
                {
                    new SerializerHelper.Member("geometries", Flags.Geometries)
                };
            }

            // protected methods
            /// <summary>
            /// Deserializes a field.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="elementName">The element name.</param>
            /// <param name="flag">The member flag.</param>
            protected override void DeserializeField(BsonDeserializationContext context, string elementName, long flag)
            {
                switch (flag)
                {
                    case Flags.Geometries: _geometries = DeserializeGeometries(context); break;
                    default: base.DeserializeField(context, elementName, flag); break;
                }
            }

            /// <summary>
            /// Serializes the fields.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="obj">The GeoJson object.</param>
            protected override void SerializeFields(BsonSerializationContext context, GeoJsonObject<TCoordinates> obj)
            {
                base.SerializeFields(context, obj);
                var geometryCollection = (GeoJsonGeometryCollection<TCoordinates>)obj;
                SerializeGeometries(context, geometryCollection.Geometries);
            }

            // private methods
            private List<GeoJsonGeometry<TCoordinates>> DeserializeGeometries(BsonDeserializationContext context)
            {
                var bsonReader = context.Reader;

                bsonReader.ReadStartArray();
                var geometries = new List<GeoJsonGeometry<TCoordinates>>();
                while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    var geometry = context.DeserializeWithChildContext(_geometrySerializer);
                    geometries.Add(geometry);
                }
                bsonReader.ReadEndArray();

                return geometries;
            }

            private void SerializeGeometries(BsonSerializationContext context, IEnumerable<GeoJsonGeometry<TCoordinates>> geometries)
            {
                var bsonWriter = context.Writer;

                bsonWriter.WriteName("geometries");
                bsonWriter.WriteStartArray();
                foreach (var geometry in geometries)
                {
                    context.SerializeWithChildContext(_geometrySerializer, geometry);
                }
                bsonWriter.WriteEndArray();
            }
        }
    }
}
