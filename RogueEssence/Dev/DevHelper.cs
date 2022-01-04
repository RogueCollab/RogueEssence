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
        //TODO: v0.6: remove this
        static int legacy = 0;

        public static void ReserializeBase()
        {
            if (legacy == 2)
            {
                string dir = PathMod.ModPath(DataManager.DATA_PATH + "Universal.bin");
                object data = LegacyLoad(dir);
                LegacySave(dir, data);
            }
            else if (legacy == 1)
            {
                string dir = PathMod.ModPath(DataManager.DATA_PATH + "Universal.bin");
                object data = LegacyLoad(dir);
                DataManager.SaveData(dir, data);
                DataManager.LoadData<ActiveEffect>(dir);
                LegacySave(dir, data);
            }
            else
            {
                string dir = PathMod.ModPath(DataManager.DATA_PATH + "Universal.bin");
                object data = LoadWithLegacySupport<ActiveEffect>(dir);
                DataManager.SaveData(dir, data);
            }

            foreach (string dir in PathMod.GetModFiles(DataManager.FX_PATH, "*.fx"))
            {
                object data;
                if (legacy == 2)
                {
                    if (Path.GetFileName(dir) == "NoCharge.fx")
                        data = LegacyLoad(dir);
                    else
                        data = LegacyLoad(dir);
                    LegacySave(dir, data);
                }
                else if (legacy == 1)
                {
                    if (Path.GetFileName(dir) == "NoCharge.fx")
                        data = LegacyLoad(dir);
                    else
                        data = LegacyLoad(dir);
                    DataManager.SaveData(dir, data);
                    if (Path.GetFileName(dir) == "NoCharge.fx")
                        data = DataManager.LoadData<EmoteFX>(dir);
                    else
                        data = DataManager.LoadData<BattleFX>(dir);
                    LegacySave(dir, data);
                }
                else
                {
                    if (Path.GetFileName(dir) == "NoCharge.fx")
                        data = LoadWithLegacySupport<EmoteFX>(dir);
                    else
                        data = LoadWithLegacySupport<BattleFX>(dir);
                    DataManager.SaveData(dir, data);
                }
            }


            foreach (string dir in Directory.GetFiles(Path.Combine(PathMod.RESOURCE_PATH, "Extensions"), "*.op"))
            {
                object data;
                if (legacy == 2)
                {
                    data = LegacyLoad(dir);
                    LegacySave(dir, data);
                }
                else if (legacy == 1)
                {
                    data = LegacyLoad(dir);
                    DataManager.SaveData(dir, data);
                    data = DataManager.LoadData<CharSheetOp>(dir);
                    LegacySave(dir, data);
                }
                else
                {
                    data = LoadWithLegacySupport<CharSheetOp>(dir);
                    DataManager.SaveData(dir, data);
                }
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
                if (legacy == 2)
                {
                    object data = LegacyLoad(dir);
                    LegacySave(dir, data);
                }
                else if (legacy == 1)
                {
                    object data = LegacyLoad(dir);
                    DataManager.SaveData(dir, data);
                    object json = DataManager.LoadData(dir, t);
                    LegacySave(dir, json);
                }
                else
                {
                    object data = LoadWithLegacySupport(dir, t);
                    DataManager.SaveData(dir, data);
                }
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
                    DataManager.SaveData(PathMod.HardMod(DataManager.MISC_PATH + baseData.FileName + DataManager.DATA_EXT), baseData);
                }
            }
            DataManager.SaveData(PathMod.HardMod(DataManager.MISC_PATH + "Index.bin"), DataManager.Instance.UniversalData);
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

                using (Stream stream = new FileStream(PathMod.HardMod(dataPath + "index.idx"), FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Serializer.SerializeData(stream, fullGuide);
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

        //TODO: v0.6 Delete this
        private static T LoadWithLegacySupport<T>(string path)
        {
            return (T)LoadWithLegacySupport(path, typeof(T));
        }

        //TODO: v0.6 Delete this
        public static object LoadWithLegacySupport(string path, Type t)
        {
            try
            {
                return DataManager.LoadData(path, t);
            }
            catch (Exception)
            {
                return LegacyLoad(path);
            }
        }

        //TODO: v0.6 Delete this
        public static object LegacyLoad(string path)
        {
            try
            {
                using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                    IFormatter formatter = new BinaryFormatter();
                    return formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return null;
        }

        //Needed to use this for testing.
        //TODO: v0.6 Delet this
        public static void LegacySave(string path, object entry)
        {
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                IFormatter formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                formatter.Serialize(stream, entry);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
            }
        }

        public static void MergeMod(string mod)
        {
            DiagManager.Instance.LogInfo(String.Format("Creating standalone from mod {0}...", PathMod.Mod));

            string outputPath = Path.Combine(PathMod.ExePath, "Build", mod);

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
            Directory.CreateDirectory(outputPath);

            //Exe - direct copy from game
            File.Copy(Path.Combine(PathMod.ExePath, PathMod.ExeName), Path.Combine(outputPath, PathMod.ExeName));
            //PNG - direct copy from game
            string pngName = Path.GetFileNameWithoutExtension(PathMod.ExeName) + ".png";
            if (File.Exists(Path.Combine(PathMod.ExePath, pngName)))
                File.Copy(Path.Combine(PathMod.ExePath, pngName), Path.Combine(outputPath, pngName));

            //Base - direct copy from game
            copyRecursive(GraphicsManager.BASE_PATH, Path.Combine(outputPath, "Base"));

            //Strings - merged copy
            Directory.CreateDirectory(outputPath);
            copyRecursive(PathMod.NoMod("Strings"), Path.Combine(outputPath, "Strings"));
            copyRecursive(PathMod.HardMod("Strings"), Path.Combine(outputPath, "Strings"));

            //Editor - direct copy from game
            copyRecursive(PathMod.RESOURCE_PATH, Path.Combine(outputPath, "Editor"));

            //Licenses - merged copy
            copyRecursive(PathMod.NoMod("Licenses"), Path.Combine(outputPath, "Licenses"));
            copyRecursive(PathMod.HardMod("Licenses"), Path.Combine(outputPath, "Licenses"));

            //Font - direct copy from game
            copyRecursive(PathMod.NoMod("Font"), Path.Combine(outputPath, "Font"));

            //Content - merged copy
            //TODO: only copy what is indexed for characters and portraits
            copyRecursive(PathMod.NoMod(GraphicsManager.CONTENT_PATH), Path.Combine(outputPath, GraphicsManager.CONTENT_PATH));
            copyRecursive(PathMod.HardMod(GraphicsManager.CONTENT_PATH), Path.Combine(outputPath, GraphicsManager.CONTENT_PATH));

            //Data - merge copy everything except script
            //TODO: only copy what is indexed for characters and portraits
            Directory.CreateDirectory(Path.Combine(outputPath, DataManager.DATA_PATH));
            foreach (string subPath in Directory.GetFiles(PathMod.NoMod(DataManager.DATA_PATH)))
            {
                string path = Path.GetFileName(subPath);
                File.Copy(subPath, Path.Combine(outputPath, DataManager.DATA_PATH, path));
            }
            string exPath = PathMod.NoMod(RogueEssence.Script.LuaEngine.SCRIPT_PATH);
            foreach (string subPath in Directory.GetDirectories(PathMod.NoMod(DataManager.DATA_PATH)))
            {
                if (Path.Equals(subPath + "/", exPath))
                    continue;
                string path = Path.GetFileName(subPath);
                copyRecursive(subPath, Path.Combine(outputPath, DataManager.DATA_PATH, path));
            }
            copyRecursive(PathMod.HardMod(DataManager.DATA_PATH), Path.Combine(outputPath, DataManager.DATA_PATH));

            DiagManager.Instance.LogInfo(String.Format("Standalone game output to {0}", outputPath));
        }

        private static void copyRecursive(string srcDir, string destDir)
        {
            if (!Directory.Exists(srcDir))
                return;

            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            foreach (string directory in Directory.GetDirectories(srcDir))
                copyRecursive(directory, Path.Combine(destDir, Path.GetFileName(directory)));

            foreach (string file in Directory.GetFiles(srcDir))
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
        }
    }
}
