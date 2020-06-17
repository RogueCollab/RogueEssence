using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class RogueDestMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 12;

        DungeonSummary summaryMenu;
        private List<int> dungeonIndices;

        public RogueDestMenu()
        {
            dungeonIndices = new List<int>();

            for (int ii = 0; ii < DataManager.Instance.DataIndices[DataManager.DataType.Zone].Count; ii++)
            {
                ZoneEntrySummary summary = DataManager.Instance.DataIndices[DataManager.DataType.Zone].Entries[ii] as ZoneEntrySummary;
                if (DataManager.Instance.Save.DungeonUnlocks[ii] == GameProgress.UnlockState.None)
                    continue;
                if (summary == null)
                    continue;
                if (summary.Rogue != RogueStatus.AllTransfer)
                    continue;

                dungeonIndices.Add(ii);
            }

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_START_RANDOM"), () => { choose(dungeonIndices[MathUtils.Rand.Next(dungeonIndices.Count)]); }));
            for (int ii = 0; ii < dungeonIndices.Count; ii++)
            {
                int zone = dungeonIndices[ii];
                flatChoices.Add(new MenuTextChoice(DataManager.Instance.GetZone(zone).Name.ToLocal(), () => { choose(zone); }));
            }
            List<MenuChoice[]> box = SortIntoPages(flatChoices, SLOTS_PER_PAGE);
            
            int totalSlots = SLOTS_PER_PAGE;
            if (box.Count == 1)
                totalSlots = box[0].Length;

            summaryMenu = new DungeonSummary(Rect.FromPoints(new Loc(176, 16), new Loc(GraphicsManager.ScreenWidth - 16, 16 + GraphicsManager.MenuBG.TileHeight * 2 + VERT_SPACE * 7)));

            Initialize(new Loc(16, 16), 160, Text.FormatKey("MENU_DUNGEON_TITLE"), box.ToArray(), 0, 0, totalSlots, false, -1);
        }

        protected override void ChoiceChanged()
        {
            int choice = CurrentPage * SpacesPerPage + CurrentChoice - 1;
            if (choice > -1)
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

        private void choose(int choice)
        {
            MenuManager.Instance.AddMenu(new RogueTeamMenu(choice), false);
        }


    }
}
