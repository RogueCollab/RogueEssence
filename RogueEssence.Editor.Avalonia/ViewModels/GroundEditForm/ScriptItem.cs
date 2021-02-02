using RogueEssence.Script;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev.ViewModels
{
    public class ScriptItem : ViewModelBase
    {
        public ScriptItem() { }
        public ScriptItem(LuaEngine.EMapCallbacks callback, bool isChecked)
        {
            this.Callback = callback;
            IsChecked = isChecked;
        }
        public LuaEngine.EMapCallbacks Callback;
        public string Description { get { return Callback.ToString(); } }
        public bool IsChecked { get; set; }
    }
}
