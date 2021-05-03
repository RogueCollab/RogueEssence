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
        private static Regex tags = new Regex("(?<colorstart>\\[color=#(?<colorval>[0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f])\\])|(?<colorend>\\[color\\])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public int LineSpace;
        public string Text { get; private set; }
        public Loc Start;
        public int Width;
        public int CurrentCharIndex;
        public bool Center;
        private List<(int idx, Color color)> textColor;
        public float TextOpacity;
        public bool Finished { get { return CurrentCharIndex < 0 || CurrentCharIndex >= Text.Length; } }

        public DialogueText(string text, Loc start, int width, int lineSpace, bool center, int startIndex)
        {
            Start = start;
            Width = width;
            LineSpace = lineSpace;
            Center = center;
            TextOpacity = 1f;
            CurrentCharIndex = startIndex;
            textColor = new List<(int idx, Color color)>();
            SetText(text);
        }
        public DialogueText(string text, Loc start, int width, int lineSpace, bool center) : this(text, start, width, lineSpace, center, -1)
        { }

        public void SetText(string text)
        {
            textColor.Clear();
            textColor.Add((0, Color.White));

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
                                textColor.Add((match.Index - lag, color));
                            }
                            break;
                        case "colorend":
                            {
                                textColor.Add((match.Index - lag, Color.Transparent));
                            }
                            break;
                    }
                }

                ranges.Add(new IntRange(match.Index, match.Index + match.Length));
                lag += match.Length;
            }

            for (int ii = ranges.Count - 1; ii >= 0; ii--)
                text = text.Remove(ranges[ii].Min, ranges[ii].Length);

            textColor.Add((text.Length, Color.Transparent));


            Text = text;
        }

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
            int endIndex = CurrentCharIndex > -1 ? CurrentCharIndex : Text.Length;
            Stack<Color> colorStack = new Stack<Color>();
            colorStack.Push(textColor[0].color);
            string[] lines = GraphicsManager.TextFont.BreakIntoLines(Text, Width, Text.Length);
            if (lines != null)
            {
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

                        GraphicsManager.TextFont.DrawText(spriteBatch, Start.X + offset.X, Start.Y + offset.Y + LineSpace * ii,
                            lines[ii], null, Center ? DirV.None : DirV.Up, Center ? DirH.None : DirH.Left,
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
