using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class MemberFeaturesMenu : InteractableMenu
    {
        Team team;
        int teamSlot;
        bool assembly;
        bool allowAssembly;
        bool guest;

        public MenuText Title;
        public MenuText PageText;
        public MenuDivider Div;

        public SpeakerPortrait Portrait;
        public MenuText Name;

        public MenuText LevelLabel;
        public MenuText Level;

        public MenuText HPLabel;
        public MenuText HP;

        public MenuText FullnessLabel;
        public MenuText Fullness;

        public MenuText CharElements;

        public MenuDivider MainDiv;
        public MenuText SkillTitle;
        public MenuText[] Skills;

        public MenuDivider IntrinsicDiv;
        public MenuText Intrinsic;
        public DialogueText IntrinsicDesc;
        public MemberFeaturesMenu(Team team, int teamSlot, bool assembly, bool allowAssembly, bool guest) : this(MenuLabel.SUMMARY_MENU_FEATS, team, teamSlot, assembly, allowAssembly, guest) { }
        public MemberFeaturesMenu(string label, Team team, int teamSlot, bool assembly, bool allowAssembly, bool guest)
        {
            Label = label;

            Bounds = Rect.FromPoints(new Loc(24, 16), new Loc(296, 224));

            this.team = team;
            this.teamSlot = teamSlot;
            this.assembly = assembly;
            this.allowAssembly = allowAssembly;
            this.guest = guest;

            Character player = null;
            if (assembly)
                player = ((ExplorerTeam)team).Assembly[teamSlot];
            else
            {
                if (guest)
                    player = team.Guests[teamSlot];
                else
                    player = team.Players[teamSlot];
            }

            MonsterData dexEntry = DataManager.Instance.GetMonster(player.BaseForm.Species);
            BaseMonsterForm formEntry = dexEntry.Forms[player.BaseForm.Form];
            
            int totalLearnsetPages = (int) Math.Ceiling((double) MemberLearnsetMenu.getEligibleSkills(formEntry) / MemberLearnsetMenu.SLOTS_PER_PAGE);
            int totalOtherMemberPages = 3;
            int totalPages = totalLearnsetPages + totalOtherMemberPages;

            Title = new MenuText(Text.FormatKey("MENU_TEAM_FEATURES"), new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            PageText = new MenuText($"(1/{totalPages})", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight), DirH.Right);
            Div = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            Portrait = new SpeakerPortrait(player.BaseForm, new EmoteStyle(0),
                new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET), false);
            string speciesText = player.GetDisplayName(true) + " / " + CharData.GetFullFormName(player.BaseForm);
            Name = new MenuText(speciesText, new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET));

            ElementData element1 = DataManager.Instance.GetElement(player.Element1);
            ElementData element2 = DataManager.Instance.GetElement(player.Element2);

            string typeString = element1.GetIconName();
            if (player.Element2 != DataManager.Instance.DefaultElement)
                typeString += "/" + element2.GetIconName();
            bool origElements = (player.Element1 == DataManager.Instance.GetMonster(player.BaseForm.Species).Forms[player.BaseForm.Form].Element1);
            origElements &= (player.Element2 == DataManager.Instance.GetMonster(player.BaseForm.Species).Forms[player.BaseForm.Form].Element2);
            CharElements = new MenuText(Text.FormatKey("MENU_TEAM_ELEMENT", typeString), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 1 + TitledStripMenu.TITLE_OFFSET), origElements ? Color.White : Color.Yellow);

            LevelLabel = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT"), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + TitledStripMenu.TITLE_OFFSET));
            Level = new MenuText(player.Level.ToString(), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48 + GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_LEVEL_SHORT")), GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + TitledStripMenu.TITLE_OFFSET), DirH.Left);

            HPLabel = new MenuText(Text.FormatKey("MENU_TEAM_HP"), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + TitledStripMenu.TITLE_OFFSET));
            HP = new MenuText(String.Format("{0}/{1}", player.HP, player.MaxHP), new Loc( GraphicsManager.MenuBG.TileWidth * 2 + GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_HP")) + 4, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + TitledStripMenu.TITLE_OFFSET), DirH.Left);
            
            FullnessLabel = new MenuText(Text.FormatKey("MENU_TEAM_HUNGER"), new Loc((Bounds.End.X - Bounds.X) / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + TitledStripMenu.TITLE_OFFSET));
            Fullness = new MenuText(String.Format("{0}/{1}", player.Fullness, player.MaxFullness), new Loc((Bounds.End.X - Bounds.X) / 2 + GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_HUNGER")) + 4, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + TitledStripMenu.TITLE_OFFSET), DirH.Left);

            MainDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            SkillTitle = new MenuText(Text.FormatKey("MENU_TEAM_SKILLS"), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4 + TitledStripMenu.TITLE_OFFSET));
            Skills = new MenuText[CharData.MAX_SKILL_SLOTS * 3];
            for (int ii = 0; ii < CharData.MAX_SKILL_SLOTS; ii++)
            {
                SlotSkill skill = player.BaseSkills[ii];
                string skillString = "-----";
                string skillCharges = "--";
                string totalCharges = "/--";
                if (!String.IsNullOrEmpty(skill.SkillNum))
                {
                    EntryDataIndex idx = DataManager.Instance.DataIndices[DataManager.DataType.Skill];
                    SkillDataSummary summary = (SkillDataSummary)idx.Get(skill.SkillNum);
                    skillString = summary.GetIconName();
                    skillCharges = skill.Charges.ToString();
                    totalCharges = "/" + (summary.BaseCharges + player.ChargeBoost);
                }
                Skills[ii * 3] = new MenuText(skillString, new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * (ii + 5) + TitledStripMenu.TITLE_OFFSET));
                Skills[ii * 3 + 1] = new MenuText(skillCharges, new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 16 - GraphicsManager.TextFont.CharSpace, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * (ii + 5) + TitledStripMenu.TITLE_OFFSET), DirH.Right);
                Skills[ii * 3 + 2] = new MenuText(totalCharges, new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 16, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * (ii + 5) + TitledStripMenu.TITLE_OFFSET), DirH.Left);
            }

            IntrinsicDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 10), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            
            bool origIntrinsic = (player.Intrinsics[0].Element.ID == player.BaseIntrinsics[0]);
            IntrinsicData entry = DataManager.Instance.GetIntrinsic(player.Intrinsics[0].Element.ID);
            Intrinsic = new MenuText(Text.FormatKey("MENU_TEAM_INTRINSIC", entry.GetColoredName()), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 9 + TitledStripMenu.TITLE_OFFSET), origIntrinsic ? Color.White : Color.Yellow);
            IntrinsicDesc = new DialogueText(entry.Desc.ToLocal(), new Rect(new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 10 + TitledStripMenu.TITLE_OFFSET),
                new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 3, Bounds.Height - GraphicsManager.MenuBG.TileHeight * 3)), LINE_HEIGHT);
        }

        protected override IEnumerable<IMenuElement> GetDrawElements()
        {
            yield return Title;
            yield return PageText;
            yield return Div;

            yield return Portrait;
            yield return Name;

            yield return LevelLabel;
            yield return Level;
            
            yield return CharElements;

            yield return HPLabel;
            yield return HP;

            yield return FullnessLabel;
            yield return Fullness;

            yield return MainDiv;

            yield return SkillTitle;
            foreach (MenuText skill in Skills)
                yield return skill;

            yield return IntrinsicDiv;
            yield return Intrinsic;
            yield return IntrinsicDesc;
        }

        public override void Update(InputManager input)
        {
            Visible = true;
            if (input.JustPressed(FrameInput.InputType.Menu))
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.ClearMenus();
            }
            else if (input.JustPressed(FrameInput.InputType.Cancel))
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.RemoveMenu();
            }
            else if (IsInputting(input, Dir8.Left))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(team, teamSlot, assembly, allowAssembly, guest, true));
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(team, teamSlot, assembly, allowAssembly, guest));
            }
            else if (IsInputting(input, Dir8.Up))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = (!assembly) ? ((ExplorerTeam)team).Assembly.Count : team.Players.Count;
                    if (teamSlot - 1 < 0)
                        MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(team, amtLimit - 1, !assembly, true, false));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(team, teamSlot - 1, assembly, true, false));
                }
                else if (guest)
                {
                    MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(team, (teamSlot + team.Guests.Count - 1) % team.Guests.Count, false, false, true));
                }
                else
                    MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(team, (teamSlot + team.Players.Count - 1) % team.Players.Count, false, false, false));
            }
            else if (IsInputting(input, Dir8.Down))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = assembly ? ((ExplorerTeam)team).Assembly.Count : team.Players.Count;
                    if (teamSlot + 1 >= amtLimit)
                        MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(team, 0, !assembly, true, false));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(team, teamSlot + 1, assembly, true, false));
                }
                else if (guest)
                {
                    MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(team, (teamSlot + 1) % team.Guests.Count, false, false, true));
                }
                else
                    MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(team, (teamSlot + 1) % team.Players.Count, false, false, false));
            }
        }
    }
}
