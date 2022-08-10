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

                try
                {
                    string file = Path.GetFileNameWithoutExtension(((GroundEditViewModel)form.GroundEditForm.DataContext).CurrentFile);
                    string mapscriptdir = LuaEngine.MakeGroundMapScriptPath(true, file, "");

                    if (OperatingSystem.IsWindows())
                        Process.Start("explorer.exe", mapscriptdir);
                    else if (OperatingSystem.IsLinux())
                        Process.Start("mimeopen", mapscriptdir);
                    else if (OperatingSystem.IsMacOS())
                        Process.Start("open", mapscriptdir);
                    else
                        throw new NotSupportedException("File open not supported on current system.");
                }
                catch (Exception e)
                {
                    DiagManager.Instance.LogError(e);
                }
            }
        }
        
        public void btnReloadScripts_Click()
        {
            DevForm.ExecuteOrPend(scriptReload);
            LoadScripts();
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
