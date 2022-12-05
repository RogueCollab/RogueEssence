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

        int teamSlot;
        bool assembly;
        bool allowAssembly;

        SkillSummary summaryMenu;

        public MemberLearnsetMenu(int teamSlot, bool assembly, bool allowAssembly, bool maxPage)
        {
            this.teamSlot = teamSlot;
            this.assembly = assembly;
            this.allowAssembly = allowAssembly;

            Character player = assembly
                ? DataManager.Instance.Save.ActiveTeam.Assembly[teamSlot]
                : DataManager.Instance.Save.ActiveTeam.Players[teamSlot];
            
            MonsterData dexEntry = DataManager.Instance.GetMonster(player.BaseForm.Species);
            BaseMonsterForm formEntry = dexEntry.Forms[player.BaseForm.Form];
            
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            foreach (LevelUpSkill levelUpSkill in formEntry.LevelSkills)
            {
                string skill = levelUpSkill.Skill;
                SkillData skillEntry = DataManager.Instance.GetSkill(levelUpSkill.Skill);

                MenuText skillText = new MenuText(skillEntry.GetIconName(), new Loc(1, 1), Color.White);
                MenuText levelUpText = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT", levelUpSkill.Level),
                    new Loc(GraphicsManager.ScreenWidth - 72, 1), DirV.Up, DirH.Right, Color.White);

                Skills.Add(skill);
                flatChoices.Add(new MenuElementChoice(() => { }, true, levelUpText, skillText));
            }

            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            summaryMenu = new SkillSummary(Rect.FromPoints(new Loc(16,
                    GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - LINE_HEIGHT * 2 -
                    VERT_SPACE * 4),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            Initialize(new Loc(16, 16), GraphicsManager.ScreenWidth - 32, "", choices, 0, maxPage ? (choices.Length - 1) : 0, SLOTS_PER_PAGE);
        }

        protected override void ChoiceChanged()
        {
            Title.SetText(Text.FormatKey("MENU_TEAM_LEARNSET"));
            summaryMenu.SetSkill(Skills[CurrentChoiceTotal]);
            base.ChoiceChanged();
        }
        
        protected override void UpdateKeys(InputManager input)
        {
            if (CurrentPage - 1 < 0 && IsInputting(input, Dir8.Left))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(teamSlot, assembly, allowAssembly));
            }
            else if (CurrentPage + 1 >= TotalChoices.Length && IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(teamSlot, assembly, allowAssembly));
            }
            else if (CurrentChoice - 1 < 0 && IsInputting(input, Dir8.Up))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = (!assembly) ? DataManager.Instance.Save.ActiveTeam.Assembly.Count : DataManager.Instance.Save.ActiveTeam.Players.Count;
                    if (teamSlot - 1 < 0)
                        MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(amtLimit - 1, !assembly, allowAssembly, false));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(teamSlot - 1, assembly, allowAssembly, false));
                }
                else
                    MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu((teamSlot + DataManager.Instance.Save.ActiveTeam.Players.Count - 1) % DataManager.Instance.Save.ActiveTeam.Players.Count, false, allowAssembly, false));
            }
            else if (CurrentChoice + 1 >= SLOTS_PER_PAGE && IsInputting(input, Dir8.Down))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = assembly ? DataManager.Instance.Save.ActiveTeam.Assembly.Count : DataManager.Instance.Save.ActiveTeam.Players.Count;
                    if (teamSlot + 1 >= amtLimit)
                        MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(0, !assembly, allowAssembly, false));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(teamSlot + 1, assembly, allowAssembly, false));
                }
                else
                    MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu((teamSlot + 1) % DataManager.Instance.Save.ActiveTeam.Players.Count, false, allowAssembly, false));
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