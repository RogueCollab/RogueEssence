using System.Collections.Generic;
using RogueElements;
using System;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class MailChosenMenu : SingleStripMenu
    {

        private bool canRescue;
        private MailMenu.OnChoosePath action;
        private Action deleteAction;

        public MailChosenMenu(bool canRescue, string fileName, MailMenu.OnChoosePath action, Action deleteAction)
        {
            this.canRescue = canRescue;
            this.action = action;
            this.deleteAction = deleteAction;

            List<MenuTextChoice> choices = new List<MenuTextChoice>();

            if (this.canRescue)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_RESCUE"), () => { ActivityAction(fileName); }));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_DELETE"), deleteAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            int choice_width = CalculateChoiceLength(choices, 72);
            Initialize(new Loc(Math.Min(204, GraphicsManager.ScreenWidth - choice_width), 8), choice_width, choices.ToArray(), 0);
        }


        private void ActivityAction(string fileName)
        {
            MenuManager.Instance.RemoveMenu();
            MenuManager.Instance.RemoveMenu();

            action(fileName);
        }


        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }
    }
}
