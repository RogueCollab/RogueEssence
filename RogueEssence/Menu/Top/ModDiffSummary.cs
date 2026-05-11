using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using System;

namespace RogueEssence.Menu
{
    public class ModDiffSummary : SummaryMenu
    {
        const int MAX_DIFF = 9;

        MenuText Title;
        MenuDivider MenuDiv;
        public List<MenuText> Diffs;

        public ModDiffSummary(string title) : this(MenuLabel.MOD_DIFF_SUMMARY, title) { }
        public ModDiffSummary(string label, string title) : base(new Rect(new Loc(16, 16), new Loc(16, 16)))
        {
            Label = label;
            Title = new MenuText(title, new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(Title);
            MenuDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT),
                Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(MenuDiv);
        }

        public void SetDiff(List<ModDiff> diff)
        {
            int rows = Math.Min(diff.Count, MAX_DIFF);
            Diffs = new List<MenuText>();
            for (int ii = 0; ii < rows; ii++)
            {
                MenuText diffMenuText = new MenuText(diff[ii].Name, new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * ii));
                Diffs.Add(diffMenuText);
                Elements.Add(diffMenuText);
            }
            int overflow = diff.Count - MAX_DIFF;
            if (overflow > 0)
            {
                MenuText diffMenuText = new MenuText(Text.FormatKey("MENU_AND_MORE", overflow), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * Diffs.Count));
                Diffs.Add(diffMenuText);
                Elements.Add(diffMenuText);
            }


            Bounds = new Rect(new Loc(16, 16), new Loc(CalculateChoiceLength(Diffs, Title.GetTextLength() + 16 + GraphicsManager.MenuBG.TileWidth * 2), Diffs.Count * VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2 + TitledStripMenu.TITLE_OFFSET));
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
