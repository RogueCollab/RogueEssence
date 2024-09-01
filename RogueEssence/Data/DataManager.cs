using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using RogueEssence.Ground;
using RogueEssence.Dev;
using RogueEssence.Script;
using RogueEssence.Content;
using System.Xml;
using System.Threading;
using Newtonsoft.Json;

namespace RogueEssence.Data
{
    public static class DataTypeExtensions
    {
        public static Type GetClassType(this DataManager.DataType dataType)
        {
            switch (dataType)
            {
                case DataManager.DataType.Monster:
                    return typeof(MonsterData);
                case DataManager.DataType.Skill:
                    return typeof(SkillData);
                case DataManager.DataType.Item:
                    return typeof(ItemData);
                case DataManager.DataType.Intrinsic:
                    return typeof(IntrinsicData);
                case DataManager.DataType.Status:
                    return typeof(StatusData);
                case DataManager.DataType.MapStatus:
                    return typeof(MapStatusData);
                case DataManager.DataType.Terrain:
                    return typeof(TerrainData);
                case DataManager.DataType.Tile:
                    return typeof(TileData);
                case DataManager.DataType.Zone:
                    return typeof(ZoneData);
                case DataManager.DataType.Emote:
                    return typeof(EmoteData);
                case DataManager.DataType.AutoTile:
                    return typeof(AutoTileData);
                case DataManager.DataType.Element:
                    return typeof(ElementData);
                case DataManager.DataType.GrowthGroup:
                    return typeof(GrowthData);
                case DataManager.DataType.SkillGroup:
                    return typeof(SkillGroupData);
                case DataManager.DataType.AI:
                    return typeof(AITactic);
                case DataManager.DataType.Rank:
                    return typeof(RankData);
                case DataManager.DataType.Skin:
                    return typeof(SkinData);
            }
            throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
        }
    }
    
    //Manages data such as items, intrinsics, etc.  Also holds save data
    public class DataManager
    {
        [Flags]
        public enum DataType
        {
            None = 0,
            Monster = 1,
            Skill = 2,
            Item = 4,
            Intrinsic = 8,
            Status = 16,
            MapStatus = 32,
            Terrain = 64,
            Tile = 128,
            Zone = 256,
            Emote = 512,
            AutoTile = 1024,
            Element = 2048,
            GrowthGroup = 4096,
            SkillGroup = 8192,
            AI = 16384,
            Rank = 32768,
            Skin = 65536,
            All = 131071
        }

        public enum LoadMode
        {
            None,
            Loading,
            Rescuing,
            Verifying
        }

        public enum SavePolicy
        {
            FileDiff,
            File,
            Diff
        }

        public enum ModStatus
        {
            Base,
            Added,
            Modded,
            DiffModded
        }

        private static DataManager instance;
        public static void InitInstance()
        {
            instance = new DataManager();
        }
        public static DataManager Instance { get { return instance; } }

        public const string DATA_PATH = "Data/";
        public const string MISC_PATH = DATA_PATH + "Misc/";
        public const string MAP_FOLDER = "Map/";
        public const string GROUND_FOLDER = "Ground/";
        public const string MAP_PATH = DATA_PATH + MAP_FOLDER;
        public const string GROUND_PATH = DATA_PATH + GROUND_FOLDER;
        public const string DATA_EXT = ".json";
        public const string PATCH_EXT = ".jsonpatch";
        public const string MAP_EXT = ".rsmap";
        public const string GROUND_EXT = ".rsground";

        public const string FX_PATH = DATA_PATH + "SystemFX/";

        public const string SAVE_PATH = "SAVE/";
        public const string REPLAY_PATH = "REPLAY/";
        public const string RESCUE_IN_PATH = "RESCUE/INBOX/";
        public const string RESCUE_OUT_PATH = "RESCUE/OUTBOX/";
        public const string SOS_FOLDER = "SOS/";
        public const string AOK_FOLDER = "AOK/";

        public const string ROGUE_PATH = SAVE_PATH + "ROGUE/";


        public const string SAVE_FILE_PATH = "SAVE.rssv";
        public const string QUICKSAVE_EXTENSION = ".rsqs";
        public const string QUICKSAVE_FILE_PATH = "QUICKSAVE" + QUICKSAVE_EXTENSION;

        public const string SOS_EXTENSION = ".sosmail";
        public const string AOK_EXTENSION = ".aokmail";
        public const string REPLAY_EXTENSION = ".rsrec";

        private const int ITEM_CACHE_SIZE = 100;
        private const int STATUS_CACHE_SIZE = 50;
        private const int INSTRINSIC_CACHE_SIZE = 50;
        private const int SKILL_CACHE_SIZE = 100;
        private const int MONSTER_CACHE_SIZE = 50;
        private const int AUTOTILE_CACHE_SIZE = 10;
        private const int MAP_STATUS_CACHE_SIZE = 50;


        private Thread preLoadZoneThread;
        private LRUCache<string, ZoneData> zoneCache;
        private LRUCache<string, ItemData> itemCache;
        private LRUCache<string, StatusData> statusCache;
        private LRUCache<string, IntrinsicData> intrinsicCache;
        private LRUCache<string, SkillData> skillCache;
        private LRUCache<string, MonsterData> monsterCache;
        private LRUCache<string, AutoTileData> autoTileCache;
        private LRUCache<string, MapStatusData> mapStatusCache;
        private Dictionary<string, TileData> tileCache;
        private Dictionary<string, TerrainData> terrainCache;
        private Dictionary<string, EmoteData> emoteCache;
        private Dictionary<string, ElementData> elementCache;
        private Dictionary<string, GrowthData> growthCache;
        private Dictionary<string, SkillGroupData> skillgroupCache;
        private Dictionary<string, AITactic> aiCache;
        private Dictionary<string, RankData> rankCache;
        private Dictionary<string, SkinData> skinCache;

        public Dictionary<DataType, EntryDataIndex> DataIndices;

        /// <summary>
        /// The parameters governing the start of the game.
        /// Such as starting character, map, level, etc.
        /// </summary>
        public StartParams Start;

        public MonsterID DefaultMonsterID { get { return new MonsterID(DefaultMonster, 0, DefaultSkin, Gender.Genderless); } }

        /// <summary>
        /// The monster ID consiered default for purposes of initialization
        /// </summary>
        public string DefaultMonster;

        /// <summary>
        /// The skill ID considered default for purposes of initialization and comparing to "nothing"
        /// </summary>
        public string DefaultSkill;

        /// <summary>
        /// The skill ID considered default for purposes of initialization and comparing to "nothing"
        /// </summary>
        public string DefaultIntrinsic;

        /// <summary>
        /// The skill ID considered default for purposes of initialization and comparing to "nothing"
        /// </summary>
        public string DefaultMapStatus;

        /// <summary>
        /// The skill ID considered default for purposes of initialization and comparing to "nothing"
        /// </summary>
        public string DefaultElement;

        /// <summary>
        /// The skill ID considered default for purposes of initialization and comparing to "nothing"
        /// </summary>
        public string DefaultTile;

        /// <summary>
        /// The skill ID considered default for purposes of initialization.
        /// </summary>
        public string DefaultZone;

        /// <summary>
        /// The skill ID considered default for purposes of initialization.
        /// </summary>
        public string DefaultRank;

        /// <summary>
        /// The skill ID considered default for purposes of initialization.
        /// </summary>
        public string DefaultAI;

        /// <summary>
        /// The skill ID considered default for purposes of initialization.
        /// </summary>
        public string DefaultSkin;

        /// <summary>
        /// The terrain ID considered to be universally "floor" in random dungeon generation
        /// </summary>
        public string GenFloor;

        /// <summary>
        /// The terrain ID considered to be universally "wall" in random dungeon generation
        /// </summary>
        public string GenWall;

        /// <summary>
        /// The terrain ID considered to be universally "unbreakable" in random dungeon generation
        /// </summary>
        public string GenUnbreakable;
        
        public UniversalBaseEffect UniversalEvent;
        
        public TypeDict<BaseData> UniversalData;

        public BattleFX HealFX;
        public BattleFX RestoreChargeFX;
        public BattleFX LoseChargeFX;
        public EmoteFX NoChargeFX;
        public BattleFX ElementFX;
        public BattleFX IntrinsicFX;
        public BattleFX SendHomeFX;
        public BattleFX ItemLostFX;
        public BattleFX WarpFX;
        public BattleFX KnockbackFX;
        public BattleFX JumpFX;
        public BattleFX ThrowFX;

        /// <summary>
        /// The current save file, loaded into memory
        /// </summary>
        public GameProgress Save { get; private set; }

        public List<string> MsgLog;


        public bool HideObjects;
        public bool HideChars;

        public bool RecordingReplay { get { return (replayWriter != null); } }
        private BinaryWriter replayWriter;
        private long replayGroundStatePos;
        public ReplayData CurrentReplay;
        public LoadMode Loading;


        public DataManager()
        {
            if (!Directory.Exists(PathMod.ModSavePath(SAVE_PATH)))
                Directory.CreateDirectory(PathMod.ModSavePath(SAVE_PATH));
            if (!Directory.Exists(PathMod.ModSavePath(ROGUE_PATH)))
                Directory.CreateDirectory(PathMod.ModSavePath(ROGUE_PATH));
            if (!Directory.Exists(PathMod.ModSavePath(REPLAY_PATH)))
                Directory.CreateDirectory(PathMod.ModSavePath(REPLAY_PATH));

            if (!Directory.Exists(PathMod.FromApp(RESCUE_IN_PATH + SOS_FOLDER)))
                Directory.CreateDirectory(PathMod.FromApp(RESCUE_IN_PATH + SOS_FOLDER));

            if (!Directory.Exists(PathMod.FromApp(RESCUE_IN_PATH + AOK_FOLDER)))
                Directory.CreateDirectory(PathMod.FromApp(RESCUE_IN_PATH + AOK_FOLDER));

            if (!Directory.Exists(PathMod.FromApp(RESCUE_OUT_PATH + SOS_FOLDER)))
                Directory.CreateDirectory(PathMod.FromApp(RESCUE_OUT_PATH + SOS_FOLDER));

            if (!Directory.Exists(PathMod.FromApp(RESCUE_OUT_PATH + AOK_FOLDER)))
                Directory.CreateDirectory(PathMod.FromApp(RESCUE_OUT_PATH + AOK_FOLDER));


            MsgLog = new List<string>();

            zoneCache = new LRUCache<string, ZoneData>(1);
            itemCache = new LRUCache<string, ItemData>(ITEM_CACHE_SIZE);
            statusCache = new LRUCache<string, StatusData>(STATUS_CACHE_SIZE);
            intrinsicCache = new LRUCache<string, IntrinsicData>(INSTRINSIC_CACHE_SIZE);
            skillCache = new LRUCache<string, SkillData>(SKILL_CACHE_SIZE);
            monsterCache = new LRUCache<string, MonsterData>(MONSTER_CACHE_SIZE);
            autoTileCache = new LRUCache<string, AutoTileData>(AUTOTILE_CACHE_SIZE);
            mapStatusCache = new LRUCache<string, MapStatusData>(MAP_STATUS_CACHE_SIZE);
            tileCache = new Dictionary<string, TileData>();
            terrainCache = new Dictionary<string, TerrainData>();
            emoteCache = new Dictionary<string, EmoteData>();
            elementCache = new Dictionary<string, ElementData>();
            growthCache = new Dictionary<string, GrowthData>();
            skillgroupCache = new Dictionary<string, SkillGroupData>();
            aiCache = new Dictionary<string, AITactic>();
            rankCache = new Dictionary<string, RankData>();
            skinCache = new Dictionary<string, SkinData>();

            DataIndices = new Dictionary<DataType, EntryDataIndex>();
            UniversalData = new TypeDict<BaseData>();
        }

