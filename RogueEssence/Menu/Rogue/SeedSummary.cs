using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class SeedSummary : SummaryMenu
    {
        MenuText Details;
        MenuDivider MenuDiv;

        public SeedSummary(Rect bounds) : this(MenuLabel.SEED_SUMMARY, bounds) { }
        public SeedSummary(string label, Rect bounds) : base(bounds)
        {
            Label = label;
            Details = new MenuText(Text.FormatKey("MENU_SEED_CUSTOMIZE", DiagManager.Instance.GetControlString(FrameInput.InputType.SortItems)), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(Details);
            MenuDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT),
                Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(MenuDiv);

        }

        public void SetDetails(ulong? seed)
        {
            while(Elements.Count > 2)
                Elements.RemoveAt(2);

            List<MenuText> rules = new List<MenuText>();

            if (seed.HasValue)
                rules.Add(new MenuText(seed.Value.ToString("X"), Loc.Zero));
            else
                rules.Add(new MenuText(Text.FormatKey("MENU_NONE"), Loc.Zero));

            for (int ii = 0; ii < rules.Count; ii++)
            {
                rules[ii].Loc = new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * ii);
                Elements.Add(rules[ii]);
            }

            Bounds.Size = new Loc(Bounds.Size.X, GraphicsManager.MenuBG.TileHeight * 2 + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * rules.Count);
        }

    }
}
