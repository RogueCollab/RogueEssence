using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class MenuDigits : IMenuElement
    {
        public const int DIGIT_SPACE = 9;

        public int Amount;
        public int MinDigits;
        public Color Color;
        public Loc Loc;

        public MenuDigits(int digits, int minDigits, Loc loc)
            : this(digits, minDigits, loc, Color.White)
        { }

        public MenuDigits(int digits, int minDigits, Loc loc, Color color)
        {
            Amount = digits;
            MinDigits = minDigits;
            Loc = loc;
            Color = color;
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            string text = Amount.ToString("D"+MinDigits);

            //Draw positions
            int dX = Loc.X + offset.X;
            int dY = Loc.Y + offset.Y;
            
            for(int ii = 0; ii < text.Length; ii++)
                GraphicsManager.TextFont.DrawText(spriteBatch, dX + ii * DIGIT_SPACE, dY, text[ii].ToString(), null, DirV.Up, DirH.Left, Color);
        }

        public int GetDigitLength()
        {
            return Amount.ToString("D" + MinDigits).Length;
        }

        public int GetTextLength()
        {
            return DIGIT_SPACE * GetDigitLength();
        }

        public Loc GetDigitLoc(int digit)
        {
            string text = Amount.ToString("D"+MinDigits);

            //Draw positions
            int dX = Loc.X;
            int dY = Loc.Y;

            return new Loc(dX + digit * DIGIT_SPACE, dY);
        }

    }
}
