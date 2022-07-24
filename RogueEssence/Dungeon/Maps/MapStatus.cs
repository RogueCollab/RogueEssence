using System;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dev;
using Newtonsoft.Json;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class MapStatus : PassiveActive
    {
        public override GameEventPriority.EventCause GetEventCause()
        {
            return GameEventPriority.EventCause.MapState;
        }
        public override PassiveData GetData() { return DataManager.Instance.GetMapStatus(ID); }
        public override string GetDisplayName() { return DataManager.Instance.GetMapStatus(ID).Name.ToLocal(); }

        public override string GetID() { return ID; }

        [JsonConverter(typeof(MapStatusConverter))]
        [DataType(0, DataManager.DataType.MapStatus, false)]
        public string ID { get; set; }
        public StateCollection<MapStatusState> StatusStates;

        public SwitchOffEmitter Emitter;
        public bool Hidden;

        public MapStatus() : base()
        {
            ID = "";
            StatusStates = new StateCollection<MapStatusState>();
            Emitter = new Content.EmptySwitchOffEmitter();
        }
        public MapStatus(string index) : this()
        {
            ID = index;
        }

        public void LoadFromData()
        {
            MapStatusData entry = DataManager.Instance.GetMapStatus(ID);

            foreach (MapStatusState state in entry.StatusStates)
            {
                if (!StatusStates.Contains(state.GetType()))
                    StatusStates.Set(state.Clone<MapStatusState>());
            }

            Emitter = (SwitchOffEmitter)entry.Emitter.Clone();//Clone Use Case; convert to Instantiate?
            Hidden = entry.DefaultHidden;
        }


        public void StartEmitter(List<IFinishableSprite>[] effects)
        {
            Emitter.SetupEmit(Loc.Zero, Loc.Zero, Dir8.Down);
            effects[(int)DrawLayer.NoDraw].Add(Emitter);
        }

        public void EndEmitter()
        {
            Emitter.SwitchOff();
        }
    }
}

