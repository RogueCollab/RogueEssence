using System;
using System.IO;
using RogueEssence.Data;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace RogueEssence.Dev
{
    public static class DevHelper
    {
        public static void ReserializeBase()
        {
            {
                string dir = PathMod.ModPath(DataManager.DATA_PATH + "Universal.bin");
                object data = DataManager.LoadData(dir, DiagManager.Instance.UpgradeBinder);
                DataManager.SaveData(dir, data);
            }

            string editPath = Path.Combine(PathMod.RESOURCE_PATH, "Extensions");
            foreach (string dir in Directory.GetFiles(editPath, "*.op"))
            {
                object data = DataManager.LoadData(dir, DiagManager.Instance.UpgradeBinder);
                DataManager.SaveData(dir, data);
            }

            foreach (string dir in PathMod.GetModFiles(DataManager.FX_PATH, "*.fx"))
            {
                object data = DataManager.LoadData(dir, DiagManager.Instance.UpgradeBinder);
                DataManager.SaveData(dir, data);
            }
        }

        public static void Reserialize(DataManager.DataType conversionFlags)
        {
            foreach (DataManager.DataType type in Enum.GetValues(typeof(DataManager.DataType)))
            {
                if (type != DataManager.DataType.All && (conversionFlags & type) != DataManager.DataType.None)
                    ReserializeData(DataManager.DATA_PATH + type.ToString() + "/", DataManager.DATA_EXT);
            }
        }

        public static void ReserializeData(string dataPath, string ext)
        {
            foreach (string dir in PathMod.GetModFiles(dataPath, "*"+ext))
            {
                IEntryData data = (IEntryData)DataManager.LoadData(dir, DiagManager.Instance.UpgradeBinder);
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
                    IndexNamedData(DataManager.DATA_PATH + type.ToString() + "/");
            }
        }


        public static void IndexNamedData(string dataPath)
        {
            try
            {
                EntryDataIndex fullGuide = new EntryDataIndex();

                foreach (string dir in PathMod.GetModFiles(dataPath, "*"+DataManager.DATA_EXT))
                {
                    string file = Path.GetFileNameWithoutExtension(dir);
                    int num = Convert.ToInt32(file);
                    IEntryData data = (IEntryData)DataManager.LoadData(dir);
                    while (fullGuide.Entries.Count <= num)
                        fullGuide.Entries.Add(new EntrySummary());
                    fullGuide.Entries[num] = data.GenerateEntrySummary();
                }

                using (Stream stream = new FileStream(PathMod.ModPath(dataPath + "index.idx"), FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(stream, fullGuide);
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
                    DemoData(DataManager.DATA_PATH + type.ToString() + "/", DataManager.DATA_EXT);
            }
        }

        public static void DemoData(string dataPath, string ext)
        {
            foreach (string dir in PathMod.GetModFiles(dataPath, "*" + ext))
            {
                IEntryData data = (IEntryData)DataManager.LoadData(dir);
                if (!data.Released)
                    data = (IEntryData)ReflectionExt.CreateMinimalInstance(data.GetType());
                DataManager.SaveData(dir, data);
            }
        }
    }
}
