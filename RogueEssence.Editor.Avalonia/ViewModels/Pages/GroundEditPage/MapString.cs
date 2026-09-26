using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace RogueEssence.Dev.ViewModels
{
    public class MapString : ViewModelBase
    {
        public MapString() { }
        public MapString(string key, string comment, string str)
        {
            Key = key;
            Comments = new Dictionary<string, string>();
            Comments["en"] = comment;
            Translations = new Dictionary<string, string>();
            Translations["en"] = str;
        }
        public MapString(string key, Dictionary<string, string> comments, Dictionary<string, string> strDict)
        {
            Key = key;
            Comments = comments;
            Translations = strDict;
        }
        public string Key { get; set; }
        public string Comment
        {
            get => Comments["en"];
            set => Comments["en"] = value;
        }
        public string String
        {
            get => Translations["en"];
            set => Translations["en"] = value;
        }

        public Dictionary<string, string> Comments;

        public Dictionary<string, string> Translations;
    }
}
