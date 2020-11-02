using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev.ViewModels
{
    public class DataListFormViewModel : ViewModelBase
    {
        public DataListFormViewModel()
        {
            SearchList = new SearchListBoxViewModel();
        }

        public SearchListBoxViewModel SearchList { get; set; }
    }
}
