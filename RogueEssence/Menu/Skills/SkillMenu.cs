using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System;
using RogueEssence.Ground;

namespace RogueEssence.Menu
{
    public class SkillMenu : MultiPageMenu
    {
        private static int defaultChoice;

        SkillSummary summaryMenu;

        public SkillMenu(int teamIndex) : this(teamIndex, -1) { }
        public SkillMenu(int teamIndex, int skillSlot)
        {
            int menuWidth = 168;

            List<Character> openPlayers = new List<Character>();
            foreach (Character character in DataManager.Instance.Save.ActiveTeam.Players)
                openPlayers.Add(character);

            MenuChoice[][] skills = new MenuChoice[openPlayers.Count][];
            for (int ii = 0; ii < openPlayers.Count; ii++)
            {
                List<MenuChoice> char_skills = new List<MenuChoice>();
                for (int jj = 0; jj < DataManager.Instance.Save.ActiveTeam.Players[ii].Skills.Count; jj++)
                {
                    Skill skill = DataManager.Instance.Save.ActiveTeam.Players[ii].Skills[jj].Element;
                    if (!String.IsNullOrEmpty(skill.SkillNum))
                    {
                        SkillData data = DataManager.Instance.GetSkill(skill.SkillNum);
                        string chkString = (skill.Enabled ? "\uE10A " : "");
                        string skillString = DiagManager.Instance.GetControlString((FrameInput.InputType)(jj + (int)FrameInput.InputType.Skill1)) + ": " + data.GetColoredName();
                        string skillCharges = skill.Charges + "/" + (data.BaseCharges + DataManager.Instance.Save.ActiveTeam.Players[ii].ChargeBoost);
                        bool disabled = (skill.Sealed || skill.Charges <= 0);
                        int index = jj;
                        MenuText chkText = new MenuText(chkString, new Loc(0, 1), disabled ? Color.Red : Color.White);
                        MenuText menuText = new MenuText(skillString, new Loc(8, 1), disabled ? Color.Red : Color.White);
                        MenuText menuCharges = new MenuText(skillCharges, new Loc(menuWidth - 8 * 4, 1), DirV.Up, DirH.Right, disabled ? Color.Red : Color.White);
                        if (jj < Character.MAX_SKILL_SLOTS-1)
                        {
                            MenuDivider div = new MenuDivider(new Loc(0, LINE_HEIGHT), menuWidth - 8 * 4);
                            char_skills.Add(new MenuElementChoice(() => { choose(index); }, true, chkText, menuText, menuCharges, div));
                        }
                        else
                            char_skills.Add(new MenuElementChoice(() => { choose(index); }, true, chkText, menuText, menuCharges));
                    }
                }
                skills[ii] = char_skills.ToArray();
            }

            if (skillSlot == -1)
                skillSlot = Math.Min(Math.Max(0, defaultChoice), skills[teamIndex].Length - 1);

            summaryMenu = new SkillSummary(Rect.FromPoints(new Loc(16,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - LINE_HEIGHT * 2 - VERT_SPACE * 4),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            Initialize(new Loc(16, 16), menuWidth, Text.FormatKey("MENU_SKILLS_TITLE", DataManager.Instance.Save.ActiveTeam.Players[CurrentPage].GetDisplayName(true)), skills, skillSlot, teamIndex, CharData.MAX_SKILL_SLOTS);

        }


        protected override void UpdateKeys(InputManager input)
        {
            if (input.JustPressed(FrameInput.InputType.SkillMenu))
                MenuManager.Instance.ClearMenus();
            else if (input.JustPressed(FrameInput.InputType.SortItems))
            {
                GameManager.Instance.SE("Menu/Toggle");
                MenuManager.Instance.NextAction = SkillMenu.MoveCommand(new GameAction(GameAction.ActionType.SetSkill, Dir8.None, CurrentPage, CurrentChoice), CurrentPage, CurrentChoice);
            }
            else if (input[FrameInput.InputType.SelectItems] && input.Direction == Dir8.Up && input.PrevDirection != Dir8.Up)
            {
                if (CurrentChoice > 0)
                {
                    GameManager.Instance.SE("Menu/Toggle");
                    MenuManager.Instance.NextAction = SkillMenu.MoveCommand(new GameAction(GameAction.ActionType.ShiftSkill, Dir8.None, CurrentPage, CurrentChoice - 1), CurrentPage, CurrentChoice - 1);
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
            else if (input[FrameInput.InputType.SelectItems] && input.Direction == Dir8.Down && input.PrevDirection != Dir8.Down)
            {
                if (CurrentChoice < Choices.Count - 1)
                {
                    GameManager.Instance.SE("Menu/Toggle");
                    MenuManager.Instance.NextAction = SkillMenu.MoveCommand(new GameAction(GameAction.ActionType.ShiftSkill, Dir8.None, CurrentPage, CurrentChoice), CurrentPage, CurrentChoice + 1);
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
            else
                base.UpdateKeys(input);
        }

        private void choose(int choice)
        {
            if (DataManager.Instance.CurrentReplay == null)
                MenuManager.Instance.AddMenu(new SkillChosenMenu(CurrentPage, choice), true);
        }

        protected override void ChoiceChanged()
        {
            defaultChoice = CurrentChoice;
            Title.SetText(Text.FormatKey("MENU_SKILLS_TITLE", DataManager.Instance.Save.ActiveTeam.Players[CurrentPage].GetDisplayName(true)));
            summaryMenu.SetSkill(DataManager.Instance.Save.ActiveTeam.Players[CurrentPage].Skills[CurrentChoice].Element.SkillNum);

            base.ChoiceChanged();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            summaryMenu.Draw(spriteBatch);
        }


        public static IEnumerator<YieldInstruction> MoveCommand(GameAction action, int teamSlot, int switchSlot)
        {
            yield return CoroutineManager.Instance.StartCoroutine((GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(action) : GroundScene.Instance.ProcessInput(action));
            MenuManager.Instance.ReplaceMenu(new SkillMenu(teamSlot, switchSlot));
        }

    }
}
