using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueEssence.Content;
using RogueEssence.Ground;
using System;

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
                bool canSwitch = true;
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance && ZoneManager.Instance.CurrentMap.NoSwitching)
                    canSwitch = false;
                if (GameManager.Instance.CurrentScene == GroundScene.Instance && ZoneManager.Instance.CurrentGround.NoSwitching)
                    canSwitch = false;
                if (DataManager.Instance.Save.NoSwitching)
                    canSwitch = false;
                if (canSwitch)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MAKE_LEADER"), MakeLeaderAction));
                if (!baseMenu.ChoosingStuckMember(teamSlot))
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ASSEMBLY_STANDBY"), SendHomeAction));
            }
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TEAM_SUMMARY"), SummaryAction));

            if (assembly)
            {
                if (!DataManager.Instance.Save.ActiveTeam.Assembly[teamSlot].NameLocked)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ASSEMBLY_RENAME"), RenameAction));
            }
            else
            {
                if (!DataManager.Instance.Save.ActiveTeam.Players[teamSlot].NameLocked)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ASSEMBLY_RENAME"), RenameAction));
            }

            if (assembly)
            {
                if (DataManager.Instance.Save.ActiveTeam.Assembly[teamSlot].IsFavorite)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_FAVORITE_OFF"), baseMenu.ToggleFave));
                else
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_FAVORITE"), baseMenu.ToggleFave));
            }
            else
            {
                if (DataManager.Instance.Save.ActiveTeam.Players[teamSlot].IsFavorite)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_FAVORITE_OFF"), baseMenu.ToggleFave));
                else
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_FAVORITE"), baseMenu.ToggleFave));
            }

            if (assembly)
            {
                bool enabled = !DataManager.Instance.Save.ActiveTeam.Assembly[teamSlot].IsFounder && !DataManager.Instance.Save.ActiveTeam.Assembly[teamSlot].IsFavorite;
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ASSEMBLY_RELEASE"), ReleaseAction, enabled, enabled ? Color.White : Color.Red));
            }

            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            int choice_width = CalculateChoiceLength(choices, 72);
            Initialize(new Loc(Math.Min(168, GraphicsManager.ScreenWidth - choice_width), 16), choice_width, choices.ToArray(), 0);
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
            MenuManager.Instance.AddMenu(new NicknameMenu(baseMenu.ConfirmRename, () => { }), false);
        }

        private void ReleaseAction()
        {
            Character player = assembly ? DataManager.Instance.Save.ActiveTeam.Assembly[teamSlot] : DataManager.Instance.Save.ActiveTeam.Players[teamSlot];
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateQuestion(MonsterID.Invalid,
                null, new EmoteStyle(0), Text.FormatKey("DLG_ASSEMBLY_RELEASE_ASK", player.GetDisplayName(true)), true, false, false, false, () =>
            {
                MenuManager.Instance.RemoveMenu();
                baseMenu.ReleaseAssembly(teamSlot);
                MenuManager.Instance.AddMenu(MenuManager.Instance.CreateDialogue(Text.FormatKey("DLG_ASSEMBLY_RELEASE", player.GetDisplayName(true))), false);
            }, () => { }, true), false);
        }

        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }
    }
}
