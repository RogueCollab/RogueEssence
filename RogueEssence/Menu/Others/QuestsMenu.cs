using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using System.IO;
using System.Xml;
using System;

namespace RogueEssence.Menu
{
    public class QuestsMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 7;

        public QuestsMenu()
        {
            List<(string, string)> quests = GetEligibleQuests();

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            foreach ((string name, string dir) quest in quests)
                flatChoices.Add(new MenuTextChoice(quest.name, () => { choose(quest.dir); }));

            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            Initialize(new Loc(8, 16), 304, Text.FormatKey("MENU_QUESTS_TITLE"), choices, 0, 0, SLOTS_PER_PAGE);
        }

        public static List<(string, string)> GetEligibleQuests()
        {
            List<(string, string)> mods = new List<(string, string)>();
            string[] files = Directory.GetDirectories(PathMod.MODS_PATH);

            foreach (string modPath in files)
            {
                string mod = Path.GetFileNameWithoutExtension(modPath);
                if (mod != "")
                {
                    //check the config for mod type of Quest
                    ModHeader header = PathMod.GetModDetails(modPath);

                    if (header.IsValid() && header.ModType == PathMod.ModType.Quest)
                        mods.Add((header.Name, modPath));
                }
            }
            return mods;
        }


        private void choose(string dir)
        {
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToQuest(PathMod.GetModDetails(dir), PathMod.Mods);
        }
    }
}
