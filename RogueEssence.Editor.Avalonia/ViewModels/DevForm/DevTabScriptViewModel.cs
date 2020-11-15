using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using RogueEssence;
using RogueEssence.Script;
using System.Collections.ObjectModel;
using RogueEssence.Dungeon;
using RogueEssence.Menu;

namespace RogueEssence.Dev.ViewModels
{
    public class DevTabScriptViewModel : ViewModelBase
    {
        private Stack<string> m_lastcommands;
        private int m_cntDownArrow;   //counts the ammount of times the down arrow has been pressed in a row, for looking through the last commands!

        public DevTabScriptViewModel()
        {
            m_lastcommands = new Stack<string>();
            ScriptLog = "";
            ScriptLine = "";
        }

        private string scriptLog;
        public string ScriptLog
        {
            get { return scriptLog; }
            set { this.RaiseAndSetIfChanged(ref scriptLog, value); }
        }

        private int scriptCaret;
        public int ScriptCaret
        {
            get { return scriptCaret; }
            set { this.RaiseAndSetIfChanged(ref scriptCaret, value); }
        }

        private string scriptLine;
        public string ScriptLine
        {
            get { return scriptLine; }
            set { this.RaiseAndSetIfChanged(ref scriptLine, value); }
        }

        public void btnReloadScripts_Click()
        {
            lock (GameBase.lockObj)
            {
                //Reload everything
                LuaEngine.Instance.Reset();
                LuaEngine.Instance.ReInit();
            }
        }

        public void SendScript()
        {
            lock (GameBase.lockObj)
            {
                ScriptLog = ScriptLog + "\n" + ScriptLine;
                m_lastcommands.Push(ScriptLine);
                ScriptCaret = ScriptLog.Length;
                //Send the text to the script engine
                LuaEngine.Instance.RunString(ScriptLine);
                ScriptLine = "";
                m_cntDownArrow = 0;
            }
        }

        public void ShiftHistory(int increment)
        {
            string[] strs = m_lastcommands.ToArray();
            m_cntDownArrow = (m_cntDownArrow + strs.Length + increment) % strs.Length;
            ScriptLine = strs[m_cntDownArrow];
        }

    }
}
