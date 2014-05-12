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
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel.Serializers
{
    /// <summary>
    /// Represents a serializer for a GeoJsonMultiPoint value.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    public class GeoJsonMultiPointSerializer<TCoordinates> : ClassSerializerBase<GeoJsonMultiPoint<TCoordinates>> where TCoordinates : GeoJsonCoordinates
    {
        // protected methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <returns>The value.</returns>
        protected override GeoJsonMultiPoint<TCoordinates> DeserializeValue(BsonDeserializationContext context)
        {
            var helper = new Helper();
            return (GeoJsonMultiPoint<TCoordinates>)helper.DeserializeValue(context);
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The value.</param>
        protected override void SerializeValue(BsonSerializationContext context, GeoJsonMultiPoint<TCoordinates> value)
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
                public const long Coordinates = BaseMaxValue << 1;
            }

            // private fields
            private readonly IBsonSerializer<GeoJsonMultiPointCoordinates<TCoordinates>> _coordinatesSerializer = BsonSerializer.LookupSerializer<GeoJsonMultiPointCoordinates<TCoordinates>>();
            private GeoJsonMultiPointCoordinates<TCoordinates> _coordinates;

            // constructors
            public Helper()
                : base(typeof(GeoJsonMultiPoint<TCoordinates>), "MultiPoint", new GeoJsonObjectArgs<TCoordinates>(), CreateMembers())
            {
            }

            // public properties
            public GeoJsonMultiPointCoordinates<TCoordinates> Coordinates
            {
                get { return _coordinates; }
                set { _coordinates = value; }
            }

            // private static methods
            private static IEnumerable<SerializerHelper.Member> CreateMembers()
            {
                return new[]
                {
                    new SerializerHelper.Member("coordinates", Flags.Coordinates)
                };
            }

            // protected methods
            protected override GeoJsonObject<TCoordinates> CreateObject()
            {
                return new GeoJsonMultiPoint<TCoordinates>(Args, _coordinates);
            }

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
                    case Flags.Coordinates: _coordinates = DeserializeCoordinates(context); break;
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
                var multiPoint = (GeoJsonMultiPoint<TCoordinates>)obj;
                SerializeCoordinates(context, multiPoint.Coordinates);
            }

            // private methods
            private GeoJsonMultiPointCoordinates<TCoordinates> DeserializeCoordinates(BsonDeserializationContext context)
            {
                return context.DeserializeWithChildContext(_coordinatesSerializer);
            }

            private void SerializeCoordinates(BsonSerializationContext context, GeoJsonMultiPointCoordinates<TCoordinates> coordinates)
            {
                context.Writer.WriteName("coordinates");
                context.SerializeWithChildContext(_coordinatesSerializer, coordinates);
            }
        }
    }
}
