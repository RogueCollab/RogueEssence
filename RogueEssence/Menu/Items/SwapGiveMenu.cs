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
        public List<int> AllowedGoods;
        private OnMultiChoice action;

        public SwapGiveMenu(int defaultChoice, int openSpaces, OnMultiChoice chooseSlots)
        {
            action = chooseSlots;
            AllowedGoods = new List<int>();

            int[] itemPresence = new int[DataManager.Instance.DataIndices[DataManager.DataType.Item].Count];
            for (int ii = 0; ii < itemPresence.Length; ii++)
                itemPresence[ii] += DataManager.Instance.Save.ActiveTeam.Storage[ii];

            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.GetInvCount(); ii++)
                itemPresence[DataManager.Instance.Save.ActiveTeam.GetInv(ii).ID]++;

            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[ii];
                if (activeChar.EquippedItem.ID > -1)
                    itemPresence[activeChar.EquippedItem.ID]++;
            }

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < itemPresence.Length; ii++)
            {
                int index = ii;
                if (itemPresence[index] > 0)
                {
                    ItemData itemEntry = DataManager.Instance.GetItem(index);
                    if (itemEntry.ItemStates.Contains<MaterialState>())
                    {
                        AllowedGoods.Add(index);

                        MenuText menuText = new MenuText((itemEntry.Icon > -1 ? ((char)(itemEntry.Icon + 0xE0A0)).ToString() : "") + DataManager.Instance.GetItem(ii).Name.ToLocal(), new Loc(2, 1));
                        MenuText menuCount = new MenuText("(" + itemPresence[index] + ")", new Loc(ItemMenu.ITEM_MENU_WIDTH - 8 * 4, 1), DirV.Up, DirH.Right, Color.White);
                        flatChoices.Add(new MenuElementChoice(() => { }, true, menuText, menuCount));
                    }
                }
            }
            defaultChoice = Math.Min(defaultChoice, flatChoices.Count - 1);
            int startChoice = defaultChoice % SLOTS_PER_PAGE;
            int startPage = defaultChoice / SLOTS_PER_PAGE;
            List<MenuChoice[]> inv = SortIntoPages(flatChoices, SLOTS_PER_PAGE);


            summaryMenu = new ItemSummary(Rect.FromPoints(new Loc(16, GraphicsManager.ScreenHeight - 8 - 4 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            int buyLimit = DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone) - DataManager.Instance.Save.ActiveTeam.GetInvCount();
            Initialize(new Loc(16, 16), ItemMenu.ITEM_MENU_WIDTH, Text.FormatKey("MENU_SHOP_TITLE"), inv.ToArray(), startChoice, startPage, SLOTS_PER_PAGE, false, new IntRange(openSpaces));

        }

        protected override void ChoseMultiIndex(List<int> slots)
        {
            int startIndex = CurrentPage * SpacesPerPage + CurrentChoice;

            List<int> indices = new List<int>();
            foreach (int slot in slots)
                indices.Add(AllowedGoods[slot]);

            MenuManager.Instance.RemoveMenu();

            action(indices);
        }


        protected override void ChoiceChanged()
        {
            InvItem item = new InvItem(AllowedGoods[CurrentPage * SpacesPerPage + CurrentChoice]);
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
