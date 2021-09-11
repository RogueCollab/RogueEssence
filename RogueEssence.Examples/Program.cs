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
            //TODO: figure out how to set this switch in appconfig
            AppContext.SetSwitch("Switch.System.Runtime.Serialization.SerializationGuard.AllowFileWrites", true);
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            string[] args = System.Environment.GetCommandLineArgs();
            PathMod.InitExePath(System.IO.Path.GetDirectoryName(args[0]));
            DiagManager.InitInstance();
            DiagManager.Instance.CurSettings = DiagManager.Instance.LoadSettings();
            //DiagManager.Instance.UpgradeBinder = new UpgradeBinder();


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
                DataManager.DataType dump = DataManager.DataType.None;
                string langArgs = "";
                for (int ii = 1; ii < args.Length; ii++)
                {
                    if (args[ii] == "-asset")
                    {
                        PathMod.ASSET_PATH = System.IO.Path.GetFullPath(PathMod.ExePath + args[ii + 1]);
                        ii++;
                    }
                    else if (args[ii] == "-raw")
                    {
                        PathMod.DEV_PATH = System.IO.Path.GetFullPath(PathMod.ExePath + args[ii + 1]);
                        ii++;
                    }
                    else if (args[ii] == "-mod")
                    {
                        PathMod.Mod = PathMod.MODS_FOLDER + args[ii + 1];
                        ii++;
                    }
                    else if (args[ii] == "-dev")
                        DiagManager.Instance.DevMode = true;
                    else if (args[ii] == "-play" && args.Length > ii + 1)
                    {
                        DiagManager.Instance.LoadInputs(args[ii + 1]);
                        ii++;
                    }
                    else if (args[ii] == "-lang" && args.Length > ii + 1)
                    {
                        langArgs = args[ii + 1];
                        ii++;
                    }
                    else if (args[ii] == "-nolog")
                        logInput = false;
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
                        ii += jj - 1;
                    }
                    else if (args[ii] == "-dump")
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
                                dump |= conv;
                            else
                                break;
                            jj++;
                        }
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
                        ii += jj - 1;
                    }
                }


                GraphicsManager.InitParams();
                Text.Init();
                Text.SetCultureCode("");

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

                if (reserializeIndices != DataManager.DataType.None)
                {
                    LuaEngine.InitInstance();
                    DataManager.InitInstance();
                    DevHelper.ReserializeBase();
                    DevHelper.Reserialize(reserializeIndices);
                    DevHelper.ReserializeData(DataManager.DATA_PATH + "Map/", DataManager.MAP_EXT);
                    DevHelper.ReserializeData(DataManager.DATA_PATH + "Ground/", DataManager.GROUND_EXT);
                    DevHelper.RunIndexing(reserializeIndices);

                    DataManager.Instance.InitData();
                    DevHelper.RunExtraIndexing(reserializeIndices);
                    return;
                }

                if (convertIndices != DataManager.DataType.None)
                {
                    LuaEngine.InitInstance();
                    DataManager.InitInstance();
                    DevHelper.RunIndexing(convertIndices);

                    DataManager.Instance.InitData();
                    DevHelper.RunExtraIndexing(reserializeIndices);
                    return;
                }

                //For exporting to data
                if (dump > DataManager.DataType.None)
                {
                    LuaEngine.InitInstance();

                    {
                        DataManager.InitInstance();
                        DataInfo.AddEditorOps();
                        DataInfo.AddSystemFX();
                        DataInfo.AddUniversalEvent();
                        DataInfo.AddUniversalData();

                        if ((dump & DataManager.DataType.Element) != DataManager.DataType.None)
                            DataInfo.AddElementData();
                        if ((dump & DataManager.DataType.GrowthGroup) != DataManager.DataType.None)
                            DataInfo.AddGrowthGroupData();
                        if ((dump & DataManager.DataType.SkillGroup) != DataManager.DataType.None)
                            DataInfo.AddSkillGroupData();
                        if ((dump & DataManager.DataType.Emote) != DataManager.DataType.None)
                            DataInfo.AddEmoteData();
                        if ((dump & DataManager.DataType.AI) != DataManager.DataType.None)
                            DataInfo.AddAIData();
                        if ((dump & DataManager.DataType.Tile) != DataManager.DataType.None)
                            DataInfo.AddTileData();
                        if ((dump & DataManager.DataType.Terrain) != DataManager.DataType.None)
                            DataInfo.AddTerrainData();
                        if ((dump & DataManager.DataType.Rank) != DataManager.DataType.None)
                            DataInfo.AddRankData();
                        if ((dump & DataManager.DataType.Skin) != DataManager.DataType.None)
                            DataInfo.AddSkinData();

                        if ((dump & DataManager.DataType.Monster) != DataManager.DataType.None)
                            DataInfo.AddMonsterData();

                        if ((dump & DataManager.DataType.Skill) != DataManager.DataType.None)
                            DataInfo.AddSkillData();

                        if ((dump & DataManager.DataType.Intrinsic) != DataManager.DataType.None)
                            DataInfo.AddIntrinsicData();
                        if ((dump & DataManager.DataType.Status) != DataManager.DataType.None)
                            DataInfo.AddStatusData();
                        if ((dump & DataManager.DataType.MapStatus) != DataManager.DataType.None)
                            DataInfo.AddMapStatusData();

                        if ((dump & DataManager.DataType.Item) != DataManager.DataType.None)
                            DataInfo.AddItemData();

                        if ((dump & DataManager.DataType.Zone) != DataManager.DataType.None)
                        {
                            DataInfo.AddMapData();
                            DataInfo.AddGroundData();
                            DataInfo.AddZoneData();
                        }

                        DevHelper.RunIndexing(dump);

                        DevHelper.RunExtraIndexing(reserializeIndices);
                    }
                    return;
                }

                if (langArgs != "" && DiagManager.Instance.CurSettings.Language == "")
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


                logInput = false; //this feature is disabled for now...
                if (DiagManager.Instance.ActiveDebugReplay == null && logInput)
                    DiagManager.Instance.BeginInput();

                if (DiagManager.Instance.DevMode)
                {
                    InitDataEditor();
                    AppBuilder builder = Dev.Program.BuildAvaloniaApp();
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
                throw ex;
            }
        }

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

            DataEditor.AddEditor(new MoneySpawnZoneStepEditor());

            //DataEditor.AddConverter(new AutoTileBaseConverter());
            DataEditor.AddEditor(new DataFolderEditor());
            DataEditor.AddEditor(new AnimDataEditor());
            DataEditor.AddEditor(new SoundEditor());
            DataEditor.AddEditor(new MusicEditor());
            DataEditor.AddEditor(new EntryDataEditor());
            DataEditor.AddEditor(new FrameTypeEditor());
            DataEditor.AddEditor(new MapItemEditor());

            DataEditor.AddEditor(new MultiStepSpawnerEditor());
            DataEditor.AddEditor(new PickerSpawnerEditor());
            DataEditor.AddEditor(new ContextSpawnerEditor());
            DataEditor.AddEditor(new TeamStepSpawnerEditor());
            DataEditor.AddEditor(new StepSpawnerEditor());

            DataEditor.AddEditor(new GridPathCircleEditor());
            DataEditor.AddEditor(new GridPathBranchEditor());

            DataEditor.AddEditor(new AddConnectedRoomsStepEditor());
            DataEditor.AddEditor(new AddDisconnectedRoomsStepEditor());
            DataEditor.AddEditor(new ConnectRoomStepEditor());
            DataEditor.AddEditor(new FloorPathBranchEditor());

            DataEditor.AddEditor(new RoomGenCrossEditor());
            DataEditor.AddEditor(new SizedRoomGenEditor());

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
            DataEditor.AddEditor(new RangeDictEditor(false, true));
            DataEditor.AddEditor(new SpawnListEditor());
            DataEditor.AddEditor(new SpawnRangeListEditor(false, true));
            DataEditor.AddEditor(new PriorityListEditor());
            DataEditor.AddEditor(new PriorityEditor());
            DataEditor.AddEditor(new SegLocEditor());
            DataEditor.AddEditor(new LocEditor());
            DataEditor.AddEditor(new RandRangeEditor(false, true));
            DataEditor.AddEditor(new RandPickerEditor());
            DataEditor.AddEditor(new MultiRandPickerEditor());
            DataEditor.AddEditor(new IntRangeEditor(false, true));
            DataEditor.AddEditor(new FlagTypeEditor());
            DataEditor.AddEditor(new ColorEditor());
            DataEditor.AddEditor(new TypeEditor());
            DataEditor.AddEditor(new ArrayEditor());
            DataEditor.AddEditor(new DictionaryEditor());
            DataEditor.AddEditor(new NoDupeListEditor());
            DataEditor.AddEditor(new ListEditor());
            DataEditor.AddEditor(new EnumEditor());
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
