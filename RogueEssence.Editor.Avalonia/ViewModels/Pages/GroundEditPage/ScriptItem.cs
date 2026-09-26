using Avalonia;
using RogueEssence.Dungeon;
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
            EventType = callback;
            IsChecked = isChecked;
        }
        public LuaEngine.EMapCallbacks EventType { get; set; }
        public bool IsChecked
        {
            get
            {
                return ZoneManager.Instance.CurrentGround.HasScriptEvent(EventType);
            }
            set
            {

            }
        }
        public string Definition
        {
            get
            {
                return LuaEngine.MakeGroundMapCallbackDef(ZoneManager.Instance.CurrentGround.AssetName, EventType);
            }
            set
            {

            }
        }

        public async void mnuCopyFun_Click()
        {
            // await Application.Current.Clipboard.SetTextAsync(Definition);
            App.CopyText(Definition);
        }
    }
}
