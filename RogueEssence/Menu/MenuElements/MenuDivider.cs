using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class MenuDivider : IMenuElement
    {
        public string Label { get; set; }
        public int Length { get; set; }
        public Loc Loc { get; set; }

        public MenuDivider(Loc loc, int length) : this("", loc, length) { }
        public MenuDivider(string label, Loc loc, int length)
        {
            Length = length;
            Loc = loc;
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            GraphicsManager.DivTex.Draw(spriteBatch, new Rectangle(Loc.X + offset.X, Loc.Y + offset.Y, Length, 2), null, Color.White);
        }
    }
}
