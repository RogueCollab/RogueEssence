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
            DungeonName = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(DungeonName);
            MenuDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT),
                Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(MenuDiv);
            Floors = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET));
            Elements.Add(Floors);

        }

        public void SetDungeon(string title, string index, bool isComplete, bool showRestrict, bool rogue)
        {
            ZoneEntrySummary zoneEntry = DataManager.Instance.DataIndices[DataManager.DataType.Zone].Get(index) as ZoneEntrySummary;
            if (zoneEntry == null)
                Visible = false;
            else
            {
                DungeonName.SetText(title);
                Floors.SetText(Text.FormatKey("MENU_DUNGEON_FLOORS", (isComplete && zoneEntry.CountedFloors > 0) ? zoneEntry.CountedFloors.ToString() : "??"));

                while (Elements.Count > 3)
                    Elements.RemoveAt(3);

                List<MenuText> rules = new List<MenuText>();

                if (zoneEntry.ExpPercent <= 0)
                    rules.Add(new MenuText(Text.FormatKey("ZONE_RESTRICT_EXP"), Loc.Zero));
                if (zoneEntry.Level > -1)
                {
                    if (zoneEntry.LevelCap || rogue)
                    {
                        rules.Add(new MenuText(Text.FormatKey("ZONE_RESTRICT_LEVEL", zoneEntry.Level), Loc.Zero));
                        if (!zoneEntry.KeepSkills && !rogue)
                        {
                            rules.Add(new MenuText(Text.FormatKey("ZONE_RESET_MOVESET"), Loc.Zero));
                        }
                    }
                    else
                        rules.Add(new MenuText(Text.FormatKey("ZONE_EXPECT_LEVEL", zoneEntry.Level), Loc.Zero));
                }

                GameProgress save = DataManager.Instance.Save;
                if (zoneEntry.TeamSize > -1)
                    rules.Add(new MenuText(Text.FormatKey("ZONE_RESTRICT_TEAM", zoneEntry.TeamSize), Loc.Zero,
                        (showRestrict && save.ActiveTeam.Players.Count > zoneEntry.TeamSize) ? Color.Red : Color.White));
                if (!rogue)
                {
                    if (zoneEntry.TeamRestrict)
                        rules.Add(new MenuText(Text.FormatKey("ZONE_RESTRICT_ALONE"), Loc.Zero,
                            (showRestrict && save.ActiveTeam.Players.Count > 1) ? Color.Red : Color.White));
                    if (zoneEntry.MoneyRestrict)
                        rules.Add(new MenuText(Text.FormatKey("ZONE_RESTRICT_MONEY"), Loc.Zero,
                            (showRestrict && save.ActiveTeam.Money > 0) ? Color.Red : Color.White));
                    if (zoneEntry.BagRestrict > -1)
                    {
                        int totalRemovable = save.GetTotalRemovableItems(zoneEntry);
                        rules.Add(new MenuText((zoneEntry.BagRestrict == 0) ? Text.FormatKey("ZONE_RESTRICT_ITEM_ALL") : Text.FormatKey("ZONE_RESTRICT_ITEM", zoneEntry.BagRestrict), Loc.Zero,
                            (showRestrict && totalRemovable > zoneEntry.BagRestrict) ? Color.Red : Color.White));
                    }
                }
                if (zoneEntry.BagSize > -1)
                    rules.Add(new MenuText(Text.FormatKey("ZONE_RESTRICT_BAG", zoneEntry.BagSize), Loc.Zero,
                        (showRestrict && save.ActiveTeam.GetInvCount() > zoneEntry.BagSize) ? Color.Red : Color.White));
                if (rogue)
                {
                    rules.Add(new MenuText(Text.FormatKey("ZONE_TRANSFER",
                        zoneEntry.Rogue == RogueStatus.AllTransfer ? Text.FormatKey("TRANSFER_ALL") : Text.FormatKey("TRANSFER_ITEMS")),
                        Loc.Zero, Color.White));
                }

                for (int ii = 0; ii < rules.Count; ii++)
                {
                    rules[ii].Loc = new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * (1 + ii));
                    Elements.Add(rules[ii]);
                }

                Bounds.Size = new Loc(Bounds.Size.X, GraphicsManager.MenuBG.TileHeight * 2 + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * (1 + rules.Count));
            }
        }

    }
}
