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
            ScriptLog = "";
            ScriptLine = "";
        }

        private string scriptLog;
        public string ScriptLog
        {
            get { return scriptLog; }
            set { this.RaiseAndSetIfChanged(ref scriptLog, value); }
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
            //txtScriptOutput.AppendText("\n" + txtScriptInput.Text);
            //m_lastcommands.Push(txtScriptInput.Text);
            ////Send the text to the script engine
            //LuaEngine.Instance.RunString(txtScriptInput.Text);
            //txtScriptInput.Clear();
            //m_cntDownArrow = 0;
        }

        public void ShiftHistory(int increment)
        {
            //string[] strs = m_lastcommands.ToArray();
            //if (m_cntDownArrow < strs.Length)
            //{
            //    txtScriptInput.Clear();
            //    txtScriptInput.Text = strs[m_cntDownArrow];
            //    m_cntDownArrow++;
            //}
            //else if (m_cntDownArrow >= strs.Length)
            //{
            //    txtScriptInput.Clear();
            //    m_cntDownArrow = 0;
            //}
            //e.Handled = true;
        }

    }
}
