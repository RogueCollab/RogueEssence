using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueEssence.Ground;

namespace RogueEssence.Menu
{
    public class SellMenu : MultiPageMenu
    {
        public const int SELL_MENU_WIDTH = 176;
        public delegate void OnChooseSlots(List<InvSlot> slots);
        private const int SLOTS_PER_PAGE = 8;

        ItemSummary summaryMenu;
        MoneySummary moneySummary;
        private OnChooseSlots action;


        public SellMenu(int defaultChoice, OnChooseSlots chooseSlots)
        {
            action = chooseSlots;

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[ii];
                int index = ii;
                if (activeChar.EquippedItem.ID > -1)
                {
                    MenuText itemText = new MenuText((index + 1).ToString() + ": " + activeChar.EquippedItem.GetName(), new Loc(2, 1));
                    MenuText itemPrice = new MenuText(activeChar.EquippedItem.GetSellValue().ToString(), new Loc(ItemMenu.ITEM_MENU_WIDTH - 8 * 4, 1), DirV.Up, DirH.Right, Color.Lime);
                    flatChoices.Add(new MenuElementChoice(() => { choose(new InvSlot(true, index)); }, true, itemText, itemPrice));
                }
            }
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.GetInvCount(); ii++)
            {
                int index = ii;

                MenuText itemText = new MenuText(DataManager.Instance.Save.ActiveTeam.GetInv(index).GetName(), new Loc(2, 1));
                MenuText itemPrice = new MenuText(DataManager.Instance.Save.ActiveTeam.GetInv(index).GetSellValue().ToString(), new Loc(ItemMenu.ITEM_MENU_WIDTH - 8 * 4, 1), DirV.Up, DirH.Right, Color.Lime);
                flatChoices.Add(new MenuElementChoice(() => { choose(new InvSlot(false, index)); }, true, itemText, itemPrice));
            }
            defaultChoice = Math.Min(defaultChoice, flatChoices.Count - 1);
            int startChoice = defaultChoice % SLOTS_PER_PAGE;
            int startPage = defaultChoice / SLOTS_PER_PAGE;
            List<MenuChoice[]> inv = SortIntoPages(flatChoices, SLOTS_PER_PAGE);


            summaryMenu = new ItemSummary(Rect.FromPoints(new Loc(16, GraphicsManager.ScreenHeight - 8 - 4 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            moneySummary = new MoneySummary(Rect.FromPoints(new Loc(16 + SELL_MENU_WIDTH, summaryMenu.Bounds.Top - LINE_SPACE * 2 - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, summaryMenu.Bounds.Top)));

            Initialize(new Loc(16, 16), SELL_MENU_WIDTH, Text.FormatKey("MENU_ITEM_TITLE"), inv.ToArray(), startChoice, startPage, SLOTS_PER_PAGE, false, flatChoices.Count);

        }

        private void choose(InvSlot slot)
        {
            int startIndex = CurrentPage * SpacesPerPage + CurrentChoice;

            MenuManager.Instance.AddMenu(new SellChosenMenu(new List<InvSlot>() { slot }, startIndex, action), true);
        }


        protected override void ChoseMultiIndex(List<int> slots)
        {
            int startIndex = CurrentPage * SpacesPerPage + CurrentChoice;

            List<int> heldIDs = new List<int>();
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[ii];
                if (activeChar.EquippedItem.ID > -1)
                    heldIDs.Add(ii);
            }

            List<InvSlot> invSlots = new List<InvSlot>();
            for (int ii = 0; ii < slots.Count; ii++)
            {
                if (slots[ii] < heldIDs.Count)
                    invSlots.Add(new InvSlot(true, heldIDs[slots[ii]]));
                else
                    invSlots.Add(new InvSlot(false, slots[ii] - heldIDs.Count));
            }

            MenuManager.Instance.AddMenu(new SellChosenMenu(invSlots, startIndex, action), true);
        }

        private int getMaxInvPages()
        {
            if (DataManager.Instance.Save.ActiveTeam.GetInvCount() == 0)
                return 0;
            return (DataManager.Instance.Save.ActiveTeam.GetInvCount() - 1) / SLOTS_PER_PAGE + 1;
        }

        protected override void ChoiceChanged()
        {
            InvItem item = getChosenItemID(CurrentPage * SpacesPerPage + CurrentChoice);
            summaryMenu.SetItem(item);
            base.ChoiceChanged();
        }

        private InvItem getChosenItemID(int menuIndex)
        {
            if (menuIndex == -1)
                return null;

            int countedEquipped = 0;
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[ii];
                if (activeChar.EquippedItem.ID > -1)
                {
                    if (countedEquipped == menuIndex)
                        return activeChar.EquippedItem;
                    countedEquipped++;
                }
            }
            menuIndex -= countedEquipped;
            return DataManager.Instance.Save.ActiveTeam.GetInv(menuIndex);
        }

        protected override void UpdateKeys(InputManager input)
        {
            if (input.JustPressed(FrameInput.InputType.SortItems))
            {
                GameManager.Instance.SE("Menu/Sort");
                MenuManager.Instance.NextAction = SortCommand();
            }
            else
                base.UpdateKeys(input);
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
        
        public IEnumerator<YieldInstruction> SortCommand()
        {
            yield return CoroutineManager.Instance.StartCoroutine(GroundScene.Instance.ProcessInput(new GameAction(GameAction.ActionType.SortItems, Dir8.None)));
            MenuManager.Instance.ReplaceMenu(new DepositMenu(0));
        }
    }
}
