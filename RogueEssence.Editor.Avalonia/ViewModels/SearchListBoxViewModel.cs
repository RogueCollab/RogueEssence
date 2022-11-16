using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using Avalonia.Controls;

namespace RogueEssence.Dev.ViewModels
{
    public class SearchListBoxViewModel : ViewModelBase
    {
        public SearchListBoxViewModel()
        {
            entries = new List<string>();
            entryMap = new List<int>();
            SearchItems = new ObservableCollection<string>();
            DataName = "";
            SearchText = "";
        }

        //total items in the box
        private List<string> entries;
        //maps index from the search list to index in the entries list
        private List<int> entryMap;

        private string dataName;
        public string DataName
        {
            get { return dataName; }
            set { this.SetIfChanged(ref dataName, value); }
        }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set { this.SetIfChanged(ref searchText, value); }
        }

        public ObservableCollection<string> SearchItems { get; }
        public int Count => entries.Count;


        private int selectedSearchIndex;
        public int SelectedSearchIndex
        {
            get { return selectedSearchIndex; }
            set
            {
                InternalIndex = (value > -1 && value < entryMap.Count) ? entryMap[value] : -1;
                this.RaiseAndSet(ref selectedSearchIndex, value);
                SelectedIndexChanged?.Invoke();
            }
        }

        public int InternalIndex { get; private set; }


        public event EventHandler<RoutedEventArgs> ListBoxMouseDoubleClick;
        public event Action SelectedIndexChanged;

        public void SetName(string name)
        {
            DataName = name + ":";
        }

        public void AddItem(string item)
        {
            entries.Add(item);

            if (SearchText == "" || entries[entries.Count - 1].IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) > -1)
            {
                SearchItems.Add(entries[entries.Count - 1]);
                entryMap.Add(entries.Count - 1);
            }
        }
        public void Clear()
        {
            entries.Clear();

            SearchItems.Clear();
            entryMap.Clear();
            SearchText = "";
        }

        public void RemoveAt(int index)
        {
            RemoveInternalAt(entryMap[index]);
        }

        public string GetItem(int index)
        {
            return entries[entryMap[index]];
        }

        public void SetItem(int index, string entry)
        {
            entries[entryMap[index]] = entry;

            if (SearchText == "" || entries[entryMap[index]].IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) > -1)
            {
                SearchItems[index] = entry;
            }
            else
            {
                SearchItems.RemoveAt(index);
                entryMap.RemoveAt(index);
            }
        }

        public int GetInternalIndex(int index)
        {
            return entryMap[index];
        }

        public string GetInternalEntry(int internalIndex)
        {
            return entries[internalIndex];
        }

        public void SetInternalEntry(int internalIndex, string entry)
        {
            bool oldAppears = (SearchText == "" || entries[internalIndex].IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) > -1);
            bool newAppears = (SearchText == "" || entry.IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) > -1);
            entries[internalIndex] = entry;

            int shownIndex = entryMap.IndexOf(internalIndex);

            if (oldAppears && newAppears)
            {
                //change
                SearchItems[shownIndex] = entry;
            }
            else if (oldAppears)
            {
                //remove
                SearchItems.RemoveAt(shownIndex);
                entryMap.RemoveAt(shownIndex);
            }
            else if (newAppears)
            {
                //add
                for (int ii = 0; ii < entryMap.Count; ii++)
                {
                    if (entryMap[ii] < internalIndex)
                    {
                        SearchItems.Insert(ii, entry);
                        entryMap.Insert(ii, internalIndex);
                        break;
                    }
                }
            }
        }

        public void RemoveInternalAt(int internalIndex)
        {
            entries.RemoveAt(internalIndex);

            int shownIndex = entryMap.IndexOf(internalIndex);
            if (shownIndex > -1)
            {
                //remove
                SearchItems.RemoveAt(shownIndex);
                entryMap.RemoveAt(shownIndex);
            }

            for (int ii = 0; ii < entryMap.Count; ii++)
            {
                if (entryMap[ii] > internalIndex)
                    entryMap[ii] = entryMap[ii] - 1;
            }
        }

        private void RefreshFilter()
        {
            int internalIndex = -1;
            if (SelectedSearchIndex > -1)
                internalIndex = InternalIndex;
            SearchItems.Clear();
            entryMap.Clear();

            int index = -1;
            for (int ii = 0; ii < entries.Count; ii++)
            {
                if (SearchText == "" || entries[ii].IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) > -1)
                {
                    entryMap.Add(ii);
                    SearchItems.Add(entries[ii]);
                    if (ii == internalIndex)
                        index = entryMap.Count - 1;
                }
            }
            if (index > -1)
                SelectedSearchIndex = index;
        }

        //public int IndexFromPoint(Point p)
        //{
        //    return lbxItems.IndexFromPoint(p);
        //}

        public void txtSearch_TextChanged(string text)
        {
            RefreshFilter();
        }

        public void lbxItems_DoubleClick(object sender, RoutedEventArgs e)
        {
            ListBoxMouseDoubleClick?.Invoke(sender, e);
        }
    }
}
