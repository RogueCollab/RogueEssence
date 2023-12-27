﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class AddToTeamMenu : MultiPageMenu
    {
        public delegate void OnChooseTeam(List<int> slot);
        private const int SLOTS_PER_PAGE = 6;

        SpeakerPortrait portrait;
        TeamMiniSummary summaryMenu;
        OnChooseTeam teamChoice;
        Action refuseAction;

        private List<int> eligibleAssembly;

        public AddToTeamMenu(OnChooseTeam teamChoice, Action refuseAction)
        {
            int menuWidth = 152;
            this.teamChoice = teamChoice;
            this.refuseAction = refuseAction;

            this.eligibleAssembly = new List<int>();
            for (int ii = 0; ii < DungeonScene.Instance.ActiveTeam.Assembly.Count; ii++)
            {
                //if (!DungeonScene.Instance.ActiveTeam.Assembly[ii].Absentee)
                    eligibleAssembly.Add(ii);
            }

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < eligibleAssembly.Count; ii++)
            {
                int index = eligibleAssembly[ii];
                Character character = DungeonScene.Instance.ActiveTeam.Assembly[index];
                MenuText memberName = new MenuText(character.GetDisplayName(true), new Loc(2, 1), character.Dead ? Color.Red : Color.White);
                MenuText memberLvLabel = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT"), new Loc(menuWidth - 8 * 7 + 6, 1),
                    DirV.Up, DirH.Right, character.Dead ? Color.Red : Color.White);
                MenuText memberLv = new MenuText(character.Level.ToString(), new Loc(menuWidth - 8 * 7 + 6 + GraphicsManager.TextFont.SubstringWidth(DataManager.Instance.Start.MaxLevel.ToString()), 1),
                    DirV.Up, DirH.Right, character.Dead ? Color.Red : Color.White);
                flatChoices.Add(new MenuElementChoice(() => { choose(index); }, !character.Dead, memberName, memberLvLabel, memberLv));
            }
            IChoosable[][] box = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            summaryMenu = new TeamMiniSummary(Rect.FromPoints(new Loc(16,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - VERT_SPACE * 5),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            portrait = new SpeakerPortrait(MonsterID.Invalid, new EmoteStyle(0), new Loc(GraphicsManager.ScreenWidth - 32 - 40, 16), true);

            Initialize(new Loc(16, 16), menuWidth, Text.FormatKey("MENU_ASSEMBLY_TITLE"), box, 0, 0, SLOTS_PER_PAGE);

        }

        private void choose(int choice)
        {
            MenuManager.Instance.RemoveMenu();
            List<int> choices = new List<int>();
            choices.Add(choice);
            teamChoice(choices);
        }

        protected override void ChoiceChanged()
        {
            Character subjectChar = DungeonScene.Instance.ActiveTeam.Assembly[this.eligibleAssembly[CurrentChoiceTotal]];
            summaryMenu.SetMember(subjectChar);

            portrait.Speaker = subjectChar.BaseForm;
                
            base.ChoiceChanged();
        }

        protected override void Canceled()
        {
            base.Canceled();
            refuseAction();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            portrait.Draw(spriteBatch, new Loc());

            //draw other windows
            summaryMenu.Draw(spriteBatch);
        }

    }
}
