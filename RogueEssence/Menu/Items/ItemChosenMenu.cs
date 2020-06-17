using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueEssence.Ground;

namespace RogueEssence.Menu
{
    public class ItemChosenMenu : SingleStripMenu
    {

        private int slot;
        private bool held;

        public ItemChosenMenu(int slot, bool held)
        {
            this.slot = slot;
            this.held = held;

            bool leader = (GameManager.Instance.CurrentScene != DungeonScene.Instance) || (DungeonScene.Instance.ActiveTeam.Leader == DungeonScene.Instance.FocusedCharacter);
            bool focus = (GameManager.Instance.CurrentScene != DungeonScene.Instance) || (held && DungeonScene.Instance.ActiveTeam.Players[slot] == DungeonScene.Instance.FocusedCharacter);
            InvItem invItem = null;
            if (held)
                invItem = DataManager.Instance.Save.ActiveTeam.Players[slot].EquippedItem;
            else
                invItem = DataManager.Instance.Save.ActiveTeam.Inventory[slot];
            Data.ItemData entry = Data.DataManager.Instance.GetItem(invItem.ID);

            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            //able to use if an item is not held, or if an item is held, but the focused character is the holder
            if (GameManager.Instance.CurrentScene != DungeonScene.Instance)
            {

            }
            else if (!held || focus)
            {
                switch (entry.UsageType)
                {
                    case Data.ItemData.UseType.Eat:
                        {
                            if (leader && !held)
                                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_EAT"), UseOtherAction, !invItem.Cursed, invItem.Cursed ? Color.Red : Color.White));
                            else
                                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_EAT"), UseSelfAction, !invItem.Cursed, invItem.Cursed ? Color.Red : Color.White));
                            break;
                        }
                    case Data.ItemData.UseType.Drink:
                        {
                            if (leader && !held)
                                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_DRINK"), UseOtherAction, !invItem.Cursed, invItem.Cursed ? Color.Red : Color.White));
                            else
                                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_DRINK"), UseSelfAction, !invItem.Cursed, invItem.Cursed ? Color.Red : Color.White));
                            break;
                        }
                    case Data.ItemData.UseType.Use:
                        {
                            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_USE"), UseSelfAction, !invItem.Cursed, invItem.Cursed ? Color.Red : Color.White));
                            break;
                        }
                    case Data.ItemData.UseType.UseOther:
                        {
                            if (leader && !held)
                                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_USE"), UseOtherAction, !invItem.Cursed, invItem.Cursed ? Color.Red : Color.White));
                            else
                                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_USE"), UseSelfAction, !invItem.Cursed, invItem.Cursed ? Color.Red : Color.White));
                            break;
                        }
                    case Data.ItemData.UseType.Learn:
                        {
                            bool canLearn = true;
                            if (!(leader && !held))
                            {
                                //if the character is teaching the skill itself, need to disable this choice if not compatible
                                if (!TeachMenu.CanLearnSkill(DungeonScene.Instance.FocusedCharacter, DungeonScene.Instance.FocusedCharacter, held ? BattleContext.EQUIP_ITEM_SLOT : slot))
                                    canLearn = false;
                            }

                            if (leader && !held)
                                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_TEACH"), TeachOtherAction, !invItem.Cursed, invItem.Cursed ? Color.Red : Color.White));
                            else
                                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_LEARN"), UseSelfAction, canLearn && !invItem.Cursed, (canLearn && !invItem.Cursed) ? Color.White : Color.Red));
                            break;
                        }
                }
            }

            if (!held)
            {
                //item is not held, can give or hold
                if (leader)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_GIVE"), GiveAction));
                else
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_HOLD"), HoldAction));
            }
            else
            {
                bool invEmpty = (DataManager.Instance.Save.ActiveTeam.Inventory.Count == 0);
                //item is held
                if (focus || leader)
                {
                    //if the focused character is the holder, it can put it back (disable if inv is full)
                    choices.Add(new MenuTextChoice(leader ? Text.FormatKey("MENU_ITEM_TAKE") : Text.FormatKey("MENU_ITEM_PUT"), PutBackAction));
                    //or swap it with an item in inventory
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_SWAP"), SwapAction, !invEmpty, invEmpty ? Color.Red : Color.White));
                }
            }

            if (GameManager.Instance.CurrentScene != DungeonScene.Instance)
            {

            }
            else if (!held || focus)
            {
                //actions can be done only if the focused character is the holder, or if it isn't held at all
                int itemSlot = ZoneManager.Instance.CurrentMap.GetItem(DungeonScene.Instance.FocusedCharacter.CharLoc);
                string dropString = Text.FormatKey("MENU_ITEM_PLACE");
                bool disableDrop = false;
                if (itemSlot > -1)
                {
                    MapItem mapItem = ZoneManager.Instance.CurrentMap.Items[itemSlot];
                    if (mapItem.IsMoney)
                        disableDrop = true;
                    else
                        dropString = Text.FormatKey("MENU_ITEM_REPLACE");
                }
                choices.Add(new MenuTextChoice(dropString, PlaceAction, !disableDrop, disableDrop ? Color.Red : Color.White));

                if (entry.UsageType == Data.ItemData.UseType.Throw)
                    choices.Insert(0, new MenuTextChoice(Text.FormatKey("MENU_ITEM_THROW"), ThrowAction));
                else
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_THROW"), ThrowAction));
            }
            if (entry.UsageType == Data.ItemData.UseType.Learn)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_INFO"), InfoAction));
            if (GameManager.Instance.CurrentScene != DungeonScene.Instance)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_TRASH"), TrashAction));
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));


