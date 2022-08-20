using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Data;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace RogueEssence.Dev.ViewModels
{
    public class ModConfigViewModel : ViewModelBase
    {
        public ModConfigViewModel(ModHeader header)
        {
            Name = header.Name;
            Namespace = header.Namespace;
            UUID = header.UUID.ToString().ToUpper();
            Version = header.Version.ToString();

            ModTypes = new ObservableCollection<string>();
            for (int ii = 0; ii < (int)PathMod.ModType.Count; ii++)
                ModTypes.Add(((PathMod.ModType)ii).ToLocal());
            ChosenModType = (int)header.ModType;
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

        private string uuid;
        public string UUID
        {
            get => uuid;
            set => this.SetIfChanged(ref uuid, value);
        }


        private string version;
        public string Version
        {
            get => version;
            set => this.SetIfChanged(ref version, value);
        }

        public ObservableCollection<string> ModTypes { get; }

        private int chosenModType;
        public int ChosenModType
        {
            get => chosenModType;
            set => this.SetIfChanged(ref chosenModType, value);
        }


        public void btnRegenUUID_Click()
        {
            UUID = Guid.NewGuid().ToString().ToUpper();
        }
    }
}
