using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Data;
using ReactiveUI;
using System.Collections.ObjectModel;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{
    public class ModNameWindowViewModel : ViewModelBase
    {
        public ModNameWindowViewModel()
        {
            Name = "";
            Namespace = "";
        }

        private string name;
        public string Name
        {
            get => name;
            set => this.SetIfChanged(ref name, value);
        }

        
        private string editNamespace;
        public string Namespace
        {
            get => editNamespace;
            set => this.SetIfChanged(ref editNamespace, value);
        }
    }
}
