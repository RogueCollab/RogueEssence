using System.Collections.Generic;
using System.IO;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Script;


namespace RogueEssence.Menu
{
    public class ReplayChosenMenu : SingleStripMenu
    {

        private string recordDir;

        public ReplayChosenMenu(string dir)
        {
            this.recordDir = dir;


            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_INFO"), SummaryAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REPLAY_REPLAY"), ReplayAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_DELETE"), DeleteAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            Initialize(new Loc(240, 0), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);
        }


        private void cannotRead()
        {
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_ERR_READ_FILE"),
                Text.FormatKey("DLG_ERR_READ_FILE_FALLBACK", recordDir)), false);
        }

        private void SummaryAction()
        {
            GameProgress ending = DataManager.Instance.GetRecord(recordDir);
            if (ending == null)
                cannotRead();
            else
                MenuManager.Instance.AddMenu(new FinalResultsMenu(ending), false);
        }

        private void ReplayAction()
        {
            ReplayData replay = DataManager.Instance.LoadReplay(recordDir, false);
            if (replay == null)
                cannotRead();
            else
            {
                MenuManager.Instance.RemoveMenu();
                TitleScene.TitleMenuSaveState = MenuManager.Instance.SaveMenuState();

                MenuManager.Instance.ClearMenus();
                GameManager.Instance.SceneOutcome = Replay(replay);
            }
        }

        private void DeleteAction()
        {
            if (File.Exists(recordDir))
                File.Delete(recordDir);

            MenuManager.Instance.RemoveMenu();

            if (DataManager.Instance.FoundRecords(DataManager.REPLAY_PATH))
                MenuManager.Instance.ReplaceMenu(new ReplaysMenu());
            else
            {
                MenuManager.Instance.RemoveMenu();
                MenuManager.Instance.ReplaceMenu(new TopMenu());
            }
        }

        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }

        public IEnumerator<YieldInstruction> Replay(ReplayData replay)
        {
            GameManager.Instance.BGM("", true);
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));

            DataManager.Instance.MsgLog.Clear();

            if (replay.States.Count > 0)
            {
                GameState state = replay.ReadState();
                if (state.Save.NextDest.IsValid())
                {
                    DataManager.Instance.SetProgress(state.Save);
                    LuaEngine.Instance.LoadSavedData(DataManager.Instance.Save); //notify script engine
                    ZoneManager.LoadFromState(state.Zone);

                    DataManager.Instance.CurrentReplay = replay;
                    yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.MoveToZone(DataManager.Instance.Save.NextDest));
                    yield break;
                }
            }

            yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_NO_ADVENTURE")));
            GameManager.Instance.SceneOutcome = GameManager.Instance.ReturnToReplayMenu();
        }
    }
}
