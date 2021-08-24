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
    public class StorageMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 8;

        ItemSummary summaryMenu;
        bool multiSelect;
        OnMultiChoice storageChoice;
        Action refuseAction;
        List<int> availableItems;

        //TODO: merge with the Withdraw menu, maybe.  don't worry about this right now because it's not used for anything
        public StorageMenu(Character player, bool multiSelect, OnMultiChoice storageChoice, Action refuseAction)
        {
            this.multiSelect = multiSelect;
            this.storageChoice = storageChoice;
            this.refuseAction = refuseAction;

            availableItems = new List<int>();
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Storage.Length; ii++)
            {
                int index = ii;
                if (DataManager.Instance.Save.ActiveTeam.Storage[ii] > 0)
                {
                    availableItems.Add(index);
                    MenuText menuText = new MenuText(DataManager.Instance.GetItem(ii).GetIconName(), new Loc(2, 1));
                    MenuText menuCount = new MenuText("(" + DataManager.Instance.Save.ActiveTeam.Storage[ii]+")", new Loc(ItemMenu.ITEM_MENU_WIDTH - 8 * 4, 1), DirV.Up, DirH.Right, Color.White);
                    flatChoices.Add(new MenuElementChoice(() => { choose(index); }, true, menuText, menuCount));
                }
            }
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.BoxStorage.Count; ii++)
            {
                int index = ii + DataManager.Instance.DataIndices[DataManager.DataType.Item].Count;
                availableItems.Add(index);
                flatChoices.Add(new MenuTextChoice(DataManager.Instance.Save.ActiveTeam.BoxStorage[ii].GetDisplayName(), () => { choose(index); }));
            }
            List<MenuChoice[]> inv = SortIntoPages(flatChoices, SLOTS_PER_PAGE);

            summaryMenu = new ItemSummary(Rect.FromPoints(new Loc(16, GraphicsManager.ScreenHeight - 8 - 4 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));


            Initialize(new Loc(16, 16), ItemMenu.ITEM_MENU_WIDTH, Text.FormatKey("MENU_STORAGE_TITLE"), inv.ToArray(), 0, 0, SLOTS_PER_PAGE);

        }

        private void choose(int choice)
        {
            MenuManager.Instance.RemoveMenu();
            List<int> choices = new List<int>();
            choices.Add(choice);
            storageChoice(choices);
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

        protected override void Canceled()
        {
            base.Canceled();
            refuseAction();
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
