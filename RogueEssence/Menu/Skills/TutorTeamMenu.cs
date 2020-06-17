using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueElements;
using RogueEssence.Data;
using System;

namespace RogueEssence.Menu
{
    public class TutorTeamMenu : TitledStripMenu
    {
        private static int defaultChoice;
        OnChooseSlot chooseSlotAction;
        Action refuseAction;

        TeamMiniSummary summaryMenu;

        public TutorTeamMenu(int teamSlot, OnChooseSlot action, Action refuseAction)
        {
            this.chooseSlotAction = action;
            this.refuseAction = refuseAction;
            int menuWidth = 160;
            List<MenuChoice> team = new List<MenuChoice>();
            foreach (Character character in DataManager.Instance.Save.ActiveTeam.Players)
            {
                int teamIndex = team.Count;

                MenuText memberName = new MenuText(character.BaseName, new Loc(2, 1), Color.White);
                MenuText memberLv = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT", character.Level), new Loc(menuWidth - 8 * 4, 1), DirV.Up, DirH.Right, Color.White);
                team.Add(new MenuElementChoice(() => { choose(teamIndex); }, true, memberName, memberLv));
            }

            summaryMenu = new TeamMiniSummary(Rect.FromPoints(new Loc(16,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - VERT_SPACE * 5),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            if (teamSlot == -1)
                teamSlot = Math.Min(Math.Max(0, defaultChoice), team.Count-1);

            Initialize(new Loc(16, 16), menuWidth, Text.FormatKey("MENU_TEAM_TITLE"), team.ToArray(), teamSlot);
        }


        protected override void UpdateKeys(InputManager input)
        {
            if (input.JustPressed(FrameInput.InputType.TeamMenu))
                MenuManager.Instance.ClearMenus();
            else
                base.UpdateKeys(input);
        }

        protected override void MenuPressed()
        {
            MenuManager.Instance.ClearToCheckpoint();
            refuseAction();
        }

        protected override void Canceled()
        {
            MenuManager.Instance.RemoveMenu();
            refuseAction();
        }

        private void choose(int choice)
        {
            MenuManager.Instance.AddMenu(new FacilityTeamChosenMenu(choice, chooseSlotAction), true);
        }


        protected override void ChoiceChanged()
        {
            defaultChoice = CurrentChoice;
            summaryMenu.SetMember(DataManager.Instance.Save.ActiveTeam.Players[CurrentChoice]);

            base.ChoiceChanged();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            //draw other windows
            summaryMenu.Draw(spriteBatch);
        }
    }
}
