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
                    return Script.LuaEngine.Instance.SaveLuaTable(tbl);
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

    //TODO: Created v0.5.10, delete on v0.6.1
    public class MapBGConverter : JsonConverter<MapBG>
    {
        public override void WriteJson(JsonWriter writer, MapBG value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override MapBG ReadJson(JsonReader reader, Type objectType, MapBG existingValue, bool hasExistingValue, JsonSerializer serializer)
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
    }

    public class DungeonUnlockConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, GameProgress.UnlockState> dict = new Dictionary<string, GameProgress.UnlockState>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<GameProgress.UnlockState> container = new List<GameProgress.UnlockState>();
                serializer.Populate(jArray.CreateReader(), container);

                for (int ii = 0; ii < container.Count; ii++)
                {
                    if (container[ii] > GameProgress.UnlockState.None)
                    {
                        string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Zone, ii);
                        dict[asset_name] = container[ii];
                    }
                }
            }
            else
            {
                JObject jObject = JObject.Load(reader);
                serializer.Populate(jObject.CreateReader(), dict);
            }
            return dict;
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
            return objectType == typeof(Dictionary<string, GameProgress.UnlockState>);
        }
    }



    public class AutotileConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                int ii = Int32.Parse(reader.Value.ToString());
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.AutoTile, ii);
                return asset_name;
            }
            else
            {
                string s = (string)reader.Value;
                return s;
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
            return objectType == typeof(string);
        }
    }


    public class AutotileSetConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            HashSet<string> dict = new HashSet<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                HashSet<int> container = new HashSet<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach(int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.AutoTile, ii);
                    dict.Add(asset_name);
                }
            }
            else
            {
                JArray jArray = JArray.Load(reader);
                serializer.Populate(jArray.CreateReader(), dict);
            }
            return dict;
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
            return objectType == typeof(HashSet<string>);
        }
    }


    public class TerrainDictAutotileDataConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JObject jObject = JObject.Load(reader);
                Dictionary<int, int> container = new Dictionary<int, int>();
                serializer.Populate(jObject.CreateReader(), container);

                foreach (int ii in container.Keys)
                {
                    string terrain_name = DataManager.Instance.MapAssetName(DataManager.DataType.Terrain, ii);
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.AutoTile, container[ii]);
                    dict[terrain_name] = asset_name;
                }
            }
            else
            {
                JObject jObject = JObject.Load(reader);
                serializer.Populate(jObject.CreateReader(), dict);
            }
            return dict;
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
            return objectType == typeof(Dictionary<string, string>);
        }
    }


    public class TerrainConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                int ii = Int32.Parse(reader.Value.ToString());
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Terrain, ii);
                return asset_name;
            }
            else
            {
                string s = (string)reader.Value;
                return s;
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
            return objectType == typeof(string);
        }
    }


    public class TerrainSetConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            HashSet<string> dict = new HashSet<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                HashSet<int> container = new HashSet<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach (int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Terrain, ii);
                    dict.Add(asset_name);
                }
            }
            else
            {
                JArray jArray = JArray.Load(reader);
                serializer.Populate(jArray.CreateReader(), dict);
            }
            return dict;
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
            return objectType == typeof(HashSet<string>);
        }
    }




    public class TerrainDictAutotileConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, AutoTile> dict = new Dictionary<string, AutoTile>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JObject jObject = JObject.Load(reader);
                Dictionary<int, AutoTile> container = new Dictionary<int, AutoTile>();
                serializer.Populate(jObject.CreateReader(), container);

                foreach (int ii in container.Keys)
                {
                    string terrain_name = DataManager.Instance.MapAssetName(DataManager.DataType.Terrain, ii);
                    dict[terrain_name] = container[ii];
                }
            }
            else
            {
                JObject jObject = JObject.Load(reader);
                serializer.Populate(jObject.CreateReader(), dict);
            }
            return dict;
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
            return objectType == typeof(Dictionary<string, AutoTile>);
        }
    }


    public class GrowthGroupConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                int ii = Int32.Parse(reader.Value.ToString());
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.GrowthGroup, ii);
                return asset_name;
            }
            else
            {
                string s = (string)reader.Value;
                return s;
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
            return objectType == typeof(string);
        }
    }


    public class SkillGroupConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                int ii = Int32.Parse(reader.Value.ToString());
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.SkillGroup, ii);
                return asset_name;
            }
            else
            {
                string s = (string)reader.Value;
                return s;
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
            return objectType == typeof(string);
        }
    }


    public class RankConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (Serializer.OldVersion < new Version(0, 5, 20, 1))
            {
                int ii = Int32.Parse(reader.Value.ToString());
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Rank, ii);
                return asset_name;
            }
            else
            {
                string s = (string)reader.Value;
                return s;
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
            return objectType == typeof(string);
        }
    }
}
