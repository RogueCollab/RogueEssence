using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RogueEssence.Dev.ViewModels
{
    public class DevTabGameViewModel : ViewModelBase
    {
        public DevTabGameViewModel()
        {
            Skills = new ObservableCollection<string>();
            Intrinsics = new ObservableCollection<string>();
            Statuses = new ObservableCollection<string>();
            Items = new ObservableCollection<string>();
        }

        public ObservableCollection<string> Skills { get; }

        private int chosenSkill;
        public int ChosenSkill
        {
            get { return chosenSkill; }
            set { this.RaiseAndSetIfChanged(ref chosenSkill, value); }
        }

        public ObservableCollection<string> Intrinsics { get; }

        private int chosenIntrinsic;
        public int ChosenIntrinsic
        {
            get { return chosenIntrinsic; }
            set { this.RaiseAndSetIfChanged(ref chosenIntrinsic, value); }
        }

        public ObservableCollection<string> Statuses { get; }

        private int chosenStatus;
        public int ChosenStatus
        {
            get { return chosenStatus; }
            set { this.RaiseAndSetIfChanged(ref chosenStatus, value); }
        }

        public ObservableCollection<string> Items { get; }

        private int chosenItem;
        public int ChosenItem
        {
            get { return chosenItem; }
            set { this.RaiseAndSetIfChanged(ref chosenItem, value); }
        }


        public void btnSpawn_Click()
        {

        }

        public void btnDespawn_Click()
        {

        }

        public void btnSpawnItem_Click()
        {

        }

        public void btnToggleStatus_Click()
        {

        }

        public void btnLearnSkill_Click()
        {

        }

        public void btnGiveSkill_Click()
        {

        }

        public void btnSetIntrinsic_Click()
        {

        }

        public void btnGiveFoes_Click()
        {

        }
    }
}
