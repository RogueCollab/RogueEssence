using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System;

namespace RogueEssence.Menu
{
    public class InvResultsMenu : SideScrollMenu
    {
        public const int MAX_LINES = 12;

        public MenuText Title;
        public MenuText Money;
        public MenuDivider Div;
        public MenuText[] Items;

        public GameProgress Ending;
        public int Page;

        public InvResultsMenu(GameProgress ending, int page) : this(MenuLabel.RESULTS_MENU_INV, ending, page) { }
        public InvResultsMenu(string label, GameProgress ending, int page)
        {
            Label = label;
            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth / 2 - 140, 16), new Loc(GraphicsManager.ScreenWidth / 2 + 140, 224));
            Ending = ending;
            Page = page;

            List<string> flatChoices = new List<string>();
            for (int ii = 0; ii < ending.ActiveTeam.Players.Count; ii++)
            {
                Character activeChar = ending.ActiveTeam.Players[ii];
                if (!String.IsNullOrEmpty(activeChar.EquippedItem.ID))
                    flatChoices.Add(activeChar.EquippedItem.GetDisplayName());
            }
            for (int ii = 0; ii < ending.ActiveTeam.GetInvCount(); ii++)
            {
                int index = ii;
                flatChoices.Add(ending.ActiveTeam.GetInv(index).GetDisplayName());
            }

            Title = new MenuText(Text.FormatKey("MENU_RESULTS_INVENTORY_TITLE", Page + 1, MathUtils.DivUp((Ending.ActiveTeam.MaxInv - 1), MAX_LINES * 2)), new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);

            Div = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            Items = new MenuText[MAX_LINES * 2];
            for (int ii = 0; ii < MAX_LINES; ii++)
            {
                for (int jj = 0; jj < 2; jj++)
                {
                    int displayIndex = ii * 2 + jj;
                    int itemIndex = displayIndex + Page * MAX_LINES * 2;
                    if (itemIndex < Ending.ActiveTeam.MaxInv)
                    {
                        string itemName = (flatChoices.Count > itemIndex) ? flatChoices[itemIndex] : "---";
                        Items[displayIndex] = new MenuText(itemName, new Loc(GraphicsManager.MenuBG.TileWidth * 2 + (Bounds.Width - GraphicsManager.MenuBG.TileWidth * 4) / 2 * jj, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * ii + TitledStripMenu.TITLE_OFFSET));
                    }
                    else
                        Items[displayIndex] = new MenuText("", Loc.Zero);
                }
            }

            Money = new MenuText(Text.FormatKey("MENU_BAG_MONEY", Text.FormatKey("MONEY_AMOUNT", Ending.ActiveTeam.Money)), new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 12 + TitledStripMenu.TITLE_OFFSET), DirH.None);
            
            base.Initialize();
        }

        protected override IEnumerable<IMenuElement> GetDrawElements()
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
                if (Page > 0)
                    MenuManager.Instance.ReplaceMenu(new InvResultsMenu(Ending, Page - 1));
                else
                    MenuManager.Instance.ReplaceMenu(new FinalResultsMenu(Ending));
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (Page < (Ending.ActiveTeam.MaxInv - 1) / (MAX_LINES * 2))
                    MenuManager.Instance.ReplaceMenu(new InvResultsMenu(Ending, Page + 1));
                else
                    MenuManager.Instance.ReplaceMenu(new PartyResultsMenu(Ending));
            }

        }
    }
}
