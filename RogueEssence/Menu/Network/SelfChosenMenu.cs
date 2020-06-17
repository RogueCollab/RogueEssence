using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using SDL2;


namespace RogueEssence.Menu
{
    public class SelfChosenMenu : SingleStripMenu
    {
        private string copyString;
        private ContactsMenu.OnChooseActivity action;
        private bool rescueMode;

        public SelfChosenMenu(string copyString, bool rescueMode, ContactsMenu.OnChooseActivity action)
        {
            this.copyString = copyString;
            this.rescueMode = rescueMode;
            this.action = action;

            List<MenuTextChoice> choices = new List<MenuTextChoice>();

            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_COPY_UUID"), CopyAction));
            if (!rescueMode)
            {
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TEAM_RENAME"), () => { MenuManager.Instance.AddMenu(new TeamNameMenu(Text.FormatKey("INPUT_TEAM_TITLE"), "", RenameAction), false); }));
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_UPDATE_TEAM"), UpdateAction));
            }
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            Initialize(new Loc(204, 8), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);
        }


        private void CopyAction()
        {
            SDL.SDL_SetClipboardText(DataManager.Instance.Save.UUID);
            GameManager.Instance.SE("Menu/Sort");
        }

        public void RenameAction(string name)
        {
            DataManager.Instance.Save.ActiveTeam.Name = name;
            foreach (Character chara in DataManager.Instance.Save.ActiveTeam.Players)
                chara.OriginalTeam = DataManager.Instance.Save.ActiveTeam.Name;
            foreach (Character chara in DataManager.Instance.Save.ActiveTeam.Assembly)
                chara.OriginalTeam = DataManager.Instance.Save.ActiveTeam.Name;

            MenuManager.Instance.RemoveMenu();

            MenuManager.Instance.ReplaceMenu(new ContactsMenu(rescueMode, action));
        }

        public void UpdateAction()
        {
            DataManager.Instance.Save.UpdateTeamProfile(false);

            MenuManager.Instance.RemoveMenu();

            MenuManager.Instance.ReplaceMenu(new ContactsMenu(rescueMode, action));
        }

        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }
    }
}
