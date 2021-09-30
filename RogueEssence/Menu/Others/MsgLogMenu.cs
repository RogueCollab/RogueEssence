using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class MsgLogMenu : InteractableMenu
    {
        private const int MAX_LINES = 13;
        public const int SIDE_BUFFER = 8;

        public MenuText Title;
        public MenuDivider Div;

        private int msgIndex;//what message the log starts at
        private int lineIndex;//what line the log starts at (inclusive)
        private int lineEndIndex;//what line the log ends at (inclusive)
        private List<string[]> coveredLines;

        private List<MenuText> entries;
        private List<MenuDivider> dividers;

        public MsgLogMenu()
        {
            entries = new List<MenuText>();
            dividers = new List<MenuDivider>();
            Bounds = Rect.FromPoints(new Loc(LiveMsgLog.SIDE_BUFFER, 24), new Loc(GraphicsManager.ScreenWidth - LiveMsgLog.SIDE_BUFFER, GraphicsManager.ScreenHeight - 8));
            Title = new MenuText(Text.FormatKey("MENU_MSG_LOG_TITLE"), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            Div = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);
            LoadMsgs();
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Title;
            yield return Div;
            foreach (IMenuElement entry in entries)
                yield return entry;
            foreach (MenuDivider divider in dividers)
            {
                if (divider != null)
                    yield return divider;
            }
        }

        private void LoadMsgs()
        {
            coveredLines = new List<string[]>();
            foreach (string entry in DataManager.Instance.GetRecentMsgs(MAX_LINES * 2))
            {
                if (entry == Text.DIVIDER_STR)
                    coveredLines.Add(new string[1] { entry });
                else
                    coveredLines.Add(MenuText.BreakIntoLines(entry, GraphicsManager.ScreenWidth - GraphicsManager.MenuBG.TileWidth * 2 - SIDE_BUFFER * 2));
            }

            if (coveredLines.Count > 0)
            {
                int displayLines = 0;
                int msgsBack = 0;
                for (int ii = coveredLines.Count - 1; ii >= 0; ii--)
                {
                    msgsBack = ii;
                    for (int jj = coveredLines[ii].Length - 1; jj >= 0; jj--)
                    {
                        lineIndex = jj;
                        if (coveredLines[ii][jj] != Text.DIVIDER_STR)
                            displayLines++;
                        if (displayLines >= MAX_LINES)
                            break;
                    }
                    if (displayLines >= MAX_LINES)
                        break;
                }
                lineEndIndex = coveredLines[coveredLines.Count - 1].Length - 1;

                msgIndex = DataManager.Instance.MsgLog.Count - (coveredLines.Count - msgsBack);
                coveredLines.RemoveRange(0, msgsBack);

                UpdateLog();
            }
        }


        private void UpdateLog()
        {
            entries.Clear();
            dividers.Clear();
            for (int ii = 0; ii < coveredLines.Count; ii++)
            {
                for (int jj = (ii == 0 ? lineIndex : 0); jj <= (ii == coveredLines.Count - 1 ? lineEndIndex : coveredLines[ii].Length - 1); jj++)
                    LiveMsgLog.LogAdded(entries, dividers, Bounds.Y + GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET, SIDE_BUFFER, coveredLines[ii][jj]);
            }
        }

        private bool msgLogOverflows()
        {
            if (DataManager.Instance.MsgLog.Count > MAX_LINES)
                return true;

            int total_lines = 0;
            foreach (string entry in DataManager.Instance.GetRecentMsgs(MAX_LINES * 2))
            {
                if (entry != Text.DIVIDER_STR)
                {
                    string[] lines = MenuText.BreakIntoLines(entry, GraphicsManager.ScreenWidth - GraphicsManager.MenuBG.TileWidth * 2 - SIDE_BUFFER * 2);
                    total_lines += lines.Length;
                    if (total_lines > MAX_LINES)
                        return true;
                }
            }
            return false;
        }

        public override void Update(InputManager input)
        {
            Visible = true;
            if (input.JustPressed(FrameInput.InputType.Menu))
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.ClearMenus();
            }
            else if (input.JustPressed(FrameInput.InputType.Cancel))
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.RemoveMenu();
            }
            else if (input.JustPressed(FrameInput.InputType.MsgLog))
                MenuManager.Instance.ClearMenus();
            else
            {
                if (msgLogOverflows())
                {
                    int delta = 0;
                    if (IsInputting(input, Dir8.Down, Dir8.DownLeft, Dir8.DownRight))
                        delta = 1;
                    else if (IsInputting(input, Dir8.Up, Dir8.UpLeft, Dir8.UpRight))
                        delta = -1;
                    else if (IsInputting(input, Dir8.Left))
                        delta = -MAX_LINES;
                    else if (IsInputting(input, Dir8.Right))
                        delta = MAX_LINES;

                    if (delta > 0)
                    {
                        int addedLines = 0;

                        addedLines += Math.Min(delta, coveredLines[coveredLines.Count - 1].Length - 1 - lineEndIndex);
                        lineEndIndex = Math.Min(delta + lineEndIndex, coveredLines[coveredLines.Count - 1].Length - 1);
                        if (addedLines < delta)
                        {
                            foreach (string entry in DataManager.Instance.GetRecentMsgs(msgIndex + coveredLines.Count, msgIndex + coveredLines.Count + delta * 2))
                            {
                                if (entry == Text.DIVIDER_STR)
                                {
                                    coveredLines.Add(new string[1] { entry });
                                    lineEndIndex = 0;
                                }
                                else
                                {
                                    string[] lines = MenuText.BreakIntoLines(entry, GraphicsManager.ScreenWidth - GraphicsManager.MenuBG.TileWidth * 2 - SIDE_BUFFER * 2);
                                    coveredLines.Add(lines);
                                    lineEndIndex = Math.Min(delta - addedLines, lines.Length) - 1;
                                    addedLines += Math.Min(delta - addedLines, lines.Length);
                                    if (addedLines >= delta)
                                        break;
                                }
                            }
                        }

                        if (addedLines > 0)
                        {
                            int removedLines = 0;
                            while (removedLines < addedLines)
                            {
                                if (coveredLines[0].Length == 1 && coveredLines[0][0] == Text.DIVIDER_STR)
                                {
                                    coveredLines.RemoveAt(0);
                                    msgIndex++;
                                }
                                else
                                {
                                    lineIndex++;
                                    removedLines++;
                                    if (lineIndex >= coveredLines[0].Length)
                                    {
                                        lineIndex = 0;
                                        coveredLines.RemoveAt(0);
                                        msgIndex++;
                                    }
                                }
                            }

                            UpdateLog();

                            GameManager.Instance.SE(addedLines > 1 ? "Menu/Skip" : "Menu/Select");
                        }
                    } 
                    else if (delta < 0)
                    {
                        delta *= -1;

                        int addedLines = 0;

                        addedLines += Math.Min(delta, lineIndex);
                        lineIndex = Math.Max(lineIndex - delta, 0);
                        if (addedLines < delta)
                        {
                            int addedEntries = 0;
                            foreach (string entry in DataManager.Instance.GetRecentMsgs(msgIndex - delta * 2, msgIndex))
                            {
                                if (entry == Text.DIVIDER_STR)
                                    coveredLines.Insert(addedEntries, new string[1] { entry });
                                else
                                    coveredLines.Insert(addedEntries, MenuText.BreakIntoLines(entry, GraphicsManager.ScreenWidth - GraphicsManager.MenuBG.TileWidth * 2 - SIDE_BUFFER * 2));
                                addedEntries++;
                            }
                            int msgsBack = 0;
                            for (int ii = addedEntries - 1; ii >= 0; ii--)
                            {
                                msgsBack = ii;
                                for (int jj = coveredLines[ii].Length - 1; jj >= 0; jj--)
                                {
                                    lineIndex = jj;
                                    if (coveredLines[ii][jj] != Text.DIVIDER_STR)
                                        addedLines++;
                                    if (addedLines >= delta)
                                        break;
                                }
                                if (addedLines >= delta)
                                    break;
                            }

                            msgIndex -= (addedEntries - msgsBack);
                            coveredLines.RemoveRange(0, msgsBack);

                        }

                        if (addedLines > 0)
                        {
                            int removedLines = 0;
                            while (removedLines < addedLines)
                            {
                                if (coveredLines[coveredLines.Count - 1].Length == 1 && coveredLines[coveredLines.Count - 1][0] == Text.DIVIDER_STR)
                                {
                                    coveredLines.RemoveAt(coveredLines.Count - 1);
                                    lineEndIndex = coveredLines[coveredLines.Count - 1].Length - 1;
                                }
                                else
                                {
                                    lineEndIndex--;
                                    removedLines++;
                                    if (lineEndIndex < 0)
                                    {
                                        coveredLines.RemoveAt(coveredLines.Count - 1);
                                        lineEndIndex = coveredLines[coveredLines.Count - 1].Length - 1;
                                    }
                                }
                            }

                            UpdateLog();

                            GameManager.Instance.SE(addedLines > 1 ? "Menu/Skip" : "Menu/Select");
                        }
                    }
                }
            }
        }
    }
}
