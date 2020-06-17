using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class MoneySummary : SummaryMenu
    {

        MenuText Title;
        MenuText Money;

        public MoneySummary(Rect bounds)
            : base(bounds)
        {
            Title = new MenuText(Text.FormatKey("MENU_STORAGE_MONEY") + ":", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight), DirH.Left);
            Elements.Add(Title);
            Money = new MenuText(Text.FormatKey("MONEY_AMOUNT", DataManager.Instance.Save.ActiveTeam.Money), Bounds.Start + new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + LINE_SPACE), DirH.None);
            Elements.Add(Money);
        }
    }
}
