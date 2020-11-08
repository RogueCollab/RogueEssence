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

namespace RogueEssence.Dev.Views
{
    public class DevForm : Window, IRootEditor
    {
        public bool LoadComplete { get; private set; }

        //private MapEditor mapEditor;
        public GroundEditForm GroundEditForm;

        public IMapEditor MapEditor => null;
        public IGroundEditor GroundEditor { get { return GroundEditForm; } }
        public bool AteMouse { get { return false; } }
        public bool AteKeyboard { get { return false; } }




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
            executeOrInvoke(load);
        }

        private void load()
        {
            lock (GameBase.lockObj)
            {
                ViewModels.DevFormViewModel devViewModel = (ViewModels.DevFormViewModel)this.DataContext;

                devViewModel.Game.HideSprites = DataManager.Instance.HideChars;
                devViewModel.Game.HideObjects = DataManager.Instance.HideObjects;

                string[] skill_names = DataManager.Instance.DataIndices[DataManager.DataType.Skill].GetLocalStringArray();
                for (int ii = 0; ii < skill_names.Length; ii++)
                    devViewModel.Game.Skills.Add(ii.ToString("D3") + ": " + skill_names[ii]);
                //regVal = Registry.GetValue(DiagManager.REG_PATH, "SkillChoice", 0);
                //cbSkills.SelectedIndex = Math.Min(cbSkills.Items.Count - 1, (regVal != null) ? (int)regVal : 0);
                devViewModel.Game.ChosenSkill = -1;
                devViewModel.Game.ChosenSkill = 0;

                string[] intrinsic_names = DataManager.Instance.DataIndices[DataManager.DataType.Intrinsic].GetLocalStringArray();
                for (int ii = 0; ii < intrinsic_names.Length; ii++)
                    devViewModel.Game.Intrinsics.Add(ii.ToString("D3") + ": " + intrinsic_names[ii]);
                //regVal = Registry.GetValue(DiagManager.REG_PATH, "IntrinsicChoice", 0);
                //cbIntrinsics.SelectedIndex = Math.Min(cbIntrinsics.Items.Count - 1, (regVal != null) ? (int)regVal : 0);
                devViewModel.Game.ChosenIntrinsic = -1;
                devViewModel.Game.ChosenIntrinsic = 0;

                string[] status_names = DataManager.Instance.DataIndices[DataManager.DataType.Status].GetLocalStringArray();
                for (int ii = 0; ii < status_names.Length; ii++)
                    devViewModel.Game.Statuses.Add(ii.ToString("D3") + ": " + status_names[ii]);
                //regVal = Registry.GetValue(DiagManager.REG_PATH, "StatusChoice", 0);
                //cbStatus.SelectedIndex = Math.Min(cbStatus.Items.Count - 1, (regVal != null) ? (int)regVal : 0);
                devViewModel.Game.ChosenStatus = -1;
                devViewModel.Game.ChosenStatus = 0;

                string[] item_names = DataManager.Instance.DataIndices[DataManager.DataType.Item].GetLocalStringArray();
                for (int ii = 0; ii < item_names.Length; ii++)
                    devViewModel.Game.Items.Add(ii.ToString("D3") + ": " + item_names[ii]);
                //object regVal = Registry.GetValue(DiagManager.REG_PATH, "ItemChoice", 0);
                //cbSpawnItem.SelectedIndex = Math.Min(cbSpawnItem.Items.Count - 1, (regVal != null) ? (int)regVal : 0);
                devViewModel.Game.ChosenItem = -1;
                devViewModel.Game.ChosenItem = 0;

                string[] monster_names = DataManager.Instance.DataIndices[DataManager.DataType.Monster].GetLocalStringArray();
                for (int ii = 0; ii < monster_names.Length; ii++)
                    devViewModel.Player.Monsters.Add(ii.ToString("D3") + ": " + monster_names[ii]);
                //cbDexNum.SelectedIndex = 0;
                devViewModel.Player.ChosenMonster = 0;


                string[] skin_names = DataManager.Instance.DataIndices[DataManager.DataType.Skin].GetLocalStringArray();
                for (int ii = 0; ii < DataManager.Instance.DataIndices[DataManager.DataType.Skin].Count; ii++)
                    devViewModel.Player.Skins.Add(skin_names[ii]);
                devViewModel.Player.ChosenSkin = 0;

                for (int ii = 0; ii < 3; ii++)
                    devViewModel.Player.Genders.Add(((Gender)ii).ToString());
                devViewModel.Player.ChosenGender = 0;

                for (int ii = 0; ii < GraphicsManager.Actions.Count; ii++)
                    devViewModel.Player.Anims.Add(GraphicsManager.Actions[ii].Name);
                devViewModel.Player.ChosenAnim = GraphicsManager.GlobalIdle;

                ZoneData zone = DataManager.Instance.GetZone(1);
                for (int ii = 0; ii < zone.GroundMaps.Count; ii++)
                    devViewModel.Travel.Grounds.Add(zone.GroundMaps[ii]);
                //regVal = Registry.GetValue(DiagManager.REG_PATH, "MapChoice", 0);
                //cbMaps.SelectedIndex = Math.Min(Math.Max(0, (regVal != null) ? (int)regVal : 0), cbMaps.Items.Count - 1);
                devViewModel.Travel.ChosenGround = 0;

                string[] dungeon_names = DataManager.Instance.DataIndices[DataManager.DataType.Zone].GetLocalStringArray();
                for (int ii = 0; ii < dungeon_names.Length; ii++)
                    devViewModel.Travel.Zones.Add(ii.ToString("D2") + ": " + dungeon_names[ii]);
                //regVal = Registry.GetValue(DiagManager.REG_PATH, "ZoneChoice", 0);
                //cbZones.SelectedIndex = Math.Min(Math.Max(0, (regVal != null) ? (int)regVal : 0), cbZones.Items.Count - 1);
                devViewModel.Travel.ChosenZone = 0;

                //regVal = Registry.GetValue(DiagManager.REG_PATH, "StructChoice", 0);
                //cbStructure.SelectedIndex = Math.Min(Math.Max(0, (regVal != null) ? (int)regVal : 0), cbStructure.Items.Count - 1);
                devViewModel.Travel.ChosenStructure = 0;

                //regVal = Registry.GetValue(DiagManager.REG_PATH, "FloorChoice", 0);
                //cbFloor.SelectedIndex = Math.Min(Math.Max(0, (regVal != null) ? (int)regVal : 0), cbFloor.Items.Count - 1);

                LoadComplete = true;
            }
        }


