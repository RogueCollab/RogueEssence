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

        List<ModHeader> mods;
        bool[] modStatus;

        ModMiniSummary modSummary;

        public ModsMenu()
        {
            mods = PathMod.GetEligibleMods(PathMod.ModType.Mod);
            modStatus = new bool[mods.Count];

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for(int ii = 0; ii < mods.Count; ii++)
            {
                int index = ii;
                modStatus[ii] = false;
                foreach (ModHeader header in PathMod.Mods)
                {
                    if (header.Path == mods[ii].Path)
                    {
                        modStatus[ii] = true;
                        break;
                    }
                }

                MenuText modName = new MenuText(mods[ii].Name, new Loc(10, 1), Color.White);
                MenuText modChecked = new MenuText(modStatus[ii] ? "\uE10A" : "", new Loc(2, 1), Color.White);
                flatChoices.Add(new MenuElementChoice(() => { chooseMod(index, modChecked); }, true, modName, modChecked));
            }
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_CONTROLS_CONFIRM"), confirm));

            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            modSummary = new ModMiniSummary(Rect.FromPoints(new Loc(8,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - VERT_SPACE * 5 - 8),
                new Loc(GraphicsManager.ScreenWidth - 8, GraphicsManager.ScreenHeight - 8)));

            Initialize(new Loc(8, 8), 304, Text.FormatKey("MENU_MODS_TITLE"), choices, 0, 0, SLOTS_PER_PAGE);
        }

        protected override void ChoiceChanged()
        {
            int totalChoice = CurrentChoice + CurrentPage * SLOTS_PER_PAGE;
            if (totalChoice < mods.Count)
            {
                modSummary.Visible = true;
                modSummary.SetMod(mods[totalChoice]);
            }
            else
                modSummary.Visible = false;

            base.ChoiceChanged();
        }

        private void chooseMod(int index, MenuText checkText)
        {
            modStatus[index] = !modStatus[index];
            checkText.SetText(modStatus[index] ? "\uE10A" : "");
        }

        private void confirm()
        {
            List<ModHeader> chosenMods = new List<ModHeader>();
            for (int ii = 0; ii < modStatus.Length; ii++)
            {
                if (modStatus[ii])
                    chosenMods.Add(PathMod.GetModDetails(PathMod.FromApp(mods[ii].Path)));
            }

            List<int> loadOrder = new List<int>();
            List<(ModRelationship, List<ModHeader>)> loadErrors = new List<(ModRelationship, List<ModHeader>)>();
            PathMod.ValidateModLoad(PathMod.Quest, chosenMods.ToArray(), loadOrder, loadErrors);
            if (loadErrors.Count > 0)
            {
                MenuManager.Instance.AddMenu(new ModLogMenu(loadErrors), false);
                return;
            }

            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToQuest(PathMod.Quest, chosenMods.ToArray(), loadOrder);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            //draw other windows
            modSummary.Draw(spriteBatch);
        }
    }
}
