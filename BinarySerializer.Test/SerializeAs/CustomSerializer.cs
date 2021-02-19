using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ProtoBuf;

namespace BinarySerialization.Test.SerializeAs
{
    [ProtoContract]
    public class PingEvent
    {
        [ProtoMember(1)]
        public uint Serial;

        public override bool Equals(object? obj)
        {
            if (obj is PingEvent target)
            {
                return Serial == target.Serial;
            }

            return false;
        }
    }

    [ProtoContract]
    public class PongEvent
    {
        [ProtoMember(1)]
        public uint Serial;
        
        [ProtoMember(2)]
        public uint Time;

        public override bool Equals(object? obj)
        {
            if (obj is PongEvent target)
            {
                return Serial == target.Serial && Time == target.Time;
            }

            return false;
        }
    }

    public class ProtoSerializer : CustomSerializer
    {
        public override void Serialize(Stream stream, object value)
        {
            var type = value.GetType();
            var attributes = type.GetCustomAttributes(true).ToList();
            var protoAttribute = attributes.OfType<ProtoContractAttribute>().FirstOrDefault();

            if (protoAttribute == null)
            {
                throw new NotSupportedException("Could not serialized class as ProtoContract: missing attribute.");
            }

            ProtoBuf.Serializer.Serialize(stream, value);
        }

        public override object Deserialize(Stream stream, Type type)
        {
            var attributes = type.GetCustomAttributes(true).ToList();
            var protoAttribute = attributes.OfType<ProtoContractAttribute>().FirstOrDefault();

            if (protoAttribute == null)
            {
                throw new NotSupportedException("Could not serialized class as ProtoContract: missing attribute.");
            }

            var method = typeof(ProtoBuf.Serializer).GetMethods().FirstOrDefault(m => m.Name == "Deserialize" && m.GetParameters().Length == 1);
            var genericMethod = method?.MakeGenericMethod(type);

            var obj = genericMethod?.Invoke(null, new object[] {stream});

            return obj;
        }
    }

    public class ProtoFactory : ISubtypeFactory
    {
        private Dictionary<EventTypes, Type> _typesMap = new Dictionary<EventTypes, Type>()
        {
            {EventTypes.Pong, typeof(PongEvent)},
            {EventTypes.Ping, typeof(PingEvent)},
        };

        public bool TryGetKey(Type valueType, out object key)
        {
            var kv = _typesMap.FirstOrDefault(x => x.Value == valueType);

            if (kv.Value != null)
            {
                key = kv.Key;
            }
            else
            {
                key = null;
            }

            return key != null;
        }

        public bool TryGetType(object key, out Type type)
        {
            return _typesMap.TryGetValue((EventTypes) key, out type);
        }
    }

    public enum EventTypes : ushort
    {
        Ping = 1,
        Pong = 2
    }

    public class ProtoBufEvent
    {
        [FieldOrder(1)]
        public uint DataLength;

        [FieldOrder(2)]
        public EventTypes OpCode;

        [FieldOrder(3)]
        [FieldLength(nameof(DataLength))]
        [SubtypeFactory(nameof(OpCode), typeof(ProtoFactory))]
        //[SubtypeDefault(typeof(byte[]))]
        [SerializeAs(SerializedType.CustomSerializer, typeof(ProtoSerializer))]
        public object ProtoData;

        public override bool Equals(object? obj)
        {
            if (obj is ProtoBufEvent target)
            {
                return OpCode == target.OpCode && ProtoData.Equals(target.ProtoData);
            }

            return false;
        }
    }

    public class JSONSerializer : CustomSerializer
    {
        public override void Serialize(Stream stream, object value)
        {
            using (var writer = new BinaryWriter(stream))
            {
                var json = JsonConvert.SerializeObject(value);

                writer.Write(json);
            }
        }

        public override object Deserialize(Stream stream, Type type)
        {
            using (var reader = new BinaryReader(stream))
            {
                var jsonString = reader.ReadString();
                
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
            }
        }
    }

    public class EventWithJSON
    {
        [FieldOrder(1)]
        public uint CollectionSize;

        [FieldOrder(2)]
        [FieldCount(nameof(CollectionSize))]
        [SerializeAs(SerializedType.CustomSerializer, typeof(JSONSerializer))]
        public Dictionary<string, string> Params;
    }
}
