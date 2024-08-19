using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using System;

namespace RogueEssence.Menu
{
    public class ModDiffSummary : SummaryMenu
    {
        const int MAX_DIFF = 10;

        MenuText Title;
        MenuDivider MenuDiv;
        public MenuText[] Diffs;

        public ModDiffSummary(string title) : base(new Rect(new Loc(16, 16), new Loc(16, 16)))
        {
            Title = new MenuText(title, new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(Title);
            MenuDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT),
                Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(MenuDiv);
        }

        public void SetDiff(List<ModDiff> diff)
        {
            Diffs = new MenuText[diff.Count];
            int rows = Math.Min(Diffs.Length, MAX_DIFF);
            for (int ii = 0; ii < rows; ii++)
            {
                int yy = ii % MAX_DIFF;
                Diffs[ii] = new MenuText(diff[ii].Name, new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * ii));
                Elements.Add(Diffs[ii]);
            }

            Bounds = new Rect(new Loc(16, 16), new Loc(CalculateChoiceLength(Diffs, Title.GetTextLength() + 16 + GraphicsManager.MenuBG.TileWidth * 2), rows * VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2 + TitledStripMenu.TITLE_OFFSET));
            MenuDiv.Length = Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2;
        }

        protected int CalculateChoiceLength(IEnumerable<MenuText> choices, int minWidth)
        {
            int maxWidth = minWidth;
            foreach (MenuText choice in choices)
                maxWidth = Math.Max(choice.GetTextLength() + 16 + GraphicsManager.MenuBG.TileWidth * 2, maxWidth);
            maxWidth = MathUtils.DivUp(maxWidth, 4) * 4;
            return maxWidth;
        }
    }
}
