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

        public async void btnOpenScriptDir_Click()
        {
            DevForm form = (DevForm)DiagManager.Instance.DevEditor;

            string file = Path.GetFileNameWithoutExtension(((GroundEditViewModel)form.GroundEditForm.DataContext).CurrentFile);
            string mapscriptdir = LuaEngine.MakeGroundMapScriptPath(file, "");

            if (!Directory.Exists(mapscriptdir))
            {
                await MessageBox.Show(form.GroundEditForm, String.Format("This map has not been saved under the current mod-under-edit.  Please switch to the desired mod and save it first."), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
            }
            else
            {
                try
                {

                    if (OperatingSystem.IsWindows())
                        Process.Start("explorer.exe", mapscriptdir);
                    else if (OperatingSystem.IsLinux())
                        Process.Start("xdg-open", mapscriptdir);
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
                LuaEngine.Instance.LoadScripts();
                LuaEngine.Instance.ReInit();
            }
        }

        public void LoadScripts()
        {
            lock (GameBase.lockObj)
            {
                //Setup callback display without triggering events
                for (int ii = 0; ii < ScriptItems.Count; ii++)
                    ScriptItems[ii] = new ScriptItem(ScriptItems[ii].EventType, false);
                var scev = ZoneManager.Instance.CurrentGround.ActiveScriptEvent();
                foreach (LuaEngine.EMapCallbacks s in scev)
                    ScriptItems[(int)s] = new ScriptItem(ScriptItems[(int)s].EventType, true);
            }
        }

    }
}
