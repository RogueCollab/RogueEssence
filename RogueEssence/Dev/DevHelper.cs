using System;
using System.IO;
using RogueEssence.Data;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Xml.Serialization;
using RogueEssence.Content;

namespace RogueEssence.Dev
{
    public static class DevHelper
    {
        public static void ReserializeBase()
        {
            {
                string dir = PathMod.ModPath(DataManager.DATA_PATH + "Universal.bin");
                object data = LoadWithLegacySupport<ActiveEffect>(dir);
                DataManager.SaveData(dir, data);
            }

            /*string editPath = Path.Combine(PathMod.RESOURCE_PATH, "Extensions");
            foreach (string dir in Directory.GetFiles(editPath, "*.op"))
            {
                object data = LoadWithLegacySupport(dir, typeof(Extension???));
                DataManager.SaveData(dir, data);
            }*/

            foreach (string dir in PathMod.GetModFiles(DataManager.FX_PATH, "*.fx"))
            {
                object data;
                if (Path.GetFileName(dir) == "NoCharge.fx")
                {
                    data = LoadWithLegacySupport<EmoteFX>(dir);
                }
                else
                {
                    data = LoadWithLegacySupport<BattleFX>(dir);
                }
                DataManager.SaveData(dir, data);
            }
        }

        public static void Reserialize(DataManager.DataType conversionFlags)
        {
            foreach (DataManager.DataType type in Enum.GetValues(typeof(DataManager.DataType)))
            {
                if (type != DataManager.DataType.All && (conversionFlags & type) != DataManager.DataType.None)
                {
                    ReserializeData(DataManager.DATA_PATH + type.ToString() + "/", DataManager.DATA_EXT, type.GetClassType());
                }
            }
        }

        public static void ReserializeData<T>(string dataPath, string ext)
        {
            ReserializeData(dataPath, ext, typeof(T));
        }

        public static void ReserializeData(string dataPath, string ext, Type t)
        {
            foreach (string dir in PathMod.GetModFiles(dataPath, "*"+ext))
            {
                object data = LoadWithLegacySupport(dir, t);
                DataManager.SaveData(dir, data);
            }
        }


        /// <summary>
        /// Bakes all assets from the "Work files" directory specified in the flags.
        /// </summary>
        /// <param name="conversionFlags">Chooses which asset type to bake</param>
        public static void RunIndexing(DataManager.DataType conversionFlags)
        {
            foreach (DataManager.DataType type in Enum.GetValues(typeof(DataManager.DataType)))
            {
                if (type != DataManager.DataType.All && (conversionFlags & type) != DataManager.DataType.None)
                    IndexNamedData(DataManager.DATA_PATH + type.ToString() + "/", type.GetClassType());
            }
        }
        public static void RunExtraIndexing(DataManager.DataType conversionFlags)
        {
            //index extra based on triggers
            foreach (BaseData baseData in DataManager.Instance.UniversalData)
            {
                if ((baseData.TriggerType & conversionFlags) != DataManager.DataType.None)
                {
                    baseData.ReIndex();
                    DataManager.SaveData(PathMod.ModPath(DataManager.MISC_PATH + baseData.FileName + DataManager.DATA_EXT), baseData);
                }
            }
        }


        public static void IndexNamedData(string dataPath, Type t)
        {
            try
            {
                EntryDataIndex fullGuide = new EntryDataIndex();
                List<EntrySummary> entries = new List<EntrySummary>();
                foreach (string dir in PathMod.GetModFiles(dataPath, "*"+DataManager.DATA_EXT))
                {
                    string file = Path.GetFileNameWithoutExtension(dir);
                    int num = Convert.ToInt32(file);
                    IEntryData data = (IEntryData)LoadWithLegacySupport(dir, t);
                    while (entries.Count <= num)
                        entries.Add(new EntrySummary());
                    entries[num] = data.GenerateEntrySummary();
                }
                fullGuide.Entries = entries.ToArray();

                using (Stream stream = new FileStream(PathMod.ModPath(dataPath + "index.idx"), FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        Serializer.Serialize(stream, fullGuide);
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new Exception("Error importing index at " + dataPath + "\n", ex));
            }
        }

        public static void DemoAllData(DataManager.DataType conversionFlags)
        {

            foreach (DataManager.DataType type in Enum.GetValues(typeof(DataManager.DataType)))
            {
                if (type != DataManager.DataType.All && (conversionFlags & type) != DataManager.DataType.None)
                    DemoData(DataManager.DATA_PATH + type.ToString() + "/", DataManager.DATA_EXT, type.GetClassType());
            }
        }

        public static void DemoData(string dataPath, string ext, Type t)
        {
            foreach (string dir in PathMod.GetModFiles(dataPath, "*" + ext))
            {
                IEntryData data = (IEntryData)LoadWithLegacySupport(dir, t);
                if (!data.Released)
                    data = (IEntryData)ReflectionExt.CreateMinimalInstance(data.GetType());
                DataManager.SaveData(dir, data);
            }
        }

        private static T LoadWithLegacySupport<T>(string path)
        {
            return (T)LoadWithLegacySupport(path, typeof(T));
        }

        private static object LoadWithLegacySupport(string path, Type t)
        {
            Console.WriteLine("Loading {0}...", path);
            try
            {
                return DataManager.LoadData(path, t);
            }
            catch (Exception)
            {
                using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
#pragma warning disable 618
                    IFormatter formatter = new BinaryFormatter();
                    return formatter.Deserialize(stream);
#pragma warning restore 618
                }
            }
        }
    }
}
