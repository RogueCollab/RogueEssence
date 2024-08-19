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
            Title = new MenuText(MenuLabel.TITLE, title, new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            NonChoices.Add(Title);
            NonChoices.Add(new MenuDivider(MenuLabel.DIV, new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2));
        }

        public override void ImportChoices(params IChoosable[] choices)
        {
            base.ImportChoices(choices);
            int index = GetNonChoiceIndexByLabel(MenuLabel.DIV);
            if (index >= 0 && NonChoices[index] is MenuDivider divider)
                divider.Length = Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2;
        }
    }
}
