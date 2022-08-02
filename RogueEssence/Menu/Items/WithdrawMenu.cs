using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class WithdrawMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 8;

        public delegate void OnWithdrawChoice(List<WithdrawSlot> slot);

        ItemSummary summaryMenu;
        List<WithdrawSlot> availableItems;
        OnWithdrawChoice storageChoice;
        bool continueOnChoose;

        public WithdrawMenu(int defaultChoice, bool continueOnChoose, OnWithdrawChoice storageChoice)
        {
            this.continueOnChoose = continueOnChoose;
            this.storageChoice = storageChoice;
            availableItems = new List<WithdrawSlot>();
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            List<string> sortedKeys = DataManager.Instance.DataIndices[DataManager.DataType.Item].GetOrderedKeys(true);
            for (int ii = 0; ii < sortedKeys.Count; ii++)
            {
                int index = ii;
                string key = sortedKeys[ii];
                int qty = DataManager.Instance.Save.ActiveTeam.Storage.GetValueOrDefault(key, 0);
                if (qty > 0)
                {
                    WithdrawSlot slot = new WithdrawSlot(false, key, 0);
                    availableItems.Add(slot);
                    ItemData entry = DataManager.Instance.GetItem(key);
                    MenuText menuText = new MenuText(DataManager.Instance.GetItem(key).GetIconName(), new Loc(2, 1));
                    MenuText menuCount = new MenuText("(" + qty + ")", new Loc(ItemMenu.ITEM_MENU_WIDTH - 8 * 4, 1), DirV.Up, DirH.Right, Color.White);
                    flatChoices.Add(new MenuElementChoice(() => { choose(slot); }, true, menuText, menuCount));
                }
            }
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.BoxStorage.Count; ii++)
            {
                WithdrawSlot slot = new WithdrawSlot(true, "", ii);
                availableItems.Add(slot);
                flatChoices.Add(new MenuTextChoice(DataManager.Instance.Save.ActiveTeam.BoxStorage[ii].GetDisplayName(), () => { choose(slot); }));
            }

            defaultChoice = Math.Min(defaultChoice, flatChoices.Count - 1);
            int startChoice = defaultChoice % SLOTS_PER_PAGE;
            int startPage = defaultChoice / SLOTS_PER_PAGE;
            IChoosable[][] inv = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            summaryMenu = new ItemSummary(Rect.FromPoints(new Loc(16, GraphicsManager.ScreenHeight - 8 - 4 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            int withdrawLimit = continueOnChoose ? DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone) - DataManager.Instance.Save.ActiveTeam.GetInvCount() : -1;
            Initialize(new Loc(16, 16), ItemMenu.ITEM_MENU_WIDTH, Text.FormatKey("MENU_STORAGE_TITLE"), inv, startChoice, startPage, SLOTS_PER_PAGE, false, withdrawLimit);

        }

        private void choose(WithdrawSlot choice)
        {
            int startIndex = CurrentChoiceTotal;
            List<WithdrawSlot> choices = new List<WithdrawSlot>();
            choices.Add(choice);
            MenuManager.Instance.AddMenu(new WithdrawChosenMenu(choices, startIndex, continueOnChoose, storageChoice), true);
        }

        protected override void ChoseMultiIndex(List<int> slots)
        {
            int startIndex = CurrentChoiceTotal;
            List<WithdrawSlot> indices = new List<WithdrawSlot>();
            foreach (int slot in slots)
                indices.Add(availableItems[slot]);
            MenuManager.Instance.AddMenu(new WithdrawChosenMenu(indices, startIndex, continueOnChoose, storageChoice), true);
        }

        protected override void ChoiceChanged()
        {
            int totalChoice = CurrentChoiceTotal;
            WithdrawSlot index = availableItems[totalChoice];
            if (!index.IsBox)
                summaryMenu.SetItem(new InvItem(index.ItemID, false, 1));
            else
                summaryMenu.SetItem(DataManager.Instance.Save.ActiveTeam.BoxStorage[index.BoxSlot]);
            base.ChoiceChanged();
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            summaryMenu.Draw(spriteBatch);
        }
    }
}
