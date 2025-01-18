using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class ItemAmountMenu : ChooseAmountMenu
    {
        MenuText AskTitle;
        private SingleStripMenu.OnChooseSlot chooseAction;

        public ItemAmountMenu(Loc start, int max, SingleStripMenu.OnChooseSlot chooseAction) : this(MenuLabel.ITEM_AMOUNT_MENU, start, max, chooseAction) { }
        public ItemAmountMenu(string label, Loc start, int max, SingleStripMenu.OnChooseSlot chooseAction)
        {
            Label = label;

            this.chooseAction = chooseAction;

            Loc size = new Loc(80, 64);
            
            int length = max.ToString().Length;
            MenuDigits digits = new MenuDigits(1, length, new Loc(size.X / 2 - MenuDigits.DIGIT_SPACE * length / 2, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT * 2));

            Initialize(new Rect(start, size), digits, 1, max, length - 1);
            
            AskTitle = new MenuText(Text.FormatKey("MENU_ITEM_AMOUNT_TITLE"), new Loc(size.X / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);
            NonChoices.Add(AskTitle);
        }
        
        protected override void Confirmed()
        {
            MenuManager.Instance.RemoveMenu();
            chooseAction(Digits.Amount);
        }
        
    }
}
