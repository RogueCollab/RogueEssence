using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using Microsoft.Xna.Framework;

namespace RogueEssence.Menu
{
    public class DungeonSummary : SummaryMenu
    {

        MenuText DungeonName;
        MenuDivider MenuDiv;
        MenuText Floors;

        public DungeonSummary(Rect bounds) : base(bounds)
        {
            DungeonName = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(DungeonName);
            MenuDiv = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_SPACE),
                Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(MenuDiv);
            Floors = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET));
            Elements.Add(Floors);

        }

        public void SetDungeon(int index, bool isComplete)
        {
            ZoneEntrySummary zoneEntry = DataManager.Instance.DataIndices[DataManager.DataType.Zone].Entries[index] as ZoneEntrySummary;
            if (zoneEntry == null)
                Visible = false;
            else
            {
                DungeonName.Text = zoneEntry.Name.ToLocal();
                DungeonName.Color = isComplete ? Color.White : Color.Cyan;
                Floors.Text = Text.FormatKey("MENU_DUNGEON_FLOORS", (isComplete && zoneEntry.CountedFloors > 0) ? zoneEntry.CountedFloors.ToString() : "??");

                while (Elements.Count > 3)
                    Elements.RemoveAt(3);

                List<MenuText> rules = new List<MenuText>();

                if (zoneEntry.NoEXP)
                    rules.Add(new MenuText(Text.FormatKey("ZONE_RESTRICT_EXP"), new Loc()));
                if (zoneEntry.Level > -1)
                {
                    if (zoneEntry.LevelCap)
                        rules.Add(new MenuText(Text.FormatKey("ZONE_RESTRICT_LEVEL", zoneEntry.Level), new Loc()));
                    else
                        rules.Add(new MenuText(Text.FormatKey("ZONE_EXPECT_LEVEL", zoneEntry.Level), new Loc()));
                }
                if (zoneEntry.TeamSize > -1)
                    rules.Add(new MenuText(Text.FormatKey("ZONE_RESTRICT_TEAM", zoneEntry.TeamSize), new Loc(),
                        (DataManager.Instance.Save.ActiveTeam.Players.Count > zoneEntry.TeamSize) ? Color.Red : Color.White));
                if (zoneEntry.TeamRestrict)
                    rules.Add(new MenuText(Text.FormatKey("ZONE_RESTRICT_ALONE"), new Loc(),
                        (DataManager.Instance.Save.ActiveTeam.Players.Count > 1) ? Color.Red : Color.White));
                if (zoneEntry.MoneyRestrict)
                    rules.Add(new MenuText(Text.FormatKey("ZONE_RESTRICT_MONEY"), new Loc(),
                        (DataManager.Instance.Save.ActiveTeam.Money > 0) ? Color.Red : Color.White));
                if (zoneEntry.BagSize > -1)
                    rules.Add(new MenuText(Text.FormatKey("ZONE_RESTRICT_BAG", zoneEntry.BagSize), new Loc(),
                        (DataManager.Instance.Save.ActiveTeam.GetInvCount() > zoneEntry.BagSize) ? Color.Red : Color.White));
                if (zoneEntry.BagRestrict > -1)
                    rules.Add(new MenuText((zoneEntry.BagRestrict == 0) ? Text.FormatKey("ZONE_RESTRICT_ITEM_ALL") : Text.FormatKey("ZONE_RESTRICT_ITEM", zoneEntry.BagRestrict), new Loc(),
                        (DataManager.Instance.Save.ActiveTeam.GetInvCount() > zoneEntry.BagRestrict) ? Color.Red : Color.White));

                for (int ii = 0; ii < rules.Count; ii++)
                {
                    rules[ii].Loc = Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * (1 + ii));
                    Elements.Add(rules[ii]);
                }

                Bounds.Size = new Loc(Bounds.Size.X, GraphicsManager.MenuBG.TileHeight * 2 + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * (1 + rules.Count));
            }
        }

    }
}
