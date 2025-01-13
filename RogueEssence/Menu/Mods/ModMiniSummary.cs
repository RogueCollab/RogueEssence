using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class ModMiniSummary : SummaryMenu
    {
        MenuText Name;
        MenuText Version;
        MenuDivider MenuDiv;
        MenuText Author;
        DialogueText Description;

        public ModMiniSummary(Rect bounds) : this(MenuLabel.MOD_MINI_SUMMARY, bounds) { }
        public ModMiniSummary(string label, Rect bounds)
            : base(bounds)
        {
            Label = label;
            Name = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(Name);

            Version = new MenuText("", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth - 2, GraphicsManager.MenuBG.TileHeight), DirH.Right);
            Elements.Add(Version);

            MenuDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT),
                Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(MenuDiv);

            Author = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET));
            Elements.Add(Author);


            Description = new DialogueText("", new Rect(new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET + LINE_HEIGHT),
                new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 4, Bounds.Height - GraphicsManager.MenuBG.TileHeight * 4)), LINE_HEIGHT);
            Elements.Add(Description);
        }

        public void SetMod(ModHeader header)
        {
            Name.SetText(header.Name);
            Version.SetText(header.Version.ToString());
            Author.SetText(Text.FormatKey("MENU_MODS_AUTHOR", header.Author));
            Description.SetAndFormatText(header.Description);
        }
    }
}
