using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueElements;
using RogueEssence.Content;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static System.Reflection.Metadata.BlobBuilder;

namespace RogueEssence.Menu
{
    public class DialogueText : BaseMenuElement
    {
        public int LineHeight;
        public Rect Rect;
        public int CurrentCharIndex;
        public bool CenterH;
        public bool CenterV;
        public float TextOpacity;
        public Color Color;
        public bool Finished { get { return CurrentCharIndex < 0 || CurrentCharIndex >= formattedTextLength; } }

        /// <summary>
        /// Text color starts and stops.
        /// </summary>
        private List<(int idx, Color color)> textColor;

        /// <summary>
        /// Text boldface starts and stops.
        /// </summary>
        private List<(int idx, bool isBold)> textBoldface;

        /// <summary>
        /// List containing individual lines' boldface values.
        /// </summary>
        private List<List<(int idx, bool isBold)>> individualLineBoldfaces;

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

        public DialogueText(string label, string text, Rect rect, int lineHeight, bool centerH, bool centerV, int startIndex, Color color)
        {
            Label = label;
            Rect = rect;
            LineHeight = lineHeight;
            CenterH = centerH;
            CenterV = centerV;
            TextOpacity = 1f;
            CurrentCharIndex = startIndex;
            Color = color;
            textColor = new List<(int idx, Color color)>();
            textBoldface = new List<(int idx, bool isBold)>();
            individualLineBoldfaces = new List<List<(int idx, bool isBold)>>();
            SetAndFormatText(text);
        }
        public DialogueText(string text, Rect rect, int lineHeight, bool centerH, bool centerV, int startIndex, Color color)
            : this("", text, rect, lineHeight, centerH, centerV, startIndex, color)
        { }

        public DialogueText(string label, string text, Rect rect, int lineHeight, bool centerH, bool centerV, int startIndex)
            : this(label, text, rect, lineHeight, centerH, centerV, startIndex, Color.White)
        {}

        public DialogueText(string text, Rect rect, int lineHeight, bool centerH, bool centerV, int startIndex)
            : this("", text, rect, lineHeight, centerH, centerV, startIndex, Color.White)
        { }

        public DialogueText(string label, string text, Rect rect, int lineHeight)
            : this(label, text, rect, lineHeight, false, false, -1, Color.White)
        { }
        public DialogueText(string text, Rect rect, int lineHeight)
            : this("", text, rect, lineHeight, false, false, -1, Color.White)
        { }


