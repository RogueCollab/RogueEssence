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
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_INFO"), () => { MenuManager.Instance.AddMenu(new RogueInfoMenu(), false); }));

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);

        }
    }
}
