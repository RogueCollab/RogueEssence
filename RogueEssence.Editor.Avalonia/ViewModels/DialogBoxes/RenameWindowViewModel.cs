using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;

namespace RogueEssence.Dev.ViewModels
{
    public class RenameWindowViewModel : ViewModelBase
    {
        public RenameWindowViewModel()
        {
            Name = "";
        }

        private string name;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

    }
}
