using RogueElements;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public abstract class UnderfootMenu : SingleStripMenu
    {
        SummaryMenu summaryMenu;

        protected void Initialize(Loc start, int width, MenuTextChoice[] choices, int defaultChoice, string name)
        {
            Loc summaryStart = new Loc(16, 16);
            summaryMenu = new SummaryMenu(new Rect(summaryStart, new Loc(144, VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2 + TitledStripMenu.TITLE_OFFSET)));


            summaryMenu.Elements.Add(new MenuText(Text.FormatKey("MENU_GROUND_TITLE"), summaryMenu.Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight)));
            summaryMenu.Elements.Add(new MenuDivider(summaryMenu.Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_SPACE), 144 - GraphicsManager.MenuBG.TileWidth * 2));
            summaryMenu.Elements.Add(new MenuText(name, summaryMenu.Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET)));

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
