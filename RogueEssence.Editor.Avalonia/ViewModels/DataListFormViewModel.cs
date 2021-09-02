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

        public ObservableCollection<DataOpContainer> OpList { get; }

        public DataListFormViewModel()
        {
            SearchList = new SearchListBoxViewModel();
            OpList = new ObservableCollection<DataOpContainer>();

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


        public void SetEntries(string[] entries)
        {
            SearchList.Clear();
            for (int ii = 0; ii < entries.Length; ii++)
                SearchList.AddItem(ii.ToString("D3") + ": " + entries[ii]);
        }

        public void SetOps(params DataOpContainer[] ops)
        {
            DataOpContainer edit = new DataOpContainer("_Edit", null, ops);
            OpList.Add(edit);
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

        //public void btnDelete_Click()
        //{

        //}

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
