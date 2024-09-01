using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class MenuStatBar : BaseMenuElement
    {
        public int Length { get; set; }
        public Color Color { get; set; }
        public Loc Loc { get; set; }
        public bool Shadow { get; set; }

        public MenuStatBar(Loc loc, int length, Color color) : this("", loc, length, color, true) { }
        public MenuStatBar(string label, Loc loc, int length, Color color) : this(label, loc, length, color, true) { }
        public MenuStatBar(Loc loc, int length, Color color, bool shadow) : this("", loc, length, color, shadow) { }
        public MenuStatBar(string label, Loc loc, int length, Color color, bool shadow)
        {
            Label = label;
            Length = length;
            Loc = loc;
            Color = color;
            Shadow = shadow;
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            if (Shadow)
                GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(offset.X + Loc.X + 1, offset.Y + Loc.Y + 1, Length, 8), null, Color.Black);
            GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(offset.X + Loc.X, offset.Y + Loc.Y, Length, 8), null, Color);
        }
    }
}
