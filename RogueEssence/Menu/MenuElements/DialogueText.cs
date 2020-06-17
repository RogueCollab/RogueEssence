using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class DialogueText : IMenuElement
    {
        public int LineSpace;
        public string Text;
        public Loc Start;
        public int Width;
        public int CurrentCharIndex;
        public bool Center;
        public Color TextColor;
        public bool Finished { get { return CurrentCharIndex < 0 || CurrentCharIndex >= Text.Length; } }

        public DialogueText(string text, Loc start, int width, int lineSpace, bool center, bool startEmpty)
        {
            Text = text;
            Start = start;
            Width = width;
            LineSpace = lineSpace;
            Center = center;
            TextColor = Color.White;
            if (!startEmpty)
                CurrentCharIndex = -1;
        }
        public DialogueText(string text, Loc start, int width, int lineSpace, bool center) : this(text, start, width, lineSpace, center, false)
        { }

        public Loc GetTextProgress()
        {
            string[] currLines = GraphicsManager.TextFont.BreakIntoLines(Text, Width, CurrentCharIndex > -1 ? CurrentCharIndex : Text.Length);
            Loc loc = new Loc(GraphicsManager.TextFont.SubstringWidth(currLines[currLines.Length - 1]), LineSpace * (currLines.Length - 1));
            if (Center)
            {
                string[] allLines = GraphicsManager.TextFont.BreakIntoLines(Text, Width, Text.Length);
                loc -= new Loc(GraphicsManager.TextFont.SubstringWidth(allLines[currLines.Length - 1]), LineSpace * (allLines.Length - 1)) / 2;
            }
            return loc;
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            string[] lines = GraphicsManager.TextFont.BreakIntoLines(Text, Width, CurrentCharIndex > -1 ? CurrentCharIndex : Text.Length);
            if (lines != null)
            {
                if (Center)
                {
                    string[] fullLines = GraphicsManager.TextFont.BreakIntoLines(Text, Width, Text.Length);
                    int vertStart = Start.Y - LineSpace * (fullLines.Length - 1) / 2;
                    for (int ii = 0; ii < lines.Length; ii++)
                    {
                        int horizStart = Start.X - GraphicsManager.TextFont.SubstringWidth(fullLines[ii]) / 2;
                        GraphicsManager.TextFont.DrawText(spriteBatch, horizStart + offset.X, vertStart + offset.Y + LineSpace * ii, lines[ii], null,
                            DirV.Up, DirH.Left, TextColor);
                    }
                }
                else
                {
                    for (int ii = 0; ii < lines.Length; ii++)
                        GraphicsManager.TextFont.DrawText(spriteBatch, Start.X + offset.X, Start.Y + offset.Y + LineSpace * ii, lines[ii], null,
                            DirV.Up, DirH.Left, TextColor);
                }
            }
        }

        public void FinishText()
        {
            CurrentCharIndex = Text.Length;
        }
    }
}
