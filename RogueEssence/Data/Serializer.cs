using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RogueEssence.Dev;

namespace RogueEssence.Data
{
    [Serializable]
    public class SerializationContainer
    {
        public object Object;
        public Version Version;
    }

    public static class Serializer
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new SerializerContractResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            TypeNameHandling = TypeNameHandling.Auto,
        };
        
        public static object Deserialize(Stream stream, Type type)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, -1, true))
            {
                SerializationContainer container = (SerializationContainer)JsonConvert.DeserializeObject(reader.ReadToEnd(), typeof(SerializationContainer), Settings);
                return container.Object;
            }
        }

        public static void Serialize(Stream stream, object entry)
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, -1, true))
            {
                SerializationContainer container = new SerializationContainer();
                container.Object = entry;
                container.Version = Versioning.GetVersion();
                string val = JsonConvert.SerializeObject(container, Settings);
                writer.Write(val);
            }
        }
        
        private class SerializerContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                FieldInfo[] fieldsLess = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                List<MemberInfo> fields = type.GetSerializableMembers();
                List<JsonProperty> props = fields.Select(f => CreateProperty(f, memberSerialization))
                    .ToList();
                props.ForEach(p => { p.Writable = true; p.Readable = true; });
                return props;
                
            }
        }
    }
}