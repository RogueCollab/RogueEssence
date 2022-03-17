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

        ItemSummary summaryMenu;
        List<int> availableItems;
        OnMultiChoice storageChoice;
        bool continueOnChoose;

        public WithdrawMenu(int defaultChoice, bool continueOnChoose, OnMultiChoice storageChoice)
        {
            this.continueOnChoose = continueOnChoose;
            this.storageChoice = storageChoice;
            availableItems = new List<int>();
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Storage.Length; ii++)
            {
                int index = ii;
                if (DataManager.Instance.Save.ActiveTeam.Storage[ii] > 0)
                {
                    availableItems.Add(index);
                    Data.ItemData entry = Data.DataManager.Instance.GetItem(ii);
                    MenuText menuText = new MenuText(DataManager.Instance.GetItem(ii).GetIconName(), new Loc(2, 1));
                    MenuText menuCount = new MenuText("(" + DataManager.Instance.Save.ActiveTeam.Storage[ii] + ")", new Loc(ItemMenu.ITEM_MENU_WIDTH - 8 * 4, 1), DirV.Up, DirH.Right, Color.White);
                    flatChoices.Add(new MenuElementChoice(() => { choose(index); }, true, menuText, menuCount));
                }
            }
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.BoxStorage.Count; ii++)
            {
                int index = ii + DataManager.Instance.DataIndices[DataManager.DataType.Item].Count;
                availableItems.Add(index);
                flatChoices.Add(new MenuTextChoice(DataManager.Instance.Save.ActiveTeam.BoxStorage[ii].GetDisplayName(), () => { choose(index); }));
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

        private void choose(int choice)
        {
            int startIndex = CurrentChoiceTotal;
            List<int> choices = new List<int>();
            choices.Add(choice);
            MenuManager.Instance.AddMenu(new WithdrawChosenMenu(choices, startIndex, continueOnChoose, storageChoice), true);
        }

        protected override void ChoseMultiIndex(List<int> slots)
        {
            int startIndex = CurrentChoiceTotal;
            List<int> indices = new List<int>();
            foreach (int slot in slots)
                indices.Add(availableItems[slot]);
            MenuManager.Instance.AddMenu(new WithdrawChosenMenu(indices, startIndex, continueOnChoose, storageChoice), true);
        }

        protected override void ChoiceChanged()
        {
            int totalChoice = CurrentChoiceTotal;
            int index = availableItems[totalChoice];
            if (index < DataManager.Instance.DataIndices[DataManager.DataType.Item].Count)
                summaryMenu.SetItem(new InvItem(index, false, 1));
            else
                summaryMenu.SetItem(DataManager.Instance.Save.ActiveTeam.BoxStorage[index - DataManager.Instance.DataIndices[DataManager.DataType.Item].Count]);
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
