using System;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;

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

        public StateCollection<MapStatusState> StatusStates;

        public SwitchOffEmitter Emitter;
        public bool Hidden;

        public MapStatus() : base()
        {
            StatusStates = new StateCollection<MapStatusState>();
            Emitter = new Content.EmptySwitchOffEmitter();
        }
        public MapStatus(int index) : this()
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

