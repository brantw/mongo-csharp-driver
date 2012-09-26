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
using System.Reflection;
using System.Text;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// Sets serialization options for a member of a given type.
    /// </summary>
    public class TypeRepresentationSerializationOptionsConvention : ConventionBase, IBsonMemberMapConvention
    {
        private readonly Type _type;
        private readonly BsonType _representation;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeRepresentationSerializationOptionsConvention"/> class.
        /// </summary>
        /// <param name="type">The type of the member.</param>
        /// <param name="representation">The BSON representation to use for this type.</param>
        public TypeRepresentationSerializationOptionsConvention(Type type, BsonType representation)
        {
            _type = type;
            _representation = representation;
        }

        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// <param name="memberMap">The member map.</param>
        public void Apply(BsonMemberMap memberMap)
        {
            if (memberMap.MemberType == _type)
            {
                memberMap.SetSerializationOptions(new RepresentationSerializationOptions(_representation));
            }
        }
    }
}