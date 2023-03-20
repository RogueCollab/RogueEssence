using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Menu;
using RogueEssence.Dev.Views;
using RogueEssence.LevelGen;
using RogueEssence.Script;
using RogueElements;

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

        public void ReloadZones()
        {
            Dictionary<string, string> entry_names = DataManager.Instance.DataIndices[DataManager.DataType.Zone].GetLocalStringArray(true);
            Zones.Clear();
            foreach (string key in entry_names.Keys)
                Zones.Add(key + ": " + entry_names[key]);
            ChosenZone = -1;
            ChosenZone = Math.Min(Math.Max(DevForm.GetConfig("ZoneChoice", 0), 0), Zones.Count - 1);

            ChosenStructure = -1;
            ChosenStructure = Math.Min(Math.Max(DevForm.GetConfig("StructChoice", 0), 0), Structures.Count - 1);

            ChosenFloor = -1;
            ChosenFloor = Math.Min(Math.Max(DevForm.GetConfig("FloorChoice", 0), 0), Floors.Count - 1);

            ChosenGround = -1;
            ChosenGround = Math.Min(Math.Max(DevForm.GetConfig("GroundChoice", 0), 0), Grounds.Count - 1);
        }


        private bool debugGen;
        public bool DebugGen
        {
            get { return debugGen; }
            set
            {
                this.SetIfChanged(ref debugGen, value);
                DiagManager.Instance.ListenGen = value;
            }
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

        private ObservableCollection<string> structures;
        public ObservableCollection<string> Structures
        {
            get { return structures; }
            set { this.SetIfChanged(ref structures, value); }
        }

        private int chosenStructure;
        public int ChosenStructure
        {
            get { return chosenStructure; }
            set { this.SetIfChanged(ref chosenStructure, value);
                StructureChanged();
            }
        }

        private ObservableCollection<string> floors;
        public ObservableCollection<string> Floors
        {
            get { return floors; }
            set { this.SetIfChanged(ref floors, value); }
        }

        private int chosenFloor;
        public int ChosenFloor
        {
            get { return chosenFloor; }
            set { this.SetIfChanged(ref chosenFloor, value); }
        }

        private List<int> floorIDs;


        private ObservableCollection<string> grounds;
        public ObservableCollection<string> Grounds
        {
            get { return grounds; }
            set { this.SetIfChanged(ref grounds, value); }
        }

        private int chosenGround;
        public int ChosenGround
        {
            get { return chosenGround; }
            set { this.SetIfChanged(ref chosenGround, value); }
        }



        private void ZoneChanged()
        {
            if (chosenZone == -1)
                return;

            lock (GameBase.lockObj)
            {
                int tempStructure = chosenStructure;
                int tempGround = chosenGround;

                string chosen_entry = Zones[chosenZone].Split(':')[0];
                ZoneData zone = DataManager.Instance.GetZone(chosen_entry);
                ObservableCollection<string> newStructures = new ObservableCollection<string>();
                for (int ii = 0; ii < zone.Segments.Count; ii++)
                    newStructures.Add(ii.ToString("D2") + ": " + getSegmentString(zone.Segments[ii]));
                Structures = newStructures;
                ChosenStructure = Math.Min(Math.Max(tempStructure, 0), Structures.Count - 1);

                ObservableCollection<string> newGrounds = new ObservableCollection<string>();
                for (int ii = 0; ii < zone.GroundMaps.Count; ii++)
                    newGrounds.Add(ii.ToString("D2") + ": " + zone.GroundMaps[ii]);
                Grounds = newGrounds;
                ChosenGround = Math.Min(Math.Max(tempGround, 0), Grounds.Count - 1);
            }
        }

        private string getSegmentString(ZoneSegmentBase segment)
        {
            foreach (ZoneStep step in segment.ZoneSteps)
            {
                var startStep = step as FloorNameIDZoneStep;
                if (startStep != null)
                    return LocalText.FormatLocalText(startStep.Name, "[X]").ToLocal().Replace('\n', ' ');
            }
            return String.Format("[{0}] {1}F", segment.GetType().GetFormattedTypeName(), "[X]");
        }

        private void StructureChanged()
        {
            lock (GameBase.lockObj)
            {
                int temp = chosenFloor;
                floorIDs.Clear();
                ObservableCollection<string> newFloors = new ObservableCollection<string>();

                if (chosenZone > -1 && chosenStructure > -1)
                {
                    string chosen_entry = Zones[chosenZone].Split(':')[0];
                    ZoneData zone = DataManager.Instance.GetZone(chosen_entry);
                    foreach (int ii in zone.Segments[chosenStructure].GetFloorIDs())
                    {
                        newFloors.Add(ii.ToString("D2") + ": " + getFloorString(zone.Segments[chosenStructure], ii));
                        floorIDs.Add(ii);
                    }
                }

                Floors = newFloors;
                ChosenFloor = Math.Min(Math.Max(temp, 0), Floors.Count - 1);
            }
        }


        private string getFloorString(ZoneSegmentBase segment, int floorID)
        {
            foreach (ZoneStep step in segment.ZoneSteps)
            {
                var startStep = step as FloorNameIDZoneStep;
                if (startStep != null)
                    return LocalText.FormatLocalText(startStep.Name, (floorID + 1).ToString()).ToLocal().Replace('\n', ' ');
            }
            return String.Format("[{0}] {1}F", segment.GetType().GetFormattedTypeName(), (floorID + 1).ToString());
        }

        public void btnEnterGround_Click()
        {
            lock (GameBase.lockObj)
            {
                DevForm.SetConfig("ZoneChoice", chosenZone);
                DevForm.SetConfig("GroundChoice", chosenGround);
                LuaEngine.Instance.BreakScripts();
                MenuManager.Instance.ClearMenus();
                // Remove common cutscene variables
                // While these should technically be untouched, in practice a travel attempt means breaking the cutscene and resetting the variables to normal.
                Content.GraphicsManager.GlobalIdle = Content.GraphicsManager.IdleAction;
                if (DataManager.Instance.Save != null)
                    DataManager.Instance.Save.CutsceneMode = false;

                string chosen_entry = Zones[chosenZone].Split(':')[0];
                GameManager.Instance.SceneOutcome = GameManager.Instance.DebugWarp(new ZoneLoc(chosen_entry, new SegLoc(-1, chosenGround)), RogueElements.MathUtils.Rand.NextUInt64());
            }
        }

        public void btnEnterDungeon_Click()
        {
            lock (GameBase.lockObj)
            {
                DevForm.SetConfig("ZoneChoice", chosenZone);
                DevForm.SetConfig("StructChoice", chosenStructure);
                DevForm.SetConfig("FloorChoice", chosenFloor);
                LuaEngine.Instance.BreakScripts();
                MenuManager.Instance.ClearMenus();
                // Remove common cutscene variables
                // While these should technically be untouched, in practice a travel attempt means breaking the cutscene and resetting the variables to normal.
                Content.GraphicsManager.GlobalIdle = Content.GraphicsManager.IdleAction;
                if (DataManager.Instance.Save != null)
                    DataManager.Instance.Save.CutsceneMode = false;

                string chosen_entry = Zones[chosenZone].Split(':')[0];
                GameManager.Instance.SceneOutcome = GameManager.Instance.DebugWarp(new ZoneLoc(chosen_entry, new SegLoc(chosenStructure, floorIDs[chosenFloor])), RogueElements.MathUtils.Rand.NextUInt64());
            }
        }

        public void btnReloadMap_Click()
        {
            lock (GameBase.lockObj)
            {
                //only if in a dungeon
                if (ZoneManager.Instance.CurrentZone != null)
                {
                    if (ZoneManager.Instance.InDevZone)
                    {
                        GameManager.Instance.SceneOutcome = GameManager.Instance.TestWarp(ZoneManager.Instance.CurrentMap.AssetName, false, MathUtils.Rand.NextUInt64());
                    }
                    else if (ZoneManager.Instance.CurrentMapID.Segment > -1)
                    {
                        //reload the dungeon map
                        ZoneManager.Instance.CurrentZone.MapsLoaded--;
                        GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToZone(new ZoneLoc(ZoneManager.Instance.CurrentZoneID, ZoneManager.Instance.CurrentMapID));
                    }
                    else
                    {
                        //reload ground map
                        GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToZone(new ZoneLoc(ZoneManager.Instance.CurrentZoneID, ZoneManager.Instance.CurrentMapID));
                    }
                }
            }
        }

    }
}