        public void InitBase()
        {
            HealFX = LoadData<BattleFX>(FX_PATH, "Heal", DATA_EXT);
            RestoreChargeFX = LoadData<BattleFX>(FX_PATH, "RestoreCharge", DATA_EXT);
            LoseChargeFX = LoadData<BattleFX>(FX_PATH, "LoseCharge", DATA_EXT);
            NoChargeFX = LoadData<EmoteFX>(FX_PATH, "NoCharge", DATA_EXT);
            ElementFX = LoadData<BattleFX>(FX_PATH, "Element", DATA_EXT);
            IntrinsicFX = LoadData<BattleFX>(FX_PATH, "Intrinsic", DATA_EXT);
            SendHomeFX = LoadData<BattleFX>(FX_PATH, "SendHome", DATA_EXT);
            ItemLostFX = LoadData<BattleFX>(FX_PATH, "ItemLost", DATA_EXT);
            WarpFX = LoadData<BattleFX>(FX_PATH, "Warp", DATA_EXT);
            KnockbackFX = LoadData<BattleFX>(FX_PATH, "Knockback", DATA_EXT);
            JumpFX = LoadData<BattleFX>(FX_PATH, "Jump", DATA_EXT);
            ThrowFX = LoadData<BattleFX>(FX_PATH, "Throw", DATA_EXT);

            UniversalEvent = LoadData<UniversalBaseEffect>(DATA_PATH, "Universal", DATA_EXT);

            UniversalData = LoadData<TypeDict<BaseData>>(MISC_PATH, "Index", DATA_EXT);
            LoadStartParams();
        }

        public void InitDataIndices()
        {
            LoadConversions();
            LoadIndex(DataType.Item);
            itemCache.Clear();
            LoadIndex(DataType.Skill);
            skillCache.Clear();
            LoadIndex(DataType.Monster);
            monsterCache.Clear();
            LoadIndex(DataType.Zone);
            zoneCache.Clear();
            LoadIndex(DataType.Status);
            statusCache.Clear();
            LoadIndex(DataType.Intrinsic);
            intrinsicCache.Clear();
            LoadIndex(DataType.AutoTile);
            autoTileCache.Clear();
            LoadIndex(DataType.MapStatus);
            mapStatusCache.Clear();
            LoadIndexFull(DataType.Tile, tileCache);
            LoadIndexFull(DataType.Terrain, terrainCache);
            LoadIndexFull(DataType.Emote, emoteCache);
            LoadIndexFull(DataType.Element, elementCache);
            LoadIndexFull(DataType.GrowthGroup, growthCache);
            LoadIndexFull(DataType.SkillGroup, skillgroupCache);
            LoadIndexFull(DataType.AI, aiCache);
            LoadIndexFull(DataType.Rank, rankCache);
            LoadIndexFull(DataType.Skin, skinCache);
            LoadUniversalIndices();
        }

        public void InitData()
        {
            InitBase();
            InitDataIndices();
        }



        public static void InitDataDirs(string baseFolder)
        {
            Directory.CreateDirectory(Path.Join(baseFolder, DATA_PATH));
            foreach (DataType type in Enum.GetValues(typeof(DataType)))
            {
                if (type != DataManager.DataType.All && type != DataManager.DataType.None)
                    Directory.CreateDirectory(Path.Join(baseFolder, DATA_PATH + type.ToString() + "/"));
            }
            Directory.CreateDirectory(Path.Join(baseFolder, MAP_PATH));
            Directory.CreateDirectory(Path.Join(baseFolder, GROUND_PATH));
            Directory.CreateDirectory(Path.Join(baseFolder, FX_PATH));
            Directory.CreateDirectory(Path.Join(baseFolder, MISC_PATH));
        }

        public static void InitSaveDirs()
        {
            Directory.CreateDirectory(PathMod.ModSavePath(SAVE_PATH));
            Directory.CreateDirectory(PathMod.ModSavePath(REPLAY_PATH));
        }

        public Dictionary<DataType, Dictionary<string, string>> Conversions;

        public void LoadConversions()
        {
            Conversions = new Dictionary<DataType, Dictionary<string, string>>();

            foreach (DataType type in Enum.GetValues(typeof(DataType)))
            {
                Dictionary<string, string> convMap = new Dictionary<string, string>();
                if (type != DataManager.DataType.All && type != DataManager.DataType.None)
                {
                    foreach (string modPath in PathMod.FallforthPaths("CONVERSION/" + type.ToString() + ".txt"))
                    {
                        if (File.Exists(modPath))
                        {
                            string[] lines = File.ReadAllLines(modPath);
                            for (int ii = 0; ii < lines.Length; ii++)
                            {
                                if (!String.IsNullOrWhiteSpace(lines[ii]))
                                {
                                    string[] split = lines[ii].Split('\t');
                                    convMap[split[0]] = split[1];
                                }
                            }
                        }
                    }
                }
                Conversions[type] = convMap;
            }
        }

        public string MapAssetName(DataType dataType, int asset)
        {
            try
            {
                if (asset < 0)
                    return "";
                return Conversions[dataType][asset.ToString()];
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                return "";
            }
        }

        public string MapAssetName(DataType dataType, string asset)
        {
            try
            {
                if (String.IsNullOrEmpty(asset))
                    return "";
                return Conversions[dataType][asset];
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                return "";
            }
        }


        public void LoadUniversalIndices()
        {
            //Universal indices on the implementer's side
            foreach (BaseData baseData in UniversalData)
            {
                try
                {
                    BaseData data = LoadData<BaseData>(MISC_PATH, baseData.FileName, DATA_EXT);
                    UniversalData.Set(data);
                }
                catch
                {
                    //leave as is
                }
            }
        }

        private void LoadStartParams()
        {
            Start = new StartParams();
            string path = PathMod.ModPath(DATA_PATH + "StartParams.xml");
            //try to load from file
            if (File.Exists(path))
            {
                try
                {
                    Start.Chars = new List<StartChar>();
                    Start.Teams = new List<string>();

                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);

                    XmlNode startChars = xmldoc.DocumentElement.SelectSingleNode("StartChars");
                    foreach (XmlNode startChar in startChars.SelectNodes("StartChar"))
                    {
                        XmlNode startSpecies = startChar.SelectSingleNode("Species");
                        string species = startSpecies.InnerText;
                        XmlNode startForm = startChar.SelectSingleNode("Form");
                        int form = Int32.Parse(startForm.InnerText);
                        XmlNode startSkin = startChar.SelectSingleNode("Skin");
                        string skin = startSkin.InnerText;
                        XmlNode startGender = startChar.SelectSingleNode("Gender");
                        Gender gender = Enum.Parse<Gender>(startGender.InnerText);

                        XmlNode startName = startChar.SelectSingleNode("Name");
                        string name = startName.InnerText;

                        Start.Chars.Add(new StartChar(new MonsterID(species, form, skin, gender), name));
                    }

                    XmlNode startTeams = xmldoc.DocumentElement.SelectSingleNode("StartTeams");
                    foreach (XmlNode startTeam in startTeams.SelectNodes("StartTeam"))
                        Start.Teams.Add(startTeam.InnerText);

                    XmlNode startLevel = xmldoc.DocumentElement.SelectSingleNode("StartLevel");
                    Start.Level = Int32.Parse(startLevel.InnerText);

                    XmlNode maxLevel = xmldoc.DocumentElement.SelectSingleNode("MaxLevel");
                    Start.MaxLevel = Int32.Parse(maxLevel.InnerText);

                    XmlNode startPersonality = xmldoc.DocumentElement.SelectSingleNode("StartPersonality");
                    Start.Personality = Int32.Parse(startPersonality.InnerText);

                    DefaultZone = xmldoc.DocumentElement.SelectSingleNode("DefaultZone").InnerText;
                    DefaultRank = xmldoc.DocumentElement.SelectSingleNode("DefaultRank").InnerText;
                    DefaultSkin = xmldoc.DocumentElement.SelectSingleNode("DefaultSkin").InnerText;
                    DefaultAI = xmldoc.DocumentElement.SelectSingleNode("DefaultAI").InnerText;
                    DefaultTile = xmldoc.DocumentElement.SelectSingleNode("DefaultTile").InnerText;
                    DefaultElement = xmldoc.DocumentElement.SelectSingleNode("DefaultElement").InnerText;
                    DefaultMapStatus = xmldoc.DocumentElement.SelectSingleNode("DefaultMapStatus").InnerText;
                    DefaultIntrinsic = xmldoc.DocumentElement.SelectSingleNode("DefaultIntrinsic").InnerText;
                    DefaultSkill = xmldoc.DocumentElement.SelectSingleNode("DefaultSkill").InnerText;
                    DefaultMonster = xmldoc.DocumentElement.SelectSingleNode("DefaultMonster").InnerText;

                    XmlNode startMap = xmldoc.DocumentElement.SelectSingleNode("StartMap");
                    Start.Map = new ZoneLoc(startMap.SelectSingleNode("Zone").InnerText,
                        new SegLoc(Int32.Parse(startMap.SelectSingleNode("Segment").InnerText), Int32.Parse(startMap.SelectSingleNode("ID").InnerText)),
                        Int32.Parse(startMap.SelectSingleNode("Entry").InnerText));

                    GenFloor = xmldoc.DocumentElement.SelectSingleNode("GenFloor").InnerText;
                    GenWall = xmldoc.DocumentElement.SelectSingleNode("GenWall").InnerText;
                    GenUnbreakable = xmldoc.DocumentElement.SelectSingleNode("GenUnbreakable").InnerText;
                    return;
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                    Start = new StartParams();
                }
            }
        }

