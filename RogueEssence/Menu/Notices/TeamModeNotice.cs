using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class TeamModeNotice : MenuBase
    {

        private MenuText menuText;

        private FrameTick timeSinceUpdate;

        public TeamModeNotice()
        {
            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth / 2 - 48, 16), new Loc(GraphicsManager.ScreenWidth / 2 + 48, 16 + LINE_HEIGHT + GraphicsManager.MenuBG.TileHeight * 2));
            menuText = new MenuText("", new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);
            timeSinceUpdate = new FrameTick();
            Visible = false;
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return menuText;
        }

        public void SetTeamMode(bool teamMode)
        {
            menuText.SetText((teamMode ? Text.FormatKey("MENU_TEAM_MODE_ON") : Text.FormatKey("MENU_TEAM_MODE_OFF")));
            int length = menuText.GetTextLength() + GraphicsManager.MenuBG.TileWidth * 4;
            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth / 2 - length / 2, 16), new Loc(GraphicsManager.ScreenWidth / 2 + length / 2, 16 + LINE_HEIGHT + GraphicsManager.MenuBG.TileHeight * 2));
            menuText.Loc = new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight);
            timeSinceUpdate = new FrameTick();
            Visible = true;
        }

        public void Update(FrameTick elapsedTime)
        {
            if (timeSinceUpdate >= 90)
                Visible = false;
            else
                timeSinceUpdate += elapsedTime;
        }
    }
}
