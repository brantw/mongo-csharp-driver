using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a class definition.
    /// </summary>
    /// <typeparam name="TClass">The class.</typeparam>
    public abstract class ClassDefinition<TClass>
    {
        private readonly int _requiredMemberFlags;
        private readonly MemberDefinition[] _memberDefinitions;
        private readonly Dictionary<string, MemberDefinition> _memberDefinitionDictionary = new Dictionary<string,MemberDefinition>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassDefinition{TClass}"/> class.
        /// </summary>
        /// <param name="memberDefinitions">The member definitions.</param>
        protected ClassDefinition(MemberDefinition[] memberDefinitions)
        {
            _memberDefinitions = memberDefinitions;

            var memberFlag = 1;
            for (int i = 0; i < _memberDefinitions.Length; i++, memberFlag <<= 1)
            {
                var memberDefinition = _memberDefinitions[i];

                _memberDefinitionDictionary[memberDefinition.ElementName] = memberDefinition;
                if (memberDefinition.IsRequired)
                {
                    _requiredMemberFlags |= memberFlag;
                }
            }
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="memberValues">The member values.</param>
        /// <param name="missingMemberFlags">The missing members.</param>
        /// <returns></returns>
        protected abstract TClass CreateInstance(object[] memberValues, int missingMemberFlags);

        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <returns>An object.</returns>
        public TClass Deserialize(BsonDeserializationContext context)
        {
            var reader = context.Reader;
            var memberValues = _memberDefinitions.Select(m => m.DefaultValue).ToArray();
            var missingMemberFlags = (int)(uint.MaxValue >> (32 - _memberDefinitions.Length));

            reader.ReadStartDocument();
            while (reader.ReadBsonType() != 0)
            {
                var name = reader.ReadName();
                var memberDefinition = GetMemberDefinition(name);
                memberValues[memberDefinition.Index] = memberDefinition.Deserialize(context);
                missingMemberFlags &= ~(1 << memberDefinition.Index);
            }
            reader.ReadEndDocument();

            if ((missingMemberFlags & _requiredMemberFlags) != 0)
            {
                var missingMember = GetFirstMissingRequiredMember(missingMemberFlags);
                throw new BsonSerializationException(string.Format("Missing element: '{0}'.", missingMember.ElementName));
            }

            return CreateInstance(memberValues, missingMemberFlags);
        }

        private MemberDefinition GetFirstMissingRequiredMember(int missingMemberFlags)
        {
            var missingRequiredMemberFlags = missingMemberFlags & _requiredMemberFlags;

            var memberFlag = 1;
            for (int i = 0; i < _memberDefinitions.Length; i++, memberFlag <<= 1)
            {
                if ((missingRequiredMemberFlags & memberFlag) != 0)
                {
                    return _memberDefinitions[i];
                }
            }

            return null;
        }

        private MemberDefinition GetMemberDefinition(string elementName)
        {
            MemberDefinition memberDefinition;
            if (!_memberDefinitionDictionary.TryGetValue(elementName, out memberDefinition))
            {
                throw new BsonSerializationException(string.Format("Invalid element: '{0}'.", elementName));
            }

            return memberDefinition;
        }
    }

    /// <summary>
    /// Represents a class definition.
    /// </summary>
    /// <typeparam name="TClass">The type of the class.</typeparam>
    /// <typeparam name="T1">The type of member 1.</typeparam>
    public class ClassDefinition<TClass, T1> : ClassDefinition<TClass>
    {
        private readonly Func<T1, int, TClass> _creator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassDefinition{TClass, T1}"/> class.
        /// </summary>
        /// <param name="m1">The definition of member 1.</param>
        /// <param name="creator">The creator.</param>
        public ClassDefinition(
            MemberDefinition<T1> m1,
            Func<T1, int, TClass> creator)
            : base(new[] { m1 })
        {
            _creator = creator;
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="memberValues">The member values.</param>
        /// <param name="missingMemberFlags">The missing member flags.</param>
        /// <returns>An instance.</returns>
        protected override TClass CreateInstance(object[] memberValues, int missingMemberFlags)
        {
            return _creator(
                (T1)memberValues[0],
                missingMemberFlags);
        }
    }

    /// <summary>
    /// Represents a class definition.
    /// </summary>
    /// <typeparam name="TClass">The type of the class.</typeparam>
    /// <typeparam name="T1">The type of member 1.</typeparam>
    /// <typeparam name="T2">The type of member 2.</typeparam>
    /// <typeparam name="T3">The type of member 3.</typeparam>
    /// <typeparam name="T4">The type of member 4.</typeparam>
    public class ClassDefinition<TClass, T1, T2, T3, T4> : ClassDefinition<TClass>
    {
        private readonly Func<T1, T2, T3, T4, int, TClass> _creator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassDefinition{TClass, T1, T2, T3, T4}"/> class.
        /// </summary>
        /// <param name="m1">The definition of member 1.</param>
        /// <param name="m2">The definition of member 2.</param>
        /// <param name="m3">The definition of member 3.</param>
        /// <param name="m4">The definition of member 4.</param>
        /// <param name="creator">The creator.</param>
        public ClassDefinition(
            MemberDefinition<T1> m1,
            MemberDefinition<T2> m2,
            MemberDefinition<T3> m3,
            MemberDefinition<T4> m4,
            Func<T1, T2, T3, T4, int, TClass> creator)
            : base(new MemberDefinition[] { m1, m2, m3, m4 })
        {
            _creator = creator;
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="memberValues">The member values.</param>
        /// <param name="missingMemberFlags">The missing member flags.</param>
        /// <returns>An instance.</returns>
        protected override TClass CreateInstance(object[] memberValues, int missingMemberFlags)
        {
            return _creator(
                (T1)memberValues[0],
                (T2)memberValues[1],
                (T3)memberValues[2],
                (T4)memberValues[3],
                missingMemberFlags);
        }
    }
}
