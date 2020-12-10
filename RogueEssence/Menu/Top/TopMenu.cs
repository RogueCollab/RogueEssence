using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using System.IO;
using RogueEssence.Dungeon;
using RogueEssence.Script;

namespace RogueEssence.Menu
{
    public class TopMenu : SingleStripMenu
    {
        public TopMenu()
        {
            bool inMod = PathMod.Mod != "";
            List<MenuTextChoice> choices = new List<MenuTextChoice>();

            if (DataManager.Instance.Save != null)
            {
                if (DataManager.Instance.Save.Rescue != null)
                {
                    if (DataManager.Instance.Save.Rescue.SOS.RescuingTeam == null)
                        choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_RESCUE_AWAIT"), () => { MenuManager.Instance.AddMenu(new RescueMenu(), false); }));
                    else
                        choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_CONTINUE"), () => { Continue(DataManager.Instance.Save.Rescue.SOS); }));
                }
                else
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_CONTINUE"), () => { Continue(null); }));
                if (DataManager.Instance.Save.ActiveTeam.Name != "" && !inMod)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_ROGUE"), () => { MenuManager.Instance.AddMenu(new RogueMenu(), false); }));
            }
            else
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_NEW"), () => { MenuManager.Instance.AddMenu(new MainStartingMenu(), false); }));


            if (DataManager.Instance.FoundRecords(Data.DataManager.REPLAY_PATH) || DataManager.Instance.Save != null || RecordHeaderData.LoadHighScores().Count > 0)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_RECORD"), () => { MenuManager.Instance.AddMenu(new RecordsMenu(), false); }));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_OPTIONS_TITLE"), () => { MenuManager.Instance.AddMenu(new OptionsMenu(), false); }));
            if (!inMod)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MODS_TITLE"), () => { MenuManager.Instance.AddMenu(new ModsMenu(), false); }));
            else
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MODS_EXIT"), exitMod));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_QUIT_GAME"), exitGame));

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);

        }

        protected override void MenuPressed()
        {

        }

        protected override void Canceled()
        {

        }

        private void exitMod()
        {
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.SetMod("");
        }


        private void exitGame()
        {
            GameBase.CurrentPhase = GameBase.LoadPhase.Unload;
        }


        private static void cannotRead(string path)
        {
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_ERR_READ_FILE"),
                Text.FormatKey("DLG_ERR_READ_FILE_FALLBACK", path)), false);
        }

        public static void Continue(SOSMail rescueMail)
        {
            //check for presence of a main save-quicksave
            ReplayData replay = null;
            string recordDir = DataManager.QUICKSAVE_FILE_PATH;
            if (File.Exists(recordDir))
            {
                replay = DataManager.Instance.LoadReplay(recordDir, true);
                if (replay == null)
                {
                    cannotRead(recordDir);
                    return;
                }
            }
            if (replay != null)
            {
                MenuManager.Instance.ClearMenus();
                GameManager.Instance.SceneOutcome = continueReplay(replay, rescueMail);
                return;
            }

            //then, we should load a main save instead
            GameState state = DataManager.Instance.LoadMainGameState();
            if (state == null)
            {
                cannotRead(DataManager.SAVE_FILE_PATH);
                return;
            }
            if (state.Save.Rescue != null)
            {
                state.Save.Rescue = null;
                DataManager.Instance.SaveGameState(state);
            }
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = continueMain(state);
        }

        private static IEnumerator<YieldInstruction> continueReplay(ReplayData replay, SOSMail rescueMail)
        {
            GameManager.Instance.BGM("", true);
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            DataManager.Instance.MsgLog.Clear();
            //load that up instead if found
            GameState state = replay.ReadState();
            DataManager.Instance.SetProgress(state.Save);
            LuaEngine.Instance.LoadSavedData(DataManager.Instance.Save); //notify script engine
            ZoneManager.LoadFromState(state.Zone);

            //NOTE: In order to preserve debug consistency, you SHOULD set the language to that of the quicksave.
            //HOWEVER, it would be too inconvenient for players sharing their quicksaves, thus this feature is LEFT OUT.

            DataManager.Instance.Loading = DataManager.LoadMode.Loading;
            DataManager.Instance.CurrentReplay = replay;

            if (rescueMail != null)
                DataManager.Instance.Save.Rescue = new RescueState(rescueMail, false);

            yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnInit());

            if (DataManager.Instance.Save.NextDest.IsValid())
            {
                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.Save.NextDest));
            }
            else
            {
                //no valid next dest happens when the player has saved in a ground map in the middle of an adventure
                DataManager.Instance.ResumePlay(DataManager.Instance.CurrentReplay.RecordDir, DataManager.Instance.CurrentReplay.QuicksavePos);
                DataManager.Instance.CurrentReplay = null;

                GameManager.Instance.SetFade(true, false);

                DataManager.Instance.Loading = DataManager.LoadMode.None;

                if (ZoneManager.Instance.CurrentMapID.Segment > -1)
                {
                    GameManager.Instance.MoveToScene(Dungeon.DungeonScene.Instance);
                    GameManager.Instance.BGM(ZoneManager.Instance.CurrentMap.Music, true);
                }
                else
                {
                    GameManager.Instance.MoveToScene(Ground.GroundScene.Instance);
                    GameManager.Instance.BGM(ZoneManager.Instance.CurrentGround.Music, true);
                }

                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeIn());
            }
        }

        private static IEnumerator<YieldInstruction> continueMain(GameState mainState)
        {
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            MainProgress mainSave = mainState.Save as MainProgress;
            DataManager.Instance.SetProgress(mainSave);
            mainSave.MergeDataTo(mainSave);
            ZoneManager.LoadFromState(mainState.Zone);

            yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnInit());
            if (ZoneManager.Instance.CurrentMapID.Segment > -1)
                GameManager.Instance.MoveToScene(DungeonScene.Instance);
            else
                GameManager.Instance.MoveToScene(Ground.GroundScene.Instance);

            if (DataManager.Instance.Save.NextDest.IsValid())
                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.Save.NextDest));
            else
            {
                if (ZoneManager.Instance.CurrentMapID.Segment > -1)
                    GameManager.Instance.BGM(ZoneManager.Instance.CurrentMap.Music, true);
                else
                    GameManager.Instance.BGM(ZoneManager.Instance.CurrentGround.Music, true);

                yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentGround.OnInit());

                Content.GraphicsManager.GlobalIdle = Content.GraphicsManager.IdleAction;
                yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeIn());
            }
        }



    }
}
