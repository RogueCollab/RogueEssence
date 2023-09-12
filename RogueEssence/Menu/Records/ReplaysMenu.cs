using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueElements;
using RogueEssence.Data;
using System.IO;
using System;

namespace RogueEssence.Menu
{
    public class ReplaysMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 14;

        public ReplaysMenu()
        {
            List<RecordHeaderData> records = DataManager.Instance.GetRecordHeaders(PathMod.ModSavePath(DataManager.REPLAY_PATH), DataManager.REPLAY_EXTENSION);

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            foreach (RecordHeaderData record in records)
            {
                string fileName = Path.GetFileNameWithoutExtension(record.Path);
                if (record.Name != "")
                {
                    try
                    {
                        string rogueSign = "";
                        if (record.Result == GameProgress.ResultType.Escaped || record.Result == GameProgress.ResultType.Cleared || record.Result == GameProgress.ResultType.Rescue)
                            rogueSign += "\uE10A";
                        else
                            rogueSign += "\uE10B";
                        if (record.IsRogue)
                        {
                            if (record.IsSeeded)
                                rogueSign += "\uE10D";
                            else
                                rogueSign += "\uE10C";
                        }

                        //also include an indicator of the floors traversed, if possible
                        fileName = rogueSign + record.Name + ": " + record.LocationString;
                    }
                    catch (Exception ex)
                    {
                        DiagManager.Instance.LogError(ex, false);
                    }
                }
                flatChoices.Add(new MenuTextChoice(fileName, () => { choose(record.Path); }));
            }
            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            //for the summary menu, include team, date, filename, location (string), seed, indication of rogue and seeded runs
            //if it can't be read, just include the filename

            Initialize(new Loc(0, 0), 224, Text.FormatKey("MENU_REPLAYS_TITLE"), choices, 0, 0, SLOTS_PER_PAGE);
        }

        private void choose(string dir)
        {
            MenuManager.Instance.AddMenu(new ReplayChosenMenu(dir), true);
        }
    }
}
