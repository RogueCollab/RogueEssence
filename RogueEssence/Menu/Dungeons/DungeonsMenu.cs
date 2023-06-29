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

        OnChooseSlot destAction;
        DungeonSummary summaryMenu;
        private List<ZoneLoc> dests;
        private List<string> names;
        private List<string> titles;

        public DungeonsMenu(List<string> names, List<string> titles, List<ZoneLoc> dests, OnChooseSlot destAction)
        {
            this.destAction = destAction;
            this.dests = dests;
            this.names = names;
            this.titles = titles;
            
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < dests.Count; ii++)
            {
                int dungeonIndex = ii;
                flatChoices.Add(new MenuTextChoice(names[ii], () => { chooseDungeon(dungeonIndex); }));
            }
            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            summaryMenu = new DungeonSummary(Rect.FromPoints(new Loc(168, 8), new Loc(GraphicsManager.ScreenWidth - 8, 8 + GraphicsManager.MenuBG.TileHeight * 2 + VERT_SPACE * 7)));

            Initialize(new Loc(0, 0), 160, Text.FormatKey("MENU_DUNGEON_TITLE"), choices, 0, 0, Math.Min(SLOTS_PER_PAGE, flatChoices.Count));
        }

        private void chooseDungeon(int choice)
        {
            MenuManager.Instance.RemoveMenu();
            destAction(choice);
        }


        protected override void ChoiceChanged()
        {
            int choice = CurrentChoiceTotal;
            if (dests[choice].StructID.Segment < 0)
                summaryMenu.Visible = false;
            else
            {
                summaryMenu.Visible = true;
                summaryMenu.SetDungeon(titles[choice], dests[choice].ID, DataManager.Instance.Save.GetDungeonUnlock(dests[choice].ID) == GameProgress.UnlockState.Completed, true);
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
