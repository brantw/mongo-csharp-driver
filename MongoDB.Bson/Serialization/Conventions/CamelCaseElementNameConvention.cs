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
using System.Linq;
using System.Reflection;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// A convention that sets the element name the same as the member name with the first character lower cased.
    /// </summary>
#pragma warning disable 618 // about obsolete IElementNameConvention
    public class CamelCaseElementNameConvention : ConventionBase, IMemberMapConvention, IElementNameConvention
#pragma warning restore 618
    {
        // private fields
        private readonly bool _handleVariableLengthPrefixes;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CamelCaseElementNameConvention"/> class.
        /// </summary>
        public CamelCaseElementNameConvention()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CamelCaseElementNameConvention"/> class.
        /// </summary>
        /// <param name="handleVariableLengthPrefixes">if set to <c>true</c> [handle variable length prefixes].</param>
        public CamelCaseElementNameConvention(bool handleVariableLengthPrefixes)
        {
            _handleVariableLengthPrefixes = handleVariableLengthPrefixes;
        }

        // public methods
        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// <param name="memberMap">The member map.</param>
        public void Apply(BsonMemberMap memberMap)
        {
            var name = memberMap.MemberName;
            name = GetElementName(name);
            memberMap.SetElementName(name);
        }

        /// <summary>
        /// Gets the element name for a member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>The element name.</returns>
        [Obsolete("Use Apply instead.")]
        public string GetElementName(MemberInfo member)
        {
            return GetElementName(member.Name);
        }

        // private methods
        private string GetElementName(string memberName)
        {
            if (memberName.Length == 0)
            {
                return "";
            }
            else if(memberName.Length == 1)
            {
                return Char.ToLowerInvariant(memberName[0]).ToString();
            }
            else 
            {
                return ToCamelCase(memberName);
            }
        }

        private bool IsPartOfPrefix(char[] chars, int i)
        {
            if (i == 0 || i == chars.Length - 1)
            {
                return true;
            }

            var nextChar = chars[i + 1];
            return IsUpperCase(nextChar);
        }

        private bool IsUpperCase(char c)
        {
            return c != char.ToLowerInvariant(c);
        }

        private string ToCamelCase(string name)
        {
            if (_handleVariableLengthPrefixes)
            {
                var chars = name.ToArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    if (IsPartOfPrefix(chars, i))
                    {
                        chars[i] = char.ToLowerInvariant(chars[i]);
                    }
                    else
                    {
                        break;
                    }
                }
                return new string(chars);
            }
            else
            {
                return Char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
        }
    }
}