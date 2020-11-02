using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace RogueEssence.Dev.ViewModels
{
    public class SearchListBoxViewModel : ViewModelBase
    {
        public SearchListBoxViewModel()
        {
            SearchItems = new ObservableCollection<string>();
            DataName = "";
            SearchText = "";
        }

        public ObservableCollection<string> SearchItems { get; }

        public string DataName { get; set; }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set { this.RaiseAndSetIfChanged(ref searchText, value); }
        }

        public void btnAdd_Click()
        {

        }
        public void btnDelete_Click()
        {

        }
    }
}
