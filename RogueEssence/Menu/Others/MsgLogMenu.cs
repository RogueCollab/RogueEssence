using System.Collections.Generic;
using System.Reflection.Emit;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class MsgLogMenu : LogMenu
    {
        public MsgLogMenu() : this(MenuLabel.MESSAGE_LOG_MENU) { }
        public MsgLogMenu(string label)
        {
            Label = label;
            Initialize(new Loc(LiveMsgLog.SIDE_BUFFER, 24), GraphicsManager.ScreenWidth - LiveMsgLog.SIDE_BUFFER * 2, 13, Text.FormatKey("MENU_MSG_LOG_TITLE"));
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
