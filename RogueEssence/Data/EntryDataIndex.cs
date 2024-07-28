using System;
using System.Collections.Generic;

namespace RogueEssence.Data
{
    [Serializable]
    public class EntryDataIndex
    {
        public int Count { get { return entries.Count; } }

        //TODO: add the modding status of the entry: diff-modded, or not?
        private Dictionary<string, List<(Guid, EntrySummary)>> entries;

        public EntryDataIndex()
        {
            entries = new Dictionary<string, List<(Guid, EntrySummary)>>();
        }

        public void SetEntries(Dictionary<string, List<(Guid, EntrySummary)>> entries)
        {
            this.entries = entries;
        }

        public Dictionary<string, EntrySummary> GetEntriesWithoutGuid()
        {
            Dictionary<string, EntrySummary> result = new Dictionary<string, EntrySummary>();
            foreach (string key in entries.Keys)
            {
                result[key] = entries[key][0].Item2;
            }
            return result;
        }

        public EntrySummary Get(string index)
        {
            string[] components = index.Split(':');
            if (components.Length > 1)
            {
                ModHeader mod = PathMod.GetModFromNamespace(components[0]);
                string asset_id = components[1];

                List<(Guid, EntrySummary)> stack = entries[asset_id];
                foreach ((Guid, EntrySummary) pair in stack)
                {
                    if (pair.Item1 == mod.UUID)
                        return pair.Item2;
                }
                throw new KeyNotFoundException(String.Format("Invalid asset ID: {0}", index));
            }
            else
            {
                string asset_id = components[0];
                List<(Guid, EntrySummary)> stack = entries[asset_id];
                return stack[0].Item2;
            }
        }

        public IEnumerable<(Guid, EntrySummary)> IterateKey(string index)
        {
            List<(Guid, EntrySummary)> stack = entries[index];
            foreach ((Guid, EntrySummary) tuple in stack)
                yield return tuple;
        }

        public void Set(Guid uuid, string entryNum, EntrySummary entrySummary)
        {
            if (!entries.ContainsKey(entryNum))
                entries[entryNum] = new List<(Guid, EntrySummary)>();
            List<(Guid, EntrySummary)> stack = entries[entryNum];
            for (int ii = 0; ii < stack.Count; ii++)
            {
                if (stack[ii].Item1 == uuid)
                {
                    stack.RemoveAt(ii);
                    break;
                }
            }
            stack.Insert(0, (uuid, entrySummary));
        }

        public void Remove(Guid uuid, string entryNum)
        {
            List<(Guid, EntrySummary)> stack = entries[entryNum];
            for (int ii = 0; ii < stack.Count; ii++)
            {
                if (stack[ii].Item1 == uuid)
                {
                    stack.RemoveAt(ii);
                    break;
                }
            }
            if (stack.Count == 0)
                entries.Remove(entryNum);
        }

        public bool ContainsKey(string key)
        {
            return entries.ContainsKey(key);
        }

        public int CompareWithSort(string key1, string key2)
        {
            int cmp = Math.Sign(Get(key1).SortOrder - Get(key2).SortOrder);
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

            foreach (string key in entries.Keys)
            {
                int idx = Get(key).SortOrder;
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

            foreach (string key in entries.Keys)
                keys.Add(key);

            if (numeric)
                keys.Sort(CompareWithSort);
            else
                keys.Sort();

            return keys;
        }

        public Dictionary<string, EntrySummary> GetModIndex(Guid uuid)
        {
            Dictionary<string, EntrySummary> result = new Dictionary<string, EntrySummary>();
            foreach (string key in entries.Keys)
            {
                List<(Guid, EntrySummary)> stack = entries[key];
                foreach ((Guid, EntrySummary) entry in stack)
                {
                    if (entry.Item1 == uuid)
                        result[key] = entry.Item2;
                }
            }
            return result;
        }

        public Dictionary<string, string> GetLocalStringArray(bool verbose = false)
        {
            Dictionary<string, string> names = new Dictionary<string, string>();

            List<string> curNames = new List<string>();
            foreach (string key in entries.Keys)
                curNames.Add(key);
            curNames.Sort();

            foreach (string name in curNames)
                names[name] = Get(name).GetLocalString(verbose);

            return names;
        }
    }


    [Serializable]
    public class EntrySummary
    {
        public LocalText Name;
        public bool Released;
        public string Comment;
        public int SortOrder;

        public EntrySummary()
        {
            Name = new LocalText();
            Comment = "";
        }

        public EntrySummary(LocalText name, bool released, string comment, int sort = 0)
        {
            Name = name;
            Released = released;
            Comment = comment;
            SortOrder = sort;
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
