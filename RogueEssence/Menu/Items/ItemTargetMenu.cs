using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueEssence.Ground;

namespace RogueEssence.Menu
{
    public class ItemTargetMenu : TitledStripMenu
    {
        SummaryMenu summaryMenu;
        MenuText Text;

        private int invSlot;
        private bool useItem;
        private int commandIdx;

        public ItemTargetMenu(int invSlot, bool useItem, int commandIdx)
        {
            this.invSlot = invSlot;
            this.useItem = useItem;
            this.commandIdx = commandIdx;

            List<MenuTextChoice> team = new List<MenuTextChoice>();
            foreach (Character character in DataManager.Instance.Save.ActiveTeam.Players)
            {
                int teamIndex = team.Count;
                bool validTarget = !character.Dead || !useItem;
                team.Add(new MenuTextChoice(character.GetDisplayName(true), () => { choose(teamIndex); }, validTarget, !validTarget ? Color.Red : Color.White));
            }

            summaryMenu = new SummaryMenu(new Rect(new Loc(16, 16 + team.Count * VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2 + ContentOffset),
                new Loc(128, VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2)));
            Text = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            summaryMenu.Elements.Add(Text);

            Initialize(new Loc(16, 16), 144, RogueEssence.Text.FormatKey("MENU_ITEM_TARGET_TITLE"), team.ToArray(), 0);

        }

        protected override void ChoiceChanged()
        {
            string itemIndex = DataManager.Instance.Save.ActiveTeam.Players[CurrentChoice].EquippedItem.ID;
            if (!String.IsNullOrEmpty(itemIndex))
                Text.SetText(RogueEssence.Text.FormatKey("MENU_HELD_ITEM", DataManager.Instance.Save.ActiveTeam.Players[CurrentChoice].EquippedItem.GetDisplayName()));
            else
                Text.SetText(RogueEssence.Text.FormatKey("MENU_HELD_NO_ITEM"));
            base.ChoiceChanged();
        }

        private void choose(int choice)
        {
            MenuManager.Instance.ClearMenus();
            //give the item at the inv slot to the given team slot
            if (useItem)
                MenuManager.Instance.EndAction = (GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.UseItem, Dir8.None, invSlot, choice)) : GroundScene.Instance.ProcessInput(new GameAction(GameAction.ActionType.UseItem, Dir8.None, invSlot, choice, commandIdx));
            else
                MenuManager.Instance.EndAction = (GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.Give, Dir8.None, invSlot, choice)) : GroundScene.Instance.ProcessInput(new GameAction(GameAction.ActionType.Give, Dir8.None, invSlot, choice));
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
