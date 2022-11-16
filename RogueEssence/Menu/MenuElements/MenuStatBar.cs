using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class MenuStatBar : IMenuElement
    {
        public int Length { get; set; }
        public Color Color { get; set; }
        public Loc Loc { get; set; }

        public MenuStatBar(Loc loc, int length, Color color)
        {
            Length = length;
            Loc = loc;
            Color = color;
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(offset.X + Loc.X + 1, offset.Y + Loc.Y + 1, Length, 8), null, Color.Black);
            GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(offset.X + Loc.X, offset.Y + Loc.Y, Length, 8), null, Color);
        }
    }
}
