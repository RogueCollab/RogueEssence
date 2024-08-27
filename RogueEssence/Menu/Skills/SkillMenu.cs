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
        public SkillMenu(int teamIndex, int skillSlot) : this(MenuLabel.SKILLS_MENU, teamIndex, skillSlot) { }
        public SkillMenu(string label, int teamIndex) : this(label, teamIndex, -1) { }
        public SkillMenu(string label, int teamIndex, int skillSlot)
        { 
            Label = label;

            List<Character> openPlayers = new List<Character>();
            foreach (Character character in DataManager.Instance.Save.ActiveTeam.Players)
                openPlayers.Add(character);

            string menuTitleText = Text.FormatKey("MENU_SKILLS_TITLE", DataManager.Instance.Save.ActiveTeam.Players[CurrentPage].GetDisplayName(true));

            int menuWidthMin = 168;
            int menuWidthMax = GraphicsManager.ScreenWidth - 32;
            int chkWidth = 8;
            int chargesWidth = 8 * 4;
            int skillIndentWidth = 8 * 4;
            int menuWidth = getMenuWidth(menuWidthMin, menuWidthMax, chkWidth, chargesWidth, skillIndentWidth, menuTitleText, openPlayers);

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
                        MenuText menuText = new MenuText(skillString, new Loc(chkWidth, 1), disabled ? Color.Red : Color.White);
                        MenuText menuCharges = new MenuText(skillCharges, new Loc(menuWidth - chargesWidth, 1), DirV.Up, DirH.Right, disabled ? Color.Red : Color.White);
                        if (jj < Character.MAX_SKILL_SLOTS-1)
                        {
                            MenuDivider div = new MenuDivider(new Loc(0, LINE_HEIGHT), menuWidth - skillIndentWidth);
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

            Initialize(new Loc(16, 16), menuWidth, menuTitleText, skills, skillSlot, teamIndex, CharData.MAX_SKILL_SLOTS);

        }


        protected override void UpdateKeys(InputManager input)
        {
            if (input.JustPressed(FrameInput.InputType.SkillMenu))
                MenuManager.Instance.ClearMenus();
            else if (input.JustPressed(FrameInput.InputType.SortItems))
            {
                GameManager.Instance.SE("Menu/Toggle");
                MenuManager.Instance.NextAction = SkillMenu.MoveCommand(new GameAction(GameAction.ActionType.SetSkill, Dir8.None, CurrentPage, CurrentChoice), Label, CurrentPage, CurrentChoice);
            }
            else if (input[FrameInput.InputType.SelectItems] && input.Direction == Dir8.Up && input.PrevDirection != Dir8.Up)
            {
                if (CurrentChoice > 0)
                {
                    GameManager.Instance.SE("Menu/Toggle");
                    MenuManager.Instance.NextAction = SkillMenu.MoveCommand(new GameAction(GameAction.ActionType.ShiftSkill, Dir8.None, CurrentPage, CurrentChoice - 1), Label, CurrentPage, CurrentChoice - 1);
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
            else if (input[FrameInput.InputType.SelectItems] && input.Direction == Dir8.Down && input.PrevDirection != Dir8.Down)
            {
                if (CurrentChoice < Choices.Count - 1)
                {
                    GameManager.Instance.SE("Menu/Toggle");
                    MenuManager.Instance.NextAction = SkillMenu.MoveCommand(new GameAction(GameAction.ActionType.ShiftSkill, Dir8.None, CurrentPage, CurrentChoice), Label, CurrentPage, CurrentChoice + 1);
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
                MenuManager.Instance.AddMenu(new SkillChosenMenu(Label, CurrentPage, choice), true);
        }

        private int getMenuWidth(int min, int max, int chkWidth, int chargesWidth, int skillIndent, string title, List<Character> openPlayers)
        {
            int menuWidth = min;
            int extraSpace = 32; // seperation between skill/titel and charges/teamIndex

            // check for all skills if they exceed min
            foreach (Character character in openPlayers)
            {
                foreach (BackReference<Skill> skillRef in character.Skills)
                {
                    Skill skill = skillRef.Element;
                    if (!String.IsNullOrEmpty(skill.SkillNum))
                    {
                        string skillName = DataManager.Instance.GetSkill(skill.SkillNum).GetColoredName();
                        int skillTextLength = new MenuText(skillName, new Loc(0, 0)).GetTextLength();
                        int skillRowLength = skillIndent + chkWidth + skillTextLength + extraSpace + chargesWidth;
                        menuWidth = Math.Max(skillRowLength, menuWidth);
                    }
                }
            }

            // check if title exceeds min
            int titleTextLength = new MenuText(title, new Loc(0, 0)).GetTextLength();
            int teamIndexLength = 8 * 4;
            int titleLength = titleTextLength + teamIndexLength + extraSpace;
            return Math.Min(max, Math.Max(titleLength, menuWidth));
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


        public static IEnumerator<YieldInstruction> MoveCommand(GameAction action, string label, int teamSlot, int switchSlot)
        {
            yield return CoroutineManager.Instance.StartCoroutine((GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(action) : GroundScene.Instance.ProcessInput(action));
            MenuManager.Instance.ReplaceMenu(new SkillMenu(label, teamSlot, switchSlot));
        }

    }
}
