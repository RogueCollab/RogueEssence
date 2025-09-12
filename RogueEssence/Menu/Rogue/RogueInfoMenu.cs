using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Menu
{
    public class RogueInfoMenu : SingleStripMenu
    {
        public RogueInfoMenu() : this(MenuLabel.ROGUE_INFO_MENU) { }
        public RogueInfoMenu(string label)
        {
            Label = label;
            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ROGUE_BASIC_INFO"), GiveInfo));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ROGUE_SPECIAL_INFO"), GiveSpecialInfo));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ROGUE_SEED_INFO"), GiveSeedInfo));

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);
        }

        private void GiveInfo()
        {
            //"Rogue is a challenge mode featuring a special set of rules."
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_ROGUE_INFO_1"),
                Text.FormatKey("DLG_ROGUE_INFO_2"), Text.FormatKey("DLG_ROGUE_INFO_3"),
                Text.FormatKey("DLG_ROGUE_INFO_4")), false);
        }

        private void GiveSpecialInfo()
        {
            //Special conditions
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_ROGUE_SPECIAL_INFO_1"),
                Text.FormatKey("DLG_ROGUE_SPECIAL_INFO_2")), false);
        }

        private void GiveSeedInfo()
        {
            //Seeded runs
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_ROGUE_SEED_INFO_1"),
                Text.FormatKey("DLG_ROGUE_SEED_INFO_2"), Text.FormatKey("DLG_ROGUE_SEED_INFO_3"),
                Text.FormatKey("DLG_ROGUE_SEED_INFO_4")), false);
        }
    }
}
