using System.Collections.Generic;
using RogueElements;
using System;
using RogueEssence.Content;
using Microsoft.Xna.Framework;

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
            bool enabled = (chooseSlotAction != null);
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_CHOOSE"), RememberAction, enabled, enabled ? Color.White : Color.Red));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TEAM_SUMMARY"), SummaryAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            int choice_width = CalculateChoiceLength(choices, 72);
            Initialize(new Loc(Math.Min(176, GraphicsManager.ScreenWidth - choice_width), 16), choice_width, choices.ToArray(), 0);
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
