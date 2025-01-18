using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class ReplayMenu : TitledStripMenu
    {

        public ReplayMenu() : this(MenuLabel.REPLAY_MENU) { }
        public ReplayMenu(string label)
        {
            Label = label;
            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REPLAY_INFO"), () => { MenuManager.Instance.AddMenu(new ReplayInfoMenu(), false); }));
            if (DiagManager.Instance.DevMode)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REPLAY_QUICKSAVE"), makeQuicksave));

            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REPLAY_END"), endReplay));

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), Text.FormatKey("MENU_REPLAY_TITLE"), choices.ToArray(), 0);
        }

        private void makeQuicksave()
        {
            DataManager.Instance.CreateQuicksaveFromReplay();
            DungeonScene.Instance.LogMsg(String.Format("Created quicksave."));
        }

        private void endReplay()
        {
            MenuManager.Instance.ClearMenus();
            DataManager.Instance.CurrentReplay.CurrentState = DataManager.Instance.CurrentReplay.States.Count;
            GameManager.Instance.SceneOutcome = GameManager.Instance.EndSegment(GameProgress.ResultType.Unknown);
        }
    }
}
