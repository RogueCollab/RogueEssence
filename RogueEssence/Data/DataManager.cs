using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using RogueEssence.Ground;
using RogueEssence.Script;
using RogueEssence.Content;
using System.Xml;

namespace RogueEssence.Data
{
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
            Rescuing
        }

        private static DataManager instance;
        public static void InitInstance()
        {
            instance = new DataManager();
        }
        public static DataManager Instance { get { return instance; } }

        public const string DATA_PATH = "Data/";
        public const string MISC_PATH = DATA_PATH + "Misc/";
        public const string MAP_PATH = DATA_PATH + "Map/";
        public const string GROUND_PATH = DATA_PATH + "Ground/";
        public const string DATA_EXT = ".bin";
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


        private LRUCache<int, ItemData> itemCache;
        private LRUCache<int, StatusData> statusCache;
        private LRUCache<int, IntrinsicData> intrinsicCache;
        private LRUCache<int, SkillData> skillCache;
        private LRUCache<int, MonsterData> monsterCache;
        private LRUCache<int, AutoTileData> autoTileCache;
        private LRUCache<int, MapStatusData> mapStatusCache;
        private Dictionary<int, TileData> tileCache;
        private Dictionary<int, TerrainData> terrainCache;
        private Dictionary<int, EmoteData> emoteCache;
        private Dictionary<int, ElementData> elementCache;
        private Dictionary<int, GrowthData> growthCache;
        private Dictionary<int, SkillGroupData> skillgroupCache;
        private Dictionary<int, AITactic> aiCache;
        private Dictionary<int, RankData> rankCache;
        private Dictionary<int, SkinData> skinCache;

        public Dictionary<DataType, EntryDataIndex> DataIndices;

        public List<(MonsterID mon, string name)> StartChars;
        public List<string> StartTeams;
        public int StartLevel;
        public int StartPersonality;
        public ZoneLoc StartMap;
        public int MaxLevel;
        public ActiveEffect UniversalEvent;
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
            if (!Directory.Exists(PathMod.NoMod(ROGUE_PATH)))
                Directory.CreateDirectory(PathMod.NoMod(ROGUE_PATH));
            if (!Directory.Exists(PathMod.ModSavePath(REPLAY_PATH)))
                Directory.CreateDirectory(PathMod.ModSavePath(REPLAY_PATH));

            if (!Directory.Exists(PathMod.NoMod(RESCUE_IN_PATH + SOS_FOLDER)))
                Directory.CreateDirectory(PathMod.NoMod(RESCUE_IN_PATH + SOS_FOLDER));

            if (!Directory.Exists(PathMod.NoMod(RESCUE_IN_PATH + AOK_FOLDER)))
                Directory.CreateDirectory(PathMod.NoMod(RESCUE_IN_PATH + AOK_FOLDER));

            if (!Directory.Exists(PathMod.NoMod(RESCUE_OUT_PATH + SOS_FOLDER)))
                Directory.CreateDirectory(PathMod.NoMod(RESCUE_OUT_PATH + SOS_FOLDER));

            if (!Directory.Exists(PathMod.NoMod(RESCUE_OUT_PATH + AOK_FOLDER)))
                Directory.CreateDirectory(PathMod.NoMod(RESCUE_OUT_PATH + AOK_FOLDER));


            MsgLog = new List<string>();

            itemCache = new LRUCache<int, ItemData>(ITEM_CACHE_SIZE);
            statusCache = new LRUCache<int, StatusData>(STATUS_CACHE_SIZE);
            intrinsicCache = new LRUCache<int, IntrinsicData>(INSTRINSIC_CACHE_SIZE);
            skillCache = new LRUCache<int, SkillData>(SKILL_CACHE_SIZE);
            monsterCache = new LRUCache<int, MonsterData>(MONSTER_CACHE_SIZE);
            autoTileCache = new LRUCache<int, AutoTileData>(AUTOTILE_CACHE_SIZE);
            mapStatusCache = new LRUCache<int, MapStatusData>(MAP_STATUS_CACHE_SIZE);
            tileCache = new Dictionary<int, TileData>();
            terrainCache = new Dictionary<int, TerrainData>();
            emoteCache = new Dictionary<int, EmoteData>();
            elementCache = new Dictionary<int, ElementData>();
            growthCache = new Dictionary<int, GrowthData>();
            skillgroupCache = new Dictionary<int, SkillGroupData>();
            aiCache = new Dictionary<int, AITactic>();
            rankCache = new Dictionary<int, RankData>();
            skinCache = new Dictionary<int, SkinData>();

            DataIndices = new Dictionary<DataType, EntryDataIndex>();
            UniversalData = new TypeDict<BaseData>();
        }

        public void InitData()
        {
            HealFX = (BattleFX)LoadData(PathMod.ModPath(FX_PATH + "Heal.fx"), null);
            RestoreChargeFX = (BattleFX)LoadData(PathMod.ModPath(FX_PATH + "RestoreCharge.fx"), null);
            LoseChargeFX = (BattleFX)LoadData(PathMod.ModPath(FX_PATH + "LoseCharge.fx"), null);
            NoChargeFX = (EmoteFX)LoadData(PathMod.ModPath(FX_PATH + "NoCharge.fx"), null);
            ElementFX = (BattleFX)LoadData(PathMod.ModPath(FX_PATH + "Element.fx"), null);
            IntrinsicFX = (BattleFX)LoadData(PathMod.ModPath(FX_PATH + "Intrinsic.fx"), null);
            SendHomeFX = (BattleFX)LoadData(PathMod.ModPath(FX_PATH + "SendHome.fx"), null);
            ItemLostFX = (BattleFX)LoadData(PathMod.ModPath(FX_PATH + "ItemLost.fx"), null);
            WarpFX = (BattleFX)LoadData(PathMod.ModPath(FX_PATH + "Warp.fx"), null);
            KnockbackFX = (BattleFX)LoadData(PathMod.ModPath(FX_PATH + "Knockback.fx"), null);
            JumpFX = (BattleFX)LoadData(PathMod.ModPath(FX_PATH + "Jump.fx"), null);
            ThrowFX = (BattleFX)LoadData(PathMod.ModPath(FX_PATH + "Throw.fx"), null);


            UniversalEvent = (ActiveEffect)LoadData(PathMod.ModPath(DATA_PATH + "Universal.bin"), null);
            UniversalData = (TypeDict<BaseData>)LoadData(PathMod.ModPath(MISC_PATH + "Index.bin"), null);
            LoadStartParams();

            LoadIndex(DataType.Item);
            LoadIndex(DataType.Skill);
            LoadIndex(DataType.Monster);
            LoadIndex(DataType.Zone);
            LoadIndex(DataType.Status);
            LoadIndex(DataType.Intrinsic);
            LoadIndex(DataType.AutoTile);
            LoadIndex(DataType.MapStatus);
            LoadIndexFull(DataType.Tile, tileCache);
            LoadIndexFull(DataType.Terrain, terrainCache);
            LoadIndexFull(DataType.Emote, emoteCache);
            LoadIndexFull(DataType.Element, elementCache);
            LoadIndexFull(DataType.GrowthGroup, growthCache);
            LoadIndexFull(DataType.SkillGroup, skillgroupCache);
            LoadIndexFull(DataType.AI, aiCache);
            LoadIndexFull(DataType.Rank, rankCache);
            LoadIndexFull(DataType.Skin, skinCache);
            LoadUniversalData();
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
        }

        public static void InitSaveDirs()
        {
            Directory.CreateDirectory(PathMod.ModSavePath(SAVE_PATH));
            Directory.CreateDirectory(PathMod.ModSavePath(REPLAY_PATH));
        }


        public void LoadUniversalData()
        {
            foreach (BaseData baseData in UniversalData)
            {
                try
                {
                    BaseData data = (BaseData)DataManager.LoadData(PathMod.ModPath(MISC_PATH + baseData.FileName + DATA_EXT));
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
            string path = PathMod.ModPath(DATA_PATH + "StartParams.xml");
            //try to load from file
            if (File.Exists(path))
            {
                try
                {
                    StartChars = new List<(MonsterID, string)>();
                    StartTeams = new List<string>();

                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);

                    XmlNode startChars = xmldoc.DocumentElement.SelectSingleNode("StartChars");
                    foreach (XmlNode startChar in startChars.SelectNodes("StartChar"))
                    {
                        XmlNode startSpecies = startChar.SelectSingleNode("Species");
                        int species = Int32.Parse(startSpecies.InnerText);
                        XmlNode startForm = startChar.SelectSingleNode("Form");
                        int form = Int32.Parse(startForm.InnerText);
                        XmlNode startSkin = startChar.SelectSingleNode("Skin");
                        int skin = Int32.Parse(startSkin.InnerText);
                        XmlNode startGender = startChar.SelectSingleNode("Gender");
                        Gender gender = Enum.Parse<Gender>(startGender.InnerText);

                        XmlNode startName = startChar.SelectSingleNode("Name");
                        string name = startName.InnerText;

                        StartChars.Add((new MonsterID(species, form, skin, gender), name));
                    }

                    XmlNode startTeams = xmldoc.DocumentElement.SelectSingleNode("StartTeams");
                    foreach (XmlNode startTeam in startTeams.SelectNodes("StartTeam"))
                        StartTeams.Add(startTeam.InnerText);

                    XmlNode startLevel = xmldoc.DocumentElement.SelectSingleNode("StartLevel");
                    StartLevel = Int32.Parse(startLevel.InnerText);

                    XmlNode maxLevel = xmldoc.DocumentElement.SelectSingleNode("MaxLevel");
                    MaxLevel = Int32.Parse(maxLevel.InnerText);

                    XmlNode startPersonality = xmldoc.DocumentElement.SelectSingleNode("StartPersonality");
                    StartPersonality = Int32.Parse(startPersonality.InnerText);

                    XmlNode startMap = xmldoc.DocumentElement.SelectSingleNode("StartMap");
                    StartMap = new ZoneLoc(Int32.Parse(startMap.SelectSingleNode("Zone").InnerText),
                        new SegLoc(Int32.Parse(startMap.SelectSingleNode("Segment").InnerText), Int32.Parse(startMap.SelectSingleNode("ID").InnerText)),
                        Int32.Parse(startMap.SelectSingleNode("Entry").InnerText));
                    return;
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }
            StartChars = new List<(MonsterID, string)>();
            StartTeams = new List<string>();
        }


        public void Unload()
        {
            //Notify script engine
            LuaEngine.Instance.OnDataUnload();
            EndPlay(null, null);
        }


        public void LoadIndex(DataType type)
        {
            try
            {
                using (Stream stream = new FileStream(PathMod.ModPath(DATA_PATH + type.ToString() + "/index.idx"), FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        EntryDataIndex result = (EntryDataIndex)formatter.Deserialize(stream);
                        DataIndices[type] = result;
                    }
                }
            }
            catch
            {
                DataIndices[type] = new EntryDataIndex();
            }
        }
        public void LoadIndexFull<T>(DataType type, Dictionary<int, T> cache) where T : IEntryData
        {
            LoadIndex(type);
            LoadCacheFull(type, cache);
        }
        public void LoadCacheFull<T>(DataType type, Dictionary<int, T> cache) where T : IEntryData
        {
            cache.Clear();
            for (int ii = 0; ii < DataIndices[type].Count; ii++)
            {
                if (File.Exists(PathMod.ModPath(DATA_PATH + type.ToString() + "/" + ii + DataManager.DATA_EXT)))
                {
                    T data = (T)LoadData(ii, type.ToString());
                    cache.Add(ii, data);
                }
            }
        }

        public void SaveIndex(DataType type)
        {
            using (Stream stream = new FileStream(PathMod.HardMod(DATA_PATH + type.ToString() + "/index.idx"), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, DataIndices[type]);
                }
            }
        }

        public void ContentChanged(DataType dataType, int entryNum, IEntryData data)
        {
            SaveData(entryNum, dataType.ToString(), data);
            ClearCache(dataType);
            EntrySummary entrySummary = data.GenerateEntrySummary();
            if (entryNum < DataIndices[dataType].Entries.Count)
                DataIndices[dataType].Entries[entryNum] = entrySummary;
            else
                DataIndices[dataType].Entries.Add(entrySummary);
            SaveIndex(dataType);

            foreach (BaseData baseData in UniversalData)
            {
                if ((baseData.TriggerType & dataType) != DataManager.DataType.None)
                {
                    baseData.ContentChanged(entryNum);
                    DataManager.SaveData(PathMod.ModPath(DataManager.MISC_PATH + baseData.FileName + DATA_EXT), baseData);
                }
            }

            DiagManager.Instance.DevEditor.ReloadData(dataType);
        }

        public static IEntryData LoadData(int indexNum, string subPath)
        {
            return (IEntryData)LoadData(PathMod.ModPath(DATA_PATH + subPath + "/" + indexNum + DATA_EXT));
        }

        public static object LoadData(string path, SerializationBinder binder = null)
        {
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                //using (BinaryReader reader = new BinaryReader(stream))
                //{
                    IFormatter formatter = new BinaryFormatter();
                    if (binder != null)
                        formatter.Binder = binder;
                    return formatter.Deserialize(stream);
                //}
            }
        }


        public static void SaveData(int indexNum, string subPath, IEntryData entry)
        {
            string folder = PathMod.HardMod(DATA_PATH + subPath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            SaveData(folder + "/" + indexNum + DATA_EXT, entry);
        }

        public static void SaveData(string path, object entry)
        {
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                //using (BinaryWriter writer = new BinaryWriter(stream))
                //{
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, entry);
                //}
            }
        }



        public ZoneData GetZone(int index)
        {
            ZoneData data = null;

            try
            {
                if (File.Exists(PathMod.ModPath(DATA_PATH + DataType.Zone.ToString() + "/" + index + DATA_EXT)))
                {
                    data = (ZoneData)LoadData(index, DataType.Zone.ToString());
                    return data;
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return data;
        }

        public Map GetMap(string name)
        {
            Map mapData = null;
            try
            {
                mapData = (Map)LoadData(PathMod.ModPath(MAP_PATH + name + ".rsmap"));
                return mapData;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }

            return mapData;
        }

        public GroundMap GetGround(string name)
        {
            GroundMap mapData = null;
            try
            {
                mapData = (GroundMap)LoadData(PathMod.ModPath(GROUND_PATH + name + ".rsground"));
                return mapData;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }

            return mapData;
        }


        public SkillData GetSkill(int index)
        {
            SkillData data;
            if (skillCache.TryGetValue(index, out data))
                return data;


            try
            {
                data = (SkillData)LoadData(index, DataType.Skill.ToString());
                skillCache.Add(index, data);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return data;
        }



        public ItemData GetItem(int index)
        {
            ItemData data;
            if (itemCache.TryGetValue(index, out data))
                return data;

            try
            {
                if (File.Exists(PathMod.ModPath(DATA_PATH + DataType.Item.ToString() + "/" + index + DATA_EXT)))
                {
                    data = (ItemData)LoadData(index, DataType.Item.ToString());
                    itemCache.Add(index, data);
                    return data;
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return data;
        }


        public AutoTileData GetAutoTile(int index)
        {
            AutoTileData data;
            if (autoTileCache.TryGetValue(index, out data))
                return data;

            try
            {
                data = (AutoTileData)LoadData(index, "AutoTile");
                autoTileCache.Add(index, data);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return data;
        }

        public MonsterData GetMonster(int index)
        {
            MonsterData data;
            if (monsterCache.TryGetValue(index, out data))
                return data;

            try
            {
                data = (MonsterData)LoadData(index, "Monster");
                monsterCache.Add(index, data);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return data;
        }

        public StatusData GetStatus(int index)
        {
            StatusData data;
            if (statusCache.TryGetValue(index, out data))
                return data;

            try
            {
                if (File.Exists(PathMod.ModPath(DATA_PATH + DataType.Status.ToString() + "/" + index + DataManager.DATA_EXT)))
                {
                    data = (StatusData)LoadData(index, DataType.Status.ToString());
                    statusCache.Add(index, data);
                    return data;
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return data;
        }

        public IntrinsicData GetIntrinsic(int index)
        {
            IntrinsicData data;
            if (intrinsicCache.TryGetValue(index, out data))
                return data;

            try
            {
                if (File.Exists(PathMod.ModPath(DATA_PATH + DataType.Intrinsic.ToString() + "/" + index + DATA_EXT)))
                {
                    data = (IntrinsicData)LoadData(index, DataType.Intrinsic.ToString());
                    intrinsicCache.Add(index, data);
                    return data;
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return data;
        }


        public MapStatusData GetMapStatus(int index)
        {
            MapStatusData data;
            if (mapStatusCache.TryGetValue(index, out data))
                return data;

            try
            {
                if (File.Exists(PathMod.ModPath(DATA_PATH + DataType.MapStatus.ToString() + "/" + index + DATA_EXT)))
                {
                    data = (MapStatusData)LoadData(index, DataType.MapStatus.ToString());
                    mapStatusCache.Add(index, data);
                    return data;
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return data;
        }


        public TileData GetTile(int index)
        {
            TileData data = null;
            if (tileCache.TryGetValue(index, out data))
                return data;

            return data;
        }

        public TerrainData GetTerrain(int index)
        {
            TerrainData data = null;
            if (terrainCache.TryGetValue(index, out data))
                return data;

            return data;
        }

        public EmoteData GetEmote(int index)
        {
            EmoteData data = null;
            if (emoteCache.TryGetValue(index, out data))
                return data;

            return null;
        }

        public ElementData GetElement(int index)
        {
            ElementData data = null;
            if (elementCache.TryGetValue(index, out data))
                return data;

            return null;
        }


        public GrowthData GetGrowth(int index)
        {
            GrowthData data = null;
            if (growthCache.TryGetValue(index, out data))
                return data;

            return null;
        }

        public SkillGroupData GetSkillGroup(int index)
        {
            SkillGroupData data = null;
            if (skillgroupCache.TryGetValue(index, out data))
                return data;

            return null;
        }

        public AITactic GetAITactic(int index)
        {
            AITactic data = null;
            if (aiCache.TryGetValue(index, out data))
                return data;

            return null;
        }

        public RankData GetRank(int index)
        {
            RankData data = null;
            if (rankCache.TryGetValue(index, out data))
                return data;

            return null;
        }

        public SkinData GetSkin(int index)
        {
            SkinData data = null;
            if (skinCache.TryGetValue(index, out data))
                return data;

            return null;
        }

        public void ClearCache(DataType conversionFlags)
        {
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

        public void BeginPlay(string filePath, int zoneId, bool rogue, bool seeded)
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
                replayWriter.Write(0);//final score, 0 for now
                replayWriter.Write(0);//final result, 0 for now
                replayWriter.Write(zoneId);
                replayWriter.Write(rogue);
                replayWriter.Write(seeded);
                replayWriter.Write(Save.ActiveTeam.GetReferenceName());
                replayWriter.Write(Save.StartDate);
                replayWriter.Write(Save.Rand.FirstSeed);
                replayWriter.Write(DiagManager.Instance.CurSettings.Language);

                replayWriter.Flush();
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public void ResumePlay(ReplayData replay)
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
                    replayWriter.BaseStream.Seek(0, SeekOrigin.End);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

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

        public void LogGroundsave()
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

        public void LogUIPlay(params int[] code)
        {
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
                        Data.GameProgress.SaveMainData(replayWriter, epitaph);

                        //pointers and score
                        replayWriter.BaseStream.Seek(sizeof(Int32) * 4, SeekOrigin.Begin);
                        replayWriter.Write(epitaphPos);
                        replayWriter.Write(epitaph.ActiveTeam.GetTotalScore());
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
                    string renamedFile = GetNonConflictingSavePath(PathMod.ModSavePath(REPLAY_PATH), outFile, REPLAY_EXTENSION);

                    if (renamedFile != null)
                        Directory.Move(fullPath, PathMod.ModSavePath(REPLAY_PATH, renamedFile + REPLAY_EXTENSION));
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

        public static string GetNonConflictingSavePath(string folderPath, string fileName, string fileExtension)
        {
            if (!File.Exists(folderPath + fileName + fileExtension))
                return fileName;

            uint copy_index = 1;
            while (copy_index < UInt32.MaxValue)
            {
                if (!File.Exists(folderPath + fileName + "_" + copy_index.ToString() + fileExtension))
                    return fileName + "_" + copy_index.ToString();
                copy_index++;
            }
            return null;
        }

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


        public bool FoundRecords(string mainPath)
        {
            if (!Directory.Exists(mainPath))
                return false;
            string[] files = Directory.GetFiles(mainPath);
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
                        Version version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                        //epitaph location
                        long endPos = reader.ReadInt64();
                        //read score
                        record.Score = reader.ReadInt32();
                        //read result
                        record.Result = (GameProgress.ResultType)reader.ReadInt32();
                        //read zone ID
                        record.Zone = reader.ReadInt32();
                        record.IsRogue = reader.ReadBoolean();
                        record.IsSeeded = reader.ReadBoolean();
                        //name, date
                        record.Name = reader.ReadString();
                        record.DateTimeString = reader.ReadString();
                        record.Seed = reader.ReadUInt64();

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
                        //read score
                        reader.ReadInt32();
                        //read result
                        reader.ReadInt32();
                        //read zone
                        reader.ReadInt32();
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
                                    GameAction play = new GameAction((GameAction.ActionType)reader.ReadByte(), (Dir8)reader.ReadByte());
                                    int totalArgs = reader.ReadByte();
                                    for (int ii = 0; ii < totalArgs; ii++)
                                        play.AddArg(reader.ReadInt32());
                                    replay.Actions.Add(play);
                                }
                                else if (type == (byte)ReplayData.ReplayLog.UILog)
                                {
                                    int totalCodes = reader.ReadByte();
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
                            ZoneLoc goal = new ZoneLoc(reader.ReadInt32(), new SegLoc(reader.ReadInt32(), reader.ReadInt32()));
                            if (goal.ID != mail.Goal.ID || goal.StructID.Segment != mail.Goal.StructID.Segment || goal.StructID.ID != mail.Goal.StructID.ID)
                                continue;
                            Version version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                            if (version != mail.DefeatedVersion)
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
                        ZoneLoc goal = new ZoneLoc(reader.ReadInt32(), new SegLoc(reader.ReadInt32(), reader.ReadInt32()));
                        Version version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

                        IFormatter formatter = new BinaryFormatter();
                        return (BaseRescueMail)formatter.Deserialize(stream);
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
                renamedFile = GetNonConflictingSavePath(folderPath, mail.TeamID.ToString(), mail.Extension);

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
                    writer.Write(mail.DefeatedVersion.Major);
                    writer.Write(mail.DefeatedVersion.Minor);
                    writer.Write(mail.DefeatedVersion.Build);
                    writer.Write(mail.DefeatedVersion.Revision);
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, mail);
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

                //TODO: v0.5: remove this
                //versionless load
                try
                {
                    using (FileStream stream = File.OpenRead(PathMod.ModSavePath(SAVE_PATH, SAVE_FILE_PATH)))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                            return (MainProgress)GameProgress.LoadMainData(reader);
                    }
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }

            return null;
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
            using (Stream stream = new FileStream(PathMod.ModSavePath(SAVE_PATH, SAVE_FILE_PATH), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                //saves scene, zone, and ground, if there will be one...
                using (BinaryWriter writer = new BinaryWriter(stream))
                    SaveGameState(writer, state);
            }
        }

        private void saveTeamMemberStatusRefs(BinaryWriter writer, ref int totalStatusRefs, Faction faction, int teamIndex, bool guest, List<Character> playerList)
        {
            for (int jj = 0; jj < playerList.Count; jj++)
            {
                foreach (StatusEffect status in playerList[jj].IterateStatusEffects())
                {
                    if (status.TargetChar != null)
                    {
                        CharIndex charIndex = ZoneManager.Instance.CurrentMap.GetCharIndex(status.TargetChar);
                        writer.Write((int)faction);//team
                        writer.Write(teamIndex);//team
                        writer.Write(guest);//guest
                        writer.Write(jj);//player
                        writer.Write(status.ID);//status ID
                        writer.Write((int)charIndex.Faction);//target team index
                        writer.Write(charIndex.Team);//target team index
                        writer.Write(charIndex.Char);//target char index
                        writer.Write(charIndex.Guest);//target char index

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

            if (ZoneManager.Instance.CurrentMap != null)
            {
                long currentPos = writer.BaseStream.Position;
                writer.Write(0);
                //on top level: save status references
                int totalStatusRefs = 0;

                saveTeamStatusRefs(writer, ref totalStatusRefs, Faction.Player, 0, Save.ActiveTeam);

                for (int ii = 0; ii < ZoneManager.Instance.CurrentMap.AllyTeams.Count; ii++)
                    saveTeamStatusRefs(writer, ref totalStatusRefs, Faction.Friend, ii, ZoneManager.Instance.CurrentMap.AllyTeams[ii]);
                for (int ii = 0; ii < ZoneManager.Instance.CurrentMap.MapTeams.Count; ii++)
                    saveTeamStatusRefs(writer, ref totalStatusRefs, Faction.Foe, ii, ZoneManager.Instance.CurrentMap.MapTeams[ii]);

                writer.BaseStream.Seek(currentPos, SeekOrigin.Begin);
                writer.Write(totalStatusRefs);
                writer.BaseStream.Seek(0, SeekOrigin.End);
            }
            
        }

        /// <summary>
        /// Returns game progress and current zone.
        /// </summary>
        /// <returns></returns>
        public GameState LoadMainGameState()
        {
            if (File.Exists(PathMod.ModSavePath(SAVE_PATH, SAVE_FILE_PATH)))
            {
                try
                {
                    using (Stream stream = new FileStream(PathMod.ModSavePath(SAVE_PATH, SAVE_FILE_PATH), FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        //loads dungeon, zone, and ground, if there will be one...
                        using (BinaryReader reader = new BinaryReader(stream))
                            return ReadGameState(reader, false);
                    }
                }
                catch (Exception ex)
                {
                    //In this case, the error will be presented clearly to the player.  Do not signal.
                    DiagManager.Instance.LogError(ex, false);
                }

                //TODO: v0.5: remove this
                //versionless read
                try
                {
                    using (Stream stream = new FileStream(PathMod.ModSavePath(SAVE_PATH, SAVE_FILE_PATH), FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        //loads dungeon, zone, and ground, if there will be one...
                        using (BinaryReader reader = new BinaryReader(stream))
                            return ReadGameState(reader, true);
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

        public GameState ReadGameState(BinaryReader reader, bool versionless)
        {
            GameState state = new GameState();
            //TODO: v0.5: remove this
            Version version = new Version();
            if (!versionless)
                version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

            state.Save = GameProgress.LoadMainData(reader);
            if (version < Versioning.GetVersion())
            {
                //reload AI
                foreach (Character player in state.Save.ActiveTeam.Players)
                {
                    AITactic ai;
                    if (player.Tactic != null)
                        ai = GetAITactic(player.Tactic.ID);
                    else
                        ai = GetAITactic(0);
                    player.Tactic = new AITactic(ai);
                }
                foreach (Character player in state.Save.ActiveTeam.Assembly)
                {
                    AITactic ai;
                    if (player.Tactic != null)
                        ai = GetAITactic(player.Tactic.ID);
                    else
                        ai = GetAITactic(0);
                    player.Tactic = new AITactic(ai);
                }

                //update unlocks
                GameProgress.UnlockState[] unlocks = new GameProgress.UnlockState[DataIndices[DataType.Monster].Count];
                Array.Copy(state.Save.Dex, unlocks, Math.Min(unlocks.Length, state.Save.Dex.Length));
                state.Save.Dex = unlocks;

                //TODO: v0.5 Remove this
                if (state.Save.RogueStarters == null)
                {
                    state.Save.RogueStarters = new bool[DataIndices[DataType.Monster].Count];
                    for (int ii = 0; ii < unlocks.Length; ii++)
                        state.Save.RogueStarters[ii] = unlocks[ii] == GameProgress.UnlockState.Completed;
                }
                bool[] starterUnlocks = new bool[DataIndices[DataType.Monster].Count];
                Array.Copy(state.Save.RogueStarters, starterUnlocks, Math.Min(starterUnlocks.Length, state.Save.Dex.Length));
                state.Save.RogueStarters = starterUnlocks;

                unlocks = new GameProgress.UnlockState[DataIndices[DataType.Zone].Count];
                Array.Copy(state.Save.DungeonUnlocks, unlocks, Math.Min(unlocks.Length, state.Save.DungeonUnlocks.Length));
                state.Save.DungeonUnlocks = unlocks;

                ZoneManager.LoadDefaultState(state);
            }
            else
                ZoneManager.LoadToState(reader, state);

            LuaEngine.Instance.UpdateZoneInstance();


            if (state.Zone.CurrentMap != null)
            {
                //need to reset the map's activeteam
                state.Zone.CurrentMap.ActiveTeam = state.Save.ActiveTeam;

                //on top level: reconnect status references
                int totalStatusRefs = reader.ReadInt32();
                for (int ii = 0; ii < totalStatusRefs; ii++)
                {
                    Faction faction = (Faction)reader.ReadInt32();//faction
                    int teamIndex = reader.ReadInt32();//team
                    bool guest = reader.ReadBoolean();//guest
                    int player = reader.ReadInt32();//player
                    int statusID = reader.ReadInt32();//status ID
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
                    List<Character> playerList = guest ? team.Guests : team.Players;
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
                    List<Character> targetPlayerList = targetGuest ? targetTeam.Guests : targetTeam.Players;
                    status.TargetChar = targetPlayerList[targetChar];
                    status.TargetChar.StatusesTargetingThis.Add(new StatusRef(status.ID, playerList[player]));
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
                //if (Directory.Exists(REPLAY_PATH))
                //{
                //    Directory.Delete(REPLAY_PATH, true);
                //    Directory.CreateDirectory(REPLAY_PATH);
                //}
            }
            catch (Exception ex)
            {
                //In this case, the error will be presented clearly to the player.  Do not signal.
                DiagManager.Instance.LogError(ex, false);
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


        /// <summary>
        /// Allows querying the unlock status of a dungeon.
        /// </summary>
        /// <param name="dungeonid"></param>
        /// <returns></returns>
        public GameProgress.UnlockState GetDungeonUnlockStatus(int dungeonid)
        {
            return DataManager.Instance.Save.DungeonUnlocks[dungeonid];
        }
        public void UnlockDungeon(int dungeonid)
        {
            if (DataManager.Instance.Save.DungeonUnlocks[dungeonid] == GameProgress.UnlockState.None)
                DataManager.Instance.Save.DungeonUnlocks[dungeonid] = GameProgress.UnlockState.Discovered;
        }

    }
}
