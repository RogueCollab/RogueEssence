using RogueElements;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Data;
using System;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class BankMenu : ChooseAmountMenu
    {
        public delegate void OnChooseAmount(int amount);

        OnChooseAmount chooseAmount;

        SummaryMenu bankMenu;
        MenuDigits BankDigits;
        int onHand;

        public BankMenu(int onHand, OnChooseAmount chooseAmount)
        {
            this.onHand = onHand;
            this.chooseAmount = chooseAmount;
            long total = (long)DataManager.Instance.Save.ActiveTeam.Bank + (long)onHand;
            int maxVal = (int)Math.Min(Int32.MaxValue, total);
            int maxLength = maxVal.ToString().Length;

            MenuDigits digits = new MenuDigits(onHand, maxLength, new Loc(GraphicsManager.ScreenWidth / 2 - MenuDigits.DIGIT_SPACE * maxLength / 2,
                128 + TitledStripMenu.TITLE_OFFSET + 8 + GraphicsManager.MenuBG.TileHeight));
            
            MenuText notes = new MenuText(Text.FormatKey("MENU_BANK_TITLE"), new Loc(GraphicsManager.ScreenWidth / 2,
                128 + TitledStripMenu.TITLE_OFFSET + LINE_HEIGHT * 3 + GraphicsManager.MenuBG.TileHeight), DirH.None);

            int minSize = (Math.Max(MenuDigits.DIGIT_SPACE * maxLength, notes.GetTextLength()) / 16 + 1) * 16;

            Initialize(new Rect(new Loc(GraphicsManager.ScreenWidth / 2 - (minSize + 16) / 2, 128),
                new Loc(minSize + 16, TitledStripMenu.TITLE_OFFSET + GraphicsManager.MenuBG.TileHeight * 2 + LINE_HEIGHT * 5)),
                digits, Math.Max(0, (int)(total - Int32.MaxValue)), maxVal, maxLength - onHand.ToString().Length);
            
            MenuText moneyTitle = new MenuText(Text.FormatKey("MENU_BANK_MONEY"), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            NonChoices.Add(moneyTitle);
            NonChoices.Add(new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2));
            NonChoices.Add(notes);

            bankMenu = new SummaryMenu(new Rect(new Loc(GraphicsManager.ScreenWidth / 2 - minSize / 2, 24),
                new Loc(minSize, TitledStripMenu.TITLE_OFFSET + GraphicsManager.MenuBG.TileHeight * 2 + LINE_HEIGHT * 2)));
            BankDigits = new MenuDigits(DataManager.Instance.Save.ActiveTeam.Bank, maxLength, new Loc(GraphicsManager.ScreenWidth / 2 - MenuDigits.DIGIT_SPACE * maxLength / 2,
                24 + TitledStripMenu.TITLE_OFFSET + 8 + GraphicsManager.MenuBG.TileHeight));
            bankMenu.Elements.Add(BankDigits);

            MenuText bankTitle = new MenuText(Text.FormatKey("MENU_BANK_BANK"), bankMenu.Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            bankMenu.Elements.Add(bankTitle);
            bankMenu.Elements.Add(new MenuDivider(bankMenu.Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), bankMenu.Bounds.End.X - bankMenu.Bounds.X - GraphicsManager.MenuBG.TileWidth * 2));
        }

        protected override void ValueChanged()
        {
            long total = (long)DataManager.Instance.Save.ActiveTeam.Bank + onHand;
            //update summary
            BankDigits.Amount = (int)(total - Digits.Amount);
        }

        protected override void Confirmed()
        {
            chooseAmount(Digits.Amount);
            if (onHand != Digits.Amount)
                GameManager.Instance.SE(GraphicsManager.MoneySE);
            MenuManager.Instance.RemoveMenu();
        }


        protected override void MenuPressed()
        {
            Canceled();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            bankMenu.Draw(spriteBatch);
        }
    }
}
