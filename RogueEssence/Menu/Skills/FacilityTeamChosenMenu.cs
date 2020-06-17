using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Menu
{
    public class FacilityTeamChosenMenu : SingleStripMenu
    {

        private int teamSlot;
        OnChooseSlot chooseSlotAction;

        public FacilityTeamChosenMenu(int teamSlot, OnChooseSlot action)
        {
            this.teamSlot = teamSlot;
            this.chooseSlotAction = action;

            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_CHOOSE"), RememberAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TEAM_SUMMARY"), SummaryAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            Initialize(new Loc(176, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);
        }

        private void SummaryAction()
        {
            MenuManager.Instance.AddMenu(new MemberFeaturesMenu(teamSlot, false, false), false);
        }

        private void RememberAction()
        {
            MenuManager.Instance.ClearMenus();
            chooseSlotAction(teamSlot);
        }

        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }
    }
}