        public void Update(GameTime gameTime)
        {
            executeOrInvoke(update);
        }

        private void update()
        {
            lock (GameBase.lockObj)
            {
                ViewModels.DevFormViewModel devViewModel = (ViewModels.DevFormViewModel)this.DataContext;
                devViewModel.Player.ChosenAnim = GraphicsManager.GlobalIdle;
                if (GameManager.Instance.IsInGame())
                {
                    devViewModel.Player.UpdateSpecies(Dungeon.DungeonScene.Instance.FocusedCharacter.BaseForm, Dungeon.DungeonScene.Instance.FocusedCharacter.Level);
                }
            }
        }
        public void Draw() { }

        public void OpenGround()
        {
            GroundEditForm = new GroundEditForm()
            {
                DataContext = new ViewModels.GroundEditViewModel(),
            };
            GroundEditForm.FormClosed += groundEditorClosed;
            GroundEditForm.LoadFromCurrentGround();
            GroundEditForm.Show();
        }

        public void groundEditorClosed(object sender, EventArgs e)
        {
            GameManager.Instance.SceneOutcome = resetEditors();
        }


        private IEnumerator<YieldInstruction> resetEditors()
        {
            GroundEditForm = null;
            //mapEditor = null;
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.RestartToTitle());
        }


        public void CloseGround()
        {
            if (GroundEditForm != null)
                GroundEditForm.Close();
        }



        void LoadGame()
        {
            // Windows - CAN run game in new thread, CAN run game in same thread via dispatch.
            // Mac - CANNOT run game in new thread, CAN run game in same thread via dispatch.
            // Linux - CAN run the game in new thread, CANNOT run game in same thread via dispatch.

            // When the game is started, it should run a continuous loop, blocking the UI
            // However, this is only happening on linux.  Why not windows and mac?
            // With Mac, cocoa can ONLY start the game window if it's on the main thread. Weird...

            if (CoreDllMap.OS == "windows" || CoreDllMap.OS == "osx")
                LoadGameDelegate();
            else
            {
                Thread thread = new Thread(LoadGameDelegate);
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void executeOrInvoke(Action action)
        {
            if (CoreDllMap.OS == "windows" || CoreDllMap.OS == "osx")
                action();
            else
                Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Background);
        }

        void LoadGameDelegate()
        {
            DiagManager.Instance.DevEditor = this;
            using (GameBase game = new GameBase())
                game.Run();
            Close();
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

    }
}
