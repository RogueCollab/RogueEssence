using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class RecordsMenu : SingleStripMenu
    {
        public RecordsMenu()
        {
            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            if (DataManager.Instance.FoundRecords(PathMod.ModSavePath(DataManager.REPLAY_PATH), DataManager.REPLAY_EXTENSION))
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REPLAYS_TITLE"), () => { MenuManager.Instance.AddMenu(new ReplaysMenu(), false); }));
            Dictionary<string, List<RecordHeaderData>> scores = RecordHeaderData.LoadHighScores();
            if (scores.Count > 0)
            {
                string minDungeon = null;
                foreach (string key in scores.Keys)
                {
                    minDungeon = key;
                    break;
                }
                if (minDungeon != null)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_SCORES_TITLE"), () => { MenuManager.Instance.AddMenu(new ScoreMenu(scores, minDungeon, null), false); }));
            }

            if (DataManager.Instance.Save != null)
            {
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_DEX_TITLE"), () => { MenuManager.Instance.AddMenu(new DexMenu(), false); }));
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_DELETE_SAVE_TITLE"), DeleteAction));
            }

            if (DataManager.Instance.ReplaysExist())
            {
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_DELETE_REPLAY_TITLE"), DeleteReplayAction));
            }

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);
        }

        private void DeleteAction()
        {
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(MonsterID.Invalid, null, new EmoteStyle(0), true, 
                () => {
                    MenuManager.Instance.AddMenu(MenuManager.Instance.CreateQuestion(MonsterID.Invalid, null, new EmoteStyle(0), Text.FormatKey("DLG_DELETE_CONFIRM"), true, false, false, false, () =>
                    {
                        MenuManager.Instance.ClearMenus();
                        DataManager.Instance.DeleteSaveData();
                        MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(false, Text.FormatKey("DLG_DELETE_COMPLETE")), false);
                        MenuManager.Instance.EndAction = GameManager.Instance.FadeOut(false);
                        GameManager.Instance.SceneOutcome = GameManager.Instance.RestartToTitle();
                    }, () => { }, true), false);
                },
                -1, false, false, false, Text.FormatKey("DLG_DELETE_SAVE")), false);
        }

        private void DeleteReplayAction()
        {
            MenuManager.Instance.ClearMenus();
            List <DialogueChoice> choices = new List<DialogueChoice>();

            choices.Add(new DialogueChoice(Text.FormatKey("DLG_CHOICE_YES"), () =>
            {
                MenuManager.Instance.ClearMenus();
                DataManager.Instance.DeleteReplayData();
                MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(false, Text.FormatKey("DLG_DELETE_REPLAY_COMPLETE")), false);
                MenuManager.Instance.EndAction = GameManager.Instance.FadeOut(false);
                GameManager.Instance.SceneOutcome = GameManager.Instance.RestartToTitle();
            }));

            choices.Add(new DialogueChoice(Text.FormatKey("DLG_CHOICE_NO"), () =>
            {
                MenuManager.Instance.ClearMenus();
                DataManager.Instance.DeleteNonFavReplayData();
                MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(false, Text.FormatKey("DLG_DELETE_REPLAY_COMPLETE")), false);
                MenuManager.Instance.EndAction = GameManager.Instance.FadeOut(false);
                GameManager.Instance.SceneOutcome = GameManager.Instance.RestartToTitle();
            }));

            choices.Add(new DialogueChoice(Text.FormatKey("MENU_CANCEL"), () => { }));

            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateMultiQuestion(Text.FormatKey("DLG_DELETE_REPLAY"), false, choices, 2, 2), true);
        }
    }
}
