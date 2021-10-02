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
    public class ShopMenu : MultiPageMenu
    {
        public const int SELL_MENU_WIDTH = 176;
        private const int SLOTS_PER_PAGE = 8;

        ItemSummary summaryMenu;
        MoneySummary moneySummary;
        private OnMultiChoice action;
        public List<Tuple<InvItem, int>> Goods;

        public ShopMenu(List<Tuple<InvItem, int>> goods, int defaultChoice, OnMultiChoice chooseSlots)
        {
            Goods = goods;
            action = chooseSlots;

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < goods.Count; ii++)
            {
                int index = ii;

                bool canAfford = goods[index].Item2 <= DataManager.Instance.Save.ActiveTeam.Money;
                MenuText itemText = new MenuText(goods[index].Item1.GetDisplayName(), new Loc(2, 1), canAfford ? Color.White : Color.Red);
                MenuText itemPrice = new MenuText(goods[index].Item2.ToString(), new Loc(ItemMenu.ITEM_MENU_WIDTH - 8 * 4, 1), DirV.Up, DirH.Right, Color.Lime);
                flatChoices.Add(new MenuElementChoice(() => { choose(index); }, true, itemText, itemPrice));
            }
            defaultChoice = Math.Min(defaultChoice, flatChoices.Count - 1);
            int startChoice = defaultChoice % SLOTS_PER_PAGE;
            int startPage = defaultChoice / SLOTS_PER_PAGE;
            IChoosable[][] inv = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);


            summaryMenu = new ItemSummary(Rect.FromPoints(new Loc(16, GraphicsManager.ScreenHeight - 8 - 4 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            moneySummary = new MoneySummary(Rect.FromPoints(new Loc(16 + ItemMenu.ITEM_MENU_WIDTH, summaryMenu.Bounds.Top - LINE_HEIGHT * 2 - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, summaryMenu.Bounds.Top)));

            int buyLimit = DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone) - DataManager.Instance.Save.ActiveTeam.GetInvCount();
            Initialize(new Loc(16, 16), SELL_MENU_WIDTH, Text.FormatKey("MENU_SHOP_TITLE"), inv, startChoice, startPage, SLOTS_PER_PAGE, false, buyLimit);

        }

        private void choose(int choice)
        {
            int startIndex = CurrentChoiceTotal;
            List<int> invSlots = new List<int>();
            invSlots.Add(choice);

            MenuManager.Instance.AddMenu(new BuyChosenMenu(invSlots, startIndex, Goods[choice].Item1.ID, action), true);
        }


        protected override void ChoseMultiIndex(List<int> slots)
        {
            int startIndex = CurrentChoiceTotal;

            MenuManager.Instance.AddMenu(new BuyChosenMenu(slots, startIndex, Goods[slots[0]].Item1.ID, action), true);
        }

        protected override void ChoiceChanged()
        {
            InvItem item = Goods[CurrentChoiceTotal].Item1;
            summaryMenu.SetItem(item);
            base.ChoiceChanged();
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            summaryMenu.Draw(spriteBatch);
            moneySummary.Draw(spriteBatch);
        }
    }
}
