using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace RogueEssence.Menu
{
    public class MenuText : IMenuElement
    {
        public string Label { get; set; }
        public string Text { get; private set; }
        public Color Color;
        public DirV AlignV;
        public DirH AlignH;
        public Loc Loc;
        private List<(int idx, Color color)> textColor;

        public MenuText(string text, Loc loc)
            : this("", text, loc, DirH.Left)
        { }
        public MenuText(string label, string text, Loc loc)
            : this(label, text, loc, DirH.Left)
        { }

        public MenuText(string text, Loc loc, Color color)
            : this("", text, loc, DirV.Up, DirH.Left, color)
        { }
        public MenuText(string label, string text, Loc loc, Color color)
            : this(label, text, loc, DirV.Up, DirH.Left, color)
        { }

        public MenuText(string text, Loc loc, DirH align)
            : this("", text, loc, DirV.Up, align, Color.White)
        { }

        public MenuText(string label, string text, Loc loc, DirH align)
            : this(label, text, loc, DirV.Up, align, Color.White)
        { }

        public MenuText(string text, Loc loc, DirV alignV, DirH alignH, Color color)
            : this("", text, loc, alignV, alignH, color)
        { }
        public MenuText(string label, string text, Loc loc, DirV alignV, DirH alignH, Color color)
        {
            Label = label;
            Loc = loc;
            AlignV = alignV;
            AlignH = alignH;
            Color = color;
            textColor = new List<(int idx, Color color)>();
            SetText(text);
        }

        public static string[] BreakIntoLines(string text, int width)
        {
            List<(int idx, string color)> textColor = new List<(int idx, string color)>();
            textColor.Add((0, "FFFFFF"));

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
                                textColor.Add((match.Index - lag, hex));
                            }
                            break;
                        case "colorend":
                            {
                                textColor.Add((match.Index - lag, ""));
                            }
                            break;
                    }
                }

                ranges.Add(new IntRange(match.Index, match.Index + match.Length));
                lag += match.Length;
            }

            for (int ii = ranges.Count - 1; ii >= 0; ii--)
                text = text.Remove(ranges[ii].Min, ranges[ii].Length);

            textColor.Add((text.Length, ""));

            string[] lines = GraphicsManager.TextFont.BreakIntoLines(text, width);


            List<string> colorStack = new List<string>();

            int curColor = 0;
            int lineChars = 0;
            for (int ii = 0; ii < lines.Length; ii++)
            {
                string workingLine = "";
                for (int jj = 0; jj < colorStack.Count; jj++)
                    workingLine += String.Format("[color=#{0}]", colorStack[jj]);

                int curChar = 0;
                while (curChar < lines[ii].Length)
                {
                    while (textColor[curColor + 1].idx - lineChars == curChar)
                    {
                        curColor++;
                        if (textColor[curColor].color == "" && colorStack.Count > 0)
                        {
                            colorStack.RemoveAt(colorStack.Count - 1);
                            workingLine += "[color]";
                        }
                        else
                        {
                            colorStack.Add(textColor[curColor].color);
                            workingLine += String.Format("[color=#{0}]", textColor[curColor].color);
                        }
                    }
                    int nextColorIdx = Math.Min(textColor[curColor + 1].idx - lineChars, lines[ii].Length);

                    workingLine += lines[ii].Substring(curChar, nextColorIdx - curChar);
                    curChar = nextColorIdx;
                }
                lineChars += lines[ii].Length;

                for (int jj = 0; jj < colorStack.Count; jj++)
                    workingLine += "[color]";
                lines[ii] = workingLine;
            }

            return lines;
        }

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

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            Stack<Color> colorStack = new Stack<Color>();
            colorStack.Push(textColor[0].color);
            int curColor = 0;
            int curChar = 0;
            while (curChar < Text.Length)
            {
                while (textColor[curColor + 1].idx == curChar)
                {
                    curColor++;
                    if (textColor[curColor].color == Color.Transparent && colorStack.Count > 1)
                        colorStack.Pop();
                    else
                        colorStack.Push(textColor[curColor].color);
                }

                int nextColorIdx = Math.Min(textColor[curColor + 1].idx, Text.Length);

                GraphicsManager.TextFont.DrawText(spriteBatch, Loc.X + offset.X, Loc.Y + offset.Y,
                    Text, null, AlignV, AlignH,
                    Color == Color.White ? colorStack.Peek() : Color, curChar, nextColorIdx - curChar);
                curChar = nextColorIdx;
            }
        }

        public int GetTextLength()
        {
            return GraphicsManager.TextFont.SubstringWidth(Text);
        }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Text))
                return "";
            return Text;
        }
    }
}
