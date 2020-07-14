using System.Collections.Generic;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class ItemUnderfootMenu : UnderfootMenu
    {
        private int mapItemSlot;

        ItemSummary summaryMenu;

        public ItemUnderfootMenu(int mapItemSlot)
        {
            this.mapItemSlot = mapItemSlot;
            MapItem mapItem = ZoneManager.Instance.CurrentMap.Items[mapItemSlot];
            string itemName = mapItem.GetDungeonName();

            List<MenuTextChoice> choices = new List<MenuTextChoice>();

            bool invFull = (DungeonScene.Instance.ActiveTeam.GetInvCount() >= DungeonScene.Instance.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone));
            bool hasItem = (DungeonScene.Instance.FocusedCharacter.EquippedItem.ID > -1);

            if (mapItem.IsMoney)
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_GROUND_GET"), PickupAction));
            else
            {
                Data.ItemData entry = Data.DataManager.Instance.GetItem(mapItem.Value);
                //disable pick up for full inv
                //disable swap for empty inv
                bool canGet = (DungeonScene.Instance.ActiveTeam.GetInvCount() < DungeonScene.Instance.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone));
                if (!canGet && entry.MaxStack > 1)
                {
                    //find an inventory slot that isn't full stack
                    foreach (InvItem item in DungeonScene.Instance.ActiveTeam.EnumerateInv())
                    {
                        if (item.ID == mapItem.Value && item.Cursed == mapItem.Cursed && item.HiddenValue < entry.MaxStack)
                        {
                            canGet = true;
                            break;
                        }
                    }
                }
                bool hasItems = (DungeonScene.Instance.ActiveTeam.GetInvCount() > 0);

                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_GROUND_GET"), PickupAction, canGet, canGet ? Color.White : Color.Red));
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_REPLACE"), ReplaceAction, hasItems, hasItems ? Color.White : Color.Red));

                switch (entry.UsageType)
                {
                    case Data.ItemData.UseType.Eat:
                        {
                            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_EAT"), UseSelfAction, !mapItem.Cursed, mapItem.Cursed ? Color.Red : Color.White));
                            break;
                        }
                    case Data.ItemData.UseType.Drink:
                        {
                            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_DRINK"), UseSelfAction, !mapItem.Cursed, mapItem.Cursed ? Color.Red : Color.White));
                            break;
                        }
                    case Data.ItemData.UseType.Use:
                    case Data.ItemData.UseType.UseOther:
                        {
                            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_USE"), UseSelfAction, !mapItem.Cursed, mapItem.Cursed ? Color.Red : Color.White));
                            break;
                        }
                    case Data.ItemData.UseType.Learn:
                        {
                            //if the character is teaching to himself, need to disable this choice if not compatible
                            bool canLearn = TeachMenu.CanLearnSkill(DungeonScene.Instance.FocusedCharacter, DungeonScene.Instance.FocusedCharacter, BattleContext.FLOOR_ITEM_SLOT);
                            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_LEARN"), UseSelfAction, canLearn && !mapItem.Cursed, (canLearn && !mapItem.Cursed) ? Color.White : Color.Red));
                            break;
                        }
                }

                //hold item
                bool allowHold = (!invFull || hasItem);
                if (hasItem)
                {
                    ItemData equipEntry = DataManager.Instance.GetItem(DungeonScene.Instance.FocusedCharacter.EquippedItem.ID);
                    if (equipEntry.CannotDrop)
                        allowHold = false;
                }
                choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_HOLD"), HoldAction, allowHold, allowHold ? Color.White : Color.Red));

                if (entry.UsageType == Data.ItemData.UseType.Throw)
                {
                    int choiceIndex = choices.Count - 1;
                    choices.Insert(choiceIndex, new MenuTextChoice(Text.FormatKey("MENU_ITEM_THROW"), ThrowAction));
                }
                else
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_ITEM_THROW"), ThrowAction));
                if (entry.UsageType == Data.ItemData.UseType.Learn)
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_INFO"), InfoAction));
            }
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), ExitAction));

            if (!mapItem.IsMoney)
            {
                summaryMenu = new ItemSummary(Rect.FromPoints(new Loc(16, GraphicsManager.ScreenHeight - 8 - 4 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                    new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));
                summaryMenu.SetItem(new InvItem(mapItem.Value, mapItem.Cursed, mapItem.HiddenValue));
            }

            int menuwidth = CalculateChoiceLength(choices, 72);
            Initialize(new Loc(GraphicsManager.ScreenWidth - 16 - menuwidth, 16), menuwidth, choices.ToArray(), 0, itemName);
        }

        private void PickupAction()
        {
            MenuManager.Instance.ClearMenus();
            MenuManager.Instance.EndAction = DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.Pickup, Dir8.None));
        }

        private void ReplaceAction()
        {
            MenuManager.Instance.AddMenu(new ItemMenu(-1), false);
        }
        private void UseSelfAction()
        {
            //character uses the item himself
            MenuManager.Instance.ClearMenus();
            MenuManager.Instance.EndAction = DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.UseItem, Dir8.None, BattleContext.FLOOR_ITEM_SLOT, -1));
        }

        private void HoldAction()
        {
            //partners can only take the item for themselves
            //called only when held is false
            MenuManager.Instance.ClearMenus();
            MenuManager.Instance.EndAction = DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.Give, Dir8.None, BattleContext.FLOOR_ITEM_SLOT, -1));
        }

        private void ThrowAction()
        {
            MenuManager.Instance.ClearMenus();
            MenuManager.Instance.EndAction = DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.Throw, Dir8.None, BattleContext.FLOOR_ITEM_SLOT));
        }

        private void InfoAction()
        {
            int mapSlot = ZoneManager.Instance.CurrentMap.GetItem(DungeonScene.Instance.FocusedCharacter.CharLoc);
            MapItem mapItem = ZoneManager.Instance.CurrentMap.Items[mapSlot];
            MenuManager.Instance.AddMenu(new TeachInfoMenu(mapItem.Value), false);
        }

        private void ExitAction()
        {
            MenuManager.Instance.RemoveMenu();
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            if (summaryMenu != null)
                summaryMenu.Draw(spriteBatch);
        }
    }
}
