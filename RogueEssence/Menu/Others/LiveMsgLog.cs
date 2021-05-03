using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class LiveMsgLog : MenuBase
    {
        public const int START_VERT = 6;
        private const int MAX_LINES = 3;
        public const int SIDE_BUFFER = 8;
        const int LOG_VISIBLE_TIME = 180;

        private List<MenuText> entries;
        private List<MenuDivider> dividers;

        private FrameTick timeSinceUpdate;

        public LiveMsgLog()
        {
            entries = new List<MenuText>();
            dividers = new List<MenuDivider>();
            Bounds = Rect.FromPoints(new Loc(SIDE_BUFFER, GraphicsManager.ScreenHeight - (16 + VERT_SPACE * MAX_LINES)), new Loc(GraphicsManager.ScreenWidth - SIDE_BUFFER, GraphicsManager.ScreenHeight - 8));
            timeSinceUpdate = new FrameTick();
            Visible = false;
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            foreach (IMenuElement entry in entries)
                yield return entry;
            foreach (MenuDivider divider in dividers)
            {
                if (divider != null)
                    yield return divider;
            }
        }

        public void LogAdded(string msg)
        {
            if (msg == Text.DIVIDER_STR)
                LogAdded(entries, dividers, Bounds.Y + START_VERT, SIDE_BUFFER, msg);
            else
            {

                string[] lines = MenuText.BreakIntoLines(msg, GraphicsManager.ScreenWidth - GraphicsManager.MenuBG.TileWidth * 2 - SIDE_BUFFER * 2);
                foreach (string line in lines)
                    LogAdded(entries, dividers, Bounds.Y + START_VERT, SIDE_BUFFER, line);
                timeSinceUpdate = new FrameTick();
                Visible = true;
            }
        }

        public static void LogAdded(List<MenuText> entries, List<MenuDivider> dividers, int startVert, int sideBuffer, string msgLine)
        {
            //methodize this for message log
            if (msgLine == Text.DIVIDER_STR)
            {
                if (entries.Count > 0)
                    dividers[dividers.Count - 1] = new MenuDivider(new Loc(sideBuffer + GraphicsManager.MenuBG.TileWidth, entries[entries.Count - 1].Loc.Y + 11),
                               GraphicsManager.ScreenWidth - GraphicsManager.MenuBG.TileWidth * 2 - sideBuffer * 2);
            }
            else
            {
                if (entries.Count > 0)
                    entries.Add(new MenuText(msgLine, entries[entries.Count - 1].Loc + new Loc(0, VERT_SPACE)));
                else
                    entries.Add(new MenuText(msgLine, new Loc(sideBuffer + GraphicsManager.MenuBG.TileWidth, startVert)));
                dividers.Add(null);
            }
        }

        private int tickRemainder;
        public void ForceOff()
        {
            timeSinceUpdate = new FrameTick(LOG_VISIBLE_TIME);
            Visible = false;
            entries.Clear();
            dividers.Clear();
            tickRemainder = 0;
        }
        public void Update(FrameTick elapsedTime)
        {
            if (timeSinceUpdate >= LOG_VISIBLE_TIME)
            {
                Visible = false;
                entries.Clear();
                dividers.Clear();
                tickRemainder = 0;
            }
            else if (entries.Count > MAX_LINES)
            {
                int multSpeed = Math.Max(2, (entries.Count - 1) / MAX_LINES);
                long moveSpeed = Math.Max(1, (long)FrameTick.FrameToTick(1) / multSpeed);
                int addAmount = (int)((elapsedTime.Ticks + tickRemainder) / moveSpeed);
                tickRemainder = (int)((elapsedTime.Ticks + tickRemainder) % moveSpeed);

                //limit the upscroll to the amount that would cause the lowest message to become totally visible
                int scrollLimit = Bounds.Y + START_VERT + VERT_SPACE * (MAX_LINES - 1);
                if (entries[entries.Count - 1].Loc.Y - addAmount < scrollLimit)
                    addAmount = entries[entries.Count - 1].Loc.Y - scrollLimit;

                for (int ii = 0; ii < entries.Count; ii++)
                {
                    entries[ii].Loc = entries[ii].Loc - new Loc(0, addAmount);
                    if (dividers[ii] != null)
                        dividers[ii].Loc = dividers[ii].Loc - new Loc(0, addAmount);
                }

                while (entries[0].Loc.Y <= Bounds.Y + START_VERT - VERT_SPACE)
                {
                    entries.RemoveAt(0);
                    dividers.RemoveAt(0);
                }
            }
            else
                timeSinceUpdate += elapsedTime;
        }
    }
}
