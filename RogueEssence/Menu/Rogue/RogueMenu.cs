using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Menu
{
    public class RogueMenu : SingleStripMenu
    {
        public RogueMenu()
        {
            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_NEW"), () => { MenuManager.Instance.AddMenu(new RogueDestMenu(), false); }));
            if (Data.DataManager.Instance.FoundRecords(Data.DataManager.ROGUE_PATH))
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_LOAD"), () => { MenuManager.Instance.AddMenu(new QuicksaveMenu(), false); }));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_INFO"), GiveInfo));
            //TODO: add support for custom games (seeds, teams, etc.)

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);

        }

        private void GiveInfo()
        {
            //"Rogue is a challenge mode featuring a special set of rules."
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_ROGUE_INFO_1"),
                Text.FormatKey("DLG_ROGUE_INFO_2"), Text.FormatKey("DLG_ROGUE_INFO_3"),
                Text.FormatKey("DLG_ROGUE_INFO_4")), false);
        }
    }
}
