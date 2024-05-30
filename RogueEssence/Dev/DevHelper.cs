using System;
using System.IO;
using RogueEssence.Data;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Xml.Serialization;
using RogueEssence.Content;
using System.Text;
using RogueEssence.Script;

namespace RogueEssence.Dev
{
    public static class DevHelper
    {
        //TODO: this is old conversion code and needs to be deleted, but maybe the code could be salvaged for somemass rename operation?
        public static Version StringAssetVersion = new Version(0, 6, 0);

        public static Version GetVersion(params string[] dirs)
        {
            Version oldVersion = Versioning.GetVersion();
            if (dirs.Length > 0)
            {
                using (Stream stream = new FileStream(dirs[0], FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, -1, true))
                    {
                        string containerStr = reader.ReadToEnd();
                        oldVersion = Serializer.GetVersion(containerStr);
                    }
                }
            }
            return oldVersion;
        } 
        private static Version GetTypeVersion(DataManager.DataType dataType)
        {
            string[] dirs = PathMod.GetHardModFiles(DataManager.DATA_PATH + dataType.ToString() + "/", "*.bin");

            Version oldVersion = GetVersion(dirs);
            return oldVersion;
        }

        private static void convertAssetType(DataManager.DataType dataType)
        {
            string convFolder = PathMod.HardMod("CONVERSION/");
            string[] dirs = PathMod.GetHardModFiles(DataManager.DATA_PATH + dataType.ToString() + "/", "*.bin");
            List<string> intToName = new List<string>();
            HashSet<string> resultNames = new HashSet<string>();
            foreach (string dir in dirs)
            {
                string folder = Path.GetDirectoryName(dir);
                string file = Path.GetFileNameWithoutExtension(dir);
                string resultName = file;
                try
                {
                    int index = Int32.Parse(file);
                    IEntryData data = DataManager.LoadObject<IEntryData>(dir);
                    while (intToName.Count <= index)
                        intToName.Add("");
                    if (data.Name.DefaultText != "")
                    {
                        resultName = Text.Sanitize(data.Name.DefaultText).ToLower();

                        if (resultNames.Contains(resultName))
                            resultName = Text.GetNonConflictingName(resultName, resultNames.Contains);
                    }

                    intToName[index] = file+"\t"+resultName;
                    resultNames.Add(resultName);
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }

            for (int ii = intToName.Count - 1; ii >= 0; ii--)
            {
                if (String.IsNullOrWhiteSpace(intToName[ii]))
                    intToName.RemoveAt(ii);
            }

            File.WriteAllLines(Path.Join(convFolder, dataType.ToString() + ".txt"), intToName);
        }



        private static string[] convertAssetTypeFromTxt(DataManager.DataType dataType)
        {
            string convFolder = PathMod.HardMod("CONVERSION/");
            string dataFolder = PathMod.HardMod(Path.Join(DataManager.DATA_PATH, dataType.ToString()));

            if (!File.Exists(Path.Join(convFolder, dataType.ToString() + ".txt")))
                throw new Exception("Missing conversion text.  Run -preconvert first.");

            string[] lines = File.ReadAllLines(Path.Join(convFolder, dataType.ToString() + ".txt"));
            foreach (string line in lines)
            {
                string[] before_after = line.Trim().Split();
                string firstName = before_after[0];
                string resultName = before_after[1];
                File.Move(Path.Join(dataFolder, firstName + ".bin"), Path.Join(dataFolder, resultName + ".bin"));
            }
            return lines;
        }

        public static void PrepareAssetConversion()
        {
            
            string convFolder = PathMod.HardMod("CONVERSION/");
            if (!Directory.Exists(convFolder))
                Directory.CreateDirectory(convFolder);

            Version oldVersion = GetTypeVersion(DataManager.DataType.AutoTile);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.AutoTile);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Emote);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.Emote);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.GrowthGroup);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.GrowthGroup);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.SkillGroup);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.SkillGroup);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Rank);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.Rank);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Element);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.Element);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.AI);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.AI);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Skin);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.Skin);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Terrain);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.AutoTile);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Tile);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.Tile);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.MapStatus);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.MapStatus);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Status);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.Status);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Intrinsic);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.Intrinsic);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Skill);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.Skill);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Monster);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.Monster);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Item);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetType(DataManager.DataType.Item);
            }


            oldVersion = GetTypeVersion(DataManager.DataType.Zone);
            if (oldVersion < StringAssetVersion)
            {
                //rename all the zone files
                convertAssetType(DataManager.DataType.Zone);
            }
        }

        public static void ConvertAssetNames()
        {
            //load mod xml and resave it
            if (PathMod.Quest.IsValid())
            {
                string modPath = PathMod.FromExe(PathMod.Quest.Path);
                PathMod.SaveModDetails(modPath, PathMod.Quest);
            }

            Version oldVersion = GetTypeVersion(DataManager.DataType.AutoTile);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.AutoTile);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Emote);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.Emote);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.GrowthGroup);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.GrowthGroup);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.SkillGroup);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.SkillGroup);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Rank);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.Rank);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Element);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.Element);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.AI);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.AI);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Skin);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.Skin);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Terrain);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.AutoTile);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Tile);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.Tile);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.MapStatus);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.MapStatus);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Status);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.Status);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Intrinsic);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.Intrinsic);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Skill);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.Skill);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Monster);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.Monster);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Item);
            if (oldVersion < StringAssetVersion)
            {
                //rename all autotile files
                convertAssetTypeFromTxt(DataManager.DataType.Item);
            }

            oldVersion = GetTypeVersion(DataManager.DataType.Zone);
            if (oldVersion < StringAssetVersion)
            {
                //rename all the zone files
                string[] intToName = convertAssetTypeFromTxt(DataManager.DataType.Zone);

                string scriptFolder = PathMod.HardMod(LuaEngine.ZONE_SCRIPT_DIR);
                //now rename the script paths
                for (int ii = 0; ii < intToName.Length; ii++)
                {
                    string[] destPair = intToName[ii].Split('\t');
                    string srcName = "zone_" + destPair[0];
                    string destName = destPair[1];

                    if (Directory.Exists(Path.Join(scriptFolder, srcName)))
                    {
                        foreach (string dir in Directory.GetFiles(Path.Join(scriptFolder, srcName + "/"), "*.lua"))
                        {
                            //open, replace, save
                            string input = File.ReadAllText(dir);
                            string output = input.Replace(srcName, destName);
                            File.WriteAllText(dir, output);
                        }

                        Directory.Move(Path.Join(scriptFolder, srcName), Path.Join(scriptFolder, destName));
                    }
                }
            }
        }

        public static string ReverseEndian(string str)
        {
            string[] words = str.Split(' ');
            Array.Reverse(words);
            return String.Join(' ', words);
        }

        public static void ReserializeBase()
        {
            //All instances of LoadObject in DevHelper need to be reworked to load on both diff and base file?
            //yes, this is so that they can load properly on the reserialization step
            {
                ActiveEffect data = DataManager.LoadModData<ActiveEffect>(PathMod.Quest, DataManager.DATA_PATH, "Universal", DataManager.DATA_EXT);

                //save it as a file or a mod based on whether it was loaded as a diff or not... aka whether it was a diff as a file or not
                //SaveData will do this automatically!
                if (data != null)
                    DataManager.SaveData(data, DataManager.DATA_PATH, "Universal", DataManager.DATA_EXT);
            }

            // search for data EXT as well as PATCH_EXT.
            // This means searching for all files first
            foreach (string dir in PathMod.GetHardModFiles(DataManager.FX_PATH, "*"))
            {
                string fileName = Path.GetFileNameWithoutExtension(dir);
                string ext = Path.GetExtension(dir);
                //then filtering just the data/patch ext
                if (ext == DataManager.DATA_EXT || ext == DataManager.PATCH_EXT)
                {
                    object data;
                    if (fileName == "NoCharge")
                        data = DataManager.LoadModData<EmoteFX>(PathMod.Quest, DataManager.FX_PATH, fileName, DataManager.DATA_EXT);
                    else
                        data = DataManager.LoadModData<BattleFX>(PathMod.Quest, DataManager.FX_PATH, fileName, DataManager.DATA_EXT);
                    DataManager.SaveData(data, DataManager.FX_PATH, fileName, DataManager.DATA_EXT);
                }
            }

            //no need for involving mod paths here
            foreach (string dir in Directory.GetFiles(Path.Combine(PathMod.RESOURCE_PATH, "Extensions"), "*.op"))
            {
                object data;
                data = DataManager.LoadObject<CharSheetOp>(dir);
                DataManager.SaveObject(data, dir);
            }
        }

        public static void Reserialize(DataManager.DataType conversionFlags)
        {
            foreach (DataManager.DataType type in Enum.GetValues(typeof(DataManager.DataType)))
            {
                if (type != DataManager.DataType.All && (conversionFlags & type) != DataManager.DataType.None)
                    ReserializeData<IEntryData>(DataManager.DATA_PATH + type.ToString() + "/", DataManager.DATA_EXT);
            }
        }

        public static void ReserializeData<T>(string dataPath, string ext)
        {
            // search for data EXT as well as diff EXT...
            // This means searching for all files first
            foreach (string dir in PathMod.GetHardModFiles(dataPath, "*"))
            {
                string fileName = Path.GetFileNameWithoutExtension(dir);
                string testExt = Path.GetExtension(dir);
                //then filtering just the data/patch ext
                if (testExt == ext || testExt == DataManager.PATCH_EXT)
                {
                    T data = DataManager.LoadModData<T>(PathMod.Quest, dataPath, fileName, ext);
                    if (data != null)
                        DataManager.SaveData(data, dataPath, fileName, ext);
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
                    IndexNamedData(DataManager.DATA_PATH + type.ToString() + "/");
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
                    DataManager.SaveData(baseData, DataManager.MISC_PATH, baseData.FileName, DataManager.DATA_EXT);
                }
            }
            DataManager.SaveData(DataManager.Instance.UniversalData, DataManager.MISC_PATH, "Index", DataManager.DATA_EXT);
        }


        public static void IndexNamedData(string dataPath)
        {
            try
            {
                Dictionary<string, EntrySummary> entries = new Dictionary<string, EntrySummary>();
                foreach (string dir in Directory.GetFiles(PathMod.HardMod(dataPath), "*" + DataManager.DATA_EXT))
                {
                    string file = Path.GetFileNameWithoutExtension(dir);
                    IEntryData data = DataManager.LoadObject<IEntryData>(dir);
                    entries[file] = data.GenerateEntrySummary();
                }

                if (entries.Count > 0)
                {
                    using (Stream stream = new FileStream(PathMod.HardMod(dataPath + "index.idx"), FileMode.Create, FileAccess.Write, FileShare.None))
                        Serializer.SerializeData(stream, entries);
                }
                else
                    File.Delete(PathMod.HardMod(dataPath + "index.idx"));
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new Exception("Error importing index at " + dataPath + "\n", ex));
            }
        }


        public static void MergeQuest(string quest)
        {
            DiagManager.Instance.LogInfo(String.Format("Creating standalone from quest: {0}", String.Join(", ", quest)));

            string outputPath = Path.Combine(PathMod.ExePath, "Build", quest);

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

            //Strings - deep merge files
            Directory.CreateDirectory(outputPath);
            copyRecursive(PathMod.NoMod("Strings"), Path.Combine(outputPath, "Strings"));
            mergeStringXmlRecursive(PathMod.HardMod("Strings"), Path.Combine(outputPath, "Strings"));

            //Editor - direct copy from game
            copyRecursive(PathMod.RESOURCE_PATH, Path.Combine(outputPath, "Editor"));

            //Licenses - merged copy
            copyRecursive(PathMod.NoMod("Licenses"), Path.Combine(outputPath, "Licenses"));
            copyRecursive(PathMod.HardMod("Licenses"), Path.Combine(outputPath, "Licenses"));

            //Font - direct copy from game
            copyRecursive(PathMod.NoMod("Font"), Path.Combine(outputPath, "Font"));

            //Controls - direct copy from game
            copyRecursive(PathMod.NoMod("Controls"), Path.Combine(outputPath, "Controls"));

            //Content - merged copy for files, deep merge for indices
            //TODO: only copy what is indexed for characters and portraits
            copyRecursive(PathMod.NoMod(GraphicsManager.CONTENT_PATH), Path.Combine(outputPath, GraphicsManager.CONTENT_PATH));
            copyRecursive(PathMod.HardMod(GraphicsManager.CONTENT_PATH), Path.Combine(outputPath, GraphicsManager.CONTENT_PATH));
            //save merged content indices
            {
                CharaIndexNode charaIndex = GraphicsManager.LoadCharaIndices(GraphicsManager.CONTENT_PATH + "Chara/");
                using (FileStream stream = new FileStream(Path.Combine(outputPath, GraphicsManager.CONTENT_PATH + "Chara/" + "index.idx"), FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                        charaIndex.Save(writer);
                }
                CharaIndexNode portraitIndex = GraphicsManager.LoadCharaIndices(GraphicsManager.CONTENT_PATH + "Portrait/");
                using (FileStream stream = new FileStream(Path.Combine(outputPath, GraphicsManager.CONTENT_PATH + "Portrait/" + "index.idx"), FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                        portraitIndex.Save(writer);
                }
                TileGuide tileIndex = GraphicsManager.LoadTileIndices(GraphicsManager.CONTENT_PATH + "Tile/");
                using (FileStream stream = new FileStream(Path.Combine(outputPath, GraphicsManager.CONTENT_PATH + "Tile/" + "index.idx"), FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                        tileIndex.Save(writer);
                }
            }

            //Data - merge copy everything including script
            //script will do fine with duplicate files being merged over, EXCEPT for strings files
            //TODO: only copy what is indexed for characters and portraits
            Directory.CreateDirectory(Path.Combine(outputPath, DataManager.DATA_PATH));

            //universal data, start params, etc.
            foreach (string subPath in Directory.GetFiles(PathMod.NoMod(DataManager.DATA_PATH)))
            {
                string path = Path.GetFileName(subPath);
                File.Copy(subPath, Path.Combine(outputPath, DataManager.DATA_PATH, path));
            }

            //actual data files
            copyRecursive(PathMod.NoMod(DataManager.DATA_PATH), Path.Combine(outputPath, DataManager.DATA_PATH));
            copyRecursive(PathMod.HardMod(DataManager.DATA_PATH), Path.Combine(outputPath, DataManager.DATA_PATH));
            //TODO: merge strings files for ground map strings instead of overwrite

            //save merged data indices
            foreach (DataManager.DataType type in Enum.GetValues(typeof(DataManager.DataType)))
            {
                if (type == DataManager.DataType.All || type == DataManager.DataType.None)
                    continue;

                EntryDataIndex idx = DataManager.GetIndex(type);
                using (Stream stream = new FileStream(Path.Combine(outputPath, DataManager.DATA_PATH + type.ToString() + "/" + "index.idx"), FileMode.Create, FileAccess.Write, FileShare.None))
                    Serializer.SerializeData(stream, idx);
            }


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

        private static void mergeStringXmlRecursive(string srcDir, string destDir)
        {
            if (!Directory.Exists(srcDir))
                return;

            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            foreach (string directory in Directory.GetDirectories(srcDir))
                mergeStringXmlRecursive(directory, Path.Combine(destDir, Path.GetFileName(directory)));

            foreach (string file in Directory.GetFiles(srcDir))
            {
                if (Path.GetExtension(file) == ".resx")
                    mergeStringXml(file, Path.Combine(destDir, Path.GetFileName(file)));
                else
                    File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
            }
        }

        private static void mergeStringXml(string srcPath, string destPath)
        {
            Dictionary<string, (string val, string comment)> srcDict = Text.LoadDevStringResx(srcPath);
            Dictionary<string, (string val, string comment)> destDict = Text.LoadDevStringResx(destPath);
            foreach (string key in srcDict.Keys)
                destDict[key] = srcDict[key];
            Text.SaveStringResx(destPath, destDict);
        }
    }
}
