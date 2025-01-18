using System.Collections.Generic;
using RogueElements;
using System;
using RogueEssence.Content;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class MailChosenMenu : SingleStripMenu
    {

        private MailMenu.OnChoosePath action;
        private Action deleteAction;

        public MailChosenMenu(bool canRescue, bool offVersion, string fileName, MailMenu.OnChoosePath action, Action deleteAction) :
            this(MenuLabel.MAIL_CHOSEN_MENU, canRescue, offVersion, fileName, action, deleteAction) { }
        public MailChosenMenu(string label, bool canRescue, bool offVersion, string fileName, MailMenu.OnChoosePath action, Action deleteAction)
        {
            Label = label;
            this.action = action;
            this.deleteAction = deleteAction;

            List<MenuTextChoice> choices = new List<MenuTextChoice>();

            if (canRescue)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_RESCUE"), () => { ActivityAction(fileName); }));
            if (offVersion)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_VERSION_INFO"), () => { ViewVersionDiff(fileName); }));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_DELETE"), deleteAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            int choice_width = CalculateChoiceLength(choices, 72);
            Initialize(new Loc(Math.Min(204, GraphicsManager.ScreenWidth - choice_width), 8), choice_width, choices.ToArray(), 0);
        }

        private void ViewVersionDiff(string fileName)
        {
            SOSMail mail = DataManager.LoadRescueMail(fileName) as SOSMail;
            List<ModVersion> curVersions = PathMod.GetModVersion();
            List<ModDiff> versionDiff = PathMod.DiffModVersions(mail.DefeatedVersion, curVersions);
            MenuManager.Instance.AddMenu(new VersionDiffMenu(versionDiff, 0), false);
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
