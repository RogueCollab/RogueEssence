using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class SpoilsMenu : MultiPageMenu
    {
        public const int SPOILS_MENU_WIDTH = 288;
        private const int SLOTS_PER_PAGE = 8;

        ItemSummary summaryMenu;
        public List<Tuple<InvItem, InvItem>> Goods;

        public SpoilsMenu(List<Tuple<InvItem, InvItem>> goods)
        {
            Goods = goods;

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < goods.Count; ii++)
            {
                int index = ii;

                MenuText boxText = new MenuText(goods[index].Item1.GetName(), new Loc(2, 1));
                MenuText itemText = new MenuText("\u2192 "+goods[index].Item2.GetName(), new Loc((SPOILS_MENU_WIDTH - 8 * 4) / 2 - 16, 1));
                flatChoices.Add(new MenuElementChoice(choose, true, boxText, itemText));
            }

            int startChoice = 0;
            int startPage = 0;
            List<MenuChoice[]> inv = SortIntoPages(flatChoices, SLOTS_PER_PAGE);


            summaryMenu = new ItemSummary(Rect.FromPoints(new Loc(16, GraphicsManager.ScreenHeight - 8 - 4 * VERT_SPACE - GraphicsManager.MenuBG.TileHeight * 2),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            int buyLimit = DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone) - DataManager.Instance.Save.ActiveTeam.GetInvCount();
            Initialize(new Loc(16, 16), SPOILS_MENU_WIDTH, Text.FormatKey("MENU_TREASURE_TITLE"), inv.ToArray(), startChoice, startPage, SLOTS_PER_PAGE);

        }

        private void choose()
        {
            MenuManager.Instance.RemoveMenu();
        }

        protected override void ChoiceChanged()
        {
            InvItem item = Goods[CurrentChoiceTotal].Item2;
            summaryMenu.SetItem(item);
            base.ChoiceChanged();
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            summaryMenu.Draw(spriteBatch);
        }
    }
}
