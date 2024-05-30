using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RogueEssence.Dev;
using RogueEssence.LevelGen;

namespace RogueEssence.Data
{
    [Serializable]
    public class SerializationContainer
    {
        public Version Version;
        public object Object;
    }

    public static class Serializer
    {
        public static JsonSerializerSettings Settings { get; private set; }

        public static void InitSettings(IContractResolver resolver, ISerializationBinder binder)
        {
            Settings = new JsonSerializerSettings()
            {
                ContractResolver = resolver,
                SerializationBinder = binder,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                TypeNameHandling = TypeNameHandling.Auto,
            };

        }

        /// <summary>
        /// A value that is temporarily set when deserializing a data object, serving as a global old version for converters in UpgradeConverters.cs to recognize the version.
        /// A bit hacky, but is currently the only way for converters to recognize version.
        /// </summary>
        public static Version OldVersion;

        private static object lockObj = new object();

        public static object Deserialize(Stream stream, Type type, params JsonConverter[] converters)
        {
            lock (lockObj)
            {
                object obj;
                Version pastVersion = OldVersion;
                OldVersion = Versioning.GetVersion();
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, -1, true))
                {
                    JsonSerializerSettings settingsCopy = new JsonSerializerSettings(Settings);
                    settingsCopy.Converters = converters;
                    obj = JsonConvert.DeserializeObject(reader.ReadToEnd(), type, settingsCopy);
                }
                OldVersion = pastVersion;
                return obj;
            }
        }

        public static void Serialize(Stream stream, object entry)
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, -1, true))
            {
                string val = JsonConvert.SerializeObject(entry, Settings);
                writer.Write(val);
            }
        }

        public static void Serialize(Stream stream, object entry, params JsonConverter[] converters)
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, -1, true))
            {
                JsonSerializerSettings settingsCopy = new JsonSerializerSettings(Settings);
                settingsCopy.Converters = converters;
                string val = JsonConvert.SerializeObject(entry, settingsCopy);
                writer.Write(val);
            }
        }

        public static Version GetVersion(string containerStr)
        {
            Version objVersion = new Version(0, 0);
            try
            {
                using (JsonTextReader textReader = new JsonTextReader(new StringReader(containerStr)))
                {
                    textReader.Read();
                    textReader.Read();
                    while (true)
                    {
                        if (textReader.TokenType == JsonToken.PropertyName && (string)textReader.Value == "Version")
                        {
                            textReader.Read();
                            objVersion = new Version((string)textReader.Value);
                            break;
                        }
                        else
                        {
                            textReader.Skip();
                            textReader.Read();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return objVersion;
        }
        public static object DeserializeDataWithDiffs(string path, params string[] diffpaths)
        {
            //TODO
            //string containerStr;
            //using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            //{
            //    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, -1, true))
            //    {
            //        containerStr = reader.ReadToEnd();
            //    }
            //}
            //string[] diffStr = new string[diffpaths.Length];
            //for each diff, read into an array

            //then, apply all diff patches (including version)

            //then, load from this string
            //SerializationContainer container = (SerializationContainer)JsonConvert.DeserializeObject(containerStr, typeof(SerializationContainer), Settings);
            //return container.Object;
            return null;
        }

        public static object DeserializeData(Stream stream)
        {
            lock (lockObj)
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, -1, true))
                {
                    string containerStr = reader.ReadToEnd();
                    //Temporarily set global old version for converters in UpgradeConverters.cs to recognize the version.
                    Version pastVersion = OldVersion;
                    OldVersion = GetVersion(containerStr);
                    SerializationContainer container = (SerializationContainer)JsonConvert.DeserializeObject(containerStr, typeof(SerializationContainer), Settings);
                    OldVersion = pastVersion;
                    return container.Object;
                }
            }
        }

        public static void SerializeDataAsDiff(string path, string basepath, object entry)
        {

        }

        public static void SerializeData(Stream stream, object entry, bool min = false)
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, -1, true))
            {
                SerializationContainer container = new SerializationContainer();
                container.Object = entry;
                container.Version = Versioning.GetVersion();
                string val = SerializeObjectInternal(container, Settings, min);
                writer.Write(val);
            }
        }

        private static string SerializeObjectInternal(object value, JsonSerializerSettings settings, bool min)
        {
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
            StringBuilder sb = new StringBuilder(256);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                if (min)
                {
                    jsonWriter.Formatting = Formatting.None;
                }
                else
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    jsonWriter.Indentation = 0;
                    jsonWriter.IndentChar = '\t';
                }

                jsonSerializer.Serialize(jsonWriter, value, null);
            }

            return sw.ToString();
        }

    }
}