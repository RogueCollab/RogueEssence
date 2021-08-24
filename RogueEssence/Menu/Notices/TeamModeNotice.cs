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
            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth / 2 - 48, 16), new Loc(GraphicsManager.ScreenWidth / 2 + 48, 16 + LINE_SPACE + GraphicsManager.MenuBG.TileHeight * 2));
            menuText = new MenuText("", new Loc(GraphicsManager.ScreenWidth / 2, Bounds.Y + GraphicsManager.MenuBG.TileHeight), DirH.None);
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