            Initialize(new Loc(ItemMenu.ITEM_MENU_WIDTH + 16, 16), CalculateChoiceLength(choices, 72), choices.ToArray(), 0);
        }

        private int getItemUseSlot()
        {
            if (held)
                return BattleContext.EQUIP_ITEM_SLOT;
            else
                return slot;
        }

        private void TeachOtherAction()
        {
            //leader gets to choose who to use item on, with previews
            MenuManager.Instance.AddMenu(new TeachMenu(getItemUseSlot()), false);
        }
        private void UseOtherAction()
        {
            //leader gets to choose who to use item on
            MenuManager.Instance.AddMenu(new ItemTargetMenu(getItemUseSlot(), true), false);
        }

        private void UseSelfAction()
        {
            //character uses the item himself
            MenuManager.Instance.ClearMenus();
            MenuManager.Instance.EndAction = DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.UseItem, Dir8.None, getItemUseSlot(), -1));
        }

        private void PutBackAction()
        {
            MenuManager.Instance.ClearMenus();
            MenuManager.Instance.EndAction = (GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.Take, Dir8.None, slot)) : GroundScene.Instance.ProcessInput(new GameAction(GameAction.ActionType.Take, Dir8.None, slot));
        }

        private void SwapAction()
        {
            MenuManager.Instance.AddMenu(new ItemMenu(slot), false);
        }
        private void GiveAction()
        {
            //leader gets to choose who to give the item to
            //called only when held is false
            MenuManager.Instance.AddMenu(new ItemTargetMenu(slot, false), false);
        }
        private void HoldAction()
        {
            //partners can only take the item for themselves
            //called only when held is false
            MenuManager.Instance.ClearMenus();
            MenuManager.Instance.EndAction = (GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.Give, Dir8.None, slot, -1)) : GroundScene.Instance.ProcessInput(new GameAction(GameAction.ActionType.Give, Dir8.None, slot, -1));
        }

        private void PlaceAction()
        {
            MenuManager.Instance.ClearMenus();
            MenuManager.Instance.EndAction = DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.Drop, Dir8.None, getItemUseSlot()));
        }

        private void TrashAction()
        {
            MenuManager.Instance.ClearMenus();
            MenuManager.Instance.EndAction = GroundScene.Instance.ProcessInput(new GameAction(GameAction.ActionType.Drop, Dir8.None, slot, held ? 1 : 0));
        }

        private void ThrowAction()
        {
            MenuManager.Instance.ClearMenus();
            MenuManager.Instance.EndAction = DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.Throw, Dir8.None, getItemUseSlot()));
        }

        private void InfoAction()
        {
            if (held)
                MenuManager.Instance.AddMenu(new TeachInfoMenu(DataManager.Instance.Save.ActiveTeam.Players[slot].EquippedItem.ID), false);
            else
                MenuManager.Instance.AddMenu(new TeachInfoMenu(DataManager.Instance.Save.ActiveTeam.Inventory[slot].ID), false);
        }

        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }

    }
}
