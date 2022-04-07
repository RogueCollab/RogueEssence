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
using Newtonsoft.Json.Linq;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    //TODO: Created v0.5.2, delete on v0.6.1
    public class ScriptVarsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
            // will this work?
            //serializer.Serialize(writer, value);

            // doesnt work due to self reference
            //serializer.Serialize(writer, serializer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                // doesn't work due to object type disagreement of some sort...
                //return serializer.Deserialize(reader, objectType);


                // will this work?
                //Script.LuaTableContainer container = new Script.LuaTableContainer();
                //reader.Read();
                ////we're now in the first property, table
                //reader.Read();
                ////now in the property data?
                //JObject jObject = JObject.Load(reader);
                //serializer.Populate(jObject.CreateReader(), container.Table);


                JObject jObject = JObject.Load(reader);
                Script.LuaTableContainer container = new Script.LuaTableContainer();
                serializer.Populate(jObject.CreateReader(), container);
                return container;
            }
            else
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
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Script.LuaTableContainer);
        }
    }

    //TODO: Created v0.5.3, delete on v0.6.1
    public class LuaTableContainerDictConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray jArray = JArray.Load(reader);
                List<object[]> container = new List<object[]>();
                serializer.Populate(jArray.CreateReader(), container);
                return container;
            }
            else
            {
                JObject jObject = JObject.Load(reader);
                Dictionary<object, object> dict = new Dictionary<object, object>();
                serializer.Populate(jObject.CreateReader(), dict);
                List<object[]> container = new List<object[]>();
                foreach (object key in dict.Keys)
                    container.Add(new object[] { key, dict[key] });
                return container;
            }
        }

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<(object, object)>);
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

    //TODO: Created v0.5.10, delete on v1.0.0
    public class MapBGConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            MapBG container = new MapBG();
            serializer.Populate(jObject.CreateReader(), container);


            if (Serializer.OldVersion <= new Version(0, 5, 8, 0))
            {
                container.RepeatX = true;
                container.RepeatY = true;
            }

            return container;
        }


        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MapBG);
        }
    }
}
