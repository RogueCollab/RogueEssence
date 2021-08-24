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
            string[] files = Directory.GetDirectories(PathMod.MODS_PATH);

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            foreach (string modPath in files)
            {
                string mod = Path.GetFileNameWithoutExtension(modPath);
                flatChoices.Add(new MenuTextChoice(mod, () => { choose(Path.Join(PathMod.MODS_FOLDER, mod)); }));
            }
            List<MenuChoice[]> choices = SortIntoPages(flatChoices, SLOTS_PER_PAGE);

            Initialize(new Loc(8, 16), 304, Text.FormatKey("MENU_MODS_TITLE"), choices.ToArray(), 0, 0, SLOTS_PER_PAGE);
        }


        private void choose(string dir)
        {
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.SetMod(dir, true);
        }
    }
}
