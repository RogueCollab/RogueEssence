using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using System.IO;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Xml;
using System;

namespace RogueEssence.Menu
{
    public class ModsMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 7;

        string[] modDirs;
        bool[] modStatus;

        public ModsMenu()
        {
            List<(string name, string dir)> mods = GetEligibleMods();
            modDirs = new string[mods.Count];
            modStatus = new bool[mods.Count];

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for(int ii = 0; ii < mods.Count; ii++)
            {
                int index = ii;
                modDirs[ii] = mods[ii].dir;
                modStatus[ii] = false;
                foreach (ModHeader header in PathMod.Mods)
                {
                    if (header.Path == modDirs[ii])
                    {
                        modStatus[ii] = true;
                        break;
                    }
                }

                MenuText modName = new MenuText(mods[ii].name, new Loc(10, 1), Color.White);
                MenuText modChecked = new MenuText(modStatus[ii] ? "\uE10A" : "", new Loc(2, 1), Color.White);
                flatChoices.Add(new MenuElementChoice(() => { chooseMod(index, modChecked); }, true, modName, modChecked));
            }
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_CONTROLS_CONFIRM"), confirm));

            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            Initialize(new Loc(8, 16), 304, Text.FormatKey("MENU_MODS_TITLE"), choices, 0, 0, SLOTS_PER_PAGE);
        }

        public static List<(string, string)> GetEligibleMods()
        {
            List<(string, string)> mods = new List<(string, string)>();
            string[] files = Directory.GetDirectories(PathMod.MODS_PATH);

            foreach (string modPath in files)
            {
                string mod = Path.GetFileNameWithoutExtension(modPath);
                if (mod != "")
                {
                    //check the config for mod type of Mod
                    ModHeader header = PathMod.GetModDetails(modPath);
                    
                    if (header.IsValid() && header.ModType == PathMod.ModType.Mod)
                        mods.Add((header.Name, Path.Join(PathMod.MODS_FOLDER, mod)));
                }
            }
            return mods;
        }

        private void chooseMod(int index, MenuText checkText)
        {
            modStatus[index] = !modStatus[index];
            checkText.SetText(modStatus[index] ? "\uE10A" : "");
        }

        private void confirm()
        {
            MenuManager.Instance.ClearMenus();
            List<ModHeader> chosenMods = new List<ModHeader>();
            for (int ii = 0; ii < modStatus.Length; ii++)
            {
                if (modStatus[ii])
                    chosenMods.Add(PathMod.GetModDetails(modDirs[ii]));
            }
            GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToQuest(PathMod.Quest, chosenMods.ToArray());
        }
    }
}
