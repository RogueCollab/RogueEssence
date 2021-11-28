using System;
using System.IO;
using RogueEssence.Data;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Xml.Serialization;
using RogueEssence.Content;
using Newtonsoft.Json;
using NLua;
using RogueElements;

namespace RogueEssence.Dev
{
    //TODO: Created v0.5.2, delete on v0.6.1
    public class ScriptVarsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string val = JsonConvert.SerializeObject(value, Serializer.Settings);
            writer.WriteValue(val);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            if (s == null)
                return null;

            try
            {
                return JsonConvert.DeserializeObject(s, objectType, Serializer.Settings);
            }
            catch (Exception ex)
            {
                LuaTable tbl = Script.LuaEngine.Instance.DeserializedLuaTable(s);
                return Script.LuaEngine.Instance.LuaTableToDict(tbl);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string) || objectType == typeof(Script.LuaTableContainer);
        }
    }
    //TODO: Created v0.5.2, delete on v0.6.1
    public class IRandomConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string val = JsonConvert.SerializeObject(value, Serializer.Settings);
            writer.WriteValue(val);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            if (s == null)
                return null;

            try
            {
                return JsonConvert.DeserializeObject(s, objectType, Serializer.Settings);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IRandom);
        }
    }
}
