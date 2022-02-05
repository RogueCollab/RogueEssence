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
            Dictionary<string, Dictionary<string, string>> rawStrings = new Dictionary<string, Dictionary<string, string>>();

            string FMTStr = String.Format("{0}{1}.{2}", ScriptStrings.STRINGS_FILE_NAME, "{0}", ScriptStrings.STRINGS_FILE_EXT);
            foreach (string code in Text.SupportedLangs)
            {
                string fname = String.Format(FMTStr, code == "en" ? "" : ("." + code));//special case for english, which is default
                string path = Path.Combine(stringsdir, fname);

                if (File.Exists(path))
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);
                    foreach (XmlNode xnode in xmldoc.DocumentElement.ChildNodes)
                    {
                        if (xnode.Name == "data")
                        {
                            string value = null;
                            string name = null;
                            var atname = xnode.Attributes["name"];
                            if (atname != null)
                                name = atname.Value;

                            //Get value
                            XmlNode valnode = xnode.SelectSingleNode("value");
                            if (valnode != null)
                                value = valnode.InnerText;

                            if (!rawStrings.ContainsKey(name))
                                rawStrings.Add(name, new Dictionary<string, string>());

                            if (!rawStrings[name].ContainsKey(code))
                                rawStrings[name].Add(code, value);
                            else
                                rawStrings[name][code] = value;
                        }
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
                MapStrings.Add(new MapString(key, "", rawStrings[key]));
        }

        /// <summary>
        /// Writes the content of the dataview into a set of resx files for each languages
        /// </summary>
        public void SaveStrings()
        {
            string stringsdir = LuaEngine.MakeGroundMapScriptPath(true, ZoneManager.Instance.CurrentGround.AssetName, "");

            string FMTStr = String.Format("{0}{1}.{2}", Script.ScriptStrings.STRINGS_FILE_NAME, "{0}", Script.ScriptStrings.STRINGS_FILE_EXT);
            foreach (string code in Text.SupportedLangs)
            {
                string fname = String.Format(FMTStr, code == "en" ? "" : ("." + code));//special case for english, which is default
                string path = Path.Combine(stringsdir, fname);
                using (ResXResourceWriter resx = new ResXResourceWriter(path))
                {
                    //Add all strings matching the specified language code
                    foreach (MapString str in MapStrings)
                    {
                        string tl;
                        if (!str.Translations.TryGetValue(code, out tl))
                            tl = "";
                        if (tl != "" || code == "en")
                            resx.AddResource(new ResXDataNode(str.Key, tl) { Comment = "" });
                    }
                    resx.Generate();
                    resx.Close();
                }
            }
        }

    }
}
