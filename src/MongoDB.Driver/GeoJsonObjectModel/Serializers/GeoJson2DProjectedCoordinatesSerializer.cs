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

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel.Serializers
{
    /// <summary>
    /// Represents a serializer for a GeoJson2DProjectedCoordinates value.
    /// </summary>
    public class GeoJson2DProjectedCoordinatesSerializer : BsonBaseSerializer<GeoJson2DProjectedCoordinates>
    {
        // private static fields
        private static readonly IBsonSerializer<double> __doubleSerializer = new DoubleSerializer();

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <returns>The value.</returns>
        public override GeoJson2DProjectedCoordinates Deserialize(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;

            if (bsonReader.GetCurrentBsonType() == BsonType.Null)
            {
                bsonReader.ReadNull();
                return null;
            }
            else
            {
                bsonReader.ReadStartArray();
                var easting = context.DeserializeWithChildContext(__doubleSerializer);
                var northing = context.DeserializeWithChildContext(__doubleSerializer);
                bsonReader.ReadEndArray();

                return new GeoJson2DProjectedCoordinates(easting, northing);
            }
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The value.</param>
        public override void Serialize(BsonSerializationContext context, GeoJson2DProjectedCoordinates value)
        {
            var bsonWriter = context.Writer;

            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                bsonWriter.WriteStartArray();
                bsonWriter.WriteDouble(value.Easting);
                bsonWriter.WriteDouble(value.Northing);
                bsonWriter.WriteEndArray();
            }
        }
    }
}
