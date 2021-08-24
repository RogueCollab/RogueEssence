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

            Title = new MenuText(title, Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            Div = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_SPACE), Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);
            
            Info = new DialogueText(message, Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET),
                Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 3 - Bounds.X, LINE_SPACE);
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
