using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueEssence.Ground;

namespace RogueEssence.Menu
{
    public class SkillChosenMenu : SingleStripMenu
    {

        private int teamIndex;
        private int skillSlot;


        public SkillChosenMenu(int teamIndex, int skillSlot)
        {
            this.teamIndex = teamIndex;
            this.skillSlot = skillSlot;

            bool shiftUp = (skillSlot > 0);
            bool shiftDown = (skillSlot < DataManager.Instance.Save.ActiveTeam.Players[teamIndex].Skills.Count - 1) && (DataManager.Instance.Save.ActiveTeam.Players[teamIndex].Skills[skillSlot + 1].Element.SkillNum > -1);

            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                CharIndex turnChar = ZoneManager.Instance.CurrentMap.CurrentTurnMap.GetCurrentTurnChar();
                if (turnChar.Faction == Faction.Player && turnChar.Char == teamIndex)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_SKILL_USE"), useAction));
            }
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_SKILL_SWITCH"), switchAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_SHIFT_UP"), () => { shiftPosition(false); }, shiftUp, shiftUp ? Color.White : Color.Red));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_SHIFT_DOWN"), () => { shiftPosition(true); }, shiftDown, shiftDown ? Color.White : Color.Red));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), MenuManager.Instance.RemoveMenu));

            Initialize(new Loc(168, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);
        }
        
        private void useAction()
        {
            MenuManager.Instance.ClearMenus();
            MenuManager.Instance.EndAction = DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.UseSkill, Dir8.None, skillSlot, -1));
        }

        private void switchAction()
        {
            MenuManager.Instance.RemoveMenu();

            MenuManager.Instance.NextAction = MoveCommand(new GameAction(GameAction.ActionType.SetSkill, Dir8.None, teamIndex, skillSlot), skillSlot);
        }

        private void shiftPosition(bool switchDown)
        {
            MenuManager.Instance.RemoveMenu();

            int swapSlot = skillSlot - 1;
            int newSlot = skillSlot - 1;
            if (switchDown)
            {
                swapSlot++;
                newSlot += 2;
            }
            MenuManager.Instance.NextAction = MoveCommand(new GameAction(GameAction.ActionType.ShiftSkill, Dir8.None, teamIndex, swapSlot), newSlot);
        }

        public IEnumerator<YieldInstruction> MoveCommand(GameAction action, int switchSlot)
        {
            yield return CoroutineManager.Instance.StartCoroutine((GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(action) : GroundScene.Instance.ProcessInput(action));
            MenuManager.Instance.ReplaceMenu(new SkillMenu(teamIndex, switchSlot));
        }

    }
}
