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
        private const int SLOTS_PER_PAGE = 12;

        DungeonSummary summaryMenu;
        private List<int> dungeonIndices;
        private SeedSummary infoMenu;
        private ulong? seed;

        public RogueDestMenu()
        {
            dungeonIndices = new List<int>();

            for (int ii = 0; ii < DataManager.Instance.DataIndices[DataManager.DataType.Zone].Count; ii++)
            {
                ZoneEntrySummary summary = DataManager.Instance.DataIndices[DataManager.DataType.Zone].Entries[ii] as ZoneEntrySummary;
                if (!DiagManager.Instance.DevMode)
                {
                    if (DataManager.Instance.Save.DungeonUnlocks[ii] == GameProgress.UnlockState.None)
                        continue;
                    if (summary == null)
                        continue;
                    if (summary.Rogue != RogueStatus.AllTransfer)
                        continue;
                }
                dungeonIndices.Add(ii);
            }

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_START_RANDOM"), () => { choose(dungeonIndices[MathUtils.Rand.Next(dungeonIndices.Count)]); }));
            for (int ii = 0; ii < dungeonIndices.Count; ii++)
            {
                int zone = dungeonIndices[ii];
                ZoneEntrySummary summary = DataManager.Instance.DataIndices[DataManager.DataType.Zone].Entries[zone] as ZoneEntrySummary;
                flatChoices.Add(new MenuTextChoice(summary.GetColoredName(), () => { choose(zone); }));
            }
            IChoosable[][] box = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            int totalSlots = SLOTS_PER_PAGE;
            if (box.Length == 1)
                totalSlots = box[0].Length;

            summaryMenu = new DungeonSummary(Rect.FromPoints(new Loc(176, 16), new Loc(GraphicsManager.ScreenWidth - 16, 16 + GraphicsManager.MenuBG.TileHeight * 2 + VERT_SPACE * 7)));

            infoMenu = new SeedSummary(new Rect(new Loc(176, 128), new Loc(128, LINE_HEIGHT + GraphicsManager.MenuBG.TileHeight * 2)));
            UpdateExtraInfo("");

            Initialize(new Loc(16, 16), 160, Text.FormatKey("MENU_DUNGEON_TITLE"), box, 0, 0, totalSlots, false, -1);
        }

        protected override void ChoiceChanged()
        {
            int choice = CurrentChoiceTotal - 1;
            if (choice > -1)
            {
                summaryMenu.Visible = true;
                bool isComplete = false;
                if (DataManager.Instance.Save != null)
                    isComplete = DataManager.Instance.Save.DungeonUnlocks[dungeonIndices[choice]] == GameProgress.UnlockState.Completed;
                summaryMenu.SetDungeon(dungeonIndices[choice], isComplete);
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

        private void choose(int choice)
        {
            MenuManager.Instance.AddMenu(new RogueTeamInputMenu(choice, seed), false);
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
    }
}
