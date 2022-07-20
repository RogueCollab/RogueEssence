using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using RogueEssence;
using RogueEssence.Dev;
using Microsoft.Xna.Framework;
using Avalonia.Threading;
using System.Threading;
using RogueEssence.Data;
using RogueEssence.Content;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace RogueEssence.Dev.Views
{
    public class DevForm : Window, IRootEditor
    {
        public bool LoadComplete { get; private set; }

        public MapEditForm MapEditForm;
        public GroundEditForm GroundEditForm;

        private Action pendingEditorAction;
        private Exception pendingException;

        public IMapEditor MapEditor { get { return MapEditForm; } }
        public IGroundEditor GroundEditor { get { return GroundEditForm; } }
        public bool AteMouse { get { return false; } }
        public bool AteKeyboard { get { return false; } }

        private static Dictionary<string, string> devConfig;
        private static bool canSave;



        public DevForm()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void IRootEditor.Load(GameBase game)
        {
            ExecuteOrInvoke(load);
        }

        private void load()
        {
            lock (GameBase.lockObj)
            {
                DevDataManager.Init();

                loadDevConfig();

                reload(DataManager.DataType.All);

                LoadComplete = true;
            }
        }


        void IRootEditor.ReloadData(DataManager.DataType dataType)
        {
            ExecuteOrInvoke(() => { reload(dataType); });
        }

        private void reload(DataManager.DataType dataType)
        {
            lock (GameBase.lockObj)
            {
                if (dataType == DataManager.DataType.All)
                    DevDataManager.ClearCaches();

                ViewModels.DevFormViewModel devViewModel = (ViewModels.DevFormViewModel)this.DataContext;

                if (dataType == DataManager.DataType.All)
                {
                    devViewModel.Game.HideSprites = DataManager.Instance.HideChars;
                    devViewModel.Game.HideObjects = DataManager.Instance.HideObjects;
                    devViewModel.Travel.DebugGen = DiagManager.Instance.ListenGen;
                }

                if ((dataType & DataManager.DataType.Skill) != DataManager.DataType.None)
                {
                    string[] skill_names = DataManager.Instance.DataIndices[DataManager.DataType.Skill].GetLocalStringArray(true);
                    devViewModel.Game.Skills.Clear();
                    for (int ii = 0; ii < skill_names.Length; ii++)
                        devViewModel.Game.Skills.Add(ii.ToString("D3") + ": " + skill_names[ii]);
                    devViewModel.Game.ChosenSkill = -1;
                    devViewModel.Game.ChosenSkill = Math.Min(Math.Max(GetConfig("SkillChoice", 0), 0), devViewModel.Game.Skills.Count - 1);
                }

                if ((dataType & DataManager.DataType.Intrinsic) != DataManager.DataType.None)
                {
                    string[] intrinsic_names = DataManager.Instance.DataIndices[DataManager.DataType.Intrinsic].GetLocalStringArray(true);
                    devViewModel.Game.Intrinsics.Clear();
                    for (int ii = 0; ii < intrinsic_names.Length; ii++)
                        devViewModel.Game.Intrinsics.Add(ii.ToString("D3") + ": " + intrinsic_names[ii]);
                    devViewModel.Game.ChosenIntrinsic = -1;
                    devViewModel.Game.ChosenIntrinsic = Math.Min(Math.Max(GetConfig("IntrinsicChoice", 0), 0), devViewModel.Game.Intrinsics.Count - 1);
                }

                if ((dataType & DataManager.DataType.Status) != DataManager.DataType.None)
                {
                    string[] status_names = DataManager.Instance.DataIndices[DataManager.DataType.Status].GetLocalStringArray(true);
                    devViewModel.Game.Statuses.Clear();
                    for (int ii = 0; ii < status_names.Length; ii++)
                        devViewModel.Game.Statuses.Add(ii.ToString("D3") + ": " + status_names[ii]);
                    devViewModel.Game.ChosenStatus = -1;
                    devViewModel.Game.ChosenStatus = Math.Min(Math.Max(GetConfig("StatusChoice", 0), 0), devViewModel.Game.Statuses.Count - 1);
                }

                if ((dataType & DataManager.DataType.Item) != DataManager.DataType.None)
                {
                    string[] item_names = DataManager.Instance.DataIndices[DataManager.DataType.Item].GetLocalStringArray(true);
                    devViewModel.Game.Items.Clear();
                    for (int ii = 0; ii < item_names.Length; ii++)
                        devViewModel.Game.Items.Add(ii.ToString("D4") + ": " + item_names[ii]);
                    devViewModel.Game.ChosenItem = -1;
                    devViewModel.Game.ChosenItem = Math.Min(Math.Max(GetConfig("ItemChoice", 0), 0), devViewModel.Game.Items.Count - 1);
                }


                if ((dataType & DataManager.DataType.Monster) != DataManager.DataType.None)
                {
                    string[] monster_names = DataManager.Instance.DataIndices[DataManager.DataType.Monster].GetLocalStringArray(true);
                    devViewModel.Player.Monsters.Clear();
                    for (int ii = 0; ii < monster_names.Length; ii++)
                        devViewModel.Player.Monsters.Add(ii.ToString("D3") + ": " + monster_names[ii]);
                    devViewModel.Player.ChosenMonster = -1;
                    devViewModel.Player.ChosenMonster = 0;

                    devViewModel.Player.ChosenForm = -1;
                    devViewModel.Player.ChosenForm = 0;

                    string[] skin_names = DataManager.Instance.DataIndices[DataManager.DataType.Skin].GetLocalStringArray(true);
                    devViewModel.Player.Skins.Clear();
                    for (int ii = 0; ii < DataManager.Instance.DataIndices[DataManager.DataType.Skin].Count; ii++)
                        devViewModel.Player.Skins.Add(skin_names[ii]);
                    devViewModel.Player.ChosenSkin = -1;
                    devViewModel.Player.ChosenSkin = 0;

                    devViewModel.Player.Genders.Clear();
                    for (int ii = 0; ii < 3; ii++)
                        devViewModel.Player.Genders.Add(((Gender)ii).ToString());
                    devViewModel.Player.ChosenGender = -1;
                    devViewModel.Player.ChosenGender = 0;
                }

                if (dataType == DataManager.DataType.All)
                {
                    int globalIdle = GraphicsManager.GlobalIdle;
                    devViewModel.Player.Anims.Clear();
                    for (int ii = 0; ii < GraphicsManager.Actions.Count; ii++)
                        devViewModel.Player.Anims.Add(GraphicsManager.Actions[ii].Name);
                    devViewModel.Player.ChosenAnim = -1;
                    devViewModel.Player.ChosenAnim = globalIdle;
                }

                if ((dataType & DataManager.DataType.Zone) != DataManager.DataType.None)
                {
                    string[] dungeon_names = DataManager.Instance.DataIndices[DataManager.DataType.Zone].GetLocalStringArray(true);
                    devViewModel.Travel.Zones.Clear();
                    for (int ii = 0; ii < dungeon_names.Length; ii++)
                        devViewModel.Travel.Zones.Add(ii.ToString("D2") + ": " + dungeon_names[ii]);
                    devViewModel.Travel.ChosenZone = -1;
                    devViewModel.Travel.ChosenZone = Math.Min(Math.Max(GetConfig("ZoneChoice", 0), 0), devViewModel.Travel.Zones.Count - 1);

                    devViewModel.Travel.ChosenStructure = -1;
                    devViewModel.Travel.ChosenStructure = Math.Min(Math.Max(GetConfig("StructChoice", 0), 0), devViewModel.Travel.Structures.Count - 1);

                    devViewModel.Travel.ChosenFloor = -1;
                    devViewModel.Travel.ChosenFloor = Math.Min(Math.Max(GetConfig("FloorChoice", 0), 0), devViewModel.Travel.Floors.Count - 1);

                    devViewModel.Travel.ChosenGround = -1;
                    devViewModel.Travel.ChosenGround = Math.Min(Math.Max(GetConfig("GroundChoice", 0), 0), devViewModel.Travel.Grounds.Count - 1);
                }

                if (dataType == DataManager.DataType.All)
                    devViewModel.Mods.UpdateMod();

                LoadComplete = true;
            }
        }


        public void Update(GameTime gameTime)
        {
            if (pendingEditorAction != null)
            {
                try
                {
                    pendingEditorAction();
                }
                catch (Exception ex)
                {
                    pendingException = ex;
                }
                pendingEditorAction = null;
            }

            ExecuteOrInvoke(update);
        }

        private void update()
        {
            lock (GameBase.lockObj)
            {
                ViewModels.DevFormViewModel devViewModel = (ViewModels.DevFormViewModel)this.DataContext;

                devViewModel.Player.UpdateLevel();
                if (GameManager.Instance.IsInGame())
                {
                    if (!devViewModel.Player.JustOnce)
                    {
                        if (devViewModel.Player.JustMe)
                        {
                            int currentIdle = Dungeon.DungeonScene.Instance.FocusedCharacter.IdleOverride;
                            if (currentIdle < 0)
                                currentIdle = GraphicsManager.GlobalIdle;
                            devViewModel.Player.ChosenAnim = currentIdle;
                        }
                        else
                            devViewModel.Player.ChosenAnim = GraphicsManager.GlobalIdle;
                    }
                    devViewModel.Player.UpdateSpecies(Dungeon.DungeonScene.Instance.FocusedCharacter.BaseForm);
                }
                if (GroundEditForm != null)
                {
                    ViewModels.GroundEditViewModel vm = (ViewModels.GroundEditViewModel)GroundEditForm.DataContext;
                    vm.Textures.TileBrowser.UpdateFrame();
                }
                if (MapEditForm != null)
                {
                    ViewModels.MapEditViewModel vm = (ViewModels.MapEditViewModel)MapEditForm.DataContext;
                    vm.Textures.TileBrowser.UpdateFrame();
                    vm.Terrain.TileBrowser.UpdateFrame();
                }

                if (canSave)
                    saveConfig();
            }
        }
        public void Draw() { }

        public void OpenGround()
        {
            ExecuteOrInvoke(openGround);
        }

        private void openGround()
        {
            GroundEditForm = new GroundEditForm();
            ViewModels.GroundEditViewModel vm = new ViewModels.GroundEditViewModel();
            GroundEditForm.DataContext = vm;
            vm.LoadFromCurrentGround();
            GroundEditForm.Show();
        }

        public void OpenMap()
        {
            ExecuteOrInvoke(openMap);
        }

        public void openMap()
        {
            MapEditForm = new MapEditForm();
            ViewModels.MapEditViewModel vm = new ViewModels.MapEditViewModel();
            MapEditForm.DataContext = vm;
            vm.LoadFromCurrentMap();
            MapEditForm.Show();
        }

        public void groundEditorClosed(object sender, EventArgs e)
        {
            GameManager.Instance.SceneOutcome = resetEditors();
        }

        public void mapEditorClosed(object sender, EventArgs e)
        {
            GameManager.Instance.SceneOutcome = resetEditors();
        }


        private IEnumerator<YieldInstruction> resetEditors()
        {
            GroundEditForm = null;
            MapEditForm = null;
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.RestartToTitle());
        }


        public void CloseGround()
        {
            if (GroundEditForm != null)
                GroundEditForm.Close();
        }

        public void CloseMap()
        {
            if (MapEditForm != null)
                MapEditForm.Close();
        }


        void LoadGame()
        {
            // Windows - CAN run game in new thread, CAN run game in same thread via dispatch.
            // Mac - CANNOT run game in new thread, CAN run game in same thread via dispatch.
            // Linux - CAN run the game in new thread, CANNOT run game in same thread via dispatch.

            // When the game is started, it should run a continuous loop, blocking the UI
            // However, this is only happening on linux.  Why not windows and mac?
            // With Mac, cocoa can ONLY start the game window if it's on the main thread. Weird...

            if (!OperatingSystem.IsLinux())
                LoadGameDelegate();
            else
            {
                Thread thread = new Thread(LoadGameDelegate);
                thread.IsBackground = true;
                thread.Start();
            }
        }

        /// <summary>
        /// This call cannot be performed within a lock!!
        /// </summary>
        /// <param name="action"></param>
        public static void ExecuteOrPend(Action action)
        {
            if (!OperatingSystem.IsLinux())
                action();
            else
            {
                DevForm editor = (DevForm)DiagManager.Instance.DevEditor;
                editor.pendingEditorAction = action;

                SpinWait.SpinUntil(() => editor.pendingEditorAction == null);

                if (editor.pendingException != null)
                {
                    Exception ex = editor.pendingException;
                    throw ex;
                }
            }
        }

        public static void ExecuteOrInvoke(Action action)
        {
            if (!OperatingSystem.IsLinux())
                action();
            else
                Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Background);
        }

        void LoadGameDelegate()
        {
            DiagManager.Instance.DevEditor = this;
            using (GameBase game = new GameBase())
                game.Run();

            ExecuteOrInvoke(Close);
        }

        public void Window_Loaded(object sender, EventArgs e)
        {
            if (Design.IsDesignMode)
                return;
            //Thread thread = new Thread(LoadGame);
            //thread.IsBackground = true;
            //thread.Start();
            Dispatcher.UIThread.InvokeAsync(LoadGame, DispatcherPriority.Background);
            //LoadGame();
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            DiagManager.Instance.LoadMsg = "Closing...";
            EnterLoadPhase(GameBase.LoadPhase.Unload);
        }

        public static void EnterLoadPhase(GameBase.LoadPhase loadState)
        {
            GameBase.CurrentPhase = loadState;
        }



        private static void loadDevConfig()
        {
            devConfig = new Dictionary<string, string>();

            try
            {
                string configPath = GetConfigPath();
                string folderPath = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                if (File.Exists(configPath))
                {
                    using (FileStream stream = File.OpenRead(configPath))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            while (reader.BaseStream.Position < reader.BaseStream.Length)
                            {
                                string key = reader.ReadString();
                                string val = reader.ReadString();
                                devConfig[key] = val;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }


        private static void saveConfig()
        {
            //save whole file
            try
            {
                using (var writer = new BinaryWriter(new FileStream(GetConfigPath(), FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    foreach (string curKey in devConfig.Keys)
                    {
                        writer.Write(curKey);
                        writer.Write(devConfig[curKey]);
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public static string GetConfig(string key, string def)
        {
            string val;
            if (devConfig.TryGetValue(key, out val))
                return val;
            return def;
        }

        public static int GetConfig(string key, int def)
        {
            string val;
            if (devConfig.TryGetValue(key, out val))
            {
                int result;
                if (Int32.TryParse(val, out result))
                    return result;
            }
            return def;
        }

        public static void SetConfig(string key, int val)
        {
            SetConfig(key, val.ToString());
        }

        public static void SetConfig(string key, string val)
        {
            if (val == null && devConfig.ContainsKey(key))
                devConfig.Remove(key);
            else
                devConfig[key] = val;

            canSave = true;
        }

        public static string GetConfigPath()
        {
            //https://jimrich.sk/environment-specialfolder-on-windows-linux-and-os-x/
            //MacOS actually uses a different folder for config data, traditionally
            //I guess it's the odd one out...
            if (OperatingSystem.IsMacOS())
                return "./devConfig";//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "/Library/Application Support/RogueEssence/config");
            else
                return "./devConfig";//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RogueEssence /devConfig");
        }
    }
}
