using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public abstract class TitledStripMenu : SingleStripMenu
    {
        public const int TITLE_OFFSET = 16;
        public override int ContentOffset { get { return TITLE_OFFSET; } }

        public MenuText Title;


        protected virtual void Initialize(Loc start, int width, string title, IChoosable[] choices, int defaultChoice)
        {
            Initialize(start, width, title, choices, defaultChoice, choices.Length);
        }

        protected virtual void Initialize(Loc start, int width, string title, IChoosable[] choices, int defaultChoice, int totalSpaces)
        {
            Initialize(start, width, title, choices, defaultChoice, choices.Length, -1);
        }

        protected virtual void Initialize(Loc start, int width, string title, IChoosable[] choices, int defaultChoice, int totalSpaces, int multiSelect)
        {
            base.Initialize(start, width, choices, defaultChoice, totalSpaces, multiSelect);
            IncludeTitle(title);
        }

        protected void IncludeTitle(string title)
        {
            Title = new MenuText(title, Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            NonChoices.Add(Title);
            NonChoices.Add(new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2));
        }
    }
}
