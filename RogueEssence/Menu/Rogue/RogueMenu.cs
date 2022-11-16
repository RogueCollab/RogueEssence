using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class RogueMenu : SingleStripMenu
    {
        public RogueMenu()
        {
            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_NEW"), () => { MenuManager.Instance.AddMenu(new RogueDestMenu(), false); }));
            if (DataManager.Instance.FoundRecords(PathMod.ModSavePath(DataManager.ROGUE_PATH), DataManager.QUICKSAVE_EXTENSION))
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TOP_LOAD"), () => { MenuManager.Instance.AddMenu(new QuicksaveMenu(), false); }));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_INFO"), () => { MenuManager.Instance.AddMenu(new RogueInfoMenu(), false); }));

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);

        }
    }
}
