using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using RogueEssence;
using RogueEssence.Script;
using System.Collections.ObjectModel;
using RogueEssence.Dungeon;
using RogueEssence.Menu;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{
    public class DevTabScriptViewModel : ViewModelBase
    {
        private Stack<string> m_lastcommands;
        private int m_cntDownArrow;   //counts the ammount of times the down arrow has been pressed in a row, for looking through the last commands!

        public DevTabScriptViewModel()
        {
            m_lastcommands = new Stack<string>();
            m_lastcommands.Push("");
            ScriptLog = "";
            ScriptLine = "";
        }

        private string scriptLog;
        public string ScriptLog
        {
            get { return scriptLog; }
            set { this.SetIfChanged(ref scriptLog, value); }
        }

        private int scriptCaret;
        public int ScriptCaret
        {
            get { return scriptCaret; }
            set { this.SetIfChanged(ref scriptCaret, value); }
        }

        private int cmdCaret;
        public int CmdCaret
        {
            get { return cmdCaret; }
            set { this.SetIfChanged(ref cmdCaret, value); }
        }

        private string scriptLine;
        public string ScriptLine
        {
            get { return scriptLine; }
            set { this.SetIfChanged(ref scriptLine, value); }
        }

        public void btnReloadScripts_Click()
        {
            DevForm.ExecuteOrPend(scriptReload);
        }

        private void scriptReload()
        {
            lock (GameBase.lockObj)
            {
                //Reload everything
                LuaEngine.Instance.Reset();
                LuaEngine.Instance.ReInit();
            }
        }

        public void btnTextTest_Click()
        {
            TextTestViewModel mv = new TextTestViewModel();
            Views.TextTestForm editForm = new Views.TextTestForm();
            editForm.DataContext = mv;
            editForm.Show();
        }

        public void SendScript()
        {
            lock (GameBase.lockObj)
            {
                ScriptLog = ScriptLog + "\n" + ScriptLine;
                m_lastcommands.Pop();
                m_lastcommands.Push(ScriptLine);
                m_lastcommands.Push("");
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
            m_cntDownArrow = Math.Clamp(m_cntDownArrow + increment, 0, strs.Length - 1);
            ScriptLine = strs[m_cntDownArrow];
        }

    }
}
