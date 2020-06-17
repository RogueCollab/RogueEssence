using System.Collections.Generic;
using RogueElements;
using System;

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

            Initialize(new Loc(204, 8), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);
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
