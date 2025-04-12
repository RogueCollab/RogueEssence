using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using Microsoft.Xna.Framework;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class MemberLearnsetMenu : MultiPageMenu
    {
        public static readonly int SLOTS_PER_PAGE = 6;
        private List<string> Skills = new List<string>();

        Team team;
        int teamSlot;
        bool assembly;
        bool allowAssembly;
        bool guest;

        SkillSummary summaryMenu;

        public MemberLearnsetMenu(Team team, int teamSlot, bool assembly, bool allowAssembly, bool guest, bool maxPage) :
            this(MenuLabel.SUMMARY_MENU_LEARNSET, team, teamSlot, assembly, allowAssembly, guest, maxPage) { }
        public MemberLearnsetMenu(string label, Team team, int teamSlot, bool assembly, bool allowAssembly, bool guest, bool maxPage)
        {
            Label = label;
            this.team = team;
            this.teamSlot = teamSlot;
            this.assembly = assembly;
            this.allowAssembly = allowAssembly;
            this.guest = guest;

            Character player = MemberFeaturesMenu.GetPresentedPlayer(team, teamSlot, assembly, guest);

            MonsterData dexEntry = DataManager.Instance.GetMonster(player.BaseForm.Species);
            BaseMonsterForm formEntry = dexEntry.Forms[player.BaseForm.Form];
            
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            foreach (LevelUpSkill levelUpSkill in formEntry.LevelSkills)
            {
                string skill = levelUpSkill.Skill;

                EntryDataIndex idx = DataManager.Instance.DataIndices[DataManager.DataType.Skill];
                SkillDataSummary summary = (SkillDataSummary)idx.Get(levelUpSkill.Skill);
                if (!summary.Released)
                    continue;

                Skills.Add(skill);
                if (levelUpSkill.Level > 0)
                {
                    MenuText skillText = new MenuText(summary.GetIconName(), new Loc(1, 1), Color.White);
                    MenuText levelLabel = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT"),
                        new Loc(GraphicsManager.ScreenWidth - 88, 1), DirH.Right);
                    MenuText level = new MenuText(levelUpSkill.Level.ToString(), new Loc(GraphicsManager.ScreenWidth - 88 + GraphicsManager.TextFont.SubstringWidth(DataManager.Instance.Start.MaxLevel.ToString()), 0), DirH.Right);

                    flatChoices.Add(new MenuElementChoice(() => { }, true, levelLabel, level, skillText));
                }
                else
                {
                    MenuText skillText = new MenuText(summary.GetIconName(), new Loc(1, 1), Color.White);
                    MenuText level = new MenuText(Text.FormatKey("MENU_TEAM_PROMOTE_SHORT"), new Loc(GraphicsManager.ScreenWidth - 88 + GraphicsManager.TextFont.SubstringWidth(DataManager.Instance.Start.MaxLevel.ToString()), 0), DirH.Right);

                    flatChoices.Add(new MenuElementChoice(() => { }, true, level, skillText));
                }
            }

            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            summaryMenu = new SkillSummary(Rect.FromPoints(new Loc(16,
                    GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - LINE_HEIGHT * 2 -
                    VERT_SPACE * 4),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            Initialize(new Loc(16, 16), GraphicsManager.ScreenWidth - 32, "", choices, 0, maxPage ? (choices.Length - 1) : 0, SLOTS_PER_PAGE);
        }

        public static int getEligibleSkills(BaseMonsterForm formEntry)
        {
            int total = 0;
            foreach (LevelUpSkill levelUpSkill in formEntry.LevelSkills)
            {
                EntrySummary skillEntry = DataManager.Instance.DataIndices[DataManager.DataType.Skill].Get(levelUpSkill.Skill);
                if (!skillEntry.Released)
                    continue;
                total++;
            }
            return total;
        }

        protected override void ChoiceChanged()
        {
            Character player = MemberFeaturesMenu.GetPresentedPlayer(team, teamSlot, assembly, guest);

            Title.SetText(Text.FormatKey("MENU_TEAM_LEARNSET", player.GetDisplayName(true)));
            summaryMenu.SetSkill(Skills[CurrentChoiceTotal]);
            base.ChoiceChanged();
        }

        protected override void UpdateKeys(InputManager input)
        {
            if (CurrentPage - 1 < 0 && IsInputting(input, Dir8.Left))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(team, teamSlot, assembly, allowAssembly, guest));
            }
            else if (CurrentPage + 1 >= TotalChoices.Length && IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(team, teamSlot, assembly, allowAssembly, guest));
            }
            else if (CurrentChoice - 1 < 0 && IsInputting(input, Dir8.Up))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = (!assembly) ? ((ExplorerTeam)team).Assembly.Count : team.Players.Count;
                    if (teamSlot - 1 < 0)
                        MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(team, amtLimit - 1, !assembly, true, false, false));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(team, teamSlot - 1, assembly, true, false, false));
                }
                else if (guest)
                {
                    if (team.Guests.Count != 1)
                        MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(team, (teamSlot + team.Guests.Count - 1) % team.Guests.Count, false, false, true, false));
                    else
                        CurrentChoice = TotalChoices[CurrentPage].Length - 1;
                }
                else
                {
                    if (team.Players.Count != 1)
                        MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(team, (teamSlot + team.Players.Count - 1) % team.Players.Count, false, false, false, false));
                    else
                        CurrentChoice = TotalChoices[CurrentPage].Length - 1;
                }
            }
            else if (CurrentChoice + 1 >= TotalChoices[CurrentPage].Length && IsInputting(input, Dir8.Down))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = assembly ? ((ExplorerTeam)team).Assembly.Count : team.Players.Count;
                    if (teamSlot + 1 >= amtLimit)
                        MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(team, 0, !assembly, true, false, false));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(team, teamSlot + 1, assembly, true, false, false));
                }
                else if (guest)
                {
                    if (team.Guests.Count != 1)
                        MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(team, (teamSlot + 1) % team.Guests.Count, false, false, true, false));
                    else
                        CurrentChoice = 0;
                }
                else
                {
                    if (team.Players.Count != 1)
                        MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(team, (teamSlot + 1) % team.Players.Count, false, false, false, false));
                    else
                        CurrentChoice = 0;
                }
            }
            else
                base.UpdateKeys(input);
        }

        protected override void SetPage(int page)
        {
            int totalOtherMemberPages = 3;
            CurrentPage = page;
            if (TotalChoices.Length == 1 && !ShowPagesOnSingle)
                PageText.SetText("");
            else
                PageText.SetText("(" + (CurrentPage + 1 + totalOtherMemberPages) + "/" + (TotalChoices.Length + totalOtherMemberPages) + ")");
            IChoosable[] choices = new IChoosable[TotalChoices[CurrentPage].Length];
            for (int ii = 0; ii < choices.Length; ii++)
                choices[ii] = TotalChoices[CurrentPage][ii];
            SetChoices(choices);
            CurrentChoice = Math.Min(CurrentChoice, choices.Length - 1);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            summaryMenu.Draw(spriteBatch);
        }
    }
}