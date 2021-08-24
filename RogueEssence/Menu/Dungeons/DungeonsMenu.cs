using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class DungeonsMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 14;

        OnChooseSlot chooseDungeonAction;
        OnChooseSlot chooseGroundAction;
        DungeonSummary summaryMenu;
        private List<int> dungeonIndices;

        public DungeonsMenu(List<int> availables, List<ZoneLoc> groundDests, OnChooseSlot dungeonAction, OnChooseSlot groundAction)
        {
            chooseDungeonAction = dungeonAction;
            chooseGroundAction = groundAction;
            dungeonIndices = availables;
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < dungeonIndices.Count; ii++)
            {
                int dungeonIndex = ii;
                flatChoices.Add(new MenuTextChoice(DataManager.Instance.DataIndices[DataManager.DataType.Zone].Entries[dungeonIndices[ii]].GetColoredName(), () => { chooseDungeon(dungeonIndex); }, true,
                    (DataManager.Instance.Save.DungeonUnlocks[dungeonIndices[ii]] == GameProgress.UnlockState.Completed) ? Color.White : Color.Cyan));
            }
            for (int ii = 0; ii < groundDests.Count; ii++)
            {
                ZoneData zone = DataManager.Instance.GetZone(groundDests[ii].ID);
                int groundIndex = ii;
                flatChoices.Add(new MenuTextChoice(DataManager.Instance.GetGround(zone.GroundMaps[groundDests[ii].StructID.ID]).GetColoredName(), () => { chooseGround(groundIndex); }));
            }
            List<MenuChoice[]> choices = SortIntoPages(flatChoices, SLOTS_PER_PAGE);

            summaryMenu = new DungeonSummary(Rect.FromPoints(new Loc(176, 16), new Loc(GraphicsManager.ScreenWidth - 16, 16 + GraphicsManager.MenuBG.TileHeight * 2 + VERT_SPACE * 7)));

            Initialize(new Loc(0, 0), 160, Text.FormatKey("MENU_DUNGEON_TITLE"), choices.ToArray(), 0, 0, Math.Min(SLOTS_PER_PAGE, flatChoices.Count));
        }

        private void chooseDungeon(int choice)
        {
            MenuManager.Instance.RemoveMenu();

            chooseDungeonAction(choice);
        }

        private void chooseGround(int choice)
        {
            MenuManager.Instance.RemoveMenu();

            chooseGroundAction(choice);
        }


        protected override void ChoiceChanged()
        {
            int choice = CurrentChoiceTotal;
            if (choice < dungeonIndices.Count)
            {
                summaryMenu.Visible = true;
                summaryMenu.SetDungeon(dungeonIndices[choice], DataManager.Instance.Save.DungeonUnlocks[dungeonIndices[choice]] == GameProgress.UnlockState.Completed);
            }
            else
                summaryMenu.Visible = false;
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