        private static void formatText(List<(int idx, Color color)> colors, List<(int idx, bool isBold)> bolds, ref string text)
        {
            colors.Add((0, Color.White));
            bolds.Add((0, false));

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

                        case "boldstart":
                            {
                                bolds.Add((match.Index - lag, true));
                            }
                            break;

                        case "boldend":
                            {
                                bolds.Add((match.Index - lag, false));
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
            bolds.Add((text.Length, false));

            /*
            for (int i = 0; i < colors.Count; i++)
            {
                Console.WriteLine(colors[i].idx);
                Console.WriteLine(colors[i].color);
            }
            for (int i = 0; i < bolds.Count; i++)
            {
                Console.WriteLine(bolds[i].idx);
                Console.WriteLine(bolds[i].isBold);
            }
            */
        }

        /// <summary>
        /// Splits the text into multiple textboxes worth of text, or just one if it can fit in the textbox.
        /// </summary>
        /// <param name="text"></param>
        public static List<DialogueText> SplitFormattedText(string text, Rect rect, int lineHeight, bool centerH, bool centerV, int startIndex)
        {
            List<(int idx, Color color)> tempColor = new List<(int idx, Color color)>();
            List<(int idx, bool isBold)> tempBoldface = new List<(int idx, bool isBold)>();
            string tempText = text;
            formatText(tempColor, tempBoldface, ref tempText);

            List<int> trimmedStarts;
            string[] tempLines = GraphicsManager.TextFont.BreakIntoLines(tempText, rect.Width, tempText.Length, out trimmedStarts, tempBoldface);

            List<DialogueText> results = new List<DialogueText>();
            int cutIdx = 0;

            int endColorIndex = 0;
            //int endBoldIndex = 0;
            List<Color> colorStack = new List<Color>();
            //List<bool> boldStack = new List<bool>();

            for (int ii = 0; ii < tempLines.Length + 1; ii++)
            {
                if (ii == tempLines.Length || GraphicsManager.TextFont.CharHeight + (ii - cutIdx) * lineHeight > rect.Height)
                {
                    string[] initLines = new string[ii - cutIdx];
                    Array.Copy(tempLines, cutIdx, initLines, 0, initLines.Length);

                    List<int> initTrim = new List<int>();
                    for (int nn = cutIdx; nn < ii; nn++)
                        initTrim.Add(trimmedStarts[nn]);

                    /*
                    // Iterate over boldface list and get a trimmed-down version
                    List<(int idx, bool isBold)> newBoldness = new List<(int idx, bool isBold)>();
                    newBoldness.Add((0, false));
                    for (int boldnessIndex = 1; boldnessIndex < tempBoldface.Count; boldnessIndex++)
                    {
                        int idx = tempBoldface[boldnessIndex].idx;
                        bool isBold = true;// tempBoldface[boldnessIndex].isBold;
                        idx -= ii;
                        if (idx > 0)
                        {
                            newBoldness.Add((idx, isBold));
                        }
                    }
                    */

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

                    //

                    DialogueText newBox = new DialogueText("", rect, lineHeight, centerH, centerV, startIndex);
                    newBox.textColor = initColor;
                    newBox.textBoldface = tempBoldface;

                    Console.WriteLine("Old boldface:");
                    for (int i = 0; i < tempBoldface.Count; i++)
                    {
                        Console.WriteLine(tempBoldface[i].idx);
                        Console.WriteLine(tempBoldface[i].isBold);
                    }

                    /*
                    Console.WriteLine("New boldface:");
                    for (int i = 0; i < newBoldness.Count; i++)
                    {
                        Console.WriteLine(newBoldness[i].idx);
                        Console.WriteLine(newBoldness[i].isBold);
                    }
                    */

                    newBox.fullLines = initLines;
                    newBox.trimmedStarts = initTrim;
                    newBox.updateFullTextLength();

                    results.Add(newBox);
                    cutIdx = ii;
                }
            }

            return results;
        }

        public void SetPreFormattedText(string text, List<(int idx, Color color)> color, List<(int idx, bool isBold)> bold)
        {
            textColor = color;
            textBoldface = bold;

            //Console.WriteLine(text);

            /*
            for (int i = 0; i < textColor.Count; i++)
            {
                Console.WriteLine(textColor[i].idx);
                Console.WriteLine(textColor[i].color);
            }
            for (int i = 0; i < textBoldface.Count; i++)
            {
                Console.WriteLine(textBoldface[i].idx);
                Console.WriteLine(textBoldface[i].isBold);
            }
            */

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
            textBoldface.Clear();
            formatText(textColor, textBoldface, ref text);

            SetPreFormattedText(text, textColor, textBoldface);
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

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
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

                    // Get boldface list for current line
                    List<(int idx, bool isBold)> individualLineBoldface = new List<(int idx, bool isBold)>();
                    individualLineBoldface.Add((0, false));
                    for (int boldfaceIndex = 1; boldfaceIndex < textBoldface.Count; boldfaceIndex++)
                    {
                        if (textBoldface[boldfaceIndex].idx - lineChars >= 0)
                        {
                            individualLineBoldface.Add((textBoldface[boldfaceIndex].idx - lineChars, textBoldface[boldfaceIndex].isBold));
                        }
                    }

                    /*
                    for (int boldfaceIndex = 0; boldfaceIndex < individualLineBoldface.Count; boldfaceIndex++)
                    {
                        Console.WriteLine(individualLineBoldface[boldfaceIndex].idx);
                        Console.WriteLine(individualLineBoldface[boldfaceIndex].isBold);
                    }
                    */

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
                            Color == Color.White ? colorStack.Peek() * TextOpacity : Color * TextOpacity, curChar, nextColorIdx - curChar, individualLineBoldface);
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
