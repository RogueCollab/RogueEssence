using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class WithdrawChosenMenu : SingleStripMenu
    {

        private int origIndex;
        private List<WithdrawSlot> selections;
        WithdrawMenu.OnWithdrawChoice storageChoice;
        bool continueOnChoose;

        public WithdrawChosenMenu(List<WithdrawSlot> selections, int origIndex, bool continueOnChoose, WithdrawMenu.OnWithdrawChoice storageChoice)
        {
            this.origIndex = origIndex;
            this.selections = selections;
            this.continueOnChoose = continueOnChoose;
            this.storageChoice = storageChoice;

            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            if (selections.Count > 1)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_WITHDRAW"), TakeAction));
            else if (!continueOnChoose)//technically, continue on choose is currently representing withdraw for rescue reward... a situation where bag is irrelevant
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_WITHDRAW"), TakeAction));
            else
            {
                WithdrawSlot selectionIndex = selections[0];
                string itemID;
                if (selectionIndex.IsBox)
                    itemID = DataManager.Instance.Save.ActiveTeam.BoxStorage[selectionIndex.BoxSlot].ID;
                else
                    itemID = selectionIndex.ItemID;

                ItemData entry = DataManager.Instance.GetItem(itemID);

                bool fitsInBag = DataManager.Instance.Save.ActiveTeam.GetInvCount() < DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone);
                if (entry.MaxStack > 1 && !fitsInBag)
                {
                    //stackable items can be added to a degree
                    for (int jj = 0; jj < DataManager.Instance.Save.ActiveTeam.GetInvCount(); jj++)
                    {
                        //should we allow refills into held item slots?
                        //ehhh it probably doesn't even matter
                        if (DataManager.Instance.Save.ActiveTeam.GetInv(jj).ID == itemID && DataManager.Instance.Save.ActiveTeam.GetInv(jj).Amount < entry.MaxStack)
                        {
                            fitsInBag = true;
                            break;
                        }
                    }
                }

                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_WITHDRAW"), TakeAction, fitsInBag, fitsInBag ? Color.White : Color.Red));
                if (entry.UsageType == Data.ItemData.UseType.Learn)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_INFO"), InfoAction));
            }

            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            int choice_width = CalculateChoiceLength(choices, 72);
            Initialize(new Loc(Math.Min(ItemMenu.ITEM_MENU_WIDTH + 16, GraphicsManager.ScreenWidth - choice_width), 16), choice_width, choices.ToArray(), 0);
        }

        private void TakeAction()
        {
            if (selections.Count > 1)
                takeItems(selections);
            else if (!continueOnChoose)
            {
                WithdrawSlot selectionIndex = selections[0];
                string itemID;
                if (selectionIndex.IsBox)
                    itemID = DataManager.Instance.Save.ActiveTeam.BoxStorage[selectionIndex.BoxSlot].ID;
                else
                    itemID = selectionIndex.ItemID;

                ItemData entry = DataManager.Instance.GetItem(itemID);

                if (entry.MaxStack > 1)
                    MenuManager.Instance.AddMenu(new ItemAmountMenu(new Loc(Bounds.X, Bounds.End.Y), entry.MaxStack, takeMultiple), true);
                else
                    takeItems(selections);
            }
            else
            {
                WithdrawSlot selectionIndex = selections[0];
                if (!selectionIndex.IsBox)
                {
                    ItemData entry = DataManager.Instance.GetItem(selectionIndex.ItemID);

                    int openSlots = DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone) - DataManager.Instance.Save.ActiveTeam.GetInvCount();
                    //stackable items need to be counted differently
                    if (entry.MaxStack > 1)
                    {
                        int residualSlots = 0;
                        for (int jj = 0; jj < DataManager.Instance.Save.ActiveTeam.GetInvCount(); jj++)
                        {
                            if (DataManager.Instance.Save.ActiveTeam.GetInv(jj).ID == selectionIndex.ItemID && DataManager.Instance.Save.ActiveTeam.GetInv(jj).Amount < entry.MaxStack)
                                residualSlots += entry.MaxStack - DataManager.Instance.Save.ActiveTeam.GetInv(jj).Amount;
                        }
                        openSlots = openSlots * entry.MaxStack + residualSlots;
                    }

                    openSlots = Math.Min(openSlots, DataManager.Instance.Save.ActiveTeam.Storage[selectionIndex.ItemID]);
                    //show the amount dialogue
                    MenuManager.Instance.AddMenu(new ItemAmountMenu(new Loc(Bounds.X, Bounds.End.Y), openSlots, takeMultiple), true);
                }
                else
                    takeItems(selections);
            }
        }

        private void takeMultiple(int amount)
        {
            List<WithdrawSlot> slots = new List<WithdrawSlot>();
            for (int ii = 0; ii < amount; ii++)
                slots.Add(selections[0]);
            takeItems(slots);
        }

        private void takeItems(List<WithdrawSlot> slots)
        {
            MenuManager.Instance.RemoveMenu();

            storageChoice(slots);

            if (continueOnChoose)
            {
                //refresh base menu
                bool hasStorage = (DataManager.Instance.Save.ActiveTeam.BoxStorage.Count > 0);
                foreach (string key in DataManager.Instance.Save.ActiveTeam.Storage.Keys)
                {
                    if (DataManager.Instance.Save.ActiveTeam.Storage.GetValueOrDefault(key, 0) > 0)
                    {
                        hasStorage = true;
                        break;
                    }
                }

                if (hasStorage)
                    MenuManager.Instance.ReplaceMenu(new WithdrawMenu(origIndex, continueOnChoose, storageChoice));
                else
                    MenuManager.Instance.RemoveMenu();
            }
            else
                MenuManager.Instance.RemoveMenu();
        }

        private void InfoAction()
        {
            MenuManager.Instance.AddMenu(new TeachInfoMenu(selections[0].ItemID), false);
        }

        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }
    }
}
