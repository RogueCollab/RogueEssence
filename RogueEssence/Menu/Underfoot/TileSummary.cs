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
            Description = new DialogueText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight),
                Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 4 - Bounds.X, LINE_SPACE);
            Elements.Add(Description);
        }

        public void SetTile(int index)
        {
            Data.TileData entry = Data.DataManager.Instance.GetTile(index);
            Description.SetText(entry.Desc.ToLocal());
        }
    }
}
