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
        public Rect Rect;
        public int CurrentCharIndex;
        public bool CenterH;
        public bool CenterV;
        public float TextOpacity;
        public bool Finished { get { return CurrentCharIndex < 0 || CurrentCharIndex >= formattedTextLength; } }

        /// <summary>
        /// Text color starts and stops.
        /// </summary>
        private List<(int idx, Color color)> textColor;

        /// <summary>
        /// The amount of space to trim when the lines were broken up.
        /// </summary>
        private List<int> trimmedStarts;

        /// <summary>
        /// Formatted lines after breaking the text down and trimming the spaces at the edges.
        /// </summary>
        private string[] fullLines;

        /// <summary>
        /// The sum of the length of all lines in fullLines.  Saves computation.
        /// </summary>
        private int formattedTextLength;

        public DialogueText(string text, Rect rect, int lineHeight, bool centerH, bool centerV, int startIndex)
        {
            Rect = rect;
            LineHeight = lineHeight;
            CenterH = centerH;
            CenterV = centerV;
            TextOpacity = 1f;
            CurrentCharIndex = startIndex;
            textColor = new List<(int idx, Color color)>();
            SetAndFormatText(text);
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

        /// <summary>
        /// Splits the text into multiple textboxes worth of text, or just one if it can fit in the textbox.
        /// </summary>
        /// <param name="text"></param>
        public static List<DialogueText> SplitFormattedText(string text, Rect rect, int lineHeight, bool centerH, bool centerV, int startIndex)
        {
            List<(int idx, Color color)> tempColor = new List<(int idx, Color color)>();
            string tempText = text;
            formatText(tempColor, ref tempText);

            List<int> trimmedStarts;
            string[] tempLines = GraphicsManager.TextFont.BreakIntoLines(tempText, rect.Width, tempText.Length, out trimmedStarts);

            List<DialogueText> results = new List<DialogueText>();
            int cutIdx = 0;
            int endColorIndex = 0;
            List<Color> colorStack = new List<Color>();
            for (int ii = 0; ii < tempLines.Length + 1; ii++)
            {
                if (ii == tempLines.Length || GraphicsManager.TextFont.CharHeight + (ii - cutIdx) * lineHeight > rect.Height)
                {
                    string[] initLines = new string[ii - cutIdx];
                    Array.Copy(tempLines, cutIdx, initLines, 0, initLines.Length);

                    List<int> initTrim = new List<int>();
                    for (int nn = cutIdx; nn < ii; nn++)
                        initTrim.Add(trimmedStarts[nn]);

                    List<(int idx, Color color)> initColor = new List<(int idx, Color color)>();
                    int startColorIndex = endColorIndex;
                    for (int nn = 0; nn < initLines.Length; nn++)
                        endColorIndex += initTrim[nn] + initLines[nn].Length;
                    //pickup the tags from before
                    for(int nn = 0; nn < colorStack.Count; nn++)
                        initColor.Add((0, colorStack[nn]));
                    //copy eligible colors for this dialogue box
                    for (int nn = 0; nn < tempColor.Count; nn++)
                    {
                        if (startColorIndex <= tempColor[nn].idx && tempColor[nn].idx < endColorIndex)
                        {
                            initColor.Add((tempColor[nn].idx - startColorIndex, tempColor[nn].color));
                            if (tempColor[nn].color == Color.Transparent)
                                colorStack.RemoveAt(colorStack.Count - 1);
                            else
                                colorStack.Add(tempColor[nn].color);
                        }
                    }
                    //close the tags cleanly
                    for (int nn = colorStack.Count - 1; nn >= 0; nn--)
                        initColor.Add((endColorIndex- startColorIndex, Color.Transparent));

                    DialogueText newBox = new DialogueText("", rect, lineHeight, centerH, centerV, startIndex);
                    newBox.textColor = initColor;
                    newBox.fullLines = initLines;
                    newBox.trimmedStarts = initTrim;
                    newBox.updateFullTextLength();

                    results.Add(newBox);
                    cutIdx = ii;
                }
            }

            return results;
        }

        public void SetPreFormattedText(string text, List<(int idx, Color color)> color)
        {
            textColor = color;

            List<int> trimmedStarts;
            fullLines = GraphicsManager.TextFont.BreakIntoLines(text, Rect.Width, text.Length, out trimmedStarts);
            this.trimmedStarts = trimmedStarts;

            updateFullTextLength();
        }

        private void updateFullTextLength()
        {
            formattedTextLength = 0;
            if (fullLines != null)
            {
                foreach (string line in fullLines)
                    formattedTextLength += line.Length;
            }
        }

        /// <summary>
        /// Sets the text and formats/spaces it properly.
        /// </summary>
        /// <param name="text"></param>
        public void SetAndFormatText(string text)
        {
            textColor.Clear();
            formatText(textColor, ref text);

            SetPreFormattedText(text, textColor);
        }

        public Loc GetTextProgress()
        {
            int curLineIndex = 0;
            int curLineProgress = 0;
            if (CurrentCharIndex == -1)
            {
                curLineIndex = fullLines.Length - 1;
                curLineProgress = fullLines[fullLines.Length - 1].Length;
            }
            else
            {
                int curLineStart = 0;
                foreach (string line in fullLines)
                {
                    if (CurrentCharIndex == -1 || curLineStart + line.Length < CurrentCharIndex)
                    {
                        curLineStart += line.Length;
                        curLineIndex++;
                    }
                    else
                        break;
                }
                curLineProgress = CurrentCharIndex - curLineStart;
            }

            string substr = fullLines[curLineIndex].Substring(0, curLineProgress);
            Loc loc = new Loc(GraphicsManager.TextFont.SubstringWidth(substr), LineHeight * curLineIndex);
            
            if (CenterH)
                loc += new Loc((Rect.Width - GraphicsManager.TextFont.SubstringWidth(fullLines[curLineIndex])) / 2, 0);
            if (CenterV)
                loc += new Loc(0, (Rect.Height - (GraphicsManager.TextFont.CharHeight + (fullLines.Length - 1) * LineHeight)) / 2);
            return loc;
        }

        public Loc GetTextSize()
        {
            int maxWidth = 0;
            foreach (string line in fullLines)
                maxWidth = Math.Max(maxWidth, GraphicsManager.TextFont.SubstringWidth(line));

            return new Loc(maxWidth, GraphicsManager.TextFont.CharHeight + (fullLines.Length - 1) * LineHeight);
        }

        public int GetLineLength(int idx)
        {
            return fullLines[idx].Length;
        }

        /// <summary>
        /// When a text is broken up into lines, some trimming occurs at the beginning.  This obtains that value so callers can keep in sync with the character position mappings.
        /// </summary>
        /// <returns></returns>
        public int GetLineTrim(int idx)
        {
            return trimmedStarts[idx];
        }

        public int GetLineCount()
        {
            return fullLines.Length;
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            int endIndex = CurrentCharIndex > -1 ? CurrentCharIndex : formattedTextLength;
            Stack<Color> colorStack = new Stack<Color>();
            colorStack.Push(textColor[0].color);

            if (fullLines != null)
            {
                int startWidth = CenterH ? Rect.Center.X : Rect.X;
                int startHeight = CenterV ? Rect.Center.Y - (GraphicsManager.TextFont.CharHeight + (fullLines.Length - 1) * LineHeight) / 2 : Rect.Y;

                int curColor = 0;
                int lineChars = 0;
                //offset to move the color tags by, since cutting string into lines removes extra spaces
                int totalTrimmedOffset = 0;
                for (int ii = 0; ii < fullLines.Length; ii++)
                {
                    int curChar = 0;
                    totalTrimmedOffset += trimmedStarts[ii];
                    while (curChar < fullLines[ii].Length)
                    {
                        while ((textColor[curColor + 1].idx - totalTrimmedOffset) - lineChars == curChar)
                        {
                            curColor++;
                            if (textColor[curColor].color == Color.Transparent && colorStack.Count > 1)
                                colorStack.Pop();
                            else
                                colorStack.Push(textColor[curColor].color);
                        }

                        int nextColorIdx = Math.Min((textColor[curColor + 1].idx - totalTrimmedOffset) - lineChars, Math.Min(fullLines[ii].Length, endIndex - lineChars));

                        GraphicsManager.TextFont.DrawText(spriteBatch, startWidth + offset.X, startHeight + offset.Y + LineHeight * ii,
                            fullLines[ii], null, DirV.Up, CenterH ? DirH.None : DirH.Left,
                            colorStack.Peek() * TextOpacity, curChar, nextColorIdx - curChar);
                        curChar = nextColorIdx;

                        if (curChar + lineChars >= endIndex)
                            break;
                    }
                    lineChars += fullLines[ii].Length;
                    if (lineChars >= endIndex)
                        break;
                }
            }
        }

        public void FinishText()
        {
            CurrentCharIndex = formattedTextLength;
        }
    }
}
