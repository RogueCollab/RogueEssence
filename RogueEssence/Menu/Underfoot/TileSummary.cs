using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class TileSummary : SummaryMenu
    {

        DialogueText Description;

        public TileSummary(Rect bounds) : this(MenuLabel.TILE_SUMMARY, bounds) { }
        public TileSummary(string label, Rect bounds)
            : base(bounds)
        {
            Label = label;
            Description = new DialogueText("", new Rect(new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight),
                new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 4, Bounds.Height - GraphicsManager.MenuBG.TileHeight * 4)), LINE_HEIGHT);
            Elements.Add(Description);
        }

        public void SetTile(string index)
        {
            Data.TileData entry = Data.DataManager.Instance.GetTile(index);
            Description.SetAndFormatText(entry.Desc.ToLocal());
        }
    }
}
