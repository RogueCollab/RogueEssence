using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class TileSummary : SummaryMenu
    {

        DialogueText Description;

        public TileSummary(Rect bounds)
            : base(bounds)
        {
            Description = new DialogueText("", new Rect(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight),
                new Loc(Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 4 - Bounds.X, Bounds.End.Y - GraphicsManager.MenuBG.TileHeight * 4 - Bounds.Y)), LINE_HEIGHT);
            Elements.Add(Description);
        }

        public void SetTile(int index)
        {
            Data.TileData entry = Data.DataManager.Instance.GetTile(index);
            Description.SetFormattedText(entry.Desc.ToLocal());
        }
    }
}
