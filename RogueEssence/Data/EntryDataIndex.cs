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

        private int compareNum(string key1, string key2)
        {
            int cmp = Math.Sign(Entries[key1].GetSortOrder() - Entries[key2].GetSortOrder());
            if (cmp != 0)
                return cmp;
            return String.Compare(key1, key2);
        }

        /// <summary>
        /// Maps one index to one key.
        /// </summary>
        /// <returns>List may contain null.</returns>
        public List<string> GetMappedKeys()
        {
            List<string> keys = new List<string>();

            foreach (string key in Entries.Keys)
            {
                int idx = Entries[key].GetSortOrder();
                while (idx >= keys.Count)
                    keys.Add(null);
                keys[idx] = key;
            }

            return keys;
        }

        /// <summary>
        /// Keys ordered alphabetically or numerically
        /// </summary>
        /// <param name="numeric"></param>
        /// <returns></returns>
        public List<string> GetOrderedKeys(bool numeric)
        {
            List<string> keys = new List<string>();

            foreach (string key in Entries.Keys)
                keys.Add(key);

            if (numeric)
                keys.Sort(compareNum);
            else
                keys.Sort();

            return keys;
        }

        public Dictionary<string, string> GetLocalStringArray(bool verbose = false)
        {
            Dictionary<string, string> names = new Dictionary<string, string>();

            List<string> curNames = new List<string>();
            foreach (string key in Entries.Keys)
                curNames.Add(key);
            curNames.Sort();

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

        public virtual int GetSortOrder()
        {
            return 0;
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
