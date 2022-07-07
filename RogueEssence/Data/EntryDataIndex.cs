using System;
using System.Collections.Generic;

namespace RogueEssence.Data
{
    [Serializable]
    public class EntryDataIndex
    {
        public int Count { get { return Entries.Count; } }

        public Dictionary<string, EntrySummary> Entries;

        public EntryDataIndex()
        {
            Entries = new Dictionary<string, EntrySummary>();
        }

        public Dictionary<string, string> GetLocalStringArray(bool verbose = false)
        {
            Dictionary<string, string> names = new Dictionary<string, string>();

            //TODO: string assets
            List<int> legacyNames = new List<int>();
            List<string> curNames = new List<string>();
            foreach (string key in Entries.Keys)
            {
                int num;
                if (Int32.TryParse(key, out num))
                    legacyNames.Add(num);
                else
                    curNames.Add(key);
            }
            legacyNames.Sort();

            foreach (int num in legacyNames)
                names[num.ToString()] = Entries[num.ToString()].GetLocalString(verbose);

            foreach (string name in curNames)
                names[name] = Entries[name].GetLocalString(verbose);

            return names;
        }
    }


    [Serializable]
    public class EntrySummary
    {
        public LocalText Name;
        public bool Released;
        public string Comment;

        public EntrySummary()
        {
            Name = new LocalText();
            Comment = "";
        }

        public EntrySummary(LocalText name, bool released, string comment)
        {
            Name = name;
            Released = released;
            Comment = comment;
        }

        public virtual string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }

        public string GetLocalString(bool verbose)
        {
            string result = Name.ToLocal();
            if (!Released)
                result = "*" + result;
            if (verbose && Comment != "")
            {
                result += "  #";
                string[] lines = Comment.Split('\n', StringSplitOptions.None);
                result += lines[0];
            }

            return result;
        }
    }


}
