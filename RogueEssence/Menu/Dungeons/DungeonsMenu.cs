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

        public DungeonsMenu(List<string> names, List<ZoneLoc> dests, OnChooseSlot destAction)
        {
            this.destAction = destAction;
            this.dests = dests;
            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < dests.Count; ii++)
            {
                int dungeonIndex = ii;
                if (dests[ii].StructID.Segment < 0)
                    flatChoices.Add(new MenuTextChoice(names[ii], () => { chooseDungeon(dungeonIndex); }));
                else
                    flatChoices.Add(new MenuTextChoice(names[ii], () => { chooseDungeon(dungeonIndex); }, true,
                        (DataManager.Instance.Save.GetDungeonUnlock(dests[ii].ID) == GameProgress.UnlockState.Completed) ? Color.White : Color.Cyan));
            }
            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            summaryMenu = new DungeonSummary(Rect.FromPoints(new Loc(176, 16), new Loc(GraphicsManager.ScreenWidth - 16, 16 + GraphicsManager.MenuBG.TileHeight * 2 + VERT_SPACE * 7)));

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
                summaryMenu.SetDungeon(dests[choice].ID, DataManager.Instance.Save.GetDungeonUnlock(dests[choice].ID) == GameProgress.UnlockState.Completed, true);
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
