using RogueEssence.Ground;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Linq;

using NLua;
using System.Collections.Generic;
using RogueElements;
using System;

namespace RogueEssence.Script
{
    public partial class ScriptGame : ILuaEngineComponent
    {


        //===================================
        // Inventory
        //===================================

        /// <summary>
        /// Finds an item in the player's team and returns its slot within the inventory or among its team's equips.
        /// </summary>
        /// <param name="id">The item ID to search for.</param>
        /// <param name="held">Check equipped items.</param>
        /// <param name="inv">Check inventory items.</param>
        /// <returns>The InvSlot of the item. Invalid if the item could not be found.</returns>
        public InvSlot FindPlayerItem(string id, bool held, bool inv)
        {
            if (held)
            {
                for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
                {
                    Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[ii];
                    if (activeChar.EquippedItem.ID == id)
                        return new InvSlot(true, ii);
                }
            }

            if (inv)
            {
                for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.GetInvCount(); ii++)
                {
                    if (DataManager.Instance.Save.ActiveTeam.GetInv(ii).ID == id)
                        return new InvSlot(false, ii);
                }
            }

            return InvSlot.Invalid;
        }

        /// <summary>
        /// Get the number of items equipped by players.  Does not include guests.
        /// </summary>
        /// <returns>The number of items.</returns>
        public int GetPlayerEquippedCount()
        {
            int nbitems = 0;
            foreach (Character player in DataManager.Instance.Save.ActiveTeam.Players)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                    nbitems++;
            }

