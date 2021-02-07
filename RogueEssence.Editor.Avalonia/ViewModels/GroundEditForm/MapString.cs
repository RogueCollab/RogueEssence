using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev.ViewModels
{
    public class MapString : ViewModelBase
    {
        public MapString() { }
        public MapString(string key, string comment, string str)
        {
            Key = key;
            Comment = comment;
            Translations = new Dictionary<string, string>();
            Translations["en"] = str;
        }
        public MapString(string key, string comment, Dictionary<string, string> strDict)
        {
            Key = key;
            Comment = comment;
            Translations = strDict;
        }
        public string Key { get; set; }
        public string Comment { get; set; }
        public string String
        {
            get => Translations["en"];
            set => Translations["en"] = value;
        }

        public Dictionary<string, string> Translations;
    }
}
