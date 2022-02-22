using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class InvResultsMenu : SideScrollMenu
    {
        public MenuText Title;
        public MenuText Money;
        public MenuDivider Div;
        public MenuText[] Items;

        public GameProgress Ending;

        public InvResultsMenu(GameProgress ending)
        {
            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth / 2 - 140, 16), new Loc(GraphicsManager.ScreenWidth / 2 + 140, 224));
            Ending = ending;

            Title = new MenuText(Text.FormatKey("MENU_RESULTS_INVENTORY_TITLE"), new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);

            Div = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            Items = new MenuText[ending.ActiveTeam.MaxInv];
            List<string> flatChoices = new List<string>();
            for (int ii = 0; ii < ending.ActiveTeam.Players.Count; ii++)
            {
                Character activeChar = ending.ActiveTeam.Players[ii];
                if (activeChar.EquippedItem.ID > -1)
                    flatChoices.Add(activeChar.EquippedItem.GetDisplayName());
            }
            for (int ii = 0; ii < ending.ActiveTeam.GetInvCount(); ii++)
            {
                int index = ii;
                flatChoices.Add(ending.ActiveTeam.GetInv(index).GetDisplayName());
            }
            for (int jj = 0; jj < 2; jj++)
            {
                for (int ii = 0; ii < ending.ActiveTeam.MaxInv / 2; ii++)
                {
                    int itemIndex = ii + ending.ActiveTeam.MaxInv / 2 * jj;
                    string itemName = (flatChoices.Count > itemIndex) ? flatChoices[itemIndex] : "---";
                    Items[itemIndex] = new MenuText(itemName, new Loc(GraphicsManager.MenuBG.TileWidth * 2 + (Bounds.Width - GraphicsManager.MenuBG.TileWidth * 4) / 2 * jj, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * ii + TitledStripMenu.TITLE_OFFSET));
                }
            }

            Money = new MenuText(Text.FormatKey("MENU_BAG_MONEY", Text.FormatKey("MONEY_AMOUNT", Ending.ActiveTeam.Money)), new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 12 + TitledStripMenu.TITLE_OFFSET), DirH.None);
            
            base.Initialize();
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Title;
            yield return Money;
            yield return Div;

            foreach (MenuText item in Items)
                yield return item;
        }

        public override void Update(InputManager input)
        {
            if (input.JustPressed(FrameInput.InputType.Menu) || input.JustPressed(FrameInput.InputType.Confirm)
                || input.JustPressed(FrameInput.InputType.Cancel))
            {
                GameManager.Instance.SE("Menu/Confirm");
                MenuManager.Instance.RemoveMenu();
            }
            else if (IsInputting(input, Dir8.Left))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new FinalResultsMenu(Ending));
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new PartyResultsMenu(Ending));
            }

        }
    }
}
