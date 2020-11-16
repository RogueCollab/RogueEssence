using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Menu;

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
            floorIDs = new List<int>();
        }

        private List<int> floorIDs;

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
            set
            {
                this.RaiseAndSetIfChanged(ref chosenZone, value);
                ZoneChanged();
            }
        }

        public ObservableCollection<string> Structures { get; }

        private int chosenStructure;
        public int ChosenStructure
        {
            get { return chosenStructure; }
            set { this.RaiseAndSetIfChanged(ref chosenStructure, value);
                StructureChanged();
            }
        }

        public ObservableCollection<string> Floors { get; }

        private int chosenFloor;
        public int ChosenFloor
        {
            get { return chosenFloor; }
            set { this.RaiseAndSetIfChanged(ref chosenFloor, value); }
        }

        private void ZoneChanged()
        {
            int temp = chosenZone;
            Structures.Clear();
            ZoneData zone = DataManager.Instance.GetZone(chosenZone);
            for (int ii = 0; ii < zone.Structures.Count; ii++)
                Structures.Add(ii.ToString()/* + " - " + zone.Structures[ii].Name.ToLocal()*/);
            ChosenStructure = Math.Clamp(temp, 0, Structures.Count - 1);
        }

        private void StructureChanged()
        {
            if (chosenStructure == -1)
                return;

            int temp = chosenFloor;
            floorIDs.Clear();
            Floors.Clear();
            ZoneData zone = DataManager.Instance.GetZone(chosenZone);
            foreach (int ii in zone.Structures[chosenStructure].GetFloorIDs())
            {
                Floors.Add(ii.ToString()/* + " - " + zone.Structures[cbStructure.SelectedIndex].Floors[ii].Name.ToLocal()*/);
                floorIDs.Add(ii);
            }
            ChosenFloor = Math.Clamp(temp, 0, Floors.Count - 1);
        }

        public void btnEnterMap_Click()
        {
            //Registry.SetValue(DiagManager.REG_PATH, "MapChoice", cbMaps.SelectedIndex);
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.DebugWarp(new ZoneLoc(1, new SegLoc(-1, chosenGround)), RogueElements.MathUtils.Rand.NextUInt64());
        }

        public void btnEnterDungeon_Click()
        {
            //Registry.SetValue(DiagManager.REG_PATH, "ZoneChoice", cbZones.SelectedIndex);
            //Registry.SetValue(DiagManager.REG_PATH, "StructChoice", cbStructure.SelectedIndex);
            //Registry.SetValue(DiagManager.REG_PATH, "FloorChoice", cbFloor.SelectedIndex);
            MenuManager.Instance.ClearMenus();
            GameManager.Instance.SceneOutcome = GameManager.Instance.DebugWarp(new ZoneLoc(chosenZone, new SegLoc(chosenStructure, floorIDs[chosenFloor])), RogueElements.MathUtils.Rand.NextUInt64());
        }

    }
}
