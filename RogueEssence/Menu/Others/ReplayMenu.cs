using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class ReplayMenu : TitledStripMenu
    {

        public ReplayMenu()
        {
            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REPLAY_INFO"), () => { MenuManager.Instance.AddMenu(new ReplayInfoMenu(), false); }));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_REPLAY_END"), endReplay));

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), Text.FormatKey("MENU_REPLAY_TITLE"), choices.ToArray(), 0);
        }

        private void endReplay()
        {
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.EndSegment(GameProgress.ResultType.Unknown);
        }
    }
}
