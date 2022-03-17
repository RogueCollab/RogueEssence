using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class InfoMenu : InteractableMenu
    {
        public MenuText Title;
        public MenuDivider Div;

        public DialogueText Info;

        public InfoMenu(string title, string message)
        {
            Bounds = new Rect(new Loc(40, 32), new Loc(240, 176));

            Title = new MenuText(title, new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            Div = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            
            Info = new DialogueText(message, new Rect(new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET),
                new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 3, Bounds.Height - GraphicsManager.MenuBG.TileHeight * 3)), LINE_HEIGHT);
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Title;
            yield return Div;

            yield return Info;
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
        }
    }
}
