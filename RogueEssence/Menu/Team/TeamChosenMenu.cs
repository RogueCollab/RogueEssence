using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueEssence.Ground;

namespace RogueEssence.Menu
{
    public class TeamChosenMenu : SingleStripMenu
    {

        private int teamSlot;

        public TeamChosenMenu(int teamSlot)
        {
            this.teamSlot = teamSlot;


            List<MenuTextChoice> choices = new List<MenuTextChoice>();

            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TEAM_SUMMARY"), SummaryAction));

            bool hasStatus = false;
            bool inDungeon = GameManager.Instance.CurrentScene == DungeonScene.Instance;
            bool inGround = GameManager.Instance.CurrentScene == GroundScene.Instance;
            if (inDungeon)
            {
                foreach (int status in ZoneManager.Instance.CurrentMap.Status.Keys)
                {
                    if (!ZoneManager.Instance.CurrentMap.Status[status].Hidden)
                    {
                        hasStatus = true;
                        break;
                    }
                }
                foreach (int status in DungeonScene.Instance.ActiveTeam.Players[teamSlot].StatusEffects.Keys)
                {
                    if (Data.DataManager.Instance.GetStatus(status).MenuName)
                    {
                        hasStatus = true;
                        break;
                    }
                }
            }

            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TEAM_STATUS_TITLE"), StatusAction, hasStatus, hasStatus ? Color.White : Color.Red));

            bool canAct = !inDungeon || (DataManager.Instance.CurrentReplay == null) && (DungeonScene.Instance.CurrentCharacter == DungeonScene.Instance.ActiveTeam.Leader);

            bool canShiftUp = canAct;
            bool canShiftDown = canAct;
            if (DataManager.Instance.Save.ActiveTeam.Players[teamSlot].IsPartner)
            {
                canShiftUp = false;
                canShiftDown = false;
            }
            if (teamSlot > 0)
            {
                if (DataManager.Instance.Save.ActiveTeam.Players[teamSlot - 1].IsPartner)
                    canShiftUp = false;
            }
            else
                canShiftUp = false;
            if (teamSlot < DataManager.Instance.Save.ActiveTeam.Players.Count - 1)
            {
                if (DataManager.Instance.Save.ActiveTeam.Players[teamSlot + 1].IsPartner)
                    canShiftDown = false;
            }
            else
                canShiftDown = false;

            if (canShiftUp)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_SHIFT_UP"), ShiftUpAction));
            if (canShiftDown)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_SHIFT_DOWN"), ShiftDownAction));

            if (teamSlot == DataManager.Instance.Save.ActiveTeam.LeaderIndex)
            {
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TEAM_MODE"), TeamModeAction, canAct, canAct ? Color.White : Color.Red));
            }
            else
            {
                bool canSwitch = canAct;
                if (inDungeon && ZoneManager.Instance.CurrentMap.NoSwitching)
                    canSwitch = false;
                if (inGround && ZoneManager.Instance.CurrentGround.NoSwitching)
                    canSwitch = false;
                if (DataManager.Instance.Save.NoSwitching)
                    canSwitch = false;
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_MAKE_LEADER"), MakeLeaderAction, canSwitch, canSwitch ? Color.White : Color.Red));
            }

            bool canSendHome = canAct;
            if (DataManager.Instance.Save.ActiveTeam.Players[teamSlot].IsPartner)
                canSendHome = false;
            if (DataManager.Instance.Save is RogueProgress && DataManager.Instance.GetSkin(DataManager.Instance.Save.ActiveTeam.Players[teamSlot].BaseForm.Skin).Challenge && !DataManager.Instance.Save.ActiveTeam.Players[teamSlot].Dead)
                canSendHome = false;
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance && teamSlot != DataManager.Instance.Save.ActiveTeam.LeaderIndex)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_TEAM_SEND_HOME"), SendHomeAction, canSendHome, canSendHome ? Color.White : Color.Red));


            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            Initialize(new Loc(176, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);
        }

        private void SummaryAction()
        {
            MenuManager.Instance.AddMenu(new MemberFeaturesMenu(teamSlot, false, false), false);
        }

        private void StatusAction()
        {
            MenuManager.Instance.AddMenu(new StatusMenu(teamSlot), false);
        }

        private void ShiftUpAction()
        {
            MenuManager.Instance.RemoveMenu();

            MenuManager.Instance.NextAction = MoveCommand(new GameAction(GameAction.ActionType.ShiftTeam, Dir8.None, teamSlot - 1), teamSlot - 1);
        }

        private void ShiftDownAction()
        {
            MenuManager.Instance.RemoveMenu();

            MenuManager.Instance.NextAction = MoveCommand(new GameAction(GameAction.ActionType.ShiftTeam, Dir8.None, teamSlot), teamSlot + 1);
        }

        private void TeamModeAction()
        {
            MenuManager.Instance.ClearMenus();

            MenuManager.Instance.EndAction = DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.TeamMode, Dir8.None));
        }

        private void MakeLeaderAction()
        {
            MenuManager.Instance.ClearMenus();

            MenuManager.Instance.EndAction = (GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.SetLeader, Dir8.None, teamSlot)) : GroundScene.Instance.ProcessInput(new GameAction(GameAction.ActionType.SetLeader, Dir8.None, teamSlot, 0));
        }

        private void SendHomeAction()
        {
            Character player = DataManager.Instance.Save.ActiveTeam.Players[teamSlot];
            MenuManager.Instance.AddMenu(MenuManager.Instance.CreateQuestion(Text.FormatKey("DLG_SEND_HOME_ASK", player.GetDisplayName(true)), () =>
            {
                MenuManager.Instance.RemoveMenu();
                List<IInteractable> save = MenuManager.Instance.SaveMenuState();

                MenuManager.Instance.ClearMenus();
                //send home
                MenuManager.Instance.NextAction = SendHomeEndAction(teamSlot, save);

            }, () => { }), false);
        }


        public IEnumerator<YieldInstruction> SendHomeEndAction(int teamSlot, List<IInteractable> save)
        {
            yield return CoroutineManager.Instance.StartCoroutine((GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.SendHome, Dir8.None, teamSlot)) : GroundScene.Instance.ProcessInput(new GameAction(GameAction.ActionType.SendHome, Dir8.None, teamSlot)));

            save[save.Count - 1] = new TeamMenu(false);
            save[save.Count - 1].BlockPrevious = true;

            MenuManager.Instance.LoadMenuState(save);
        }

        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }



        public IEnumerator<YieldInstruction> MoveCommand(GameAction action, int switchSlot)
        {
            yield return CoroutineManager.Instance.StartCoroutine((GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(action) : GroundScene.Instance.ProcessInput(action));
            MenuManager.Instance.ReplaceMenu(new TeamMenu(false, switchSlot));
        }
    }
}
