using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace YAMB.Serialization
{
    internal sealed class JsonMessageSerializer : IMessageSerializer
    {
        private static readonly Encoding UTF8NoBOM = new UTF8Encoding(false, true);

        private readonly JsonSerializer _serializer;

        public JsonMessageSerializer()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Binder = new CustomSerializationBinder()
            };

            _serializer = JsonSerializer.CreateDefault(settings);
        }

        public void Serialize(Stream stream, Message message)
        {
            using (var writer = new StreamWriter(stream, UTF8NoBOM, 0x400, true))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                _serializer.Serialize(jsonWriter, message);
            }
        }

        public Message Deserialize(Stream stream)
        {
            using (var reader = new StreamReader(stream, UTF8NoBOM))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return _serializer.Deserialize<Message>(jsonReader);
            }
        }

        private class CustomSerializationBinder : SerializationBinder
        {
            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = serializedType.Assembly.FullName;
                typeName = serializedType.FullName;
            }

            public override Type BindToType(string assemblyName, string typeName)
            {
                return Type.GetType(typeName + ", " + assemblyName);
            }
        }
    }
}