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
    public class SwapShopMenu : MultiPageMenu
    {
        public const int SWAP_MENU_WIDTH = 160;
        private const int SLOTS_PER_PAGE = 8;

        ItemSummary summaryMenu;
        MoneySummary moneySummary;
        TradeSummary tradeSummary;
        private OnChooseSlot action;
        public List<Tuple<int, int[]>> Goods;
        public int[] PriceList;
        public List<int> AllowedGoods;
        bool[] itemPresence;
        bool[] tradePresence;
        int presenceCount;

        public SwapShopMenu(List<Tuple<int, int[]>> goods, int[] priceList, int defaultChoice, OnChooseSlot chooseSlot)
        {
            Goods = goods;
            PriceList = priceList;
            action = chooseSlot;
            AllowedGoods = new List<int>();

            itemPresence = new bool[DataManager.Instance.DataIndices[DataManager.DataType.Item].Count];
            tradePresence = new bool[DataManager.Instance.DataIndices[DataManager.DataType.Item].Count];
            for (int ii = 0; ii < itemPresence.Length; ii++)
            {
                if (DataManager.Instance.Save.ActiveTeam.Storage[ii] > 0)
                    updatePresence(ii);
            }
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.GetInvCount(); ii++)
                updatePresence(DataManager.Instance.Save.ActiveTeam.GetInv(ii).ID);

            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[ii];
                if (activeChar.EquippedItem.ID > -1)
                    updatePresence(activeChar.EquippedItem.ID);
            }

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < goods.Count; ii++)
            {
                int index = ii;

                bool canView = false;
                bool canTrade = true;
                int wildcards = 0;
                int[] reqs = goods[ii].Item2;
                for (int jj = 0; jj < reqs.Length; jj++)
                {
                    if (reqs[jj] > -1)
                    {
                        if (!itemPresence[reqs[jj]])
                        {
                            canTrade = false;
                        }
                        else
                            canView = true;
                    }
                    else
                    {
                        wildcards++;
                        canView = true;
                    }
                }
                
                if (canView)
                {
                    AllowedGoods.Add(ii);
                    ItemData itemEntry = DataManager.Instance.GetItem(goods[ii].Item1);
                    if (PriceList[itemEntry.Rarity] > DataManager.Instance.Save.ActiveTeam.Money || wildcards > presenceCount)
                        canTrade = false;
                    flatChoices.Add(new MenuTextChoice(itemEntry.GetIconName(), () => { choose(index); }, canTrade, canTrade ? Color.White : Color.Red));
                }
            }
            defaultChoice = Math.Min(defaultChoice, flatChoices.Count - 1);
            int startChoice = defaultChoice % SLOTS_PER_PAGE;
            int startPage = defaultChoice / SLOTS_PER_PAGE;
            List<MenuChoice[]> inv = SortIntoPages(flatChoices, SLOTS_PER_PAGE);


            summaryMenu = new ItemSummary(Rect.FromPoints(new Loc(16, GraphicsManager.ScreenHeight - 8 - 4 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            tradeSummary = new TradeSummary(Rect.FromPoints(new Loc(16 + SWAP_MENU_WIDTH, summaryMenu.Bounds.Top - LINE_HEIGHT * 7 - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, summaryMenu.Bounds.Top)));
            moneySummary = new MoneySummary(Rect.FromPoints(new Loc(16 + SWAP_MENU_WIDTH, tradeSummary.Bounds.Top - LINE_HEIGHT * 2 - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, tradeSummary.Bounds.Top)));

            int buyLimit = DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone) - DataManager.Instance.Save.ActiveTeam.GetInvCount();
            Initialize(new Loc(16, 16), SWAP_MENU_WIDTH, Text.FormatKey("MENU_SHOP_TITLE"), inv.ToArray(), startChoice, startPage, SLOTS_PER_PAGE);

        }

        private void updatePresence(int index)
        {
            if (!itemPresence[index])
            {
                itemPresence[index] = true;
                ItemEntrySummary itemEntry = DataManager.Instance.DataIndices[DataManager.DataType.Item].Entries[index] as ItemEntrySummary;

                if (itemEntry.ContainsState<MaterialState>())
                {
                    presenceCount++;
                    tradePresence[index] = true;
                }
            }
        }

        private void choose(int choice)
        {
            int startIndex = CurrentChoiceTotal;

            MenuManager.Instance.RemoveMenu();

            action(choice);
        }


        protected override void ChoiceChanged()
        {
            Tuple<int, int[]> trade = Goods[AllowedGoods[CurrentChoiceTotal]];
            ItemData entry = DataManager.Instance.GetItem(trade.Item1);
            InvItem item = new InvItem(trade.Item1);
            summaryMenu.SetItem(item);
            tradeSummary.SetTrade(trade.Item2, PriceList[entry.Rarity], itemPresence, presenceCount);
            base.ChoiceChanged();
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            summaryMenu.Draw(spriteBatch);
            tradeSummary.Draw(spriteBatch);
            moneySummary.Draw(spriteBatch);
        }
    }
}
