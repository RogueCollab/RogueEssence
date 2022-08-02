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
    public class ItemMenu : MultiPageMenu
    {
        private static int defaultChoice;

        public const int ITEM_MENU_WIDTH = 176;
        private const int SLOTS_PER_PAGE = 8;

        private int replaceSlot;

        ItemSummary summaryMenu;

        //-2 for no replace slot, -1 for replace with ground, positive numbers for replace held team index's item
        public ItemMenu() : this(-2) { }
        public ItemMenu(int replaceSlot)
        {
            this.replaceSlot = replaceSlot;
            
            bool enableHeld = (replaceSlot == -2);
            bool enableBound = (replaceSlot != -1);

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[ii];
                int index = ii;
                if (!String.IsNullOrEmpty(activeChar.EquippedItem.ID))
                {
                    MenuText itemText = new MenuText((index + 1).ToString() + ": " + activeChar.EquippedItem.GetDisplayName(), new Loc(2, 1), !enableHeld ? Color.Red : Color.White);
                    MenuText itemPrice = new MenuText(activeChar.EquippedItem.GetPriceString(), new Loc(ItemMenu.ITEM_MENU_WIDTH - 8 * 4, 1), DirV.Up, DirH.Right, !enableHeld ? Color.Red : Color.White);
                    flatChoices.Add(new MenuElementChoice(() => { choose(-index - 1); }, enableHeld, itemText, itemPrice));
                }
            }
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.GetInvCount(); ii++)
            {
                int index = ii;
                ItemData entry = DataManager.Instance.GetItem(DataManager.Instance.Save.ActiveTeam.GetInv(index).ID);
                bool enable = !entry.CannotDrop || enableBound;
                MenuText itemText = new MenuText(DataManager.Instance.Save.ActiveTeam.GetInv(index).GetDisplayName(), new Loc(2, 1), !enable ? Color.Red : Color.White);
                MenuText itemPrice = new MenuText(DataManager.Instance.Save.ActiveTeam.GetInv(index).GetPriceString(), new Loc(ItemMenu.ITEM_MENU_WIDTH - 8 * 4, 1), DirV.Up, DirH.Right, !enable ? Color.Red : Color.White);
                flatChoices.Add(new MenuElementChoice(() => { choose(index); }, true, itemText, itemPrice));
            }

            int actualChoice = Math.Min(Math.Max(0, defaultChoice), flatChoices.Count - 1);

            IChoosable[][] inv = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);


            summaryMenu = new ItemSummary(Rect.FromPoints(new Loc(16, GraphicsManager.ScreenHeight - 8 - 4 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            int startPage = actualChoice / SLOTS_PER_PAGE;
            int startIndex = actualChoice % SLOTS_PER_PAGE;

            Initialize(new Loc(16, 16), ITEM_MENU_WIDTH, (replaceSlot == -2) ? Text.FormatKey("MENU_ITEM_TITLE") : Text.FormatKey("MENU_ITEM_SWAP_TITLE"), inv, startIndex, startPage, SLOTS_PER_PAGE);

        }

        private void choose(int choice)
        {
            //read-only when replaying
            if (Data.DataManager.Instance.CurrentReplay == null)
            {
                if (replaceSlot == -2)
                {
                    if (choice < 0)
                        MenuManager.Instance.AddMenu(new ItemChosenMenu(-choice - 1, true), true);
                    else
                        MenuManager.Instance.AddMenu(new ItemChosenMenu(choice, false), true);
                }
                else if (replaceSlot == -1)
                {
                    MenuManager.Instance.ClearMenus();
                    MenuManager.Instance.EndAction = DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.Drop, Dir8.None, choice));
                }
                else
                {
                    MenuManager.Instance.ClearMenus();
                    MenuManager.Instance.EndAction = (GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.Give, Dir8.None, choice, replaceSlot)) : GroundScene.Instance.ProcessInput(new GameAction(GameAction.ActionType.Give, Dir8.None, choice, replaceSlot));
                }
            }
        }

        private int getMaxInvPages()
        {
            if (DataManager.Instance.Save.ActiveTeam.GetInvCount() == 0)
                return 0;
            return MathUtils.DivUp(DataManager.Instance.Save.ActiveTeam.GetInvCount(), SLOTS_PER_PAGE);
        }

        protected override void ChoiceChanged()
        {
            defaultChoice = CurrentChoiceTotal;
            InvItem item = getChosenItemID(CurrentChoiceTotal);

            summaryMenu.SetItem(item);
            base.ChoiceChanged();
        }

        private InvItem getChosenItemID(int menuIndex)
        {
            int countedHeld = 0;
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[ii];
                if (!String.IsNullOrEmpty(activeChar.EquippedItem.ID))
                {
                    if (countedHeld == menuIndex)
                        return activeChar.EquippedItem;
                    countedHeld++;
                }
            }
            menuIndex -= countedHeld;
            return DataManager.Instance.Save.ActiveTeam.GetInv(menuIndex);
        }

        protected override void UpdateKeys(InputManager input)
        {
            if (input.JustPressed(FrameInput.InputType.SortItems))
            {
                if (replaceSlot == -2 && Data.DataManager.Instance.CurrentReplay == null)
                {
                    GameManager.Instance.SE("Menu/Sort");
                    MenuManager.Instance.NextAction = SortCommand();
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
            else if (input.JustPressed(FrameInput.InputType.ItemMenu))
                MenuManager.Instance.ClearMenus();
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
            yield return CoroutineManager.Instance.StartCoroutine((GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.SortItems, Dir8.None)) : GroundScene.Instance.ProcessInput(new GameAction(GameAction.ActionType.SortItems, Dir8.None)));
            MenuManager.Instance.ReplaceMenu(new ItemMenu(replaceSlot));
        }
    }
}
