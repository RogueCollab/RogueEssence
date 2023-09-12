using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using System.Threading.Tasks;

namespace RogueEssence.Dev.ViewModels
{
    public class DataOpContainer
    {
        public delegate Task TaskAction();
        public TaskAction CommandAction;
        public string Name { get; private set; }
        public ObservableCollection<DataOpContainer> Items { get; }


        public DataOpContainer(string name, TaskAction command, params DataOpContainer[] items)
        {
            Name = name;
            CommandAction = command;
            Items = new ObservableCollection<DataOpContainer>();
            foreach (DataOpContainer op in items)
                Items.Add(op);

        }

        public async Task Command()
        {
            await CommandAction();
        }
    }

    public class DataListFormViewModel : ViewModelBase
    {
        public event Action SelectedOKEvent;
        public event Action SelectedAddEvent;
        public event Action SelectedDeleteEvent;

        public ObservableCollection<DataOpContainer> OpList { get; }

        private List<string> keys;

        public DataListFormViewModel()
        {
            SearchList = new SearchListBoxViewModel();
            OpList = new ObservableCollection<DataOpContainer>();

            SearchList.SetName("Select Item");
            SearchList.ListBoxMouseDoubleClick += slbEntries_MouseDoubleClick;

            keys = new List<string>();
        }

        public SearchListBoxViewModel SearchList { get; set; }

        public string ChosenAsset { get { return SearchList.InternalIndex > -1 ? keys[SearchList.InternalIndex] : null; } }

        private string name;
        public string Name
        {
            get { return name; }
            set { this.SetIfChanged(ref name, value); }
        }

        public void SetEntries(Dictionary<string, string> entries)
        {
            SearchList.Clear();
            keys.Clear();
            List<string> items = new List<string>();
            foreach (string key in entries.Keys)
            {
                keys.Add(key);
                items.Add(key + ": " + entries[key]);
            }
            SearchList.SetItems(items);
        }

        public void SetOps(params DataOpContainer[] ops)
        {
            DataOpContainer edit = new DataOpContainer("_Edit", null, ops);
            OpList.Add(edit);
        }

        public void ModifyEntry(string index, string entry)
        {
            int intIndex = keys.IndexOf(index);
            SearchList.SetInternalEntry(intIndex, index + ": " + entry);
        }

        public void AddEntry(string key, string entry)
        {
            keys.Add(key);
            SearchList.AddItem(key + ": " + entry);
        }

        public void DeleteEntry(string key)
        {
            int idx = keys.IndexOf(key);
            keys.RemoveAt(idx);
            SearchList.RemoveInternalAt(idx);
        }

        public void btnAdd_Click()
        {
            SelectedAddEvent?.Invoke();
        }

        public void btnDelete_Click()
        {
            SelectedDeleteEvent?.Invoke();
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