            return nbitems;
        }

        /// <summary>
        /// Get the number of items in the bag.
        /// </summary>
        /// <returns>The number of items.</returns>
        public int GetPlayerBagCount()
        {
            return DataManager.Instance.Save.ActiveTeam.GetInvCount();
        }

        /// <summary>
        /// Gets the maximum amount of item the player's team can carry.
        /// </summary>
        /// <returns>The number of items.</returns>
        public int GetPlayerBagLimit()
        {
            return DataManager.Instance.Save.ActiveTeam.MaxInv;
        }

        /// <summary>
        /// Gets the equipped item for the character in the specified slot.
        /// </summary>
        /// <param name="slot">The team slot of the character to check</param>
        /// <returns>The character's equipped item</returns>
        public object GetPlayerEquippedItem(int slot)
        {
            return DataManager.Instance.Save.ActiveTeam.Players[slot].EquippedItem;
        }

        /// <summary>
        /// Gets the equipped item for the character in the specified guest slot.
        /// </summary>
        /// <param name="slot">The guest slot of the character to check</param>
        /// <returns>The character's equipped item</returns>
        public object GetGuestEquippedItem(int slot)
        {
            return DataManager.Instance.Save.ActiveTeam.Guests[slot].EquippedItem;
        }

        /// <summary>
        /// Gives an item and adds it to the player team's bag.
        /// </summary>
        /// <param name="item">The item to give</param>
        public void GivePlayerItem(InvItem item)
        {
            ItemData entry = DataManager.Instance.GetItem(item.ID);
            if (entry.MaxStack > 1)
            {
                int amount = item.Amount;
                foreach (InvItem inv in DataManager.Instance.Save.ActiveTeam.EnumerateInv())
                {
                    if (inv.ID == item.ID && inv.Cursed == item.Cursed && inv.Amount < entry.MaxStack)
                    {
                        int addValue = Math.Min(entry.MaxStack - inv.Amount, item.Amount);
                        inv.Amount += addValue;
                        amount -= addValue;
                        if (amount <= 0)
                            break;
                    }
                }
                if (amount > 0 && DataManager.Instance.Save.ActiveTeam.GetInvCount() < DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone))
                {
                    InvItem newInv = new InvItem(item);
                    newInv.Amount = amount;
                    DataManager.Instance.Save.ActiveTeam.AddToInv(newInv);
                }
            }
            else
                DataManager.Instance.Save.ActiveTeam.AddToInv(item);
        }

        /// <summary>
        /// Gives an item and adds it to the player team's bag.
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="count">The amount to give. Default 1</param>
        /// <param name="cursed">Whether the item is cursed. Default false.</param>
        /// <param name="hiddenval">The hidden value of the item. Default empty string.</param>
        public void GivePlayerItem(string id, int count = 1, bool cursed = false, string hiddenval = "")
        {
            count = Math.Max(1, count);
            for (int ii = 0; ii < count; ii++)
            {
                InvItem item = new InvItem(id, cursed, 1);
                item.HiddenValue = hiddenval;
                GivePlayerItem(item);
            }
        }

        /// <summary>
        /// Gets the item found at the specified slot of the player's bag.
        /// </summary>
        /// <param name="slot">The slot to check</param>
        /// <returns>The item found in the slot</returns>
        public object GetPlayerBagItem(int slot)
        {
            return DataManager.Instance.Save.ActiveTeam.GetInv(slot);
        }

        /// <summary>
        /// Remove an item from player inventory
        /// </summary>
        /// <param name="slot">The slot from which to remove the item</param>
        /// <param name="takeAll"></param>
        public void TakePlayerBagItem(int slot, bool takeAll = false)
        {
            if (!takeAll)
            {
                InvItem item = DataManager.Instance.Save.ActiveTeam.GetInv(slot);
                ItemData entry = (ItemData)item.GetData();
                if (entry.MaxStack > 1 && item.Amount > 1)
                {
                    item.Amount--;
                    return;
                }
            }
            DataManager.Instance.Save.ActiveTeam.RemoveFromInv(slot);
        }

        /// <summary>
        /// Remove the equipped item from a chosen member of the team
        /// </summary>
        /// <param name="slot">The slot of the character on the team from which to remove the item</param>
        /// <param name="takeAll">Removes all stacks if set to true, removes one if set to false.  Has no effect for unstackable items.</param>
        public void TakePlayerEquippedItem(int slot, bool takeAll = false)
        {
            if (!takeAll)
            {
                InvItem item = DataManager.Instance.Save.ActiveTeam.Players[slot].EquippedItem;
                ItemData entry = (ItemData)item.GetData();

                if (entry.MaxStack > 1 && item.Amount > 1)
                {
                    item.Amount--;
                    return;
                }
            }
            DataManager.Instance.Save.ActiveTeam.Players[slot].SilentDequipItem();
        }

        /// <summary>
        /// Remove the equipped item from a chosen guest of the team
        /// </summary>
        /// <param name="slot">The slot of the character on the team's guest list from which to remove the item</param>
        /// <param name="takeAll">Removes all stacks if set to true, removes one if set to false.  Has no effect for unstackable items.</param>
        public void TakeGuestEquippedItem(int slot, bool takeAll = false)
        {
            if (!takeAll)
            {
                InvItem item = DataManager.Instance.Save.ActiveTeam.Guests[slot].EquippedItem;
                ItemData entry = (ItemData)item.GetData();

                if (entry.MaxStack > 1 && item.Amount > 1)
                {
                    item.Amount--;
                    return;
                }
            }
            DataManager.Instance.Save.ActiveTeam.Guests[slot].SilentDequipItem();
        }

        /// <summary>
        /// Get the amount of items in the player's storage
        /// </summary>
        /// <returns></returns>
        public int GetPlayerStorageCount()
        {
            int count = DataManager.Instance.Save.ActiveTeam.BoxStorage.Count;
            foreach (string nb in DataManager.Instance.Save.ActiveTeam.Storage.Keys)
                count += DataManager.Instance.Save.ActiveTeam.Storage[nb];
            return count;
        }

        /// <summary>
        /// Get the amount of a specific item in the player's storage
        /// </summary>
        /// <param name="id">ID of the item ot check</param>
        /// <returns>The amount of copies currently in storage</returns>
        public int GetPlayerStorageItemCount(string id)
        {
            int val;
            if (DataManager.Instance.Save.ActiveTeam.Storage.TryGetValue(id, out val))
                return val;
            return 0;
        }

        /// <summary>
        /// Gives an item and adds it to the player team's storage.
        /// </summary>
        /// <param name="item">The item to give</param>
        public void GivePlayerStorageItem(InvItem item)
        {
            DataManager.Instance.Save.ActiveTeam.StoreItems(new List<InvItem> { item });
        }

        /// <summary>
        /// Gives an item and adds it to the player team's storage.
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="count">The amount to give. Default 1</param>
        /// <param name="cursed">Whether the item is cursed. Default false.</param>
        /// <param name="hiddenval">The hidden value of the item. Default empty string.</param>
        public void GivePlayerStorageItem(string id, int count = 1, bool cursed = false, string hiddenval = "")
        {
            for (int ii = 0; ii < count; ii++)
            {
                InvItem item = new InvItem(id, cursed, 1);
                item.HiddenValue = hiddenval;
                DataManager.Instance.Save.ActiveTeam.StoreItems(new List<InvItem> { item });
            }
        }

        /// <summary>
        /// Takes an item from the storage
        /// </summary>
        /// <param name="id">The ID of the item to take</param>
        public void TakePlayerStorageItem(string id)
        {
            DataManager.Instance.Save.ActiveTeam.TakeItems(new List<WithdrawSlot> { new WithdrawSlot(false, id, 0) });
        }

        /// <summary>
        /// Takes all items in the player team's bag and equipped items, and deposits them in storage.
        /// </summary>
        public void DepositAll()
        {
            List<InvItem> items = new List<InvItem>();
            int item_count = DataManager.Instance.Save.ActiveTeam.GetInvCount();

            // Get list from held items
            foreach (Character player in DataManager.Instance.Save.ActiveTeam.Players)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                    items.Add(player.EquippedItem);
            }

            for (int ii = 0; ii < item_count; ii++)
            {
                // Get a list of inventory items.
                InvItem item = DataManager.Instance.Save.ActiveTeam.GetInv(ii);
                items.Add(item);
            }
            ;

            // Store all items in the inventory.
            DataManager.Instance.Save.ActiveTeam.StoreItems(items);

            // Remove held items
            foreach (Character player in DataManager.Instance.Save.ActiveTeam.Players)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                    player.SilentDequipItem();
            }

            // Remove the items back to front to prevent removing them in the wrong order.
            for (int ii = DataManager.Instance.Save.ActiveTeam.GetInvCount() - 1; ii >= 0; ii--)
            {
                DataManager.Instance.Save.ActiveTeam.RemoveFromInv(ii);
            }
        }

        /// <summary>
        /// Gets the amount of money the player currently has on hand.
        /// </summary>
        /// <returns>The amount of money.</returns>
        public int GetPlayerMoney()
        {
            return DataManager.Instance.Save.ActiveTeam.Money;
        }

        /// <summary>
        /// Adds money to the player's wallet.
        /// </summary>
        /// <param name="toadd">The amount of money to add.</param>
        public void AddToPlayerMoney(int toadd)
        {
            DataManager.Instance.Save.ActiveTeam.AddMoney(null, toadd);
        }

        /// <summary>
        /// Removes money from the player's wallet.
        /// </summary>
        /// <param name="toremove">The amount of money to remove.</param>
        public void RemoveFromPlayerMoney(int toremove)
        {
            DataManager.Instance.Save.ActiveTeam.LoseMoney(null, toremove);
        }

        /// <summary>
        /// Gets the amount of money in the player's bank
        /// </summary>
        /// <returns>The amount of money.</returns>
        public int GetPlayerMoneyBank()
        {
            return DataManager.Instance.Save.ActiveTeam.Bank;
        }

        /// <summary>
        /// Adds money to the player's bank.
        /// </summary>
        /// <param name="toadd">The amount of money to add.</param>
        public void AddToPlayerMoneyBank(int toadd)
        {
            DataManager.Instance.Save.ActiveTeam.Bank += toadd;
        }

        /// <summary>
        /// Removes money from the player's bank.
        /// </summary>
        /// <param name="toremove">The amount of money to remove.</param>
        public void RemoveFromPlayerMoneyBank(int toremove)
        {
            DataManager.Instance.Save.ActiveTeam.Bank -= toremove;
        }


    }
}
