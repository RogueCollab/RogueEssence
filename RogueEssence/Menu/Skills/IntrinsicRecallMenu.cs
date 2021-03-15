using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class IntrinsicRecallMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 5;

        int[] intrinsicChoices;
        OnChooseSlot chooseSlotAction;
        Action refuseAction;
        Character player;

        SummaryMenu summaryMenu;
        DialogueText Description;


        public IntrinsicRecallMenu(Character player, int[] intrinsicChoices, OnChooseSlot action, Action refuseAction)
        {
            this.player = player;
            this.intrinsicChoices = intrinsicChoices;
            this.chooseSlotAction = action;
            this.refuseAction = refuseAction;


            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < intrinsicChoices.Length; ii++)
            {
                int index = ii;
                flatChoices.Add(new MenuTextChoice(Data.DataManager.Instance.GetIntrinsic(intrinsicChoices[index]).Name.ToLocal(), () => { choose(index); }));
            }
            List<MenuChoice[]> intrinsics = SortIntoPages(flatChoices, SLOTS_PER_PAGE);
            
            summaryMenu = new SummaryMenu(Rect.FromPoints(new Loc(16,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - LINE_SPACE * 3),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            Description = new DialogueText("", summaryMenu.Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight),
                summaryMenu.Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 4 - summaryMenu.Bounds.X, LINE_SPACE, false);
            summaryMenu.Elements.Add(Description);

            Initialize(new Loc(16, 16), 144, Text.FormatKey("MENU_INTRINSIC_RECALL"), intrinsics.ToArray(), 0, 0, SLOTS_PER_PAGE);
        }

        protected override void MenuPressed()
        {

        }

        protected override void Canceled()
        {
            MenuManager.Instance.RemoveMenu();
            refuseAction();
        }

        private void choose(int choice)
        {
            MenuManager.Instance.RemoveMenu();

            chooseSlotAction(intrinsicChoices[choice]);
        }

        protected override void ChoiceChanged()
        {
            Data.IntrinsicData entry = Data.DataManager.Instance.GetIntrinsic(intrinsicChoices[CurrentChoiceTotal]);
            Description.Text = entry.Desc.ToLocal();

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
