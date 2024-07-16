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
    public class GroundTabStringsViewModel : ViewModelBase
    {
        public GroundTabStringsViewModel()
        {
            MapStrings = new ObservableCollection<MapString>();
        }


        // Later: implement variable length columns workaround https://github.com/AvaloniaUI/Avalonia/issues/2781
        public ObservableCollection<MapString> MapStrings { get; set; }

        private int currentString;
        public int CurrentString
        {
            get { return currentString; }
            set
            {
                this.SetIfChanged(ref currentString, value);
            }
        }

        public void btnAddString_Click()
        {
            string defname = String.Format("String_{0}", MapStrings.Count);
            //if (CurrentString > -1)
            //    MapStrings.Insert(CurrentString, new MapString(defname, "", ""));
            //else
            MapStrings.Add(new MapString(defname, "", ""));
        }
        public void btnDeleteString_Click()
        {
            MapStrings.RemoveAt(CurrentString);
        }

        /// <summary>
        /// Loads the strings files content into the data grid view for editing
        /// </summary>
        /// <param name="stringsdir">Directory in which string resx files are stored!</param>
        public void LoadStrings()
        {
            string stringsdir = Path.GetDirectoryName(LuaEngine.MakeGroundMapScriptPath(false, ZoneManager.Instance.CurrentGround.AssetName, "/init.lua"));
            //Clear old strings
            Dictionary<string, Dictionary<string, (string val, string comment)>> rawStrings = new Dictionary<string, Dictionary<string, (string, string)>>();

            string FMTStr = String.Format("{0}{1}{2}", ScriptStrings.STRINGS_FILE_NAME, "{0}", Text.STRINGS_FILE_EXT);
            foreach (string code in Text.SupportedLangs)
            {
                string fname = String.Format(FMTStr, code == "en" ? "" : ("." + code));//special case for english, which is default
                string path = Path.Combine(stringsdir, fname);

                if (File.Exists(path))
                {
                    Dictionary<string, (string val, string comment)> xmlDict = Text.LoadDevStringResx(path);
                    foreach (string name in xmlDict.Keys)
                    {
                        if (!rawStrings.ContainsKey(name))
                            rawStrings.Add(name, new Dictionary<string, (string val, string comment)>());

                        if (!rawStrings[name].ContainsKey(code))
                            rawStrings[name].Add(code, xmlDict[name]);
                        else
                            rawStrings[name][code] = xmlDict[name];
                    }

                    DiagManager.Instance.LogInfo(String.Format("GroundEditor.LoadStrings({0}): Loaded succesfully the \"{1}\" strings file for this map!", stringsdir, fname));
                }
                else
                {
                    DiagManager.Instance.LogInfo(String.Format("GroundEditor.LoadStrings({0}): Couldn't open the \"{1}\" strings file for this map!", stringsdir, fname));
                }

            }

            MapStrings.Clear();
            foreach (string key in rawStrings.Keys)
            {
                Dictionary<string, string> comments = new Dictionary<string, string>();
                Dictionary<string, string> vals = new Dictionary<string, string>();
                foreach (string name in rawStrings[key].Keys)
                {
                    comments[name] = rawStrings[key][name].comment;
                    vals[name] = rawStrings[key][name].val;
                }
                MapStrings.Add(new MapString(key, comments, vals));
            }
        }

        /// <summary>
        /// Writes the content of the dataview into a set of resx files for each languages
        /// </summary>
        public void SaveStrings()
        {
            string stringsdir = LuaEngine.MakeGroundMapScriptPath(true, ZoneManager.Instance.CurrentGround.AssetName, "");

            string FMTStr = String.Format("{0}{1}{2}", Script.ScriptStrings.STRINGS_FILE_NAME, "{0}", Text.STRINGS_FILE_EXT);
            foreach (string code in Text.SupportedLangs)
            {
                string fname = String.Format(FMTStr, code == "en" ? "" : ("." + code));//special case for english, which is default
                string path = Path.Combine(stringsdir, fname);
                Dictionary<string, (string val, string comment)> stringDict = new Dictionary<string, (string val, string comment)>();

                foreach (MapString str in MapStrings)
                {
                    string cm, tl;
                    if (!str.Comments.TryGetValue(code, out cm))
                        cm = "";
                    if (!str.Translations.TryGetValue(code, out tl))
                        tl = "";
                    if (tl != "" || code == "en")
                        stringDict[str.Key] = (tl, cm);
                }
                Text.SaveStringResx(path, stringDict);
            }
        }

    }
}
