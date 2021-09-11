using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using System.IO;

namespace RogueEssence.Menu
{
    public class ModsMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 7;

        public ModsMenu()
        {
            List<string> mods = GetEligibleMods();

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            foreach (string mod in mods)
                flatChoices.Add(new MenuTextChoice(mod, () => { choose(Path.Join(PathMod.MODS_FOLDER, mod)); }));

            List<MenuChoice[]> choices = SortIntoPages(flatChoices, SLOTS_PER_PAGE);

            Initialize(new Loc(8, 16), 304, Text.FormatKey("MENU_MODS_TITLE"), choices.ToArray(), 0, 0, SLOTS_PER_PAGE);
        }

        public static List<string> GetEligibleMods()
        {
            List<string> mods = new List<string>();
            string[] files = Directory.GetDirectories(PathMod.MODS_PATH);

            foreach (string modPath in files)
            {
                string mod = Path.GetFileNameWithoutExtension(modPath);
                if (mod != "")
                    mods.Add(mod);
            }
            return mods;
        }


        private void choose(string dir)
        {
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.SetMod(dir, true);
        }
    }
}
