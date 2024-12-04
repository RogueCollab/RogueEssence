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
    //TODO: Created v0.5.2, delete on v1.1
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

    //TODO: Created v0.5.3, delete on v1.1
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


    //TODO: Created v0.5.2, delete on v1.1
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

    //TODO: Created v0.5.10, delete on v1.1
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

    //TODO: Created v0.6.0, delete on v1.1
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
                        if (DataManager.Instance.Conversions[DataManager.DataType.Zone].ContainsKey(ii.ToString()))
                        {
                            string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Zone, ii);
                            int val;
                            if (int.TryParse(asset_name, out val))
                                continue;
                            dict[asset_name] = container[ii];
                        }
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

    public class DungeonConverter : JsonConverter
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
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Zone, ii);
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
                if (reader.TokenType == JsonToken.StartArray)
                {
                    JArray jArray = JArray.Load(reader);
                    HashSet<int> container = new HashSet<int>();
                    serializer.Populate(jArray.CreateReader(), container);

                    foreach (int ii in container)
                    {
                        string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.AutoTile, ii);
                        dict.Add(asset_name);
                    }
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


    public class TerrainAutotileDictConverter : JsonConverter
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
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
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


    public class AIConverter : JsonConverter
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
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.AI, ii);
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


    public class TileConverter : JsonConverter
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
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Tile, ii);
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


    public class TileListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> dict = new List<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<int> container = new List<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach (int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Tile, ii);
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
            return objectType == typeof(List<string>);
        }
    }




    public class ElementConverter : JsonConverter
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
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Element, ii);
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

    public class ElementSetConverter : JsonConverter
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
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Element, ii);
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

    public class ElementListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> dict = new List<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                HashSet<int> container = new HashSet<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach (int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Element, ii);
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
            return objectType == typeof(List<string>);
        }
    }

    public class ElementArrayConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> dict = new List<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<int> container = new List<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach (int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Element, ii);
                    dict.Add(asset_name);
                }
            }
            else
            {
                JArray jArray = JArray.Load(reader);
                serializer.Populate(jArray.CreateReader(), dict);
            }
            return dict.ToArray();
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
            return objectType == typeof(string[]);
        }
    }


    public class ItemElementDictConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if(Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JObject jObject = JObject.Load(reader);
                Dictionary<int, int> container = new Dictionary<int, int>();
                serializer.Populate(jObject.CreateReader(), container);

                foreach (int ii in container.Keys)
                {
                    string item_name = DataManager.Instance.MapAssetName(DataManager.DataType.Item, ii);
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Element, container[ii]);
                    dict[item_name] = asset_name;
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

    public class MapStatusElementDictConverter : JsonConverter
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
                    string item_name = DataManager.Instance.MapAssetName(DataManager.DataType.MapStatus, ii);
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Element, container[ii]);
                    dict[item_name] = asset_name;
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
            return objectType == typeof(Dictionary<int, string>);
        }
    }


    public class ElementMapStatusDictConverter : JsonConverter
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
                    string item_name = DataManager.Instance.MapAssetName(DataManager.DataType.Element, ii);
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.MapStatus, container[ii]);
                    dict[item_name] = asset_name;
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
            return objectType == typeof(Dictionary<string, int>);
        }
    }


    public class ElementItemDictConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if(Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JObject jObject = JObject.Load(reader);
                Dictionary<int, int> container = new Dictionary<int, int>();
                serializer.Populate(jObject.CreateReader(), container);

                foreach (int ii in container.Keys)
                {
                    string item_name = DataManager.Instance.MapAssetName(DataManager.DataType.Element, ii);
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Item, container[ii]);
                    dict[item_name] = asset_name;
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
            return objectType == typeof(Dictionary<string, int>);
        }
    }


    public class ElementSkillDictConverter : JsonConverter
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
                    string item_name = DataManager.Instance.MapAssetName(DataManager.DataType.Element, ii);
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Skill, container[ii]);
                    dict[item_name] = asset_name;
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
            return objectType == typeof(Dictionary<string, int>);
        }
    }


    public class ElementBattleEventDictConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, BattleEvent> dict = new Dictionary<string, BattleEvent>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JObject jObject = JObject.Load(reader);
                Dictionary<int, BattleEvent> container = new Dictionary<int, BattleEvent>();
                serializer.Populate(jObject.CreateReader(), container);

                foreach (int ii in container.Keys)
                {
                    string item_name = DataManager.Instance.MapAssetName(DataManager.DataType.Element, ii);
                    dict[item_name] = container[ii];
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
            return objectType == typeof(Dictionary<string, BattleEvent>);
        }
    }


    public class MapStatusConverter : JsonConverter
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
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.MapStatus, ii);
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


    public class MapStatusArrayConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> dict = new List<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<int> container = new List<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach (int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.MapStatus, ii);
                    dict.Add(asset_name);
                }
            }
            else
            {
                JArray jArray = JArray.Load(reader);
                serializer.Populate(jArray.CreateReader(), dict);
            }
            return dict.ToArray();
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
            return objectType == typeof(string[]);
        }
    }


    public class MapStatusDictConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, MapStatus> dict = new Dictionary<string, MapStatus>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JObject jObject = JObject.Load(reader);
                Dictionary<int, MapStatus> container = new Dictionary<int, MapStatus>();
                serializer.Populate(jObject.CreateReader(), container);

                foreach (int ii in container.Keys)
                {
                    string item_name = DataManager.Instance.MapAssetName(DataManager.DataType.MapStatus, ii);
                    dict[item_name] = container[ii];
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
            return objectType == typeof(Dictionary<string, MapStatus>);
        }
    }


    public class MapStatusIntDictConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JObject jObject = JObject.Load(reader);
                Dictionary<int, int> container = new Dictionary<int, int>();
                serializer.Populate(jObject.CreateReader(), container);

                foreach (int ii in container.Keys)
                {
                    string item_name = DataManager.Instance.MapAssetName(DataManager.DataType.MapStatus, ii);
                    dict[item_name] = container[ii];
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
            return objectType == typeof(Dictionary<string, int>);
        }
    }


    public class MapStatusListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> dict = new List<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<int> container = new List<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach (int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.MapStatus, ii);
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
            return objectType == typeof(List<string>);
        }
    }



    public class MapStatusBattleDataDictConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, BattleData> dict = new Dictionary<string, BattleData>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JObject jObject = JObject.Load(reader);
                Dictionary<int, BattleData> container = new Dictionary<int, BattleData>();
                serializer.Populate(jObject.CreateReader(), container);

                foreach (int ii in container.Keys)
                {
                    string item_name = DataManager.Instance.MapAssetName(DataManager.DataType.MapStatus, ii);
                    dict[item_name] = container[ii];
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
            return objectType == typeof(Dictionary<string, BattleData>);
        }
    }

    public class MapStatusSkillDictConverter : JsonConverter
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
                    string item_name = DataManager.Instance.MapAssetName(DataManager.DataType.MapStatus, ii);
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Skill, container[ii]);
                    dict[item_name] = asset_name;
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
            return objectType == typeof(Dictionary<string, int>);
        }
    }



    public class MapStatusBattleEventDictConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, BattleEvent> dict = new Dictionary<string, BattleEvent>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JObject jObject = JObject.Load(reader);
                Dictionary<int, BattleEvent> container = new Dictionary<int, BattleEvent>();
                serializer.Populate(jObject.CreateReader(), container);

                foreach (int ii in container.Keys)
                {
                    string item_name = DataManager.Instance.MapAssetName(DataManager.DataType.MapStatus, ii);
                    dict[item_name] = container[ii];
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
            return objectType == typeof(Dictionary<string, BattleEvent>);
        }
    }



    public class MapStatusBoolDictConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, bool> dict = new Dictionary<string, bool>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JObject jObject = JObject.Load(reader);
                Dictionary<int, bool> container = new Dictionary<int, bool>();
                serializer.Populate(jObject.CreateReader(), container);

                foreach (int ii in container.Keys)
                {
                    string item_name = DataManager.Instance.MapAssetName(DataManager.DataType.MapStatus, ii);
                    dict[item_name] = container[ii];
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
            return objectType == typeof(Dictionary<string, bool>);
        }
    }




    public class IntrinsicConverter : JsonConverter
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
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Intrinsic, ii);
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


    public class IntrinsicListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> dict = new List<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<int> container = new List<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach (int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Intrinsic, ii);
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
            return objectType == typeof(List<string>);
        }
    }




    public class StatusConverter : JsonConverter
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
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Status, ii);
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

    public class StatusDictConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, StatusEffect> dict = new Dictionary<string, StatusEffect>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JObject jObject = JObject.Load(reader);
                Dictionary<int, StatusEffect> container = new Dictionary<int, StatusEffect>();
                serializer.Populate(jObject.CreateReader(), container);

                foreach (int ii in container.Keys)
                {
                    string item_name = DataManager.Instance.MapAssetName(DataManager.DataType.Status, ii);
                    dict[item_name] = container[ii];
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
            return objectType == typeof(Dictionary<string, MapStatus>);
        }
    }



    public class StatusSetConverter : JsonConverter
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
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Status, ii);
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



    public class StatusListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> dict = new List<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<int> container = new List<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach (int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Status, ii);
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


    public class StatusArrayConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> dict = new List<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<int> container = new List<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach (int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Status, ii);
                    dict.Add(asset_name);
                }
            }
            else
            {
                JArray jArray = JArray.Load(reader);
                serializer.Populate(jArray.CreateReader(), dict);
            }
            return dict.ToArray();
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
            return objectType == typeof(string[]);
        }
    }


    public class SkillConverter : JsonConverter
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
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Skill, ii);
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


    public class RelearnableConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, bool> dict = new Dictionary<string, bool>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<bool> container = new List<bool>();
                serializer.Populate(jArray.CreateReader(), container);

                for(int ii = 0; ii < container.Count; ii++)
                {
                    if (container[ii])
                    {
                        string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Skill, ii);
                        dict[asset_name] = true;
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
            return objectType == typeof(Dictionary<string, bool>);
        }
    }

    public class SkillListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> dict = new List<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<int> container = new List<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach (int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Skill, ii);
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
            return objectType == typeof(List<string>);
        }
    }



    public class SkillArrayConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> dict = new List<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<int> container = new List<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach (int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Skill, ii);
                    dict.Add(asset_name);
                }
            }
            else
            {
                JArray jArray = JArray.Load(reader);
                serializer.Populate(jArray.CreateReader(), dict);
            }
            return dict.ToArray();
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
            return objectType == typeof(string[]);
        }
    }

    public class SkinConverter : JsonConverter
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
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Skin, ii);
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

    public class MonsterConverter : JsonConverter
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
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Monster, ii);
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


    public class MonsterListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> dict = new List<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<int> container = new List<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach (int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Monster, ii);
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
            return objectType == typeof(List<string>);
        }
    }


    public class MonsterUnlockConverter : JsonConverter
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
                        if (DataManager.Instance.Conversions[DataManager.DataType.Monster].ContainsKey(ii.ToString()))
                        {
                            string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Monster, ii);
                            int val;
                            if (int.TryParse(asset_name, out val))
                                continue;
                            dict[asset_name] = container[ii];
                        }
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


    public class MonsterBoolDictConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, bool> dict = new Dictionary<string, bool>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<bool> container = new List<bool>();
                serializer.Populate(jArray.CreateReader(), container);

                for (int ii = 0; ii < container.Count; ii++)
                {
                    if (container[ii])
                    {
                        string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Monster, ii);
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
            return objectType == typeof(Dictionary<string, bool>);
        }
    }



    public class ItemConverter : JsonConverter
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
                string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Item, ii);
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

    public class ItemStorageConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<int> container = new List<int>();
                serializer.Populate(jArray.CreateReader(), container);

                for (int ii = 0; ii < container.Count; ii++)
                {
                    if (DataManager.Instance.Conversions[DataManager.DataType.Item].ContainsKey(ii.ToString()))
                    {
                        string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Item, ii);
                        int val;
                        if (int.TryParse(asset_name, out val))
                            continue;
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
            return objectType == typeof(Dictionary<string, int>);
        }
    }


    public class ItemListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> dict = new List<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JArray jArray = JArray.Load(reader);
                List<int> container = new List<int>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach (int ii in container)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Item, ii);
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
            return objectType == typeof(List<string>);
        }
    }



    public class ItemRangeToListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("We shouldn't be here.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<string> dict = new List<string>();
            if (Serializer.OldVersion < DevHelper.StringAssetVersion)
            {
                JObject jObject = JObject.Load(reader);
                IntRange container = new IntRange();
                serializer.Populate(jObject.CreateReader(), container);

                for(int ii = container.Min; ii < container.Max; ii++)
                {
                    string asset_name = DataManager.Instance.MapAssetName(DataManager.DataType.Item, ii);
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
            return objectType == typeof(List<string>);
        }
    }

    public class SegLocTableConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Dictionary<SegLoc, Map> dict = (Dictionary<SegLoc, Map>)value;
            writer.WriteStartArray();
            foreach (SegLoc item in dict.Keys)
            {
                serializer.Serialize(writer, (item, dict[item]));
            }
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<SegLoc, Map> dict = new Dictionary<SegLoc, Map>();
            //TODO: Remove in v1.1
            if (Serializer.OldVersion < new Version(0, 7, 21))
            {
                JObject jObject = JObject.Load(reader);
                serializer.Populate(jObject.CreateReader(), dict);
            }
            else
            {
                JArray jArray = JArray.Load(reader);
                List<(SegLoc, Map)> container = new List<(SegLoc, Map)>();
                serializer.Populate(jArray.CreateReader(), container);

                foreach ((SegLoc, Map) item in container)
                    dict[item.Item1] = item.Item2;
            }

            return dict;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<SegLoc, Map>);
        }
    }

    public class SegLocIntTableConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Dictionary<SegLoc, int> dict = (Dictionary<SegLoc, int>)value;
            writer.WriteStartArray();
            foreach (SegLoc item in dict.Keys)
            {
                serializer.Serialize(writer, (item, dict[item]));
            }
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<SegLoc, int> dict = new Dictionary<SegLoc, int>();

            JArray jArray = JArray.Load(reader);
            List<(SegLoc, int)> container = new List<(SegLoc, int)>();
            serializer.Populate(jArray.CreateReader(), container);

            foreach ((SegLoc, int) item in container)
                dict[item.Item1] = item.Item2;

            return dict;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<SegLoc, Map>);
        }
    }
}
