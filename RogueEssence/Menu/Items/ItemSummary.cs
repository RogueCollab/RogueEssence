using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class ItemSummary : SummaryMenu
    {

        DialogueText Description;
        MenuText Rarity;
        MenuText SalePrice;

        public ItemSummary(Rect bounds)
            : base(bounds)
        {
            Description = new DialogueText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight),
                Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 4 - Bounds.X, LINE_SPACE);
            Elements.Add(Description);
            SalePrice = new MenuText("", new Loc(Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 2, Bounds.Y + GraphicsManager.MenuBG.TileHeight + 4 * LINE_SPACE), DirH.Right);
            Elements.Add(SalePrice);
            Rarity = new MenuText("", new Loc(Bounds.Start.X + GraphicsManager.MenuBG.TileWidth * 2, Bounds.Y + GraphicsManager.MenuBG.TileHeight + 4 * LINE_SPACE), DirH.Left);
            Elements.Add(Rarity);
        }

        public void SetItem(InvItem item)
        {
            Data.ItemData entry = Data.DataManager.Instance.GetItem(item.ID);
            Description.SetText(entry.Desc.ToLocal());
            SalePrice.SetText(Text.FormatKey("MENU_ITEM_VALUE", Text.FormatKey("MONEY_AMOUNT", item.GetSellValue())));
            if (entry.Rarity > 0)
            {
                string rarityStr = "";
                for (int ii = 0; ii < entry.Rarity; ii++)
                    rarityStr += "\uE10C";
                Rarity.SetText(Text.FormatKey("MENU_ITEM_RARITY", rarityStr));
            }
            else
                Rarity.SetText("");
        }
    }
}
