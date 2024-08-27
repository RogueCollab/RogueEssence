using System;
using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class InfoMenu : InteractableMenu
    {
        public MenuText Title;
        public MenuDivider Div;

        public DialogueText Info;

        private Action action;

        public InfoMenu(string title, string message, Action action) : this(MenuLabel.INFO_MENU, title, message, action) { }
        public InfoMenu(string label, string title, string message, Action action)
        {
            Label = label;
            this.action = action;

            Bounds = new Rect(new Loc(40, 32), new Loc(240, 176));

            Title = new MenuText(MenuLabel.TITLE, title, new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(Title);

            Div = new MenuDivider(MenuLabel.DIV, new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(Div);

            Info = new DialogueText(MenuLabel.MESSAGE, message, new Rect(new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET),
                new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 3, Bounds.Height - GraphicsManager.MenuBG.TileHeight * 3)), LINE_HEIGHT);
            Elements.Add(Info);
        }

        public override void Update(InputManager input)
        {
            Visible = true;
            if (input.JustPressed(FrameInput.InputType.Menu))
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.ClearMenus();
            }
            else if (input.JustPressed(FrameInput.InputType.Cancel))
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.RemoveMenu();
            }
            else if (input.JustPressed(FrameInput.InputType.Confirm))
            {
                GameManager.Instance.SE("Menu/Confirm");
                MenuManager.Instance.RemoveMenu();
                action();
            }
        }
    }
}
