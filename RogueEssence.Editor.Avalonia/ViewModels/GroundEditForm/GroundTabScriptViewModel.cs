using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Dev.Models;
using System.Collections.ObjectModel;
using RogueEssence.Dungeon;
using RogueEssence.Script;

namespace RogueEssence.Dev.ViewModels
{
    public class GroundTabScriptViewModel : ViewModelBase
    {
        public GroundTabScriptViewModel()
        {
            ScriptItems = new ObservableCollection<ScriptItem>();
            foreach (LuaEngine.EMapCallbacks v in LuaEngine.EnumerateCallbackTypes())
                ScriptItems.Add(new ScriptItem(v.ToString(), false));
            fillInMapScriptData();
        }

        public ObservableCollection<ScriptItem> ScriptItems { get; }

        public void btnOpenScriptDir_Click()
        {

        }
        public void btnReloadScripts_Click()
        {

        }



        private void fillInMapScriptData()
        {
            //Setup callback display without triggering events
            var scev = ZoneManager.Instance.CurrentGround.ActiveScriptEvent();
            foreach (LuaEngine.EMapCallbacks s in scev)
            {
                ScriptItems[(int)s].IsChecked = true;
            }
        }

    }
}
