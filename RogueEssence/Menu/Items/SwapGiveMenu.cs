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
    public class SwapGiveMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 8;

        ItemSummary summaryMenu;
        public List<string> AllowedGoods;
        private OnMultiChoice action;

        public SwapGiveMenu(int defaultChoice, int openSpaces, OnMultiChoice chooseSlots) :
            this(MenuLabel.SWAP_SHOP_MENU_GIVE, defaultChoice, openSpaces, chooseSlots) { }
        public SwapGiveMenu(string label, int defaultChoice, int openSpaces, OnMultiChoice chooseSlots)
        {
            Label = label;
            action = chooseSlots;
            AllowedGoods = new List<string>();

            Dictionary<string, int> itemPresence = new Dictionary<string, int>();
            foreach (string key in DataManager.Instance.Save.ActiveTeam.Storage.Keys)
                itemPresence[key] = DataManager.Instance.Save.ActiveTeam.Storage[key];

            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.GetInvCount(); ii++)
                itemPresence[DataManager.Instance.Save.ActiveTeam.GetInv(ii).ID] = itemPresence.GetValueOrDefault(DataManager.Instance.Save.ActiveTeam.GetInv(ii).ID, 0) + 1;

            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[ii];
                if (!String.IsNullOrEmpty(activeChar.EquippedItem.ID))
                    itemPresence[activeChar.EquippedItem.ID] = itemPresence.GetValueOrDefault(activeChar.EquippedItem.ID, 0) + 1;
            }

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            foreach (string key in itemPresence.Keys)
            {
                if (itemPresence[key] > 0)
                {
                    if (DataManager.Instance.DataIndices[DataManager.DataType.Item].ContainsKey(key))
                    {
                        ItemEntrySummary itemEntry = DataManager.Instance.DataIndices[DataManager.DataType.Item].Get(key) as ItemEntrySummary;

                        if (itemEntry.ContainsState<MaterialState>())
                        {
                            AllowedGoods.Add(key);

                            MenuText menuText = new MenuText(itemEntry.GetIconName(), new Loc(2, 1));
                            MenuText menuCount = new MenuText("(" + itemPresence[key] + ")", new Loc(ItemMenu.ITEM_MENU_WIDTH - 8 * 4, 1), DirV.Up, DirH.Right, Color.White);
                            flatChoices.Add(new MenuElementChoice(() => { }, true, menuText, menuCount));
                        }
                    }
                }
            }
            defaultChoice = Math.Min(defaultChoice, flatChoices.Count - 1);
            int startChoice = defaultChoice % SLOTS_PER_PAGE;
            int startPage = defaultChoice / SLOTS_PER_PAGE;
            IChoosable[][] inv = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);


            summaryMenu = new ItemSummary(Rect.FromPoints(new Loc(16, GraphicsManager.ScreenHeight - 8 - 4 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            int buyLimit = DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone) - DataManager.Instance.Save.ActiveTeam.GetInvCount();
            Initialize(new Loc(16, 16), ItemMenu.ITEM_MENU_WIDTH, Text.FormatKey("MENU_SHOP_TITLE"), inv, startChoice, startPage, SLOTS_PER_PAGE, false, new IntRange(openSpaces));

        }

        protected override void ChoseMultiIndex(List<int> slots)
        {
            int startIndex = CurrentChoiceTotal;

            MenuManager.Instance.RemoveMenu();

            action(slots);
        }


        protected override void ChoiceChanged()
        {
            InvItem item = new InvItem(AllowedGoods[CurrentChoiceTotal]);
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
        }
    }
}
