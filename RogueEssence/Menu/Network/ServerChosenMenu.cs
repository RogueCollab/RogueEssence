using System.Collections.Generic;
using RogueElements;
using System;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class ServerChosenMenu : SingleStripMenu
    {

        private int chosenIndex;


        public ServerChosenMenu(int index)
        {
            this.chosenIndex = index;

            List<MenuTextChoice> choices = new List<MenuTextChoice>();

            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_CHOOSE"), ChooseAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_DELETE"), DeleteAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            int choice_width = CalculateChoiceLength(choices, 72);
            Initialize(new Loc(Math.Min(232, GraphicsManager.ScreenWidth - choice_width), 8), choice_width, choices.ToArray(), 0);
        }


        private void ChooseAction()
        {

            ServerInfo info = DiagManager.Instance.CurSettings.ServerList[chosenIndex];
            DiagManager.Instance.CurSettings.ServerList.RemoveAt(chosenIndex);
            DiagManager.Instance.CurSettings.ServerList.Insert(0, info);
            DiagManager.Instance.SaveSettings(DiagManager.Instance.CurSettings);

            MenuManager.Instance.RemoveMenu();

            MenuManager.Instance.ReplaceMenu(new ServersMenu());
        }

        private void DeleteAction()
        {
            DiagManager.Instance.CurSettings.ServerList.RemoveAt(chosenIndex);
            DiagManager.Instance.SaveSettings(DiagManager.Instance.CurSettings);

            MenuManager.Instance.RemoveMenu();

            MenuManager.Instance.ReplaceMenu(new ServersMenu());
        }

        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }
    }
}
