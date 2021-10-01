using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace RogueEssence.Menu
{
    public class DialogueText : IMenuElement
    {
        public int LineHeight;
        public string Text { get; private set; }
        public Rect Rect;
        public int CurrentCharIndex;
        public bool CenterH;
        public bool CenterV;
        private List<(int idx, Color color)> textColor;
        public float TextOpacity;
        public bool Finished { get { return CurrentCharIndex < 0 || CurrentCharIndex >= Text.Length; } }

        public DialogueText(string text, Rect rect, int lineHeight, bool centerH, bool centerV, int startIndex)
        {
            Rect = rect;
            LineHeight = lineHeight;
            CenterH = centerH;
            CenterV = centerV;
            TextOpacity = 1f;
            CurrentCharIndex = startIndex;
            textColor = new List<(int idx, Color color)>();
            SetFormattedText(text);
        }
        public DialogueText(string text, Rect rect, int lineHeight) : this(text, rect, lineHeight, false, false, -1)
        { }

        private static void formatText(List<(int idx, Color color)> colors, ref string text)
        {
            colors.Add((0, Color.White));

            List<IntRange> ranges = new List<IntRange>();
            int lag = 0;
            MatchCollection matches = RogueEssence.Text.MsgTags.Matches(text);
            foreach (Match match in matches)
            {
                foreach (string key in match.Groups.Keys)
                {
                    if (!match.Groups[key].Success)
                        continue;
                    switch (key)
                    {
                        case "pause":
                            break;
                        case "colorstart":
                            {
                                string hex = match.Groups["colorval"].Value;
                                Color color = new Color(Convert.ToInt32(hex.Substring(0, 2), 16), Convert.ToInt32(hex.Substring(2, 2), 16), Convert.ToInt32(hex.Substring(4, 2), 16));
                                colors.Add((match.Index - lag, color));
                            }
                            break;
                        case "colorend":
                            {
                                colors.Add((match.Index - lag, Color.Transparent));
                            }
                            break;
                    }
                }

                ranges.Add(new IntRange(match.Index, match.Index + match.Length));
                lag += match.Length;
            }

            for (int ii = ranges.Count - 1; ii >= 0; ii--)
                text = text.Remove(ranges[ii].Min, ranges[ii].Length);

            colors.Add((text.Length, Color.Transparent));

        }

        public void SetFormattedText(string text)
        {
            textColor.Clear();
            formatText(textColor, ref text);

            Text = text;
        }

        public Loc GetTextProgress()
        {
            string[] currLines = GraphicsManager.TextFont.BreakIntoLines(Text, Rect.Width, CurrentCharIndex > -1 ? CurrentCharIndex : Text.Length);
            Loc loc = new Loc(GraphicsManager.TextFont.SubstringWidth(currLines[currLines.Length - 1]), LineHeight * (currLines.Length - 1));
            
            string[] allLines = GraphicsManager.TextFont.BreakIntoLines(Text, Rect.Width, Text.Length);
            if (CenterH)
                loc += new Loc((Rect.Width - GraphicsManager.TextFont.SubstringWidth(allLines[currLines.Length - 1])) / 2, 0);
            if (CenterV)
                loc += new Loc(0, (Rect.Height - (GraphicsManager.TextFont.CharHeight + (allLines.Length - 1) * LineHeight)) / 2);
            return loc;
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            int endIndex = CurrentCharIndex > -1 ? CurrentCharIndex : Text.Length;
            Stack<Color> colorStack = new Stack<Color>();
            colorStack.Push(textColor[0].color);
            string[] lines = GraphicsManager.TextFont.BreakIntoLines(Text, Rect.Width, Text.Length);
            if (lines != null)
            {
                int startWidth = CenterH ? Rect.Center.X : Rect.X;
                int startHeight = CenterV ? Rect.Center.Y - (GraphicsManager.TextFont.CharHeight + (lines.Length - 1) * LineHeight) / 2 : Rect.Y;

                int curColor = 0;
                int lineChars = 0;
                for (int ii = 0; ii < lines.Length; ii++)
                {
                    int curChar = 0;
                    while (curChar < lines[ii].Length)
                    {
                        while (textColor[curColor + 1].idx - lineChars == curChar)
                        {
                            curColor++;
                            if (textColor[curColor].color == Color.Transparent && colorStack.Count > 1)
                                colorStack.Pop();
                            else
                                colorStack.Push(textColor[curColor].color);
                        }

                        int nextColorIdx = Math.Min(textColor[curColor + 1].idx - lineChars, Math.Min(lines[ii].Length, endIndex - lineChars));

                        GraphicsManager.TextFont.DrawText(spriteBatch, startWidth + offset.X, startHeight + offset.Y + LineHeight * ii,
                            lines[ii], null, DirV.Up, CenterH ? DirH.None : DirH.Left,
                            colorStack.Peek() * TextOpacity, curChar, nextColorIdx - curChar);
                        curChar = nextColorIdx;

                        if (curChar + lineChars >= endIndex)
                            break;
                    }
                    lineChars += lines[ii].Length;
                    if (lineChars >= endIndex)
                        break;
                }
            }
        }

        public void FinishText()
        {
            CurrentCharIndex = Text.Length;
        }
    }
}
