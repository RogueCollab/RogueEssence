using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Menu
{
    public class BuyChosenMenu : SingleStripMenu
    {

        private int origIndex;
        private List<int> selections;
        private OnMultiChoice action;

        public BuyChosenMenu(List<int> selections, int origIndex, int itemID, OnMultiChoice chooseSlots)
        {
            this.origIndex = origIndex;
            this.selections = selections;
            action = chooseSlots;

            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_SHOP_BUY"), BuyAction));

            if (selections.Count == 1)
            {
                Data.ItemData entry = Data.DataManager.Instance.GetItem(itemID);

                if (entry.UsageType == Data.ItemData.UseType.Learn)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_INFO"), () => { MenuManager.Instance.AddMenu(new TeachInfoMenu(itemID), false); }));
            }

            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            Initialize(new Loc(ItemMenu.ITEM_MENU_WIDTH + 16, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);
        }

        private void BuyAction()
        {
            //remove both this and parent menu
            MenuManager.Instance.RemoveMenu();
            MenuManager.Instance.RemoveMenu();

            action(selections);
        }

        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }
        
    }
}
