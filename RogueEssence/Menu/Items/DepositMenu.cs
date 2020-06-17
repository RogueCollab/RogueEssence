using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueEssence.Ground;

namespace RogueEssence.Menu
{
    public class DepositMenu : MultiPageMenu
    {
        //TODO: create a delegate OnChooseSlots and have it called when a deposit is decided
        private const int SLOTS_PER_PAGE = 8;

        ItemSummary summaryMenu;

        public DepositMenu(int defaultChoice)
        {
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[ii];
                int index = ii;
                if (activeChar.EquippedItem.ID > -1)
                    flatChoices.Add(new MenuTextChoice((index + 1).ToString() + ": " + activeChar.EquippedItem.GetName(), () => { choose(new InvSlot(true, index)); }));
            }
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Inventory.Count; ii++)
            {
                int index = ii;
                flatChoices.Add(new MenuTextChoice(DataManager.Instance.Save.ActiveTeam.Inventory[index].GetName(), () => { choose(new InvSlot(false, index)); }));
            }
            defaultChoice = Math.Min(defaultChoice, flatChoices.Count - 1);
            int startChoice = defaultChoice % SLOTS_PER_PAGE;
            int startPage = defaultChoice / SLOTS_PER_PAGE;
            List<MenuChoice[]> inv = SortIntoPages(flatChoices, SLOTS_PER_PAGE);


            summaryMenu = new ItemSummary(Rect.FromPoints(new Loc(16, GraphicsManager.ScreenHeight - 8 - 4 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            Initialize(new Loc(16, 16), ItemMenu.ITEM_MENU_WIDTH, Text.FormatKey("MENU_ITEM_TITLE"), inv.ToArray(), startChoice, startPage, SLOTS_PER_PAGE, false, flatChoices.Count);

        }

        private void choose(InvSlot slot)
        {
            int startIndex = CurrentPage * SpacesPerPage + CurrentChoice;
            MenuManager.Instance.AddMenu(new DepositChosenMenu(new List<InvSlot>() { slot }, startIndex), true);
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

            MenuManager.Instance.AddMenu(new DepositChosenMenu(invSlots, startIndex), true);
        }

        private int getMaxInvPages()
        {
            if (DataManager.Instance.Save.ActiveTeam.Inventory.Count == 0)
                return 0;
            return (DataManager.Instance.Save.ActiveTeam.Inventory.Count - 1) / SLOTS_PER_PAGE + 1;
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

            int countedHeld = 0;
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[ii];
                if (activeChar.EquippedItem.ID > -1)
                {
                    if (countedHeld == menuIndex)
                        return activeChar.EquippedItem;
                    countedHeld++;
                }
            }
            menuIndex -= countedHeld;
            return DataManager.Instance.Save.ActiveTeam.Inventory[menuIndex];
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
        }
        
        public IEnumerator<YieldInstruction> SortCommand()
        {
            yield return CoroutineManager.Instance.StartCoroutine(GroundScene.Instance.ProcessInput(new GameAction(GameAction.ActionType.SortItems, Dir8.None)));
            MenuManager.Instance.ReplaceMenu(new DepositMenu(0));
        }
    }
}
