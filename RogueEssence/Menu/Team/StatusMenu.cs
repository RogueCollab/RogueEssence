using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueElements;

namespace RogueEssence.Menu
{
    public class StatusMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 8;

        SummaryMenu summaryMenu;
        DialogueText Description;

        List<int> mapIndices;
        List<int> indices;

        public StatusMenu(int teamSlot)
        {
            int menuWidth = 168;
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            mapIndices = new List<int>();
            foreach (int status in ZoneManager.Instance.CurrentMap.Status.Keys)
            {
                MapStatus statusInstance = ZoneManager.Instance.CurrentMap.Status[status];
                if (!statusInstance.Hidden)
                {
                    Data.MapStatusData statusData = Data.DataManager.Instance.GetMapStatus(status);
                    mapIndices.Add(status);
                    MenuText statusName = statusName = new MenuText(statusData.Name.ToLocal(), new Loc(2, 1));
                    MapCountDownState countDown = statusInstance.StatusStates.GetWithDefault<MapCountDownState>();
                    if (countDown != null && countDown.Counter > 0)
                        flatChoices.Add(new MenuElementChoice(() => { }, true, statusName, new MenuText("[" + countDown.Counter + "]", new Loc(menuWidth - 8 * 4, 1), DirH.Right)));
                    else
                        flatChoices.Add(new MenuElementChoice(() => { }, true, statusName));
                }
            }
            indices = new List<int>();
            foreach (int status in DungeonScene.Instance.ActiveTeam.Players[teamSlot].StatusEffects.Keys)
            {
                if (Data.DataManager.Instance.GetStatus(status).MenuName)
                {
                    indices.Add(status);
                    MenuText statusName = null;
                    StackState stack = DungeonScene.Instance.ActiveTeam.Players[teamSlot].StatusEffects[status].StatusStates.GetWithDefault<StackState>();
                    if (stack != null)
                        statusName = new MenuText(Data.DataManager.Instance.GetStatus(status).Name.ToLocal() + (stack.Stack < 0 ? " " : " +") + stack.Stack, new Loc(2, 1));
                    else
                        statusName = new MenuText(Data.DataManager.Instance.GetStatus(status).Name.ToLocal(), new Loc(2, 1));

                    CountDownState countDown = DungeonScene.Instance.ActiveTeam.Players[teamSlot].StatusEffects[status].StatusStates.GetWithDefault<CountDownState>();
                    if (countDown != null && countDown.Counter > 0)
                        flatChoices.Add(new MenuElementChoice(() => { }, true, statusName, new MenuText("[" + countDown.Counter + "]", new Loc(menuWidth - 8 * 4, 1), DirH.Right)));
                    else
                        flatChoices.Add(new MenuElementChoice(() => { }, true, statusName));
                }
            }
            List<MenuChoice[]> statuses = SortIntoPages(flatChoices, SLOTS_PER_PAGE);
            
            summaryMenu = new SummaryMenu(Rect.FromPoints(new Loc(16, GraphicsManager.ScreenHeight - 8 - 4 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            Description = new DialogueText("", summaryMenu.Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight),
                summaryMenu.Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 4 - summaryMenu.Bounds.X, LINE_SPACE, false);
            summaryMenu.Elements.Add(Description);

            Initialize(new Loc(16, 16), menuWidth, Text.FormatKey("MENU_TEAM_STATUS_TITLE"), statuses.ToArray(), 0, 0, SLOTS_PER_PAGE);

        }

        private void choose(int choice)
        {
            //do nothing, or show more info?
        }

        protected override void ChoiceChanged()
        {
            int index = CurrentPage * SpacesPerPage + CurrentChoice;
            if (index < mapIndices.Count)
            {
                int entryIndex = mapIndices[index];
                Data.MapStatusData entry = Data.DataManager.Instance.GetMapStatus(entryIndex);
                Description.Text = entry.Desc.ToLocal();
            }
            else
            {
                int entryIndex = indices[index - mapIndices.Count];
                Data.StatusData entry = Data.DataManager.Instance.GetStatus(entryIndex);
                Description.Text = entry.Desc.ToLocal();
            }
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
