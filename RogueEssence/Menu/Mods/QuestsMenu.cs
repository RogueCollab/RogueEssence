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

        List<ModHeader> quests;
        ModMiniSummary modSummary;

        public QuestsMenu()
        {
            quests = PathMod.GetEligibleMods(PathMod.ModType.Quest);

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            foreach (ModHeader quest in quests)
                flatChoices.Add(new MenuTextChoice(quest.Name, () => { choose(quest.Path); }));

            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            modSummary = new ModMiniSummary(Rect.FromPoints(new Loc(8,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - VERT_SPACE * 5 - 8),
                new Loc(GraphicsManager.ScreenWidth - 8, GraphicsManager.ScreenHeight - 8)));

            Initialize(new Loc(8, 8), 304, Text.FormatKey("MENU_QUESTS_TITLE"), choices, 0, 0, SLOTS_PER_PAGE);
        }


        protected override void ChoiceChanged()
        {
            int totalChoice = CurrentChoice + CurrentPage * SLOTS_PER_PAGE;
            if (totalChoice < quests.Count)
            {
                modSummary.Visible = true;
                modSummary.SetMod(quests[totalChoice]);
            }
            else
                modSummary.Visible = false;

            base.ChoiceChanged();
        }

        private void choose(string dir)
        {
            List<int> loadOrder = new List<int>();
            List<(ModRelationship, List<ModHeader>)> loadErrors = new List<(ModRelationship, List<ModHeader>)>();
            PathMod.ValidateModLoad(PathMod.GetModDetails(PathMod.FromApp(dir)), PathMod.Mods, loadOrder, loadErrors);
            if (loadErrors.Count > 0)
            {
                MenuManager.Instance.AddMenu(new ModLogMenu(loadErrors), false);
                return;
            }

            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToQuest(PathMod.GetModDetails(PathMod.FromApp(dir)), PathMod.Mods, loadOrder);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            //draw other windows
            modSummary.Draw(spriteBatch);
        }
    }
}
