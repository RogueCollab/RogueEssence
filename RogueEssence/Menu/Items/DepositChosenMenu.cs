using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class DepositChosenMenu : SingleStripMenu
    {

        private int origIndex;
        private List<InvSlot> selections;

        public DepositChosenMenu(List<InvSlot> selections, int origIndex)
        {
            this.origIndex = origIndex;
            this.selections = selections;

            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_STORE"), StoreAction));

            if (selections.Count == 1)
            {
                InvItem invItem = null;
                if (selections[0].IsEquipped)
                    invItem = DataManager.Instance.Save.ActiveTeam.Players[selections[0].Slot].EquippedItem;
                else
                    invItem = DataManager.Instance.Save.ActiveTeam.GetInv(selections[0].Slot);
                ItemData entry = DataManager.Instance.GetItem(invItem.ID);

                if (entry.UsageType == ItemData.UseType.Learn)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_INFO"), InfoAction));
            }

            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            int choice_width = CalculateChoiceLength(choices, 72);
            Initialize(new Loc(Math.Min(ItemMenu.ITEM_MENU_WIDTH + 16, GraphicsManager.ScreenWidth - choice_width), 16), choice_width, choices.ToArray(), 0);
        }

        private void StoreAction()
        {
            //called only when held is false
            //store items
            List<InvItem> items = new List<InvItem>();

            MenuManager.Instance.RemoveMenu();

            bool[] removal = new bool[DataManager.Instance.Save.ActiveTeam.GetInvCount()];
            for (int ii = 0; ii < selections.Count; ii++)
            {
                if (selections[ii].IsEquipped)
                {
                    items.Add(DataManager.Instance.Save.ActiveTeam.Players[selections[ii].Slot].EquippedItem);
                    DataManager.Instance.Save.ActiveTeam.Players[selections[ii].Slot].DequipItem();
                }
                else
                {
                    items.Add(DataManager.Instance.Save.ActiveTeam.GetInv(selections[ii].Slot));
                    removal[selections[ii].Slot] = true;
                }
            }
            for (int ii = removal.Length - 1; ii >= 0; ii--)
            {
                if (removal[ii])
                    DataManager.Instance.Save.ActiveTeam.RemoveFromInv(ii);
            }

            DataManager.Instance.Save.ActiveTeam.StoreItems(items);
            //refresh base menu
            bool hasItems = (DataManager.Instance.Save.ActiveTeam.GetInvCount() > 0);
            foreach (Character player in DataManager.Instance.Save.ActiveTeam.Players)
                hasItems |= !String.IsNullOrEmpty(player.EquippedItem.ID);

            if (hasItems)
                MenuManager.Instance.ReplaceMenu(new DepositMenu(origIndex));
            else
                MenuManager.Instance.RemoveMenu();
        }

        private void InfoAction()
        {
            if (selections[0].IsEquipped)
                MenuManager.Instance.AddMenu(new TeachInfoMenu(DataManager.Instance.Save.ActiveTeam.Players[selections[0].Slot].EquippedItem.ID), false);
            else
                MenuManager.Instance.AddMenu(new TeachInfoMenu(DataManager.Instance.Save.ActiveTeam.GetInv(selections[0].Slot).ID), false);
        }

        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }
        
    }
}
