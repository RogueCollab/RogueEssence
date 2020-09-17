using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class QuicksaveMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 14;

        public QuicksaveMenu()
        {
            List<RecordHeaderData> records = DataManager.Instance.GetRecordHeaders(DataManager.ROGUE_PATH);

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            foreach (RecordHeaderData record in records)
            {
                string fileName = record.Path.Substring(record.Path.LastIndexOf('/') + 1);
                if (record.ScoreValid)
                    flatChoices.Add(new MenuTextChoice(record.Name + " (" + fileName + ")", () => { choose(record.Path); }));
                else
                    flatChoices.Add(new MenuTextChoice("(" + fileName + ")", () => { choose(record.Path); }, true, Color.Red));
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
