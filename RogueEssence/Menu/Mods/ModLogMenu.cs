using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class ModLogMenu : LogMenu
    {
        List<string> failMsgs;

        public ModLogMenu(List<(ModRelationship, List<ModHeader>)> loadErrors) : this(MenuLabel.MOD_LOG_MENU, loadErrors) { }
        public ModLogMenu(string label, List<(ModRelationship, List<ModHeader>)> loadErrors)
        {
            Label = label;
            failMsgs = new List<string>();
            foreach ((ModRelationship, List<ModHeader>) loadError in loadErrors)
            {
                List<ModHeader> involved = loadError.Item2;
                switch (loadError.Item1)
                {
                    case ModRelationship.Incompatible:
                        {
                            failMsgs.Add(Text.FormatKey("MENU_MOD_LOG_INCOMPATIBLE", involved[0].Namespace, involved[1].Namespace));
                            failMsgs.Add("\n");
                        }
                        break;
                    case ModRelationship.DependsOn:
                        {
                            if (String.IsNullOrEmpty(involved[1].Namespace))
                                failMsgs.Add(Text.FormatKey("MENU_MOD_LOG_GAME_DEPENDENCY", involved[0].Namespace, involved[1].Version));
                            else
                                failMsgs.Add(Text.FormatKey("MENU_MOD_LOG_DEPENDENCY", involved[0].Namespace, involved[1].Namespace));
                            failMsgs.Add("\n");
                        }
                        break;
                    case ModRelationship.LoadBefore:
                    case ModRelationship.LoadAfter:
                        {
                            List<string> cycle = new List<string>();
                            foreach (ModHeader header in involved)
                                cycle.Add(header.Namespace);
                            failMsgs.Add(Text.FormatKey("MENU_MOD_LOG_ORDER", String.Join(", ", cycle.ToArray())));
                            failMsgs.Add("\n");
                        }
                        break;
                }
            }

            Initialize(new Loc(LiveMsgLog.SIDE_BUFFER, 24), GraphicsManager.ScreenWidth - LiveMsgLog.SIDE_BUFFER * 2, 13, Text.FormatKey("MENU_MOD_LOG_TITLE"));
        }

        protected override IEnumerable<string> GetRecentMsgs(int entries)
        {
            return GetRecentMsgs(failMsgs.Count - entries, failMsgs.Count);
        }

        protected override IEnumerable<string> GetRecentMsgs(int entriesStart, int entriesEnd)
        {
            entriesStart = Math.Max(0, entriesStart);
            entriesEnd = Math.Min(failMsgs.Count, entriesEnd);

            for (int ii = entriesStart; ii < entriesEnd; ii++)
                yield return failMsgs[ii];
        }

    }
}
