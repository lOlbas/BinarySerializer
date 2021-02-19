using System;
using System.IO;

namespace BinarySerialization
{
    public abstract class CustomSerializer
    {
        public abstract void Serialize(Stream stream, object value);

        public abstract object Deserialize(Stream stream, Type type);
    }
}