        public void SaveStartParams()
        {
            string path = PathMod.HardMod(DATA_PATH + "StartParams.xml");
            try
            {
                XmlDocument xmldoc = new XmlDocument();

                XmlNode docNode = xmldoc.CreateElement("root");
                xmldoc.AppendChild(docNode);

                XmlNode charsNode = xmldoc.CreateElement("StartChars");
                docNode.AppendChild(charsNode);

                foreach (StartChar startChar in Start.Chars)
                {
                    XmlNode charNode = xmldoc.CreateElement("StartChar");
                    charsNode.AppendChild(charNode);

                    charNode.AppendInnerTextChild(xmldoc, "Species", startChar.ID.Species.ToString());
                    charNode.AppendInnerTextChild(xmldoc, "Form", startChar.ID.Form.ToString());
                    charNode.AppendInnerTextChild(xmldoc, "Skin", startChar.ID.Skin.ToString());
                    charNode.AppendInnerTextChild(xmldoc, "Gender", startChar.ID.Gender.ToString());
                    charNode.AppendInnerTextChild(xmldoc, "Name", startChar.Name);
                }

                XmlNode teamsNode = xmldoc.CreateElement("StartTeams");
                docNode.AppendChild(teamsNode);

                foreach (string startTeam in Start.Teams)
                    teamsNode.AppendInnerTextChild(xmldoc, "StartTeam", startTeam);

                docNode.AppendInnerTextChild(xmldoc, "StartLevel", Start.Level.ToString());
                docNode.AppendInnerTextChild(xmldoc, "MaxLevel", Start.MaxLevel.ToString());
                docNode.AppendInnerTextChild(xmldoc, "StartPersonality", Start.Personality.ToString());

                docNode.AppendInnerTextChild(xmldoc, "DefaultZone", DefaultZone.ToString());
                docNode.AppendInnerTextChild(xmldoc, "DefaultRank", DefaultRank.ToString());
                docNode.AppendInnerTextChild(xmldoc, "DefaultSkin", DefaultSkin.ToString());
                docNode.AppendInnerTextChild(xmldoc, "DefaultAI", DefaultAI.ToString());
                docNode.AppendInnerTextChild(xmldoc, "DefaultTile", DefaultTile.ToString());
                docNode.AppendInnerTextChild(xmldoc, "DefaultElement", DefaultElement.ToString());
                docNode.AppendInnerTextChild(xmldoc, "DefaultMapStatus", DefaultMapStatus.ToString());
                docNode.AppendInnerTextChild(xmldoc, "DefaultIntrinsic", DefaultIntrinsic.ToString());
                docNode.AppendInnerTextChild(xmldoc, "DefaultSkill", DefaultSkill.ToString());
                docNode.AppendInnerTextChild(xmldoc, "DefaultMonster", DefaultMonster.ToString());


                XmlNode mapNode = xmldoc.CreateElement("StartMap");
                docNode.AppendChild(mapNode);

                mapNode.AppendInnerTextChild(xmldoc, "Zone", Start.Map.ID.ToString());
                mapNode.AppendInnerTextChild(xmldoc, "Segment", Start.Map.StructID.Segment.ToString());
                mapNode.AppendInnerTextChild(xmldoc, "ID", Start.Map.StructID.ID.ToString());
                mapNode.AppendInnerTextChild(xmldoc, "Entry", Start.Map.EntryPoint.ToString());

                docNode.AppendInnerTextChild(xmldoc, "GenFloor", GenFloor.ToString());
                docNode.AppendInnerTextChild(xmldoc, "GenWall", GenWall.ToString());
                docNode.AppendInnerTextChild(xmldoc, "GenUnbreakable", GenUnbreakable.ToString());

                xmldoc.Save(path);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public void Unload()
        {
            //Notify script engine
            LuaEngine.Instance.OnDataUnload();
            //close the file gracefully
            if (replayWriter != null)
                replayWriter.Close();
        }


        public void LoadIndex(DataType type)
        {
            DataIndices[type] = GetIndex(type);
        }

        /// <summary>
        /// Index paths are modified like mods.  However, if multiple mods have conflicting indices, a combined index must be generated.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static EntryDataIndex GetIndex(DataType type)
        {
            try
            {
                EntryDataIndex compositeIndex = new EntryDataIndex();
                foreach ((ModHeader, string) modWithPath in PathMod.FallforthPathsWithHeader(DATA_PATH + type.ToString() + "/index.idx"))
                {
                    using (Stream stream = new FileStream(modWithPath.Item2, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        Dictionary<string, EntrySummary> result = (Dictionary<string, EntrySummary>)Serializer.DeserializeData(stream);
                        foreach(string key in result.Keys)
                            compositeIndex.Set(modWithPath.Item1.UUID, key, result[key]);
                    }
                }
                return compositeIndex;
            }
            catch
            {
                return new EntryDataIndex();
            }
        }
        public void LoadIndexFull<T>(DataType type, Dictionary<string, T> cache) where T : IEntryData
        {
            LoadIndex(type);
            LoadCacheFull(type, cache);
        }
        public void LoadCacheFull<T>(DataType type, Dictionary<string, T> cache) where T : IEntryData
        {
            try
            {
                cache.Clear();
                foreach (string key in DataIndices[type].GetOrderedKeys(true))
                {
                    bool firstIdx = false;
                    foreach ((Guid, EntrySummary) tuple in DataIndices[type].IterateKey(key))
                    {
                        ModHeader mod = PathMod.GetModFromUuid(tuple.Item1);
                        T data = LoadModEntryData<T>(mod, key, type.ToString());
                        if (data != null)
                        {
                            cache.Add(mod.Namespace + ":" + key, data);

                            if (!firstIdx)
                                cache.Add(key, data);
                            firstIdx = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                cache.Clear();
            }
        }

        public void SaveIndex(DataType type)
        {
            using (Stream stream = new FileStream(PathMod.HardMod(DATA_PATH + type.ToString() + "/index.idx"), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Dictionary<string, EntrySummary> thisModIndex = DataIndices[type].GetModIndex(PathMod.Quest.UUID);
                Serializer.SerializeData(stream, thisModIndex);
            }
        }

        public void ContentResaved(DataType dataType, string entryNum, IEntryData data, bool asDiff)
        {
            SaveEntryData(entryNum, dataType.ToString(), data, asDiff ? SavePolicy.Diff : SavePolicy.File);

            ModStatus modStatus = GetEntryDataModStatus(entryNum, dataType.ToString());
            if (modStatus != ModStatus.Base)
            {
                EntrySummary entrySummary = data.GenerateEntrySummary();
                DataIndices[dataType].Set(PathMod.Quest.UUID, entryNum, entrySummary);
            }
            else
                DataIndices[dataType].Remove(PathMod.Quest.UUID, entryNum);
            SaveIndex(dataType);

            //Don't need to clear cache

            //don't need to save universal indices

            //don't need to reload editor data
        }

        public void ContentChanged(DataType dataType, string entryNum, IEntryData data)
        {
            if (data != null)
            {
                SaveEntryData(entryNum, dataType.ToString(), data);
                EntrySummary entrySummary = data.GenerateEntrySummary();
                DataIndices[dataType].Set(PathMod.Quest.UUID, entryNum, entrySummary);
            }
            else
            {
                DeleteEntryData(entryNum, dataType.ToString());
                DataIndices[dataType].Remove(PathMod.Quest.UUID, entryNum);
            }
            SaveIndex(dataType);
            ClearCache(dataType);

            foreach (BaseData baseData in UniversalData)
            {
                if ((baseData.TriggerType & dataType) != DataManager.DataType.None)
                {
                    baseData.ContentChanged(entryNum);
                    DataManager.SaveData(baseData, DataManager.MISC_PATH, baseData.FileName, DATA_EXT);
                }
            }

            DiagManager.Instance.DevEditor.ReloadData(dataType);
        }

        public static T LoadNamespacedData<T>(string namespacedNum, string subPath, string ext = DATA_EXT) where T : IEntryData
        {
            string[] components = namespacedNum.Split(':');
            T result;
            if (components.Length > 1)
            {
                ModHeader mod = PathMod.GetModFromNamespace(components[0]);
                result = LoadModEntryData<T>(mod, components[1], subPath, ext);
            }
            else
                result = LoadEntryData<T>(components[0], subPath, ext);
            
            if (result == null)
                throw new FileNotFoundException(String.Format("Could not find {0} ID: '{1}'", subPath, namespacedNum));
            return result;
        }

        public static T LoadModEntryData<T>(ModHeader mod, string indexNum, string subPath, string ext = DATA_EXT) where T : IEntryData
        {
            return LoadModData<T>(mod, DATA_PATH + subPath, indexNum, ext);
        }

        public static T LoadEntryData<T>(string indexNum, string subPath, string ext = DATA_EXT) where T : IEntryData
        {
            return LoadData<T>(DATA_PATH + subPath, indexNum, ext);
        }

        /// <summary>
        /// Loads the data of the specified mod, and does not fall back to base if there is no mod.
        /// Used for reserializing/resaving where either the base or the mod's files ONLY need to be resaved.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mod"></param>
        /// <param name="subpath"></param>
        /// <param name="file"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static T LoadModData<T>(ModHeader mod, string subpath, string file, string ext)
        {
            string fullPath = PathMod.HardMod(mod.Path, Path.Join(subpath, file + ext));
            string diffPath = PathMod.HardMod(mod.Path, Path.Join(subpath, file + PATCH_EXT));

            string filePath = null;
            List<string> diffPaths = new List<string>();
            foreach (string modPath in PathMod.FallbackPaths(subpath))
            {
                //do not add to diff list until we reach the desired hardmod
                //and STOP when we get to a full file
                //but do we want hardmod to be THIS mod's diff on top of base?
                //or THIS mod's diff on top of everything else on top of base?
                //...the latter!
                string newPath = Path.Join(modPath, file + ext);
                string newDiffPath = Path.Join(modPath, file + PATCH_EXT);
                if (diffPaths.Count > 0 || newPath == fullPath || newDiffPath == diffPath)
                {
                    if (File.Exists(newPath))
                    {
                        filePath = newPath;
                        break;
                    }
                    if (File.Exists(newDiffPath))
                        diffPaths.Insert(0, newDiffPath);
                }
            }
            if (filePath != null)
                return LoadObject<T>(filePath, diffPaths.ToArray());
            return default(T);
        }

        public static T LoadData<T>(string subpath, string file, string ext)
        {
            //fall back on paths
            //fall back until a file with ext is found
            //all diff files found along the way need to be included in the argument pass
            string filePath = null;
            List<string> diffPaths = new List<string>();
            foreach (string modPath in PathMod.FallbackPaths(subpath))
            {
                string newPath = Path.Join(modPath, file + ext);
                if (File.Exists(newPath))
                {
                    filePath = newPath;
                    break;
                }
                string newDiffPath = Path.Join(modPath, file + PATCH_EXT);
                if (File.Exists(newDiffPath))
                    diffPaths.Insert(0, newDiffPath);
            }
            if (filePath != null)
                return LoadObject<T>(filePath, diffPaths.ToArray());
            return default(T);
        }

        //All instances of LoadObject in DevHelper need to be reworked to load on both diff and base file?
        //yes, this is so that they can load properly on the reserialization step
        //presumably, it should be saved based on whether it was loaded as a diff or not...
        public static T LoadObject<T>(string path, params string[] diffpaths)
        {
            return (T)loadObject(typeof(T), path, diffpaths);
        }

        private static object loadObject(Type t, string path, params string[] diffpaths)
        {
            try
            {
                if (diffpaths.Length == 0)
                {
                    using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        return Serializer.DeserializeData(stream);
                    }
                }
                else
                    return Serializer.DeserializeDataWithDiffs(path, diffpaths);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new FileLoadException("Could not deserialize file", path, ex));
                return null;
            }
        }


        public static ModStatus GetEntryDataModStatus(string indexNum, string subPath)
        {
            return GetDataModStatus(Path.Join(DATA_PATH, subPath), indexNum, DATA_EXT);
        }

        /// <summary>
        /// Returns information of how a file has been modded, if at all.
        /// </summary>
        /// <param name="subpath"></param>
        /// <param name="file"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static ModStatus GetDataModStatus(string subpath, string file, string ext)
        {
            string folder = PathMod.HardMod(subpath);
            if (File.Exists(Path.Join(folder, file + ext)))
            {
                string baseFolder = PathMod.NoMod(subpath);
                if (!File.Exists(Path.Join(baseFolder, file + ext)))
                    return ModStatus.Added;
                else
                    return ModStatus.Modded;
            }
            if (File.Exists(Path.Join(folder, file + PATCH_EXT)))
                return ModStatus.DiffModded;
            return ModStatus.Base;
        }

        public static void SaveEntryData(string indexNum, string subPath, IEntryData entry, SavePolicy savePolicy = SavePolicy.FileDiff)
        {
            SaveData(entry, Path.Join(DATA_PATH, subPath), indexNum, DATA_EXT, savePolicy);
        }

        /// <summary>
        /// Provides the ability to save it as a file or a mod based on whether it was loaded as a diff or not... aka whether it was a diff as a file or not.
        /// Can also save explicitly as a file or diff.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="subpath"></param>
        /// <param name="file"></param>
        /// <param name="ext"></param>
        /// <param name="savePolicy"></param>
        public static void SaveData(object entry, string subpath, string file, string ext, SavePolicy savePolicy = SavePolicy.FileDiff)
        {
            string folder = PathMod.HardMod(subpath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            //Check if a diff file is located here
            bool saveAsDiff;
            switch (savePolicy)
            {
                case SavePolicy.File:
                    saveAsDiff = false;
                    break;
                case SavePolicy.Diff:
                    saveAsDiff = true;
                    break;
                default:
                    saveAsDiff = File.Exists(Path.Join(folder, file + PATCH_EXT));
                    break;
            }

            string saveDest = Path.Join(folder, file + ext);
            string baseFile = PathMod.NoMod(Path.Join(subpath, file + ext));
            if (saveAsDiff && saveDest != baseFile && File.Exists(baseFile)) //if so, call SaveObject with the diff ext, and the additional argument consisting of the base item
                SaveObject(entry, saveDest, baseFile);
            else //if not, just save as normal
                SaveObject(entry, saveDest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="path">The location to save the file if not as a patch.</param>
        /// <param name="basePath">The base file to diff the json against.  Do not save as a patch if left blank.</param>
        public static void SaveObject(object entry, string path, string basePath = "")
        {
            //The location of a hypothetical patch file.
            //if basePath is not empty, save as the patch file.  if basePath is empty, save as a full file
            string directory = Path.GetDirectoryName(path);
            string file = Path.GetFileNameWithoutExtension(path);
            string diffPath = Path.Join(directory, file + PATCH_EXT);
            if (basePath == "")
            {
                using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Serializer.SerializeData(stream, entry);
                }

                //Delete the diff file, if it exists
                if (File.Exists(diffPath))
                    File.Delete(diffPath);
            }
            else
            {
                Serializer.SerializeDataAsDiff(diffPath, basePath, entry);

                //Delete the non-diff file, if it exists
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        public static void DeleteEntryData(string indexNum, string subPath)
        {
            DeleteData(Path.Join(DATA_PATH, subPath), indexNum, DATA_EXT);
        }

        public static void DeleteData(string subpath, string file, string ext)
        {
            string folder = PathMod.HardMod(subpath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            //Check if a diff file is located here
            if (File.Exists(Path.Join(folder, file + PATCH_EXT))) //if so, call DeleteObject with the diff ext, and the additional argument consisting of the base item
                DeleteObject(Path.Join(folder, file + PATCH_EXT));
            else //if not, just delete the normal mod file
                DeleteObject(Path.Join(folder, file + ext));
        }

        public static void DeleteObject(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        public void PreLoadZone(string index)
        {
#if !NO_THREADING
            preLoadZoneThread = new Thread(() => PreLoadZoneInBackground(index));
            preLoadZoneThread.IsBackground = true;
            //thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            //thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            preLoadZoneThread.Start();
#endif
        }

        void PreLoadZoneInBackground(string index)
        {
            ZoneData data = LoadNamespacedData<ZoneData>(index, DataType.Zone.ToString());
            zoneCache.Add(index, data);
        }

        /// <summary>
        /// Gets a zone based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ZoneData GetZone(string index)
        {
            ZoneData data = null;

            //wait for any preloading to finish
            if (preLoadZoneThread != null)
            {
                preLoadZoneThread.Join();
                preLoadZoneThread = null;
            }

            if (zoneCache.TryGetValue(index, out data))
            {
                zoneCache.Clear();
                return data;
            }
            zoneCache.Clear();

            try
            {

                data = LoadNamespacedData<ZoneData>(index, DataType.Zone.ToString());
                return data;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new FileNotFoundException(String.Format("Missing Data: {0}", index), ex));
            }
            return data;
        }

        /// <summary>
        /// Gets a map based on its ID
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Map GetMap(string name)
        {
            Map mapData = null;
            try
            {
                mapData = LoadNamespacedData<Map>(name, MAP_FOLDER, ".rsmap");
                mapData.AssetName = name;
                return mapData;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new FileNotFoundException(String.Format("Missing Data: {0}", name), ex));
            }

            return mapData;
        }

        /// <summary>
        /// Gets a ground map based on its ID
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GroundMap GetGround(string name)
        {
            GroundMap mapData = null;
            try
            {
                mapData = LoadEntryData<GroundMap>(name, GROUND_FOLDER, ".rsground");
                mapData.AssetName = name;
                return mapData;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new FileNotFoundException(String.Format("Missing Data: {0}", name), ex));
            }

            return mapData;
        }

        /// <summary>
        /// Gets the data for a skill based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SkillData GetSkill(string index)
        {
            SkillData data;
            if (skillCache.TryGetValue(index, out data))
                return data;

            try
            {
                data = LoadNamespacedData<SkillData>(index, DataType.Skill.ToString());
                skillCache.Add(index, data);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new FileNotFoundException(String.Format("Missing Data: {0}", index), ex));
            }
            return data;
        }


        /// <summary>
        /// Gets the data for an item based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ItemData GetItem(string index)
        {
            ItemData data;
            if (itemCache.TryGetValue(index, out data))
                return data;

            try
            {
                data = LoadNamespacedData<ItemData>(index, DataType.Item.ToString());
                itemCache.Add(index, data);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new FileNotFoundException(String.Format("Missing Data: {0}", index), ex));
            }
            return data;
        }

        /// <summary>
        /// Gets the data for an autotile based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public AutoTileData GetAutoTile(string index)
        {
            AutoTileData data;
            if (autoTileCache.TryGetValue(index, out data))
                return data;

            try
            {
                data = LoadNamespacedData<AutoTileData>(index, "AutoTile");
                autoTileCache.Add(index, data);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new FileNotFoundException(String.Format("Missing Data: {0}", index), ex));
            }
            return data;
        }

        /// <summary>
        /// Gets the data for a monster based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public MonsterData GetMonster(string index)
        {
            MonsterData data;
            if (monsterCache.TryGetValue(index, out data))
                return data;

            try
            {
                data = LoadNamespacedData<MonsterData>(index, "Monster");
                monsterCache.Add(index, data);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new FileNotFoundException(String.Format("Missing Data: {0}", index), ex));
            }
            return data;
        }

        /// <summary>
        /// Gets the data for a status effect based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public StatusData GetStatus(string index)
        {
            StatusData data;
            if (statusCache.TryGetValue(index, out data))
                return data;

            try
            {
                data = LoadNamespacedData<StatusData>(index, DataType.Status.ToString());
                statusCache.Add(index, data);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new FileNotFoundException(String.Format("Missing Data: {0}", index), ex));
            }
            return data;
        }

        /// <summary>
        /// Gets the data for an intrinsic (passive ability) based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IntrinsicData GetIntrinsic(string index)
        {
            IntrinsicData data;
            if (intrinsicCache.TryGetValue(index, out data))
                return data;

            try
            {
                data = LoadNamespacedData<IntrinsicData>(index, DataType.Intrinsic.ToString());
                intrinsicCache.Add(index, data);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new FileNotFoundException(String.Format("Missing Data: {0}", index), ex));
            }
            return data;
        }

        /// <summary>
        /// Gets the data for a map-wide status effect based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public MapStatusData GetMapStatus(string index)
        {
            MapStatusData data;
            if (mapStatusCache.TryGetValue(index, out data))
                return data;

            try
            {
                data = LoadNamespacedData<MapStatusData>(index, DataType.MapStatus.ToString());
                mapStatusCache.Add(index, data);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new FileNotFoundException(String.Format("Missing Data: {0}", index), ex));
            }
            return data;
        }

        /// <summary>
        /// Gets the data for a tile, such as stairs or traps, based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TileData GetTile(string index)
        {
            TileData data = null;
            if (tileCache.TryGetValue(index, out data))
                return data;

            return data;
        }

        /// <summary>
        /// Gets the data for a terrain type based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TerrainData GetTerrain(string index)
        {
            TerrainData data = null;
            if (terrainCache.TryGetValue(index, out data))
                return data;

            return data;
        }

        /// <summary>
        /// Gets the data for an emote based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public EmoteData GetEmote(string index)
        {
            EmoteData data = null;
            if (emoteCache.TryGetValue(index, out data))
                return data;

            return null;
        }

        /// <summary>
        /// Gets the data for an elemental type based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ElementData GetElement(string index)
        {
            ElementData data = null;
            if (elementCache.TryGetValue(index, out data))
                return data;

            return null;
        }

        /// <summary>
        /// Gets the data for a growth group based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GrowthData GetGrowth(string index)
        {
            GrowthData data = null;
            if (growthCache.TryGetValue(index, out data))
                return data;

            return null;
        }

        /// <summary>
        /// Gets the data for a skill-sharing group based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SkillGroupData GetSkillGroup(string index)
        {
            SkillGroupData data = null;
            if (skillgroupCache.TryGetValue(index, out data))
                return data;

            return null;
        }

        /// <summary>
        /// Gets the data for an ai tactic based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public AITactic GetAITactic(string index)
        {
            AITactic data = null;
            if (aiCache.TryGetValue(index, out data))
                return data;

            return null;
        }

        /// <summary>
        /// Gets the data for a team rank based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RankData GetRank(string index)
        {
            RankData data = null;
            if (rankCache.TryGetValue(index, out data))
                return data;

            return null;
        }

        /// <summary>
        /// Gets the data for a skin based on its ID
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SkinData GetSkin(string index)
        {
            SkinData data = null;
            if (skinCache.TryGetValue(index, out data))
                return data;

            return null;
        }

        public void ClearCache(DataType conversionFlags)
        {
            if ((conversionFlags & DataType.Zone) != DataType.None)
                zoneCache.Clear();
            if ((conversionFlags & DataType.Item) != DataType.None)
                itemCache.Clear();
            if ((conversionFlags & DataType.Status) != DataType.None)
                statusCache.Clear();
            if ((conversionFlags & DataType.Intrinsic) != DataType.None)
                intrinsicCache.Clear();
            if ((conversionFlags & DataType.Skill) != DataType.None)
                skillCache.Clear();
            if ((conversionFlags & DataType.Monster) != DataType.None)
                monsterCache.Clear();
            if ((conversionFlags & DataType.AutoTile) != DataType.None)
                autoTileCache.Clear();
            if ((conversionFlags & DataType.MapStatus) != DataType.None)
                mapStatusCache.Clear();

            if ((conversionFlags & DataType.Tile) != DataType.None)
                LoadCacheFull(DataType.Tile, tileCache);
            if ((conversionFlags & DataType.Terrain) != DataType.None)
                LoadCacheFull(DataType.Terrain, terrainCache);
            if ((conversionFlags & DataType.Emote) != DataType.None)
                LoadCacheFull(DataType.Emote, emoteCache);
            if ((conversionFlags & DataType.Element) != DataType.None)
                LoadCacheFull(DataType.Element, elementCache);
            if ((conversionFlags & DataType.GrowthGroup) != DataType.None)
                LoadCacheFull(DataType.GrowthGroup, growthCache);
            if ((conversionFlags & DataType.SkillGroup) != DataType.None)
                LoadCacheFull(DataType.SkillGroup, skillgroupCache);
            if ((conversionFlags & DataType.AI) != DataType.None)
                LoadCacheFull(DataType.AI, aiCache);
            if ((conversionFlags & DataType.Rank) != DataType.None)
                LoadCacheFull(DataType.Rank, rankCache);
            if ((conversionFlags & DataType.Skin) != DataType.None)
                LoadCacheFull(DataType.Skin, skinCache);

        }

        /// <summary>
        /// Starts recording the quicksave for a new adventure.
        /// From here on, the replayWriter will remain open as a way to continue writing game states and player inputs.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="zoneId"></param>
        /// <param name="rogue"></param>
        /// <param name="seeded"></param>
        /// <param name="sessionStart"></param>
        public void BeginPlay(string filePath, string zoneId, bool rogue, bool seeded, DateTime sessionStart)
        {
            try
            {
                if (replayWriter != null)
                    throw new Exception("Started a new play before closing the existing one!");

                replayWriter = new BinaryWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None));
                replayGroundStatePos = 0;

                //write start info
                Version version = Versioning.GetVersion();
                replayWriter.Write(version.Major);
                replayWriter.Write(version.Minor);
                replayWriter.Write(version.Build);
                replayWriter.Write(version.Revision);
                replayWriter.Write(0L);//pointer to epitaph location, 0 for now
                replayWriter.Write(0L);//final time, 0 for now
                replayWriter.Write(sessionStart.Ticks);//session start time
                replayWriter.Write(0);//final score, 0 for now
                replayWriter.Write(0);//final result, 0 for now
                replayWriter.Write(zoneId);
                replayWriter.Write(rogue);
                replayWriter.Write(seeded);
                replayWriter.Write(Save.ActiveTeam.GetReferenceName());
                replayWriter.Write(Save.StartDate);
                replayWriter.Write(Save.Rand.FirstSeed);
                replayWriter.Write(false);
                replayWriter.Write(DiagManager.Instance.CurSettings.Language);

                replayWriter.Flush();
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        /// <summary>
        /// Called when resuming an adventure from a quicksave.
        /// The quicksave file is loaded and the stream position is set to the end, so that it can continue writing the replay.
        /// </summary>
        /// <param name="replay">The quicksave replay to resume from.</param>
        /// <param name="sessionResumeTime"></param>
        public void ResumePlay(ReplayData replay, DateTime sessionResumeTime)
        {
            try
            {
                if (replayWriter != null)
                    throw new Exception("Started a new play before closing the existing one!");

                replayWriter = new BinaryWriter(new FileStream(replay.RecordDir, FileMode.Open, FileAccess.Write, FileShare.None));
                replayGroundStatePos = replay.GroundsavePos;
                if (replay.QuicksavePos > 0)
                {
                    replayWriter.BaseStream.SetLength(replay.QuicksavePos);
                    replayWriter.BaseStream.Seek(replay.QuicksavePos, SeekOrigin.Begin);
                    replayWriter.Flush();
                }
                else
                {
                    //TODO: remove this logic when save data is used for quicksaves
                    replayWriter.BaseStream.Seek(sizeof(Int32) * 4 + sizeof(Int64) * 2, SeekOrigin.Begin);
                    replayWriter.Write(sessionResumeTime.Ticks);

                    replayWriter.BaseStream.Seek(0, SeekOrigin.End);
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        /// <summary>
        /// Writes the game state to the current replay.  The player must be in dungeon mode.
        /// This is used when the player begins a new dungeon in their adventure. (One adventure can contain a trek through multiple dungeons)
        /// </summary>
        public void LogState()
        {
            if (replayWriter != null)
            {
                try
                {
                    //serialize the entire save data to mark the start of the dungeon
                    replayWriter.Write((byte)ReplayData.ReplayLog.StateLog);
                    SaveMainGameState(replayWriter);

                    replayWriter.Flush();
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }
        }

        /// <summary>
        /// Writes the game state to the current replay.  The player must be in ground mode.
        /// This is used when a player saves in the middle of an adventure in a ground mode rest area.
        /// </summary>
        public void LogGroundSave()
        {
            if (replayWriter != null)
            {
                try
                {
                    //erase the previous groundsave on this log
                    if (replayGroundStatePos > 0)
                        replayWriter.BaseStream.SetLength(replayGroundStatePos);

                    //serialize the entire save data to mark the start of the dungeon
                    replayWriter.Write((byte)ReplayData.ReplayLog.GroundsaveLog);
                    SaveMainGameState(replayWriter);

                    replayWriter.Flush();
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }
        }

        /// <summary>
        /// Writes the entire game state for faster loading of quicksaves.
        /// Currently not used due to quicksaves still loading from the start of the dungeon and replaying every step up to the current point.
        /// </summary>
        public void LogQuicksave()
        {
            if (replayWriter != null)
            {
                try
                {
                    //serialize the entire save data to mark the start of the dungeon
                    replayWriter.Write((byte)ReplayData.ReplayLog.QuicksaveLog);
                    SaveMainGameState(replayWriter);

                    replayWriter.Flush();
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }
        }

        /// <summary>
        /// Logs a player action to the current quicksave replay.
        /// </summary>
        /// <param name="play"></param>
        public void LogPlay(GameAction play)
        {
            if (replayWriter != null)
            {
                try
                {
                    replayWriter.Write((byte)ReplayData.ReplayLog.GameLog);
                    replayWriter.Write((byte)((int)play.Type));
                    replayWriter.Write((byte)((int)play.Dir));
                    replayWriter.Write((byte)play.ArgCount);
                    for (int ii = 0; ii < play.ArgCount; ii++)
                        replayWriter.Write(play[ii]);

                    replayWriter.Flush();
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }
        }

        /// <summary>
        /// Logs a player UI action to the current quicksave replay.
        /// </summary>
        public void LogUIPlay(params int[] code)
        {
            if (uiQueue != null)
            {
                uiQueue.Enqueue(code);
                return;
            }

            if (replayWriter != null)
            {
                try
                {
                    replayWriter.Write((byte)ReplayData.ReplayLog.UILog);
                    replayWriter.Write((byte)code.Length);
                    for (int ii = 0; ii < code.Length; ii++)
                        replayWriter.Write(code[ii]);

                    replayWriter.Flush();
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }
        }

        /// <summary>
        /// Logs a string to the current quicksave replay.  Used for name inputs.
        /// </summary>
        public void LogUIStringPlay(string str)
        {
            LogUIPlay(str.Length);
            for (int ii = 0; ii < str.Length; ii++)
                LogUIPlay((int)str[ii]);
        }

        Queue<int[]> uiQueue;

        /// <summary>
        /// Starts queueing UI commands to the current replay quicksave.
        /// UI commands need to be queued sometimes, because they may happen mid-action for an action that may fail later.
        /// If a player action fails, it is not logged.  Thus, if queues didn't exist, UI actions would be logged for actions that didn't actually happen.
        /// </summary>
        public void QueueLogUI()
        {
            uiQueue = new Queue<int[]>();
        }

        /// <summary>
        /// Stops queueing UI commands to the current replay quicksave and writes the current queue to the replay.
        /// </summary>
        public void DequeueLogUI()
        {
            if (uiQueue == null)
                return;
            Queue<int[]> tempQueue = uiQueue;
            uiQueue = null;

            while (tempQueue.Count > 0)
                LogUIPlay(tempQueue.Dequeue());
        }

        /// <summary>
        /// Called when an adventure is ended.  Closes the replay writing stream and saves the quicksave into a replay.
        /// </summary>
        /// <param name="epitaph"></param>
        /// <param name="outFile"></param>
        /// <returns></returns>
        public string EndPlay(GameProgress epitaph, string outFile)
        {
            try
            {
                string fullPath = null;
                string fileName = null;
                if (replayWriter != null)
                {
                    fullPath = ((FileStream)replayWriter.BaseStream).Name.Replace('\\', '/');
                    fileName = Path.GetFileNameWithoutExtension(fullPath);
                    if (outFile == null)
                        outFile = fileName;

                    if (epitaph != null)
                    {
                        epitaph.EndDate = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);

                        long epitaphPos = replayWriter.BaseStream.Position;
                        //save the location string
                        replayWriter.Write(epitaph.Location);
                        //save the epitaph
                        GameProgress.SaveMainData(replayWriter, epitaph);

                        //pointers and score
                        replayWriter.BaseStream.Seek(sizeof(Int32) * 4, SeekOrigin.Begin);
                        replayWriter.Write(epitaphPos);
                        replayWriter.Write(epitaph.SessionTime.Ticks);
                        replayWriter.Write(epitaph.SessionStartTime.Ticks);
                        replayWriter.Write(epitaph.GetTotalScore());
                        replayWriter.Write((int)epitaph.Outcome);
                    }

                    replayWriter.Close();
                    replayWriter = null;
                    replayGroundStatePos = 0;
                }

                if (fileName == null)
                {
                    return "";
                }
                else if (File.Exists(PathMod.ModSavePath(REPLAY_PATH, outFile + REPLAY_EXTENSION)))
                {
                    string renamedFile = Text.GetNonConflictingSavePath(PathMod.ModSavePath(REPLAY_PATH), outFile, REPLAY_EXTENSION);

                    if (renamedFile != null)
                    {
                        Directory.Move(fullPath, PathMod.ModSavePath(REPLAY_PATH, renamedFile + REPLAY_EXTENSION));
                        return renamedFile + REPLAY_EXTENSION;
                    }
                }
                else
                    Directory.Move(fullPath, PathMod.ModSavePath(REPLAY_PATH, outFile + REPLAY_EXTENSION));
                return outFile + REPLAY_EXTENSION;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return null;
        }

        //TODO: remove this when LogQuicksave is working
        /// <summary>
        /// Saves the current session time
        /// </summary>
        /// <param name="sessionTime"></param>
        public void SaveSessionTime(TimeSpan sessionTime)
        {
            try
            {
                if (replayWriter != null)
                {
                    replayWriter.BaseStream.Seek(sizeof(Int32) * 4 + sizeof(Int64), SeekOrigin.Begin);
                    replayWriter.Write(sessionTime.Ticks);
                    replayWriter.Write(0L);
                    replayWriter.BaseStream.Seek(0, SeekOrigin.End);
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        /// <summary>
        /// Called when an adventure is suspended.  Closes the replay writing stream to allow for clean exit.
        /// Note how nothing else is done aside form closing the stream.
        /// Quicksaves already save every action from the player as it happens, so even if they closed the game there is no lost data.
        /// </summary>
        /// <returns></returns>
        public void SuspendPlay()
        {
            try
            {
                if (replayWriter != null)
                {
                    replayWriter.Close();
                    replayWriter = null;
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public bool ReplaysExist()
        {
            return FoundRecords(PathMod.ModSavePath(DataManager.REPLAY_PATH), REPLAY_EXTENSION);
        }

        public bool FoundRecords(string mainPath, string ext)
        {
            if (!Directory.Exists(mainPath))
                return false;
            string[] files = Directory.GetFiles(mainPath, "*" + ext);
            return ContainsNonTrivialFiles(files);
        }

        public static bool ContainsNonTrivialFiles(string[] files)
        {
            foreach (string file in files)
            {
                if (IsNonTrivialFile(file))
                    return true;
            }
            return false;
        }

        public static bool IsNonTrivialFile(string file)
        {
            string bareFile = Path.GetFileName(file);
            return !bareFile.StartsWith(".");
        }

        public List<RecordHeaderData> GetRecordHeaders(string recordDir, string ext)
        {
            List<RecordHeaderData> results = new List<RecordHeaderData>();

            if (Directory.Exists(recordDir))
            {
                string[] files = Directory.GetFiles(recordDir, "*" + ext);
                foreach (string file in files)
                {
                    if (!IsNonTrivialFile(file))
                        continue;

                    RecordHeaderData record = GetRecordHeader(file);
                    results.Add(record);
                }
            }
            return results;
        }

        public RecordHeaderData GetRecordHeader(string file)
        {
            RecordHeaderData record = new RecordHeaderData(file);
            try
            {
                using (FileStream stream = File.OpenRead(file))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        //version string
                        record.Version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                        //epitaph location
                        long endPos = reader.ReadInt64();
                        //read time
                        reader.ReadInt64();
                        //read session start time
                        reader.ReadInt64();
                        //read score
                        record.Score = reader.ReadInt32();
                        //read result
                        record.Result = (GameProgress.ResultType)reader.ReadInt32();
                        //read zone ID
                        record.Zone = reader.ReadString();
                        record.IsRogue = reader.ReadBoolean();
                        record.IsSeeded = reader.ReadBoolean();
                        //name, date
                        record.Name = reader.ReadString();
                        record.DateTimeString = reader.ReadString();
                        record.Seed = reader.ReadUInt64();
                        record.IsFavorite = reader.ReadBoolean();

                        if (endPos > 0)
                        {
                            reader.BaseStream.Seek(endPos, SeekOrigin.Begin);
                            //read location string
                            record.LocationString = reader.ReadString();
                        }
                        return record;
                    }
                }
            }
            catch (Exception ex)
            {
                //In this case, the error will be presented clearly to the player.  Do not signal.
                DiagManager.Instance.LogError(ex, false);
            }
            return record;
        }

        public GameProgress GetRecord(string dir)
        {
            try
            {
                using (FileStream stream = File.OpenRead(dir))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        //version string
                        Version version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                        //epitaph location
                        long endPos = reader.ReadInt64();

                        if (endPos > 0)
                        {
                            reader.BaseStream.Seek(endPos, SeekOrigin.Begin);
                            //read location string
                            reader.ReadString();
                            return GameProgress.LoadMainData(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //In this case, the error will be presented clearly to the player.  Do not signal.
                DiagManager.Instance.LogError(ex, false);
            }
            return null;
        }

        public byte[] ReadReplayFile(string recordDir)
        {
            byte[] replay = null;
            try
            {
                replay = File.ReadAllBytes(recordDir);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
            }
            return replay;
        }

        public void ReplaySetFavorite(string recordDir, bool favorite_value)
        {
            byte[] file = ReadReplayFile(recordDir);

            using (MemoryStream memory = new MemoryStream(file))
            {
                using (BinaryReader reader = new BinaryReader(memory))
                {
                    using (BinaryWriter writer = new BinaryWriter(new FileStream(recordDir, FileMode.Create, FileAccess.Write, FileShare.None)))
                    {
                        //read version
                        writer.Write(reader.ReadInt32());
                        writer.Write(reader.ReadInt32());
                        writer.Write(reader.ReadInt32());
                        writer.Write(reader.ReadInt32());
                        //read the pointer for location of epitaph
                        writer.Write(reader.ReadInt64());
                        //read time
                        writer.Write(reader.ReadInt64());
                        //read session time
                        writer.Write(reader.ReadInt64());
                        //read score
                        writer.Write(reader.ReadInt32());
                        //read result
                        writer.Write(reader.ReadInt32());
                        //read zone
                        writer.Write(reader.ReadString());
                        //read rogue mode
                        writer.Write(reader.ReadBoolean());
                        //seeded run
                        writer.Write(reader.ReadBoolean());
                        //read name
                        writer.Write(reader.ReadString());
                        //read startdate
                        writer.Write(reader.ReadString());
                        //read seed
                        writer.Write(reader.ReadUInt64());
                        //read Fave
                        reader.ReadBoolean();
                        writer.Write(favorite_value);
                        //read language that the game was played in
                        writer.Write(reader.ReadString());
                        //read the rest of the file
                        while (memory.Position < memory.Length)
                        {
                            writer.Write(reader.ReadByte());
                        }
                    }
                }
            }
        }

        public ReplayData LoadReplay(string recordDir, bool quickload)
        {
            try
            {
                ReplayData replay = new ReplayData();
                replay.RecordDir = recordDir;
                using (FileStream stream = File.OpenRead(recordDir))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        //read version
                        replay.RecordVersion = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                        //read the pointer for location of epitaph
                        long endPos = reader.ReadInt64();
                        if (endPos > 0 && quickload)
                            throw new Exception("Cannot quickload a completed file!");
                        //read time
                        replay.SessionTime = reader.ReadInt64();
                        //read session time
                        replay.SessionStartTime = reader.ReadInt64();
                        //read score
                        reader.ReadInt32();
                        //read result
                        reader.ReadInt32();
                        //read zone
                        reader.ReadString();
                        //read rogue mode
                        reader.ReadBoolean();
                        //seeded run
                        reader.ReadBoolean();
                        //read name
                        reader.ReadString();
                        //read startdate
                        reader.ReadString();
                        //read seed
                        reader.ReadUInt64();
                        //read favorite
                        reader.ReadBoolean();
                        //read language that the game was played in
                        replay.RecordLang = reader.ReadString();
                        //read commands
                        if (endPos == 0)
                            endPos = reader.BaseStream.Length;
                        while (reader.BaseStream.Position != endPos)
                        {
                            try
                            {
                                long savePos = reader.BaseStream.Position;
                                byte type = reader.ReadByte();
                                if (type == (byte)ReplayData.ReplayLog.StateLog || type == (byte)ReplayData.ReplayLog.QuicksaveLog || type == (byte)ReplayData.ReplayLog.GroundsaveLog)
                                {
                                    if (quickload)
                                    {
                                        replay.States.Clear();
                                        replay.Actions.Clear();
                                        replay.UICodes.Clear();
                                    }
                                    //read team info
                                    GameState gameState = ReadGameState(reader, false);
                                    replay.States.Add(gameState);

                                    if (type == (byte)ReplayData.ReplayLog.QuicksaveLog)
                                        replay.QuicksavePos = savePos;
                                    else if (type == (byte)ReplayData.ReplayLog.GroundsaveLog)
                                        replay.GroundsavePos = savePos;
                                }
                                else if (type == (byte)ReplayData.ReplayLog.GameLog)
                                {
                                    GameAction.ActionType actionType = (GameAction.ActionType)reader.ReadByte();
                                    Dir8 dir = (Dir8)reader.ReadByte();
                                    if ((int)dir == 255)
                                        dir = Dir8.None;
                                    GameAction play = new GameAction(actionType, dir);
                                    byte totalArgs = reader.ReadByte();
                                    for (int ii = 0; ii < totalArgs; ii++)
                                        play.AddArg(reader.ReadInt32());
                                    replay.Actions.Add(play);
                                }
                                else if (type == (byte)ReplayData.ReplayLog.UILog)
                                {
                                    byte totalCodes = reader.ReadByte();
                                    for (int ii = 0; ii < totalCodes; ii++)
                                        replay.UICodes.Add(reader.ReadInt32());
                                }
                                else
                                    throw new Exception("Invalid Replay command type: " + type);
                            }
                            catch (Exception ex)
                            {
                                //In this case, the error will be presented clearly to the player.  Do not signal.
                                DiagManager.Instance.LogError(ex, false);
                                break;
                            }
                        }
                    }
                }
                return replay;
            }
            catch (Exception ex)
            {
                //In this case, the error will be presented clearly to the player.  Do not signal.
                DiagManager.Instance.LogError(ex, false);
            }
            return null;
        }

        public void CreateQuicksaveFromReplay()
        {
            string quicksavePath = PathMod.ModSavePath(SAVE_PATH, QUICKSAVE_FILE_PATH);

            RecordHeaderData record = GetRecordHeader(CurrentReplay.RecordDir);
            if (record.IsRogue)
                quicksavePath = PathMod.ModSavePath(ROGUE_PATH, Path.GetFileNameWithoutExtension(CurrentReplay.RecordDir) + QUICKSAVE_EXTENSION);
            //delete existing quicksave
            File.Delete(quicksavePath);

            //load and save data
            using (FileStream stream = File.OpenRead(CurrentReplay.RecordDir))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    using (BinaryWriter writer = new BinaryWriter(new FileStream(quicksavePath, FileMode.Create, FileAccess.Write, FileShare.None)))
                    {
                        //read version
                        writer.Write(reader.ReadInt32());
                        writer.Write(reader.ReadInt32());
                        writer.Write(reader.ReadInt32());
                        writer.Write(reader.ReadInt32());
                        //read the pointer for location of epitaph
                        reader.ReadInt64();
                        writer.Write(0L);
                        //read time
                        writer.Write(reader.ReadInt64());
                        //read session time
                        writer.Write(reader.ReadInt64());
                        //read score
                        writer.Write(reader.ReadInt32());
                        //read result
                        writer.Write(reader.ReadInt32());
                        //read zone
                        writer.Write(reader.ReadString());
                        //read rogue mode
                        writer.Write(reader.ReadBoolean());
                        //seeded run
                        writer.Write(reader.ReadBoolean());
                        //read name
                        writer.Write(reader.ReadString());
                        //read startdate
                        writer.Write(reader.ReadString());
                        //read seed
                        writer.Write(reader.ReadUInt64());
                        //read favorite, no need to write it though
                        reader.ReadBoolean();
                        writer.Write(false);
                        //read language that the game was played in
                        writer.Write(reader.ReadString());
                        //read commands
                        int currentAction = 0;
                        while (currentAction < CurrentReplay.CurrentAction)
                        {
                            try
                            {
                                long savePos = reader.BaseStream.Position;
                                byte type = reader.ReadByte();
                                writer.Write(type);
                                if (type == (byte)ReplayData.ReplayLog.StateLog || type == (byte)ReplayData.ReplayLog.QuicksaveLog || type == (byte)ReplayData.ReplayLog.GroundsaveLog)
                                {
                                    //read team info
                                    GameState gameState = ReadGameState(reader, false);
                                    SaveGameState(writer, gameState);
                                }
                                else if (type == (byte)ReplayData.ReplayLog.GameLog)
                                {
                                    writer.Write(reader.ReadByte());
                                    writer.Write(reader.ReadByte());
                                    byte totalArgs = reader.ReadByte();
                                    writer.Write(totalArgs);
                                    for (int ii = 0; ii < totalArgs; ii++)
                                        writer.Write(reader.ReadInt32());
                                    currentAction++;
                                }
                                else if (type == (byte)ReplayData.ReplayLog.UILog)
                                {
                                    byte totalCodes = reader.ReadByte();
                                    writer.Write(totalCodes);
                                    for (int ii = 0; ii < totalCodes; ii++)
                                        writer.Write(reader.ReadInt32());
                                }
                                else
                                    throw new Exception("Invalid Replay command type: " + type);
                            }
                            catch (Exception ex)
                            {
                                //In this case, the error will be presented clearly to the player.  Do not signal.
                                DiagManager.Instance.LogError(ex, false);
                            }
                        }
                    }
                }
            }

        }


        public static string FindRescueMail(string filepath, BaseRescueMail mail, string extension)
        {
            string[] filenames = Directory.GetFiles(filepath, "*"+extension);
            foreach (string filename in filenames)
            {
                try
                {
                    using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            string teamID = reader.ReadString();
                            if (teamID != mail.TeamID)
                                continue;
                            ulong seed = reader.ReadUInt64();
                            if (seed != mail.Seed)
                                continue;
                            int turns = reader.ReadInt32();
                            if (turns != mail.TurnsTaken)
                                continue;
                            string dateDefeated = reader.ReadString();
                            if (dateDefeated != mail.DateDefeated)
                                continue;
                            ZoneLoc goal = new ZoneLoc(reader.ReadString(), new SegLoc(reader.ReadInt32(), reader.ReadInt32()));
                            if (goal.ID != mail.Goal.ID || goal.StructID.Segment != mail.Goal.StructID.Segment || goal.StructID.ID != mail.Goal.StructID.ID)
                                continue;
                            int versionCount = reader.ReadInt32();
                            List<ModVersion> versions = new List<ModVersion>();
                            for (int ii = 0; ii < versionCount; ii++)
                            {
                                string name = reader.ReadString();
                                Guid uuid = Guid.Parse(reader.ReadString());
                                int major = reader.ReadInt32();
                                int minor = reader.ReadInt32();
                                int build = reader.ReadInt32();
                                int rev = reader.ReadInt32();
                                Version version;
                                if (rev > -1)
                                    version = new Version(major, minor, build, rev);
                                else if (build > -1)
                                    version = new Version(major, minor, build);
                                else
                                    version = new Version(major, minor);
                                ModVersion diff = new ModVersion(name, uuid, version);
                                versions.Add(diff);
                            }
                            List<ModVersion> curVersions = PathMod.GetModVersion();
                            List<ModDiff> versionDiff = PathMod.DiffModVersions(versions, curVersions);
                            if (versionDiff.Count > 0)
                                continue;

                            return filename;
                        }
                    }
                }
                catch
                {
                    //let it slide here.  if a file is bad because it was the wrong format, it wasn't right anyway
                }
            }
            return null;
        }

        public static BaseRescueMail LoadRescueMail(string filename)
        {
            try
            {
                using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        string teamID = reader.ReadString();
                        ulong seed = reader.ReadUInt64();
                        int turnstaken = reader.ReadInt32();
                        string dateDefeated = reader.ReadString();
                        ZoneLoc goal = new ZoneLoc(reader.ReadString(), new SegLoc(reader.ReadInt32(), reader.ReadInt32()));
                        int versionCount = reader.ReadInt32();
                        for (int ii = 0; ii < versionCount; ii++)
                        {
                            string name = reader.ReadString();
                            Guid uuid = Guid.Parse(reader.ReadString());
                            int major = reader.ReadInt32();
                            int minor = reader.ReadInt32();
                            int build = reader.ReadInt32();
                            int rev = reader.ReadInt32();
                            Version version;
                            if (rev > -1)
                                version = new Version(major, minor, build, rev);
                            else if (build > -1)
                                version = new Version(major, minor, build);
                            else
                                version = new Version(major, minor);
                        }
                        return (BaseRescueMail)Serializer.DeserializeData(stream);
                    }
                }
            }
            catch
            {
                //let it slide here.  if a file is bad because it was the wrong format, it wasn't right anyway
            }
            return null;
        }

        public static string SaveRescueMail(string folderPath, BaseRescueMail mail, bool force)
        {
            string renamedFile = mail.TeamID.ToString();
            if (!force)
                renamedFile = Text.GetNonConflictingSavePath(folderPath, mail.TeamID.ToString(), mail.Extension);

            try
            {
                if (renamedFile != null)
                    SaveRescueMail(folderPath + renamedFile + mail.Extension, mail);
                return renamedFile + mail.Extension;
            }
            catch
            {
                //let it slide here.  if a file is bad because it was the wrong format, it wasn't right anyway
            }
            return null;
        }

        public static void SaveRescueMail(string fullPath, BaseRescueMail mail)
        {
            using (Stream stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(mail.TeamID);
                    writer.Write(mail.Seed);
                    writer.Write(mail.TurnsTaken);
                    writer.Write(mail.DateDefeated);
                    writer.Write(mail.Goal.ID);
                    writer.Write(mail.Goal.StructID.Segment);
                    writer.Write(mail.Goal.StructID.ID);
                    writer.Write(mail.DefeatedVersion.Count);
                    for (int ii = 0; ii < mail.DefeatedVersion.Count; ii++)
                    {
                        writer.Write(mail.DefeatedVersion[ii].Name);
                        writer.Write(mail.DefeatedVersion[ii].UUID.ToString());
                        writer.Write(mail.DefeatedVersion[ii].Version.Major);
                        writer.Write(mail.DefeatedVersion[ii].Version.Minor);
                        writer.Write(mail.DefeatedVersion[ii].Version.Build);
                        writer.Write(mail.DefeatedVersion[ii].Version.Revision);
                    }
                    Serializer.SerializeData(stream, mail);
                }
            }
        }

        public void LoadProgress()
        {
            Save = GetProgress();
            LuaEngine.Instance.LoadSavedData(Save);

        }
        public void SetProgress(GameProgress progress)
        {
            Save = progress;
            LuaEngine.Instance.LoadSavedData(Save);
        }


        public MainProgress GetProgress()
        {
            //try to load from file
            if (File.Exists(PathMod.ModSavePath(SAVE_PATH, SAVE_FILE_PATH)))
            {
                try
                {
                    using (FileStream stream = File.OpenRead(PathMod.ModSavePath(SAVE_PATH, SAVE_FILE_PATH)))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            Version version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                            return (MainProgress)GameProgress.LoadMainData(reader);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }

            return null;
        }

        public GameState CopyMainGameState()
        {
            GameState state = new GameState();
            state.Save = Save;
            state.Zone = ZoneManager.Instance;

            //notify script engine
            LuaEngine.Instance.SaveData(state.Save);

            using (MemoryStream tempStream = new MemoryStream())
            {
                //saves scene, zone, and ground, if there will be one...
                using (BinaryWriter writer = new BinaryWriter(tempStream))
                {
                    SaveGameState(writer, state);

                    tempStream.Seek(0, SeekOrigin.Begin);
                    //loads dungeon, zone, and ground, if there will be one...
                    using (BinaryReader reader = new BinaryReader(tempStream))
                        return ReadGameState(reader, false);
                }
            }
        }

        public void SaveMainGameState()
        {
            GameState state = new GameState();
            state.Save = Save;
            state.Zone = ZoneManager.Instance;

            //notify script engine
            LuaEngine.Instance.SaveData(state.Save);

            SaveGameState(state);
        }

        public void SaveMainGameState(BinaryWriter writer)
        {
            GameState state = new GameState();
            state.Save = Save;
            state.Zone = ZoneManager.Instance;

            //notify script engine
            LuaEngine.Instance.SaveData(state.Save);

            SaveGameState(writer, state);
        }

        public void SaveGameState(GameState state)
        {
            using (MemoryStream tempStream = new MemoryStream())
            {
                //saves scene, zone, and ground, if there will be one...
                using (BinaryWriter writer = new BinaryWriter(tempStream))
                {
                    SaveGameState(writer, state);

                    using (Stream stream = new FileStream(PathMod.ModSavePath(SAVE_PATH, SAVE_FILE_PATH), FileMode.Create, FileAccess.Write, FileShare.None))
                        tempStream.WriteTo(stream);
                }
            }
        }

        private void saveTeamMemberStatusRefs(BinaryWriter writer, ref int totalStatusRefs, Faction faction, int teamIndex, bool guest, EventedList<Character> playerList)
        {
            for (int jj = 0; jj < playerList.Count; jj++)
            {
                foreach (StatusEffect status in playerList[jj].IterateStatusEffects())
                {
                    if (status.TargetChar != null)
                    {
                        CharIndex charIndex = ZoneManager.Instance.CurrentMap.GetCharIndex(status.TargetChar);
                        writer.Write((int)faction);//team
                        writer.Write(teamIndex);//team index
                        writer.Write(guest);//guest
                        writer.Write(jj);//player index
                        writer.Write(status.ID);//status ID
                        writer.Write((int)charIndex.Faction);//target team index
                        writer.Write(charIndex.Team);//target team index
                        writer.Write(charIndex.Guest);//target guest status
                        writer.Write(charIndex.Char);//target char index

                        totalStatusRefs++;
                    }
                }
            }
        }

        private void saveTeamStatusRefs(BinaryWriter writer, ref int totalStatusRefs, Faction faction, int teamIndex, Team team)
        {
            saveTeamMemberStatusRefs(writer, ref totalStatusRefs, faction, teamIndex, false, team.Players);
            saveTeamMemberStatusRefs(writer, ref totalStatusRefs, faction, teamIndex, true, team.Guests);
        }

        public void SaveGameState(BinaryWriter writer, GameState state)
        {
            Version version = Versioning.GetVersion();
            writer.Write(version.Major);
            writer.Write(version.Minor);
            writer.Write(version.Build);
            writer.Write(version.Revision);

            GameProgress.SaveMainData(writer, state.Save);
            ZoneManager.SaveToState(writer, state);

            if (state.Zone.CurrentMap != null)
            {
                long currentPos = writer.BaseStream.Position;
                writer.Write(0);
                //on top level: save status references
                int totalStatusRefs = 0;

                saveTeamStatusRefs(writer, ref totalStatusRefs, Faction.Player, 0, state.Save.ActiveTeam);

                for (int ii = 0; ii < state.Zone.CurrentMap.AllyTeams.Count; ii++)
                    saveTeamStatusRefs(writer, ref totalStatusRefs, Faction.Friend, ii, state.Zone.CurrentMap.AllyTeams[ii]);
                for (int ii = 0; ii < state.Zone.CurrentMap.MapTeams.Count; ii++)
                    saveTeamStatusRefs(writer, ref totalStatusRefs, Faction.Foe, ii, state.Zone.CurrentMap.MapTeams[ii]);

                writer.BaseStream.Seek(currentPos, SeekOrigin.Begin);
                writer.Write(totalStatusRefs);
                writer.BaseStream.Seek(0, SeekOrigin.End);
            }
            
        }

        /// <summary>
        /// Returns game progress loaded from the save folder and current zone.
        /// </summary>
        /// <returns></returns>
        public GameState LoadMainGameState(bool allowUpgrade)
        {
            if (File.Exists(PathMod.ModSavePath(SAVE_PATH, SAVE_FILE_PATH)))
            {
                try
                {
                    using (Stream stream = new FileStream(PathMod.ModSavePath(SAVE_PATH, SAVE_FILE_PATH), FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        //loads dungeon, zone, and ground, if there will be one...
                        using (BinaryReader reader = new BinaryReader(stream))
                            return ReadGameState(reader, allowUpgrade);
                    }
                }
                catch (Exception ex)
                {
                    //In this case, the error will be presented clearly to the player.  Do not signal.
                    DiagManager.Instance.LogError(ex, false);
                }
            }
            return null;
        }

        public GameState ReadGameState(BinaryReader reader, bool allowUpgrade)
        {
            GameState state = new GameState();

            Version version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

            state.Save = GameProgress.LoadMainData(reader);

            ZoneManager.LoadToState(reader, state);
            if (allowUpgrade && state.Save.IsOldVersion())
            {
                try
                {
                    //ZoneManager.LoadDefaultState(state);

                    //reload AI
                    foreach (Character player in state.Save.ActiveTeam.Players)
                    {
                        AITactic ai;
                        if (player.Tactic != null)
                            ai = GetAITactic(player.Tactic.ID);
                        else
                            ai = GetAITactic(DataManager.Instance.DefaultAI);
                        player.Tactic = new AITactic(ai);
                    }
                    foreach (Character player in state.Save.ActiveTeam.Assembly)
                    {
                        AITactic ai;
                        if (player.Tactic != null)
                            ai = GetAITactic(player.Tactic.ID);
                        else
                            ai = GetAITactic(DataManager.Instance.DefaultAI);
                        player.Tactic = new AITactic(ai);
                    }
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }


            if (state.Zone.CurrentMap != null)
            {
                //need to reset the map's activeteam
                state.Zone.CurrentMap.ActiveTeam = state.Save.ActiveTeam;

                try
                {
                    //on top level: reconnect status references
                    int totalStatusRefs = reader.ReadInt32();
                    for (int ii = 0; ii < totalStatusRefs; ii++)
                    {
                        Faction faction = (Faction)reader.ReadInt32();//faction
                        int teamIndex = reader.ReadInt32();//team
                        bool guest = reader.ReadBoolean();//guest
                        int player = reader.ReadInt32();//player
                    string statusID = reader.ReadString();//status ID
                        Faction targetFaction = (Faction)reader.ReadInt32();//target faction index
                        int targetTeamIndex = reader.ReadInt32();//target team index
                        bool targetGuest = reader.ReadBoolean();//target guest status
                        int targetChar = reader.ReadInt32();//target char index
                        Team team = null;
                        switch (faction)
                        {
                            case Faction.Foe:
                                team = state.Zone.CurrentMap.MapTeams[teamIndex];
                                break;
                            case Faction.Friend:
                                team = state.Zone.CurrentMap.AllyTeams[teamIndex];
                                break;
                            default:
                                team = state.Save.ActiveTeam;
                                break;
                        }
                        EventedList<Character> playerList = guest ? team.Guests : team.Players;
                        StatusEffect status = playerList[player].StatusEffects[statusID];

                        Team targetTeam = null;
                        switch (targetFaction)
                        {
                            case Faction.Foe:
                                targetTeam = state.Zone.CurrentMap.MapTeams[targetTeamIndex];
                                break;
                            case Faction.Friend:
                                targetTeam = state.Zone.CurrentMap.AllyTeams[targetTeamIndex];
                                break;
                            default:
                                targetTeam = state.Save.ActiveTeam;
                                break;
                        }
                        EventedList<Character> targetPlayerList = targetGuest ? targetTeam.Guests : targetTeam.Players;
                        status.TargetChar = targetPlayerList[targetChar];
                        status.TargetChar.StatusesTargetingThis.Add(new StatusRef(status.ID, playerList[player]));
                    }
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }
            

            return state;
        }

        public void DeleteSaveData()
        {
            try
            {
                if (File.Exists(PathMod.ModSavePath(SAVE_PATH, SAVE_FILE_PATH)))
                    File.Delete(PathMod.ModSavePath(SAVE_PATH, SAVE_FILE_PATH));
                if (File.Exists(PathMod.ModSavePath(SAVE_PATH, QUICKSAVE_FILE_PATH)))
                    File.Delete(PathMod.ModSavePath(SAVE_PATH, QUICKSAVE_FILE_PATH));

                //if (Directory.Exists(ROGUE_PATH))
                //{
                //    Directory.Delete(ROGUE_PATH, true);
                //    Directory.CreateDirectory(ROGUE_PATH);
                //}
            }
            catch (Exception ex)
            {
                //In this case, the error will be presented clearly to the player.  Do not signal.
                DiagManager.Instance.LogError(ex, false);
            }
        }

        /// <summary>
        /// Deletes replays from the replay folder corresponding to the current mod.
        /// </summary>
        /// <param name="includeFav">Favorites will be deleted too.</param>
        public void DeleteReplayData(bool includeFav)
        {
            string[] files = Directory.GetFiles(PathMod.ModSavePath(DataManager.REPLAY_PATH), "*" + REPLAY_EXTENSION);

            foreach (string file in files)
            {
                if (!IsNonTrivialFile(file))
                    continue;

                RecordHeaderData record = GetRecordHeader(file);
                // in order for a file to be spared,
                // the record must be valid
                // the record must be a fave
                // includeFav must be FALSE
                if (record != null && !includeFav && record.IsFavorite)
                    continue;
                File.Delete(file);
            }
        }

        public IEnumerable<string> GetRecentMsgs(int entries)
        {
            return GetRecentMsgs(MsgLog.Count - entries, MsgLog.Count);
        }
        public IEnumerable<string> GetRecentMsgs(int entriesStart, int entriesEnd)
        {
            entriesStart = Math.Max(0, entriesStart);
            entriesEnd = Math.Min(MsgLog.Count, entriesEnd);

            for (int ii = entriesStart; ii < entriesEnd; ii++)
                yield return MsgLog[ii];
        }
    }

}
