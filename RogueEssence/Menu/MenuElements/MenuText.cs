using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class MenuText : IMenuElement
    {

        private string text;
        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        public Color Color;
        public DirV AlignV;
        public DirH AlignH;
        public Loc Loc;

        public MenuText(string text, Loc loc)
            : this(text, loc, DirH.Left)
        { }

        public MenuText(string text, Loc loc, Color color)
            : this(text, loc, DirV.Up, DirH.Left, color)
        { }

        public MenuText(string text, Loc loc, DirH align)
            : this(text, loc, DirV.Up, align, Color.White)
        { }

        public MenuText(string text, Loc loc, DirV alignV, DirH alignH, Color color)
        {
            Text = text;
            Loc = loc;
            AlignV = alignV;
            AlignH = alignH;
            Color = color;
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            GraphicsManager.TextFont.DrawText(spriteBatch, Loc.X + offset.X, Loc.Y + offset.Y, Text, null, AlignV, AlignH, Color);
        }

        public int GetTextLength()
        {
            return GraphicsManager.TextFont.SubstringWidth(Text);
        }

    }
}
