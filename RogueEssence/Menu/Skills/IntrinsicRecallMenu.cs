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

        string[] intrinsicChoices;
        OnChooseSlot chooseSlotAction;
        Action refuseAction;
        Character player;

        SummaryMenu summaryMenu;
        DialogueText Description;


        public IntrinsicRecallMenu(Character player, string[] intrinsicChoices, OnChooseSlot action, Action refuseAction) :
            this(MenuLabel.INTRINSIC_RECALL_MENU, player, intrinsicChoices, action, refuseAction) { }
        public IntrinsicRecallMenu(string label, Character player, string[] intrinsicChoices, OnChooseSlot action, Action refuseAction)
        {
            Label = label;
            this.player = player;
            this.intrinsicChoices = intrinsicChoices;
            this.chooseSlotAction = action;
            this.refuseAction = refuseAction;


            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < intrinsicChoices.Length; ii++)
            {
                int index = ii;
                flatChoices.Add(new MenuTextChoice(Data.DataManager.Instance.GetIntrinsic(intrinsicChoices[index]).GetColoredName(), () => { choose(index); }));
            }
            IChoosable[][] intrinsics = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            summaryMenu = new SummaryMenu(Rect.FromPoints(new Loc(16,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - LINE_HEIGHT * 3),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            Description = new DialogueText("", new Rect(new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight),
                new Loc(summaryMenu.Bounds.Width - GraphicsManager.MenuBG.TileWidth * 4, summaryMenu.Bounds.Height - GraphicsManager.MenuBG.TileHeight * 4)), LINE_HEIGHT);
            summaryMenu.Elements.Add(Description);

            Initialize(new Loc(16, 16), 144, Text.FormatKey("MENU_INTRINSIC_RECALL"), intrinsics, 0, 0, SLOTS_PER_PAGE);
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

            chooseSlotAction(choice);
        }

        protected override void ChoiceChanged()
        {
            Data.IntrinsicData entry = Data.DataManager.Instance.GetIntrinsic(intrinsicChoices[CurrentChoiceTotal]);
            Description.SetAndFormatText(entry.Desc.ToLocal());

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
