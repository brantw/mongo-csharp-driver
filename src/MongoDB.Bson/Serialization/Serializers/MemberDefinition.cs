using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a member definition.
    /// </summary>
    public abstract class MemberDefinition
    {
        private readonly object _defaultValue;
        private readonly string _elementName;
        private readonly int _index;
        private readonly bool _isRequired;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberDefinition"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="isRequired">if set to <c>true</c> [is required].</param>
        /// <param name="defaultValue">The default value.</param>
        protected MemberDefinition(
            int index,
            string elementName,
            bool isRequired,
            object defaultValue)
        {
            _index = index;
            _elementName = elementName;
            _isRequired = isRequired;
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public object DefaultValue
        {
            get { return _defaultValue; }
        }

        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        /// <value>
        /// The name of the element.
        /// </value>
        public string ElementName
        {
            get { return _elementName; }
        }

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired
        {
            get { return _isRequired; }
        }

        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <returns>An object.</returns>
        public abstract object Deserialize(BsonDeserializationContext context);
    }

    /// <summary>
    /// Represents a member definition.
    /// </summary>
    /// <typeparam name="TMember">The type of the member.</typeparam>
    public class MemberDefinition<TMember> : MemberDefinition
    {
        private readonly IBsonSerializer<TMember> _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberDefinition{TMember}"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="isRequired">if set to <c>true</c> [is required].</param>
        /// <param name="defaultValue">The default value.</param>
        public MemberDefinition(
            int index,
            string elementName,
            IBsonSerializer<TMember> serializer,
            bool isRequired = true,
            TMember defaultValue = default(TMember))
            : base(index, elementName, isRequired, defaultValue)
        {
            _serializer = serializer;
        }

        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <returns>
        /// An object.
        /// </returns>
        public override object Deserialize(BsonDeserializationContext context)
        {
            return context.DeserializeWithChildContext(_serializer);
        }
    }
}
