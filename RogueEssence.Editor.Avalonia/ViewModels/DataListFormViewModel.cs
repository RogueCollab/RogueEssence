using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace RogueEssence.Dev.ViewModels
{
    public class DataListFormViewModel : ViewModelBase
    {
        public delegate void ItemSelectedEvent();
        public event ItemSelectedEvent SelectedOKEvent;
        public event ItemSelectedEvent SelectedAddEvent;

        public DataListFormViewModel()
        {
            SearchList = new SearchListBoxViewModel();

            SearchList.SetName("Select Item");
            SearchList.ListBoxMouseDoubleClick += slbEntries_MouseDoubleClick;
        }

        public SearchListBoxViewModel SearchList { get; set; }

        private string name;
        public string Name
        {
            get { return name; }
            set { this.SetIfChanged(ref name, value); }
        }


        public void AddEntries(string[] entries)
        {
            for (int ii = 0; ii < entries.Length; ii++)
                SearchList.AddItem(ii.ToString("D3") + ": " + entries[ii]);
        }

        public void ModifyEntry(int index, string entry)
        {
            SearchList.SetInternalEntry(index, index.ToString("D3") + ": " + entry);
        }

        public void AddEntry(string entry)
        {
            SearchList.AddItem(SearchList.Count.ToString("D3") + ": " + entry);
        }

        public void btnAdd_Click()
        {
            SelectedAddEvent?.Invoke();
        }

        public void btnDelete_Click()
        {

        }

        public void slbEntries_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            //int index = slbEntries.IndexFromPoint(e.Location);
            //if (index != ListBox.NoMatches)
            //{
            //    ChosenEntry = slbEntries.GetInternalIndex(index);
            //    SelectedOKEvent?.Invoke();
            //}

            if (SearchList.InternalIndex > -1)
                SelectedOKEvent?.Invoke();
        }
    }
}
