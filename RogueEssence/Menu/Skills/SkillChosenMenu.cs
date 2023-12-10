using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System;
using RogueEssence.Content;

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
            bool shiftDown = (skillSlot < DataManager.Instance.Save.ActiveTeam.Players[teamIndex].Skills.Count - 1) && !String.IsNullOrEmpty(DataManager.Instance.Save.ActiveTeam.Players[teamIndex].Skills[skillSlot + 1].Element.SkillNum);

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

            int choice_width = CalculateChoiceLength(choices, 72);
            Initialize(new Loc(Math.Min(184, GraphicsManager.ScreenWidth - choice_width), 16), choice_width, choices.ToArray(), 0);
        }
        
        private void useAction()
        {
            MenuManager.Instance.ClearMenus();
            MenuManager.Instance.EndAction = DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.UseSkill, Dir8.None, skillSlot, -1));
        }

        private void switchAction()
        {
            MenuManager.Instance.RemoveMenu();

            MenuManager.Instance.NextAction = SkillMenu.MoveCommand(new GameAction(GameAction.ActionType.SetSkill, Dir8.None, teamIndex, skillSlot), teamIndex, skillSlot);
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
            MenuManager.Instance.NextAction = SkillMenu.MoveCommand(new GameAction(GameAction.ActionType.ShiftSkill, Dir8.None, teamIndex, swapSlot), teamIndex, newSlot);
        }
    }
}
