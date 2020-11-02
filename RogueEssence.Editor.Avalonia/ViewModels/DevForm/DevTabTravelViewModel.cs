using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace RogueEssence.Dev.ViewModels
{
    public class DevTabTravelViewModel : ViewModelBase
    {
        public DevTabTravelViewModel()
        {
            Grounds = new ObservableCollection<string>();
            Zones = new ObservableCollection<string>();
            Structures = new ObservableCollection<string>();
            Floors = new ObservableCollection<string>();
        }

        public ObservableCollection<string> Grounds { get; }

        private int chosenGround;
        public int ChosenGround
        {
            get { return chosenGround; }
            set { this.RaiseAndSetIfChanged(ref chosenGround, value); }
        }


        public ObservableCollection<string> Zones { get; }

        private int chosenZone;
        public int ChosenZone
        {
            get { return chosenZone; }
            set { this.RaiseAndSetIfChanged(ref chosenZone, value); }
        }

        public ObservableCollection<string> Structures { get; }

        private int chosenStructure;
        public int ChosenStructure
        {
            get { return chosenStructure; }
            set { this.RaiseAndSetIfChanged(ref chosenStructure, value); }
        }

        public ObservableCollection<string> Floors { get; }

        private int chosenFloor;
        public int ChosenFloor
        {
            get { return chosenFloor; }
            set { this.RaiseAndSetIfChanged(ref chosenFloor, value); }
        }

        public void btnEnterMap_Click()
        {

        }

        public void btnEnterDungeon_Click()
        {

        }

    }
}
