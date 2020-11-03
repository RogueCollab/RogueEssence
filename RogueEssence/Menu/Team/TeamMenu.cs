using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Ground;
using System;

namespace RogueEssence.Menu
{
    public class TeamMenu : TitledStripMenu
    {
        private static int defaultChoice;

        TeamMiniSummary summaryMenu;

        bool sendHome;

        public TeamMenu(bool sendHome) : this(sendHome, -1)
        { }
        public TeamMenu(bool sendHome, int teamSlot)
        {
            int menuWidth = 160;
            this.sendHome = sendHome;
            bool checkSkin = DataManager.Instance.Save is RogueProgress && sendHome;
            if (checkSkin && DataManager.Instance.Save.ActiveTeam.Players.Count > ExplorerTeam.MAX_TEAM_SLOTS)
            {
                checkSkin = false;
                foreach (Character character in DataManager.Instance.Save.ActiveTeam.Players)
                {
                    if (!DataManager.Instance.GetSkin(character.BaseForm.Skin).Challenge || character.Dead)
                        checkSkin = true;
                }
            }
            List<MenuChoice> team = new List<MenuChoice>();
            foreach (Character character in DataManager.Instance.Save.ActiveTeam.Players)
            {
                int teamIndex = team.Count;
                CharIndex turnChar = ZoneManager.Instance.CurrentMap.CurrentTurnMap.GetCurrentTurnChar();
                bool disabled = (sendHome && turnChar.Char == team.Count);//disable the current turn choice in send home mode
                if (checkSkin)
                    disabled |= DataManager.Instance.GetSkin(character.BaseForm.Skin).Challenge && !character.Dead;

                MenuText memberName = new MenuText(character.BaseName, new Loc(2, 1), disabled ? Color.Red : Color.White);
                MenuText memberLv = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT", character.Level), new Loc(menuWidth - 8 * 4, 1), DirV.Up, DirH.Right, disabled ? Color.Red : Color.White);
                team.Add(new MenuElementChoice(() => { choose(teamIndex); }, !disabled, memberName, memberLv));
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
            if (!sendHome)
                MenuManager.Instance.ClearToCheckpoint();
        }

        protected override void Canceled()
        {
            if (!sendHome)
                MenuManager.Instance.RemoveMenu();
        }

        private void choose(int choice)
        {
            if (!sendHome)
                MenuManager.Instance.AddMenu(new TeamChosenMenu(choice), true);
            else
            {
                Character player = DataManager.Instance.Save.ActiveTeam.Players[choice];
                MenuManager.Instance.AddMenu(MenuManager.Instance.CreateQuestion(Text.FormatKey("DLG_SEND_HOME_ASK", player.Name), () =>
                {
                    MenuManager.Instance.ClearMenus();
                    //send home
                    MenuManager.Instance.EndAction = (GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ChooseSendHome(choice) : GroundScene.Instance.ChooseSendHome(choice);
                }, () => { }), false);
            }
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
