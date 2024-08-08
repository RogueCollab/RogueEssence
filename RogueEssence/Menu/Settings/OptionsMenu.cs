using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Menu
{
    public class OptionsMenu : TitledStripMenu
    {

        public OptionsMenu()
        {
            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_SETTINGS_TITLE"), () => { MenuManager.Instance.AddMenu(new SettingsTitleMenu(), false); }));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_KEYBOARD_TITLE"), () => { MenuManager.Instance.AddMenu(new KeyControlsMenu(), false); }));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_GAMEPAD_TITLE"), () => { MenuManager.Instance.AddMenu(new GamepadControlsMenu(), false); }));

            Initialize(new Loc(16, 16), CalculateChoiceLength(choices, 72), Text.FormatKey("MENU_OPTIONS_TITLE"), choices.ToArray(), 0);
        }

    }
}
