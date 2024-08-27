#region Using Statements
using System;
using System.Threading;
using System.Globalization;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev;
using RogueEssence.Dungeon;
using RogueEssence.Script;
using RogueEssence;
using System.Reflection;
using Microsoft.Xna.Framework;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using SDL2;
using RogueEssence.Ground;
using RogueElements;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
#endregion

namespace RogueEssence.Examples
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            InitDllMap();
            //SetProcessDPIAware();
            //TODO: figure out how to set this switch in appconfig
            AppContext.SetSwitch("Switch.System.Runtime.Serialization.SerializationGuard.AllowFileWrites", true);
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            string[] args = Environment.GetCommandLineArgs();
            PathMod.InitPathMod(args[0]);
            DiagManager.InitInstance();
            Serializer.InitSettings(new SerializerContractResolver(), new DefaultSerializationBinder());
            DiagManager.Instance.CurSettings = DiagManager.Instance.LoadSettings();

            try
            {
                DiagManager.Instance.LogInfo("=========================================");
                DiagManager.Instance.LogInfo(String.Format("SESSION STARTED: {0}", String.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now)));
                DiagManager.Instance.LogInfo("Version: " + Versioning.GetVersion().ToString());
                DiagManager.Instance.LogInfo(Versioning.GetDotNetInfo());
                DiagManager.Instance.LogInfo("=========================================");


                bool logInput = true;
                GraphicsManager.AssetType convertAssets = GraphicsManager.AssetType.None;
                DataManager.DataType convertIndices = DataManager.DataType.None;
                DataManager.DataType reserializeIndices = DataManager.DataType.None;
                string langArgs = "";
                bool dev = false;
                bool devLua = false;
                string quest = "";
                List<string> mod = new List<string>();
                bool buildQuest = false;
                bool loadModXml = true;
                string playInputs = null;
                bool dump = false;
                bool preConvert = false;
                for (int ii = 1; ii < args.Length; ii++)
                {
                    if (args[ii] == "-dev")
                    {
                        dev = true;
                        devLua = true;
                    }
                    else if (args[ii] == "-play" && args.Length > ii + 1)
                    {
                        playInputs = args[ii + 1];
                        ii++;
                    }
                    else if (args[ii] == "-lang" && args.Length > ii + 1)
                    {
                        langArgs = args[ii + 1];
                        ii++;
                    }
                    else if (args[ii] == "-nolog")
                        logInput = false;
                    else if (args[ii] == "-asset")
                    {
                        PathMod.ASSET_PATH = Path.GetFullPath(args[ii + 1]);
                        ii++;
                    }
                    else if (args[ii] == "-raw")
                    {
                        PathMod.DEV_PATH = Path.GetFullPath(args[ii + 1]);
                        ii++;
                    }
                    else if (args[ii] == "-quest")
                    {
                        quest = args[ii + 1];
                        loadModXml = false;
                        ii++;
                    }
                    else if (args[ii] == "-mod")
                    {
                        int jj = 1;
                        while (args.Length > ii + jj)
                        {
                            if (args[ii + jj].StartsWith("-"))
                                break;
                            else
                                mod.Add(args[ii + jj]);
                            jj++;
                        }
                        loadModXml = false;
                        ii += jj - 1;
                    }
                    else if (args[ii] == "-build")
                    {
                        buildQuest = true;
                        loadModXml = false;
                        ii++;
                    }
                    else if (args[ii] == "-convert")
                    {
                        int jj = 1;
                        while (args.Length > ii + jj)
                        {
                            GraphicsManager.AssetType conv = GraphicsManager.AssetType.None;
                            foreach (GraphicsManager.AssetType type in Enum.GetValues(typeof(GraphicsManager.AssetType)))
                            {
                                if (args[ii + jj].ToLower() == type.ToString().ToLower())
                                {
                                    conv = type;
                                    break;
                                }
                            }
                            if (conv != GraphicsManager.AssetType.None)
                                convertAssets |= conv;
                            else
                                break;
                            jj++;
                        }
                        loadModXml = false;
                        ii += jj - 1;
                    }
                    else if (args[ii] == "-index")
                    {
                        int jj = 1;
                        while (args.Length > ii + jj)
                        {
                            DataManager.DataType conv = DataManager.DataType.None;
                            foreach (DataManager.DataType type in Enum.GetValues(typeof(DataManager.DataType)))
                            {
                                if (args[ii + jj].ToLower() == type.ToString().ToLower())
                                {
                                    conv = type;
                                    break;
                                }
                            }
                            if (conv != DataManager.DataType.None)
                                convertIndices |= conv;
                            else
                                break;
                            jj++;
                        }
                        loadModXml = false;
                        ii += jj - 1;
                    }
                    else if (args[ii] == "-reserialize")
                    {
                        int jj = 1;
                        while (args.Length > ii + jj)
                        {
                            DataManager.DataType conv = DataManager.DataType.None;
                            foreach (DataManager.DataType type in Enum.GetValues(typeof(DataManager.DataType)))
                            {
                                if (args[ii + jj].ToLower() == type.ToString().ToLower())
                                {
                                    conv = type;
                                    break;
                                }
                            }
                            if (conv != DataManager.DataType.None)
                                reserializeIndices |= conv;
                            else
                                break;
                            jj++;
                        }
                        loadModXml = false;
                        ii += jj - 1;
                    }
                    else if (args[ii] == "-dump")
                    {
                        dump = true;
                        ii++;
                    }
                    else if (args[ii] == "-preconvert")
                    {
                        preConvert = true;
                        ii++;
                    }
                }

                PathMod.InitNamespaces();
                GraphicsManager.InitParams();
                DiagManager.Instance.SetupInputs();

                DiagManager.Instance.DevMode = dev;
                DiagManager.Instance.DebugLua = devLua;

                ModHeader newQuest = ModHeader.Invalid;
                ModHeader[] newMods = new ModHeader[0] { };
                if (quest != "")
                {
                    ModHeader header = PathMod.GetModDetails(Path.Combine(PathMod.MODS_PATH, quest));
                    if (header.IsValid())
                    {
                        newQuest = header;
                        DiagManager.Instance.LogInfo(String.Format("Queued quest for loading: \"{0}\".", quest));
                    }
                    else
                        DiagManager.Instance.LogInfo(String.Format("Cannot find quest \"{0}\" in {1}. Falling back to base game.", quest, PathMod.MODS_PATH));
                }

                if (mod.Count > 0)
                {
                    List<ModHeader> workingMods = new List<ModHeader>();
                    for (int ii = 0; ii < mod.Count; ii++)
                    {
                        ModHeader header = PathMod.GetModDetails(Path.Combine(PathMod.MODS_PATH, mod[ii]));
                        if (header.IsValid())
                        {
                            workingMods.Add(header);
                            DiagManager.Instance.LogInfo(String.Format("Queued mod for loading: \"{0}\".", String.Join(", ", mod[ii])));
                        }
                        else
                        {
                            DiagManager.Instance.LogInfo(String.Format("Cannot find mod \"{0}\" in {1}. It will be ignored.", mod, PathMod.MODS_PATH));
                            mod.RemoveAt(ii);
                            ii--;
                        }
                    }
                    newMods = workingMods.ToArray();
                }

                if (loadModXml)
                    (newQuest, newMods) = DiagManager.Instance.LoadModSettings();

                List<int> loadOrder = new List<int>();
                List<(ModRelationship, List<ModHeader>)> loadErrors = new List<(ModRelationship, List<ModHeader>)>();
                PathMod.ValidateModLoad(newQuest, newMods, loadOrder, loadErrors);
                PathMod.SetMods(newQuest, newMods, loadOrder);
                if (loadErrors.Count > 0)
                {
                    List<string> errorMsgs = new List<string>();
                    foreach ((ModRelationship, List<ModHeader>) loadError in loadErrors)
                    {
                        List<ModHeader> involved = loadError.Item2;
                        switch (loadError.Item1)
                        {
                            case ModRelationship.Incompatible:
                                {
                                    errorMsgs.Add(String.Format("{0} is incompatible with {1}.", involved[0].Namespace, involved[1].Namespace));
                                    errorMsgs.Add("\n");
                                }
                                break;
                            case ModRelationship.DependsOn:
                                {
                                    if (String.IsNullOrEmpty(involved[1].Namespace))
                                        errorMsgs.Add(String.Format("{0} depends on game version {1}.", involved[0].Namespace, involved[1].Version));
                                    else
                                        errorMsgs.Add(String.Format("{0} depends on missing mod {1}.", involved[0].Namespace, involved[1].Namespace));
                                    errorMsgs.Add("\n");
                                }
                                break;
                            case ModRelationship.LoadBefore:
                            case ModRelationship.LoadAfter:
                                {
                                    List<string> cycle = new List<string>();
                                    foreach (ModHeader header in involved)
                                        cycle.Add(header.Namespace);
                                    errorMsgs.Add(String.Format("Load-order loop: {0}.", String.Join(", ", cycle.ToArray())));
                                    errorMsgs.Add("\n");
                                }
                                break;
                        }
                    }
                    DiagManager.Instance.LogError(new Exception("Errors detected in mod load:\n" + String.Join("", errorMsgs.ToArray())));
                    DiagManager.Instance.LogInfo(String.Format("The game will continue execution with mods loaded, but order will be broken!"));
                }
                DiagManager.Instance.PrintModSettings();


                if (playInputs != null)
                    DiagManager.Instance.LoadInputs(playInputs);

                Text.Init();
                if (langArgs != "")
                {
                    if (langArgs.Length > 0)
                    {
                        DiagManager.Instance.CurSettings.Language = langArgs.ToLower();
                        Text.SetCultureCode(langArgs.ToLower());
                    }
                    else
                        DiagManager.Instance.CurSettings.Language = "en";
                }
                Text.SetCultureCode(DiagManager.Instance.CurSettings.Language == "" ? "" : DiagManager.Instance.CurSettings.Language.ToString());

                if (buildQuest)
                {
                    if (!PathMod.Quest.IsValid())
                    {
                        DiagManager.Instance.LogInfo("No quest specified to build.");
                        return;
                    }

                    //we need the datamanager for this, but only while data is hardcoded
                    //TODO: remove when data is no longer hardcoded
                    LuaEngine.InitInstance();
                    LuaEngine.Instance.LoadScripts();
                    DataManager.InitInstance();

                    RogueEssence.Dev.DevHelper.MergeQuest(quest);

                    return;
                }
                if (convertAssets != GraphicsManager.AssetType.None)
                {
                    //run conversions
                    using (GameBase game = new GameBase())
                    {
                        GraphicsManager.InitSystem(game.GraphicsDevice);
                        GraphicsManager.RunConversions(convertAssets);
                    }
                    return;
                }

                if (preConvert)
                {
                    using (GameBase game = new GameBase())
                    {
                        GraphicsManager.InitSystem(game.GraphicsDevice);
                        GraphicsManager.RebuildIndices(GraphicsManager.AssetType.All);
                    }

                    LuaEngine.InitInstance();
                    LuaEngine.Instance.LoadScripts();
                    DataManager.InitInstance();
                    DataManager.Instance.LoadConversions();
                    RogueEssence.Dev.DevHelper.PrepareAssetConversion();
                    return;
                }

                if (reserializeIndices != DataManager.DataType.None)
                {
                    DiagManager.Instance.LogInfo("Beginning Reserialization");

                    using (GameBase game = new GameBase())
                    {
                        GraphicsManager.InitSystem(game.GraphicsDevice);
                        GraphicsManager.RebuildIndices(GraphicsManager.AssetType.All);
                    }

                    //we need the datamanager for this, but only while data is hardcoded
                    //TODO: remove when data is no longer hardcoded
                    LuaEngine.InitInstance();
                    LuaEngine.Instance.LoadScripts();
                    DataManager.InitInstance();
                    DataManager.Instance.LoadConversions();

                    DataManager.InitDataDirs(PathMod.ModPath(""));

                    //load conversions a second time because it mightve changed
                    DataManager.Instance.LoadConversions();
                    RogueEssence.Dev.DevHelper.ReserializeBase();
                    DiagManager.Instance.LogInfo("Reserializing main data");
                    RogueEssence.Dev.DevHelper.Reserialize(reserializeIndices);
                    DiagManager.Instance.LogInfo("Reserializing map data");
                    if ((reserializeIndices & DataManager.DataType.Zone) != DataManager.DataType.None)
                    {
                        RogueEssence.Dev.DevHelper.ReserializeData<Map>(DataManager.DATA_PATH + "Map/", DataManager.MAP_EXT);
                        RogueEssence.Dev.DevHelper.ReserializeData<GroundMap>(DataManager.DATA_PATH + "Ground/", DataManager.GROUND_EXT);
                    }
                    DiagManager.Instance.LogInfo("Reserializing indices");
                    RogueEssence.Dev.DevHelper.RunIndexing(reserializeIndices);

                    DataManager.InitInstance();
                    DataManager.Instance.LoadConversions();

                    DataManager.Instance.UniversalData = DataManager.LoadData<TypeDict<BaseData>>(DataManager.MISC_PATH, "Index", DataManager.DATA_EXT);
                    RogueEssence.Dev.DevHelper.RunExtraIndexing(reserializeIndices);
                    return;
                }

                if (convertIndices != DataManager.DataType.None)
                {
                    //we need the datamanager for this, but only while data is hardcoded
                    //TODO: remove when data is no longer hardcoded
                    LuaEngine.InitInstance();
                    LuaEngine.Instance.LoadScripts();
                    DataManager.InitInstance();
                    DiagManager.Instance.LogInfo("Reserializing indices");
                    DataManager.InitDataDirs(PathMod.ModPath(""));
                    RogueEssence.Dev.DevHelper.RunIndexing(convertIndices);

                    DataManager.Instance.UniversalData = DataManager.LoadData<TypeDict<BaseData>>(DataManager.MISC_PATH, "Index", DataManager.DATA_EXT);
                    RogueEssence.Dev.DevHelper.RunExtraIndexing(convertIndices);
                    return;
                }


                logInput = false; //this feature is disabled for now...
                if (DiagManager.Instance.ActiveDebugReplay == null && logInput)
                    DiagManager.Instance.BeginInput();

                if (DiagManager.Instance.DevMode)
                {
                    InitDataEditor();
                    AppBuilder builder = RogueEssence.Dev.Program.BuildAvaloniaApp();
                    builder.StartWithClassicDesktopLifetime(args);
                }
                else
                {
                    DiagManager.Instance.DevEditor = new EmptyEditor();
                    using (GameBase game = new GameBase())
                        game.Run();
                }

            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                throw;
            }
        }

        // We used to have to map dlls manually, but FNA has a provisional solution now.
        // Keep these comments for clarity
        public static void InitDllMap()
        {
            //CoreDllMap.Init();
            //Assembly fnaAssembly = Assembly.GetAssembly(typeof(Game));
            //CoreDllMap.Register(fnaAssembly);
            //load SDL first before FNA3D to sidestep multiple dylibs problem
            SDL.SDL_GetPlatform();
        }

        public static void InitDataEditor()
        {
            DataEditor.Init();

            DataEditor.AddEditor(new StatusEffectEditor());
            DataEditor.AddEditor(new MapStatusEditor());

            DataEditor.AddEditor(new MoneySpawnZoneStepEditor());
            DataEditor.AddEditor(new SpreadPlanSpacedEditor());
            DataEditor.AddEditor(new SpreadPlanQuotaEditor());
            DataEditor.AddEditor(new SpreadPlanBaseEditor());

            DataEditor.AddEditor(new AutoTileBaseEditor());
            DataEditor.AddEditor(new DataFolderEditor());
            DataEditor.AddEditor(new AnimDataEditor());
            DataEditor.AddEditor(new SoundEditor());
            DataEditor.AddEditor(new MusicEditor());
            DataEditor.AddEditor(new EntryDataEditor());
            DataEditor.AddEditor(new FrameTypeEditor());
            DataEditor.AddEditor(new MapItemEditor());
            DataEditor.AddEditor(new InvItemEditor());

            DataEditor.AddEditor(new MultiStepSpawnerEditor());
            DataEditor.AddEditor(new PickerSpawnerEditor());
            DataEditor.AddEditor(new TeamContextSpawnerEditor());
            DataEditor.AddEditor(new ContextSpawnerEditor());
            DataEditor.AddEditor(new MoneyDivSpawnerEditor());
            DataEditor.AddEditor(new TeamStepSpawnerEditor());
            DataEditor.AddEditor(new StepSpawnerEditor());

            DataEditor.AddEditor(new GridPathCircleEditor());
            DataEditor.AddEditor(new GridPathBranchEditor());

            DataEditor.AddEditor(new AddConnectedRoomsStepEditor());
            DataEditor.AddEditor(new AddDisconnectedRoomsStepEditor());
            DataEditor.AddEditor(new ConnectRoomStepEditor());
            DataEditor.AddEditor(new FloorPathBranchEditor());

            DataEditor.AddEditor(new BaseSpawnStepEditor());
            DataEditor.AddEditor(new MoneySpawnStepEditor());
            DataEditor.AddEditor(new PlaceMobsStepEditor());

            DataEditor.AddEditor(new RoomGenCrossEditor());
            DataEditor.AddEditor(new SizedRoomGenEditor());

            DataEditor.AddEditor(new PromoteBranchEditor());

            DataEditor.AddEditor(new MonsterIDEditor());

            DataEditor.AddEditor(new TeamMemberSpawnEditor());
            DataEditor.AddEditor(new MobSpawnEditor());

            DataEditor.AddEditor(new BaseEmitterEditor());
            DataEditor.AddEditor(new ZoneDataEditor());
            DataEditor.AddEditor(new BattleDataEditor());
            DataEditor.AddEditor(new BattleFXEditor());
            DataEditor.AddEditor(new CircleSquareEmitterEditor());
            DataEditor.AddEditor(new CombatActionEditor());
            DataEditor.AddEditor(new ExplosionDataEditor());
            //DataEditor.AddConverter(new ItemDataConverter());
            //DataEditor.AddConverter(new TileLayerConverter());
            DataEditor.AddEditor(new ShootingEmitterEditor());
            DataEditor.AddEditor(new SkillDataEditor());
            DataEditor.AddEditor(new ColumnAnimEditor());
            DataEditor.AddEditor(new StaticAnimEditor());
            DataEditor.AddEditor(new TypeDictEditor());
            DataEditor.AddEditor(new CategorySpawnEditor(true));
            DataEditor.AddEditor(new DictSpawnEditor());
            DataEditor.AddEditor(new RangeDictEditor(false, true));
            DataEditor.AddEditor(new SpawnListEditor());
            DataEditor.AddEditor(new SpawnRangeListEditor(false, true));
            DataEditor.AddEditor(new PriorityListEditor());
            DataEditor.AddEditor(new PriorityEditor());
            DataEditor.AddEditor(new SegLocEditor());
            DataEditor.AddEditor(new LocEditor());
            DataEditor.AddEditor(new LoopedRandEditor());
            DataEditor.AddEditor(new PresetMultiRandEditor());
            DataEditor.AddEditor(new MoneySpawnRangeEditor(false, true));
            DataEditor.AddEditor(new RandRangeEditor(false, true));
            DataEditor.AddEditor(new RandPickerEditor());
            DataEditor.AddEditor(new MultiRandPickerEditor());
            DataEditor.AddEditor(new IntRangeEditor(false, true));
            DataEditor.AddEditor(new FlagTypeEditor());
            DataEditor.AddEditor(new ColorEditor());
            DataEditor.AddEditor(new TypeEditor());
            DataEditor.AddEditor(new AliasDataEditor());

            //TODO: there is no parameterless interface for hashset
            //so instead we have to do the painful process of manually adding every hashset of every type we actually use.  ugh
            DataEditor.AddEditor(new HashSetEditor<int>());

            DataEditor.AddEditor(new ArrayEditor());
            DataEditor.AddEditor(new DictionaryEditor());
            DataEditor.AddEditor(new NoDupeListEditor());
            DataEditor.AddEditor(new ListEditor());
            DataEditor.AddEditor(new EnumEditor());
            DataEditor.AddEditor(new GuidEditor());
            DataEditor.AddEditor(new StringEditor());
            DataEditor.AddEditor(new DoubleEditor());
            DataEditor.AddEditor(new SingleEditor());
            DataEditor.AddEditor(new BooleanEditor());
            DataEditor.AddEditor(new IntEditor());
            DataEditor.AddEditor(new ByteEditor());
            DataEditor.AddEditor(new ObjectEditor());
        }
    }

}
