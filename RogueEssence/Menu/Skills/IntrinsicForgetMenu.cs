using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class IntrinsicForgetMenu : TitledStripMenu
    {
        OnChooseSlot chooseSlotAction;
        Action refuseAction;
        Character player;

        SummaryMenu summaryMenu;
        DialogueText Description;

        public IntrinsicForgetMenu(Character player, OnChooseSlot action, Action refuseAction)
        {
            this.player = player;
            this.chooseSlotAction = action;
            this.refuseAction = refuseAction;

            List<MenuTextChoice> intrinsics = new List<MenuTextChoice>();
            for (int ii = 0; ii < CharData.MAX_INTRINSIC_SLOTS; ii++)
            {
                if (player.BaseIntrinsics[ii] > -1)
                    intrinsics.Add(new MenuTextChoice(Data.DataManager.Instance.GetIntrinsic(player.BaseIntrinsics[ii]).GetColoredName(), () => { choose(ii); }));
            }
            
            summaryMenu = new SummaryMenu(Rect.FromPoints(new Loc(16,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - LINE_SPACE * 3),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            Description = new DialogueText("", summaryMenu.Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight),
                summaryMenu.Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 4 - summaryMenu.Bounds.X, LINE_SPACE);
            summaryMenu.Elements.Add(Description);

            Initialize(new Loc(16, 16), 144, Text.FormatKey("MENU_INTRINSIC_TITLE", player.GetDisplayName(true)), intrinsics.ToArray(), 0);
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
            Data.IntrinsicData entry = Data.DataManager.Instance.GetIntrinsic(player.BaseIntrinsics[CurrentChoice]);
            Description.SetText(entry.Desc.ToLocal());

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
