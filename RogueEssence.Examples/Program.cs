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
            
            string[] args = Environment.GetCommandLineArgs();
            PathMod.InitExePath(args[0]);
            DiagManager.InitInstance();
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
                string mod = "";
                bool buildMod = false;
                string playInputs = null;
                for (int ii = 1; ii < args.Length; ii++)
                {
                    if (args[ii] == "-dev")
                        dev = true;
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
                    else if (args[ii] == "-mod")
                    {
                        mod = args[ii + 1];
                        ii++;
                    }
                    else if (args[ii] == "-build")
                    {
                        buildMod = true;
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

                DiagManager.Instance.DevMode = dev;

                if (mod != "")
                {
                    if (Directory.Exists(Path.Combine(PathMod.MODS_PATH, mod)))
                    {
                        PathMod.Mod = PathMod.MODS_FOLDER + mod;

                        DiagManager.Instance.LogInfo(String.Format("Loaded mod \"{0}\".", mod));
                    }
                    else
                        DiagManager.Instance.LogInfo(String.Format("Cannot find mod \"{0}\" in {1}. Falling back to base game.", mod, PathMod.MODS_PATH));
                }


                if (playInputs != null)
                    DiagManager.Instance.LoadInputs(playInputs);

                Text.Init();
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

                if (buildMod)
                {
                    if (PathMod.Mod == "")
                    {
                        DiagManager.Instance.LogInfo("No mod specified to build.");
                        return;
                    }
                    RogueEssence.Dev.DevHelper.MergeMod(mod);

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

                if (reserializeIndices != DataManager.DataType.None)
                {
                    LuaEngine.InitInstance();
                    DataManager.InitInstance();

                    DataManager.InitDataDirs(PathMod.ModPath(""));
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

                    DataManager.Instance.UniversalData = (TypeDict<BaseData>)RogueEssence.Dev.DevHelper.LoadWithLegacySupport(PathMod.ModPath(DataManager.MISC_PATH + "Index.bin"), typeof(TypeDict<BaseData>));
                    RogueEssence.Dev.DevHelper.RunExtraIndexing(reserializeIndices);
                    return;
                }

                if (convertIndices != DataManager.DataType.None)
                {
                    LuaEngine.InitInstance();
                    DataManager.InitInstance();
                    DataManager.InitDataDirs(PathMod.ModPath(""));
                    RogueEssence.Dev.DevHelper.RunIndexing(convertIndices);

                    DataManager.Instance.UniversalData = (TypeDict<BaseData>)RogueEssence.Dev.DevHelper.LoadWithLegacySupport(PathMod.ModPath(DataManager.MISC_PATH + "Index.bin"), typeof(TypeDict<BaseData>));
                    RogueEssence.Dev.DevHelper.RunExtraIndexing(convertIndices);
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

            DataEditor.AddEditor(new StatusEffectEditor());
            DataEditor.AddEditor(new MapStatusEditor());

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

            DataEditor.AddEditor(new HashSetEditor<int>());
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
