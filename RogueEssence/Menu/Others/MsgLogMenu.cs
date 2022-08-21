using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class MsgLogMenu : LogMenu
    {
        public MsgLogMenu()
        {
            Initialize(new Loc(LiveMsgLog.SIDE_BUFFER, 24), GraphicsManager.ScreenWidth - LiveMsgLog.SIDE_BUFFER, 13, Text.FormatKey("MENU_MSG_LOG_TITLE"));
        }

        protected override IEnumerable<string> GetRecentMsgs(int entries)
        {
            return DataManager.Instance.GetRecentMsgs(entries);
        }

        protected override IEnumerable<string> GetRecentMsgs(int entriesStart, int entriesEnd)
        {
            return DataManager.Instance.GetRecentMsgs(entriesStart, entriesEnd);
        }

    }
}
