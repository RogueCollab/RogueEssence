using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class MoneySummary : SummaryMenu
    {

        MenuText Title;
        MenuText Money;

        public MoneySummary(Rect bounds) : this(MenuLabel.MONEY_SUMMARY, bounds) { }
        public MoneySummary(string label, Rect bounds)
            : base(bounds)
        {
            Label = label;
            Title = new MenuText(Text.FormatKey("MENU_STORAGE_MONEY") + ":", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(Title);
            Money = new MenuText(Text.FormatKey("MONEY_AMOUNT", DataManager.Instance.Save.ActiveTeam.Money), new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), DirH.None);
            Elements.Add(Money);
        }
    }
}
