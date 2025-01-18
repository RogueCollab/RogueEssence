using System;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class RogueDestMenu : MultiPageMenu
    {
        private static int defaultChoice;

        private const int SLOTS_PER_PAGE = 14;

        DungeonSummary summaryMenu;
        private List<string> dungeonIndices;
        private SeedSummary infoMenu;
        private ulong? seed;

        public RogueDestMenu() : this(MenuLabel.ROGUE_DEST_MENU) { }
        public RogueDestMenu(string label)
        {
            Label = label;
            dungeonIndices = GetDestinationsList();

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_START_RANDOM"), () => { choose(dungeonIndices[MathUtils.Rand.Next(dungeonIndices.Count)], true); }));
            for (int ii = 0; ii < dungeonIndices.Count; ii++)
            {
                string zone = dungeonIndices[ii];
                ZoneEntrySummary summary = DataManager.Instance.DataIndices[DataManager.DataType.Zone].Get(zone) as ZoneEntrySummary;
                flatChoices.Add(new MenuTextChoice(summary.GetColoredName(), () => { choose(zone, false); }));
            }

            int actualChoice = Math.Min(Math.Max(0, defaultChoice), flatChoices.Count - 1);
            IChoosable[][] box = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            int totalSlots = SLOTS_PER_PAGE;
            if (box.Length == 1)
                totalSlots = box[0].Length;

            summaryMenu = new DungeonSummary(Rect.FromPoints(new Loc(168, 8), new Loc(GraphicsManager.ScreenWidth - 8, 8 + GraphicsManager.MenuBG.TileHeight * 2 + VERT_SPACE * 7)));

            infoMenu = new SeedSummary(new Rect(new Loc(168, 128), new Loc(144, LINE_HEIGHT + GraphicsManager.MenuBG.TileHeight * 2)));
            UpdateExtraInfo("");

            int startPage = actualChoice / SLOTS_PER_PAGE;
            int startIndex = actualChoice % SLOTS_PER_PAGE;

            Initialize(new Loc(), 160, Text.FormatKey("MENU_DUNGEON_TITLE"), box, startIndex, startPage, totalSlots, false, -1);
        }

        protected override void ChoiceChanged()
        {
            defaultChoice = CurrentChoiceTotal;
            int choice = CurrentChoiceTotal - 1;
            if (choice > -1)
            {
                summaryMenu.Visible = true;
                bool isComplete = false;
                if (DataManager.Instance.Save != null)
                    isComplete = DataManager.Instance.Save.GetDungeonUnlock(dungeonIndices[choice]) == GameProgress.UnlockState.Completed;

                ZoneEntrySummary zoneEntry = DataManager.Instance.DataIndices[DataManager.DataType.Zone].Get(dungeonIndices[choice]) as ZoneEntrySummary;
                summaryMenu.SetDungeon(zoneEntry.GetColoredName(), dungeonIndices[choice], isComplete, false, true);
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

            infoMenu.Draw(spriteBatch);
        }

        private void choose(string choice, bool randomized)
        {
            RogueConfig config = new RogueConfig();
            config.Destination = choice;
            config.DestinationRandomized = randomized;
            bool seedRandomized = !seed.HasValue;
            ulong seedVal = seedRandomized ?  MathUtils.Rand.NextUInt64() : seed.Value;
            config.Seed = seedVal;
            config.SeedRandomized = seedRandomized;
            MenuManager.Instance.AddMenu(new RogueTeamInputMenu(config), false);
        }


        protected override void UpdateKeys(InputManager input)
        {
            //when entering the customization choice, limit the actual choice to the defaults
            //allow the player to choose any combination of traits within a given species
            //however, when switching between species, the settings are kept even if invalid for new species, just display legal substitutes in those cases
            if (input.JustPressed(FrameInput.InputType.SortItems))
            {
                GameManager.Instance.SE("Menu/Confirm");
                SeedInputMenu menu = new SeedInputMenu(UpdateExtraInfo, seed);
                MenuManager.Instance.AddMenu(menu, false);
            }
            else
                base.UpdateKeys(input);
        }

        public void UpdateExtraInfo(string text)
        {
            ulong newSeed;
            if (UInt64.TryParse(text, NumberStyles.HexNumber, null, out newSeed))
                seed = newSeed;
            else
                seed = null;

            infoMenu.SetDetails(seed);
        }

        public static List<string> GetDestinationsList()
        {
            List<string> dungeonIndices = new List<string>();

            foreach(string key in DataManager.Instance.DataIndices[DataManager.DataType.Zone].GetOrderedKeys(true))
            {
                ZoneEntrySummary summary = DataManager.Instance.DataIndices[DataManager.DataType.Zone].Get(key) as ZoneEntrySummary;
                if (!DiagManager.Instance.DevMode)
                {
                    if (DataManager.Instance.Save.GetDungeonUnlock(key) == GameProgress.UnlockState.None)
                        continue;
                    if (summary == null)
                        continue;
                    if (summary.Rogue != RogueStatus.AllTransfer && summary.Rogue != RogueStatus.ItemTransfer)
                        continue;
                }
                dungeonIndices.Add(key); 
            }

            return dungeonIndices;
        }
    }
}
