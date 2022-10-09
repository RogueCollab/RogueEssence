using System.Collections.Generic;
using System.IO;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Script;
using SDL2;
using System;
using RogueEssence.Content;


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
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REPLAY_SEED"), SeedAction));

            if (DataManager.Instance.GetRecordHeader(recordDir).IsFavorite)
            {
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_FAVORITE_OFF"), UnFavoriteAction));
            }
            else
            {
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_FAVORITE"), FavoriteAction));
            }

            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_DELETE"), DeleteAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            int choiceLength = CalculateChoiceLength(choices, 72);
            Initialize(new Loc(Math.Min(224, GraphicsManager.ScreenWidth - choiceLength), 0), choiceLength, choices.ToArray(), 0);
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

        private void SeedAction()
        {
            GameProgress ending = DataManager.Instance.GetRecord(recordDir);
            if (ending == null)
                cannotRead();
            else
            {
                SDL.SDL_SetClipboardText(ending.Rand.FirstSeed.ToString("X"));
                GameManager.Instance.SE("Menu/Sort");
            }
        }

        // FavoriteAction and UnFavoriteAction could be moved into one function
        private void FavoriteAction()
        {
            DataManager.Instance.ReplaySetFavorite(recordDir, true);
            MenuManager.Instance.RemoveMenu();
        }

        private void UnFavoriteAction()
        {
            DataManager.Instance.ReplaySetFavorite(recordDir, false);
            MenuManager.Instance.RemoveMenu();
        }

        private void DeleteAction()
        {
            if (File.Exists(recordDir))
                File.Delete(recordDir);

            MenuManager.Instance.RemoveMenu();

            if (DataManager.Instance.FoundRecords(PathMod.ModSavePath(DataManager.REPLAY_PATH), DataManager.REPLAY_EXTENSION))
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
                    LuaEngine.Instance.UpdateZoneInstance();

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
