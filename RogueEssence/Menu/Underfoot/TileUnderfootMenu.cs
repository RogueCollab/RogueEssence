using System.Collections.Generic;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using Microsoft.Xna.Framework.Graphics;
using RogueElements;
using System;

namespace RogueEssence.Menu
{
    public class TileUnderfootMenu : UnderfootMenu
    {
        TileSummary summaryMenu;

        public TileUnderfootMenu(string tileIndex) : this(MenuLabel.GROUND_TILE, tileIndex) { }
        public TileUnderfootMenu(string label, string tileIndex)
        {
            Label = label;
            Data.TileData entry = Data.DataManager.Instance.GetTile(tileIndex);
            List<MenuTextChoice> choices = new List<MenuTextChoice>();
            
            switch (entry.StepType)
            {
                case Data.TileData.TriggerType.Site:
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_GROUND_CHECK"), () => { choose(0); }));
                    break;
                case Data.TileData.TriggerType.Passage:
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_GROUND_PROCEED"), () => { choose(0); }));
                    break;
                case Data.TileData.TriggerType.Trap:
                case Data.TileData.TriggerType.Switch:
                    choices.Add(new MenuTextChoice(Text.FormatKey("MENU_GROUND_TRIGGER"), () => { choose(0); }));
                    break;
            }
            
            choices.Add(new MenuTextChoice(Text.FormatKey("MENU_EXIT"), () => { choose(1); }));

            summaryMenu = new TileSummary(Rect.FromPoints(new Loc(16, GraphicsManager.ScreenHeight - 8 - 3 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));
            summaryMenu.SetTile(tileIndex);

            int menuwidth = CalculateChoiceLength(choices, 72);
            Initialize(new Loc(GraphicsManager.ScreenWidth - 16 - menuwidth, 16), menuwidth, choices.ToArray(), 0, entry.Name.ToLocal(), null);
        }

        private void choose(int choice)
        {
            switch (choice)
            {
                case 0:
                    {//trigger
                        MenuManager.Instance.ClearMenus();
                        MenuManager.Instance.EndAction = DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.Tile, Dir8.None, 0));
                    }
                    break;
                case 1:
                    {//exit
                        MenuManager.Instance.RemoveMenu();
                        break;
                    }
            }
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
