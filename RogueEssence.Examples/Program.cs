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
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
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
            DiagManager.InitInstance();
            GraphicsManager.InitParams();
            Text.Init();
            Text.SetCultureCode("");


            try
            {
                DiagManager.Instance.LogInfo("=========================================");
                DiagManager.Instance.LogInfo(String.Format("SESSION STARTED: {0}", String.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now)));
                DiagManager.Instance.LogInfo("Version: " + Versioning.GetVersion().ToString());
                DiagManager.Instance.LogInfo(Versioning.GetDotNetInfo());
                DiagManager.Instance.LogInfo("=========================================");

                string[] args = System.Environment.GetCommandLineArgs();
                bool logInput = true;
                GraphicsManager.AssetType convertAssets = GraphicsManager.AssetType.None;
                DataManager.DataType convertIndices = DataManager.DataType.None;
                DataManager.DataType reserializeIndices = DataManager.DataType.None;
                string langArgs = "";
                for (int ii = 1; ii < args.Length; ii++)
                {
                    if (args[ii] == "-dev")
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
                    RogueEssence.Dev.DevHelper.Reserialize(reserializeIndices, null);
                    RogueEssence.Dev.DevHelper.ReserializeData(DataManager.DATA_PATH + "Map/", DataManager.MAP_EXT, null);
                    RogueEssence.Dev.DevHelper.ReserializeData(DataManager.DATA_PATH + "Ground/", DataManager.GROUND_EXT, null);
                    RogueEssence.Dev.DevHelper.RunIndexing(reserializeIndices);
                    return;
                }

                if (convertIndices != DataManager.DataType.None)
                {
                    LuaEngine.InitInstance();
                    DataManager.InitInstance();
                    DevHelper.RunIndexing(convertIndices);
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

                //DiagManager.Instance.DevEditor = new DevWindow();

                if (DiagManager.Instance.DevMode)
                {
                    AppBuilder builder = RogueEssence.Dev.Program.BuildAvaloniaApp();
                    builder.StartWithClassicDesktopLifetime(args);
                }
                else
                {
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

        // TheSpyDog's branch on resolving dllmap for DotNetCore
        // https://github.com/FNA-XNA/FNA/pull/315
        public static void InitDllMap()
        {
            CoreDllMap.Init();
            Assembly fnaAssembly = Assembly.GetAssembly(typeof(Game));
            CoreDllMap.Register(fnaAssembly);
        }
    }

}
