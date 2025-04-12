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

        public BankMenu(int onHand, OnChooseAmount chooseAmount) : this(MenuLabel.BANK_MENU, onHand, chooseAmount) { }
        public BankMenu(string label, int onHand, OnChooseAmount chooseAmount)
        {
            Label = label;

            this.onHand = onHand;
            this.chooseAmount = chooseAmount;
            long total = (long)DataManager.Instance.Save.ActiveTeam.Bank + (long)onHand;
            int maxVal = (int)Math.Min(Int32.MaxValue, total);
            int maxLength = maxVal.ToString().Length;
            int height = 128;
            int minSize = (Math.Max(MenuDigits.DIGIT_SPACE * maxLength, GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_BANK_TITLE"))) / 16 + 1) * 16;
            int width = minSize + GraphicsManager.MenuBG.TileWidth * 2;

            MenuDigits digits = new MenuDigits(onHand, maxLength, new Loc(width / 2 - MenuDigits.DIGIT_SPACE * maxLength / 2,
                TitledStripMenu.TITLE_OFFSET + 8 + GraphicsManager.MenuBG.TileHeight));
            
            MenuText notes = new MenuText(Text.FormatKey("MENU_BANK_TITLE"), new Loc(width / 2,
                TitledStripMenu.TITLE_OFFSET + LINE_HEIGHT * 3 + GraphicsManager.MenuBG.TileHeight), DirH.None);

            Initialize(new Rect(new Loc(GraphicsManager.ScreenWidth / 2 - width / 2, height),
                new Loc(width, TitledStripMenu.TITLE_OFFSET + GraphicsManager.MenuBG.TileHeight * 2 + LINE_HEIGHT * 5)),
                digits, Math.Max(0, (int)(total - Int32.MaxValue)), maxVal, maxLength - onHand.ToString().Length);
            
            MenuText moneyTitle = new MenuText(Text.FormatKey("MENU_BANK_MONEY"), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            NonChoices.Add(moneyTitle);
            NonChoices.Add(new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2));
            NonChoices.Add(notes);

            bankMenu = new SummaryMenu(new Rect(new Loc(GraphicsManager.ScreenWidth / 2 - minSize / 2, 24),
                new Loc(minSize, TitledStripMenu.TITLE_OFFSET + GraphicsManager.MenuBG.TileHeight * 2 + LINE_HEIGHT * 2)));
            BankDigits = new MenuDigits(DataManager.Instance.Save.ActiveTeam.Bank, maxLength, new Loc(bankMenu.Bounds.Width / 2 - MenuDigits.DIGIT_SPACE * maxLength / 2,
                TitledStripMenu.TITLE_OFFSET + 8 + GraphicsManager.MenuBG.TileHeight));
            bankMenu.Elements.Add(BankDigits);

            MenuText bankTitle = new MenuText(Text.FormatKey("MENU_BANK_BANK"), new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            bankMenu.Elements.Add(bankTitle);
            bankMenu.Elements.Add(new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), bankMenu.Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2));
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
