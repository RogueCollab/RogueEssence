using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueElements;
using RogueEssence.Data;
using System.IO;

namespace RogueEssence.Menu
{
    public class QuicksaveMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 14;

        public QuicksaveMenu()
        {
            List<RecordHeaderData> records = DataManager.Instance.GetRecordHeaders(DataManager.ROGUE_PATH, DataManager.QUICKSAVE_EXTENSION);

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            foreach (RecordHeaderData record in records)
            {
                string fileName = Path.GetFileNameWithoutExtension(record.Path);
                if (record.Name != "")
                {
                    LocalText zoneName = DataManager.Instance.DataIndices[DataManager.DataType.Zone].Entries[record.Zone].Name;
                    string rogueSign = "";
                    if (record.IsRogue)
                    {
                        if (record.IsSeeded)
                            rogueSign = "\uE10D";
                        else
                            rogueSign = "\uE10C";
                    }
                    //also include an indicator of the floors traversed, if possible
                    flatChoices.Add(new MenuTextChoice(rogueSign + record.Name + ": " + zoneName.ToLocal(), () => { choose(record.Path); }));
                }
                else
                    flatChoices.Add(new MenuTextChoice(fileName, () => { choose(record.Path); }));
            }
            List<MenuChoice[]> choices = SortIntoPages(flatChoices, SLOTS_PER_PAGE);


            Initialize(new Loc(0, 0), 240, Text.FormatKey("MENU_SAVE_TITLE"), choices.ToArray(), 0, 0, SLOTS_PER_PAGE);
        }

        private void choose(string dir)
        {
            MenuManager.Instance.AddMenu(new QuicksaveChosenMenu(dir), true);
        }

    }
}
