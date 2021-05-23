using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Menu;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{
    public class DevTabTravelViewModel : ViewModelBase
    {
        public DevTabTravelViewModel()
        {
            Grounds = new ObservableCollection<string>();
            Maps = new ObservableCollection<string>();
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
            set { this.SetIfChanged(ref chosenGround, value); }
        }


        public ObservableCollection<string> Maps { get; }

        private int chosenMap;
        public int ChosenMap
        {
            get { return chosenMap; }
            set { this.SetIfChanged(ref chosenMap, value); }
        }


        public ObservableCollection<string> Zones { get; }

        private int chosenZone;
        public int ChosenZone
        {
            get { return chosenZone; }
            set
            {
                this.SetIfChanged(ref chosenZone, value);
                ZoneChanged();
            }
        }

        public ObservableCollection<string> Structures { get; }

        private int chosenStructure;
        public int ChosenStructure
        {
            get { return chosenStructure; }
            set { this.SetIfChanged(ref chosenStructure, value);
                StructureChanged();
            }
        }

        public ObservableCollection<string> Floors { get; }

        private int chosenFloor;
        public int ChosenFloor
        {
            get { return chosenFloor; }
            set { this.SetIfChanged(ref chosenFloor, value); }
        }

        private void ZoneChanged()
        {
            if (chosenZone == -1)
                return;

            lock (GameBase.lockObj)
            {
                int temp = chosenStructure;
                Structures.Clear();
                ZoneData zone = DataManager.Instance.GetZone(chosenZone);
                for (int ii = 0; ii < zone.Segments.Count; ii++)
                    Structures.Add(ii.ToString()/* + " - " + zone.Structures[ii].Name.ToLocal()*/);
                ChosenStructure = Math.Clamp(temp, 0, Structures.Count - 1);
            }
        }

        private void StructureChanged()
        {
            lock (GameBase.lockObj)
            {
                if (chosenStructure == -1)
                    return;

                int temp = chosenFloor;
                floorIDs.Clear();
                Floors.Clear();
                ZoneData zone = DataManager.Instance.GetZone(chosenZone);
                foreach (int ii in zone.Segments[chosenStructure].GetFloorIDs())
                {
                    Floors.Add(ii.ToString()/* + " - " + zone.Structures[cbStructure.SelectedIndex].Floors[ii].Name.ToLocal()*/);
                    floorIDs.Add(ii);
                }
                ChosenFloor = Math.Clamp(temp, 0, Floors.Count - 1);
            }
        }

        public void btnEnterGround_Click()
        {
            lock (GameBase.lockObj)
            {
                DevForm.SetConfig("GroundChoice", chosenGround);
                MenuManager.Instance.ClearMenus();
                GameManager.Instance.SceneOutcome = GameManager.Instance.TestWarp(Grounds[chosenGround], true, RogueElements.MathUtils.Rand.NextUInt64());
            }
        }


        public void btnEnterMap_Click()
        {
            lock (GameBase.lockObj)
            {
                DevForm.SetConfig("MapChoice", chosenMap);
                MenuManager.Instance.ClearMenus();
                GameManager.Instance.SceneOutcome = GameManager.Instance.TestWarp(Maps[chosenMap], false, RogueElements.MathUtils.Rand.NextUInt64());
            }
        }

        public void btnEnterDungeon_Click()
        {
            lock (GameBase.lockObj)
            {
                DevForm.SetConfig("ZoneChoice", chosenZone);
                DevForm.SetConfig("StructChoice", chosenStructure);
                DevForm.SetConfig("FloorChoice", chosenFloor);
                MenuManager.Instance.ClearMenus();
                GameManager.Instance.SceneOutcome = GameManager.Instance.DebugWarp(new ZoneLoc(chosenZone, new SegLoc(chosenStructure, floorIDs[chosenFloor])), RogueElements.MathUtils.Rand.NextUInt64());
            }
        }

    }
}
