using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Menu
{
    public class OthersMenu : TitledStripMenu
    {
        private List<MenuTextChoice> Choices;
        public OthersMenu()
        {
            
        }

        public void SetupChoices()
        {
            Choices = new List<MenuTextChoice>();
            Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MSG_LOG_TITLE"), () => { MenuManager.Instance.AddMenu(new MsgLogMenu(), false); }));
            Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_SETTINGS_TITLE"), () => { MenuManager.Instance.AddMenu(new SettingsMenu(), false); }));
            Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_KEYBOARD_TITLE"), () => { MenuManager.Instance.AddMenu(new KeyControlsMenu(), false); }));
            Choices.Add(new MenuTextChoice(Text.FormatKey("MENU_GAMEPAD_TITLE"), () => { MenuManager.Instance.AddMenu(new GamepadControlsMenu(), false); }));
 
        }

        public void InitMenu()
        {
            Initialize(new Loc(16, 16), CalculateChoiceLength(Choices, 72), Text.FormatKey("MENU_OTHERS_TITLE"), Choices.ToArray(), 0);
        }

    }
}
