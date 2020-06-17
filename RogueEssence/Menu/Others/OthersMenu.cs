using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Menu
{
    public class OthersMenu : TitledStripMenu
    {

        public OthersMenu()
        {
            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MSG_LOG_TITLE"), () => { MenuManager.Instance.AddMenu(new MsgLogMenu(), false); }));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_SETTINGS_TITLE"), () => { MenuManager.Instance.AddMenu(new SettingsMenu(true), false); }));

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), Text.FormatKey("MENU_OTHERS_TITLE"), choices.ToArray(), 0);
        }

    }
}
