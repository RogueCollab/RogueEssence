using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class AssemblyChosenMenu : SingleStripMenu
    {
        private int teamSlot;
        private bool assembly;

        private AssemblyMenu baseMenu;

        public AssemblyChosenMenu(int teamSlot, bool assembly, AssemblyMenu baseMenu)
        {
            this.teamSlot = teamSlot;
            this.assembly = assembly;
            this.baseMenu = baseMenu;

            List<MenuTextChoice> choices = new List<MenuTextChoice>();

            if (assembly)
            {
                bool enabled = baseMenu.CanChooseAssembly(teamSlot);
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ASSEMBLY_JOIN"), JoinAction, enabled, enabled ? Color.White : Color.Red));
            }
            else if (!baseMenu.ChoosingLeader(teamSlot))
            {
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MAKE_LEADER"), MakeLeaderAction));
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ASSEMBLY_STANDBY"), SendHomeAction));
            }
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TEAM_SUMMARY"), SummaryAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ASSEMBLY_RENAME"), RenameAction));
            if (DataManager.Instance.Save.ActiveTeam.Assembly[teamSlot].IsFavorite)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ASSEMBLY_UNFAVORITE"), baseMenu.ToggleFave));
            else
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ASSEMBLY_FAVORITE"), baseMenu.ToggleFave));

            if (assembly)
            {
                bool enabled = !DataManager.Instance.Save.ActiveTeam.Assembly[teamSlot].IsFounder && !DataManager.Instance.Save.ActiveTeam.Assembly[teamSlot].IsFavorite;
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ASSEMBLY_RELEASE"), ReleaseAction, enabled, enabled ? Color.White : Color.Red));
            }

            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            Initialize(new Loc(168, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);
        }

        private void SummaryAction()
        {
            MenuManager.Instance.AddMenu(new MemberFeaturesMenu(teamSlot, assembly, DataManager.Instance.Save.ActiveTeam.Assembly.Count > 0), false);
        }

        private void JoinAction()
        {
            MenuManager.Instance.RemoveMenu();
            baseMenu.ChooseAssembly(teamSlot);
        }

        private void SendHomeAction()
        {
            MenuManager.Instance.RemoveMenu();
            baseMenu.ChooseTeam(teamSlot);
        }

        private void MakeLeaderAction()
        {
            MenuManager.Instance.RemoveMenu();
            baseMenu.ChooseLeader(teamSlot);
        }

        private void RenameAction()
        {
            MenuManager.Instance.AddMenu(new NicknameMenu(baseMenu.ConfirmRename), false);
        }

        private void ReleaseAction()
        {
            Character player = assembly ? DataManager.Instance.Save.ActiveTeam.Assembly[teamSlot] : DataManager.Instance.Save.ActiveTeam.Players[teamSlot];
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateQuestion(MonsterID.Invalid,
                null, new EmoteStyle(0), Text.FormatKey("DLG_ASSEMBLY_RELEASE_ASK", player.BaseName), true, () =>
            {
                MenuManager.Instance.RemoveMenu();
                baseMenu.ReleaseAssembly(teamSlot);
                MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_ASSEMBLY_RELEASE", player.BaseName)), false);
            }, () => { }, true), false);
        }

        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }
    }
}
