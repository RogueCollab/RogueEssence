using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;

namespace RogueEssence.Dev.ViewModels;

public class StringEditPageViewModel : EditorPageViewModel
{
    public bool IsMenuText { get; private set;  }
    
    public string HeaderText { get; }
    
    public StringEditPageViewModel(EditorContext context, OpenEditorNodeWithParams node, Action<EditorPageViewModel> onPageOpen = null)
        : base(context, node, onPageOpen)
    {
        GameStrings = new ObservableCollection<MapString>();
        IsMenuText = (bool)node.ExtraParams[0];
        HeaderText = node.Title;
    }
    public override void OnPageLoad()
    {
        LoadStringEntries();
    }
    
    protected override bool IsSamePage(EditorPageViewModel other)
    {
        if (other is not StringEditPageViewModel stringPage)
            return false;

        return this.IsMenuText == stringPage.IsMenuText;
    }
        
    
    public string Name
    {
        get { return IsMenuText ? "stringsEx" : "strings"; }
    }


    // Later: implement variable length columns workaround https://github.com/AvaloniaUI/Avalonia/issues/2781
    public ObservableCollection<MapString> GameStrings { get; set; }

    private int currentString;

    public int CurrentString
    {
        get { return currentString; }
        set { this.SetIfChanged(ref currentString, value); }
    }


    public void btnAddString_Click()
    {
        string defname = String.Format("String_{0}", GameStrings.Count);
        //if (CurrentString > -1)
        //    GameStrings.Insert(CurrentString, new MapString(defname, "", ""));
        //else
        GameStrings.Add(new MapString(defname, "", ""));
    }

    public void btnDeleteString_Click()
    {
        GameStrings.RemoveAt(CurrentString);
    }

    

    public void LoadStringEntries()
    {
        LoadStrings();
    }


    public void btnAdd_Click()
    {
        string defname = String.Format("STRING_{0}", GameStrings.Count);
        GameStrings.Add(new MapString(defname, "", ""));
    }

    public void btnDelete_Click()
    {
        GameStrings.RemoveAt(CurrentString);
    }

    public void btnSave_Click()
    {
        SaveStrings();
    }


    /// <summary>
    /// Loads the strings files content into the data grid view for editing
    /// </summary>
    /// <param name="stringsdir">Directory in which string resx files are stored!</param>
    public void LoadStrings()
    {
        populateStringTable(GameStrings, false);
    }

    private void populateStringTable(IList<MapString> strings, bool skipTopMod)
    {
        string stringsdir = "Strings/";
        //Clear old strings
        Dictionary<string, Dictionary<string, (string val, string comment)>> rawStrings =
            new Dictionary<string, Dictionary<string, (string, string)>>();


        string FMTStr = String.Format("{0}{1}{2}", Name, "{0}", Text.STRINGS_FILE_EXT);
        foreach (string code in Text.SupportedLangs)
        {
            string fname =
                String.Format(FMTStr, code == "en" ? "" : ("." + code)); //special case for english, which is default
            string path = Path.Combine(stringsdir, fname);

            bool first = true;
            foreach (string modPath in PathMod.FallbackPaths(path))
            {
                if (first && skipTopMod)
                {
                    first = false;
                    continue;
                }

                Dictionary<string, (string val, string comment)> xmlDict = Text.LoadDevStringResx(modPath);
                foreach (string name in xmlDict.Keys)
                {
                    if (!rawStrings.ContainsKey(name))
                        rawStrings.Add(name, new Dictionary<string, (string val, string comment)>());

                    if (!rawStrings[name].ContainsKey(code))
                        rawStrings[name].Add(code, xmlDict[name]);
                }

                first = false;
            }
        }

        strings.Clear();
        foreach (string key in rawStrings.Keys)
        {
            Dictionary<string, string> comments = new Dictionary<string, string>();
            Dictionary<string, string> vals = new Dictionary<string, string>();
            foreach (string name in rawStrings[key].Keys)
            {
                comments[name] = rawStrings[key][name].comment;
                vals[name] = rawStrings[key][name].val;
            }

            strings.Add(new MapString(key, comments, vals));
        }
    }

    /// <summary>
    /// Writes the content of the dataview into a set of resx files for each languages
    /// </summary>
    public void SaveStrings()
    {
        string stringsdir = "Strings/";

        string FMTStr = String.Format("{0}{1}{2}", Name, "{0}", Text.STRINGS_FILE_EXT);
        foreach (string code in Text.SupportedLangs)
        {
            string fname =
                String.Format(FMTStr, code == "en" ? "" : ("." + code)); //special case for english, which is default
            string path = Path.Combine(stringsdir, fname);
            string modPath = PathMod.HardMod(path);
            Dictionary<string, (string val, string comment)> stringDict =
                new Dictionary<string, (string val, string comment)>();
            List<MapString> baseStrings = new List<MapString>();
            populateStringTable(baseStrings, true);

            foreach (MapString str in GameStrings)
            {
                string cm, tl;
                if (!str.Comments.TryGetValue(code, out cm))
                    cm = "";
                if (!str.Translations.TryGetValue(code, out tl))
                    tl = "";
                if (tl != "" || code == "en")
                    stringDict[str.Key] = (tl, cm);
            }

            //remove the ones identical to baseStrings
            foreach (MapString str in baseStrings)
            {
                string cm, tl;
                if (!str.Comments.TryGetValue(code, out cm))
                    cm = "";
                if (!str.Translations.TryGetValue(code, out tl))
                    tl = "";
                (string val, string comment) item;
                if (stringDict.TryGetValue(str.Key, out item))
                {
                    if (item.val == tl && item.comment == cm)
                        stringDict.Remove(str.Key);
                }
            }

            Text.SaveStringResx(modPath, stringDict);


            lock (GameBase.lockObj)
                Text.SetCultureCode(DiagManager.Instance.CurSettings.Language);
        }
    }
}