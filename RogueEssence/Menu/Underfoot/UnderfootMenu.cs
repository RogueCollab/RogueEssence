using RogueElements;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using System;

namespace RogueEssence.Menu
{
    public abstract class UnderfootMenu : SingleStripMenu
    {
        SummaryMenu summaryMenu;

        protected void Initialize(Loc start, int width, MenuTextChoice[] choices, int defaultChoice, string name, string price)
        {
            Loc summaryStart = new Loc(16, 16);

            summaryMenu = new SummaryMenu(new Rect(summaryStart, new Loc(176, VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2 + TitledStripMenu.TITLE_OFFSET)));


            summaryMenu.Elements.Add(new MenuText(Text.FormatKey("MENU_GROUND_TITLE"), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight)));
            summaryMenu.Elements.Add(new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), summaryMenu.Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2));
            summaryMenu.Elements.Add(new MenuText(name, new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET)));
            if (!String.IsNullOrEmpty(price))
                summaryMenu.Elements.Add(new MenuText(price, new Loc(summaryMenu.Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET), DirH.Right));

            Initialize(start, width, choices, defaultChoice);
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
