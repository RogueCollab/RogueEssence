using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using RogueEssence;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Data;
using Avalonia.Controls;
using System.IO;
using RogueEssence.Dev.Views;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Script;
using System.Linq;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Resources.NetStandard;
using System.Xml;

namespace RogueEssence.Dev.ViewModels
{
    public class GroundTabScriptViewModel : ViewModelBase
    {
        public GroundTabScriptViewModel()
        {
            ScriptItems = new ObservableCollection<ScriptItem>();
            foreach (LuaEngine.EMapCallbacks v in LuaEngine.EnumerateCallbackTypes())
                ScriptItems.Add(new ScriptItem(v, false));

        }

        public ObservableCollection<ScriptItem> ScriptItems { get; }

        public void btnOpenScriptDir_Click()
        {
            lock (GameBase.lockObj)
            {
                DevForm form = (DevForm)DiagManager.Instance.DevEditor;
                string mapscriptdir = LuaEngine.MakeMapScriptPath(Path.GetFileNameWithoutExtension(((GroundEditViewModel)form.GroundEditForm.DataContext).CurrentFile));
                mapscriptdir = Path.GetFullPath(mapscriptdir);
                Process.Start("explorer.exe", mapscriptdir);
            }
        }
        public void btnReloadScripts_Click()
        {
            lock (GameBase.lockObj)
            {
                LuaEngine.Instance.Reset();
                LuaEngine.Instance.ReInit();
                LoadScripts();
            }
        }

        public void LoadScripts()
        {
            lock (GameBase.lockObj)
            {
                //Setup callback display without triggering events
                for (int ii = 0; ii < ScriptItems.Count; ii++)
                    ScriptItems[ii] = new ScriptItem(ScriptItems[ii].Callback, false);
                var scev = ZoneManager.Instance.CurrentGround.ActiveScriptEvent();
                foreach (LuaEngine.EMapCallbacks s in scev)
                    ScriptItems[(int)s] = new ScriptItem(ScriptItems[(int)s].Callback, true);
            }
        }

    }
}
