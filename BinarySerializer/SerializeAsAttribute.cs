using System;
using System.Reflection;

namespace BinarySerialization
{
    /// <summary>
    ///     Provides the <see cref="BinarySerializer" /> with information used to serialize the decorated member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SerializeAsAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SerializeAsAttribute" /> class.
        /// </summary>
        public SerializeAsAttribute()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the SerializeAs class with a specified <see cref="SerializedType" />.
        /// </summary>
        public SerializeAsAttribute(SerializedType serializedType, Type customSerializerType = null)
        {
            SerializedType = serializedType;

            if (customSerializerType != null)
            {
                if (SerializedType != SerializedType.CustomSerializer)
                {
                    throw new ArgumentException("Parameter \"customSerializerType\"  ");
                }
                
                if (!typeof(CustomSerializer).IsAssignableFrom(customSerializerType))
                {
                    throw new ArgumentException("Type must implement CustomSerializer", nameof(customSerializerType));
                }
                
                CustomSerializerType = customSerializerType;
            }
        }

        /// <summary>
        ///     Specifies the type to which to serialize the member.
        /// </summary>
        public SerializedType SerializedType { get; set; }

        /// <summary>
        /// Specify the string terminator when the serialized type is TerminatedString.  Null (zero) by default.
        /// </summary>
        public char StringTerminator { get; set; }

        public Type CustomSerializerType { get; set; }
    }
}
