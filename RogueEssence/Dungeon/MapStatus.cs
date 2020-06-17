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
        public override string GetName() { return DataManager.Instance.GetMapStatus(ID).Name.ToLocal(); }

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


        public void EnqueueOnAddMapStart(StablePriorityQueue<GameEventPriority, IEnumerator<YieldInstruction>> instructionQueue, int maxPriority, ref int nextPriority)
        {
            int oldPriority = nextPriority;
            StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, SingleCharEvent>> queue = new StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, SingleCharEvent>>();

            MapStatusData entry = DataManager.Instance.GetMapStatus(ID);
            AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, entry.OnMapStarts);

            if (nextPriority != oldPriority)
                instructionQueue.Clear();
            while (queue.Count > 0)
            {
                GameEventPriority priority = queue.FrontPriority();
                Tuple<GameEventOwner, Character, SingleCharEvent> effect = queue.Dequeue();
                instructionQueue.Enqueue(priority, effect.Item3.Apply(effect.Item1, effect.Item2, null));
            }
        }


        public void EnqueueOnAddMapTurnEnd(StablePriorityQueue<GameEventPriority, IEnumerator<YieldInstruction>> instructionQueue, int maxPriority, ref int nextPriority)
        {
            int oldPriority = nextPriority;
            StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, SingleCharEvent>> queue = new StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, SingleCharEvent>>();

            MapStatusData entry = DataManager.Instance.GetMapStatus(ID);
            AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, entry.OnMapTurnEnds);

            if (nextPriority != oldPriority)
                instructionQueue.Clear();
            while (queue.Count > 0)
            {
                GameEventPriority priority = queue.FrontPriority();
                Tuple<GameEventOwner, Character, SingleCharEvent> effect = queue.Dequeue();
                instructionQueue.Enqueue(priority, effect.Item3.Apply(effect.Item1, effect.Item2, null));
            }
        }


        public void EnqueueOnAddMapStatus(StablePriorityQueue<GameEventPriority, IEnumerator<YieldInstruction>> instructionQueue, int maxPriority, ref int nextPriority, MapStatus status, bool msg)
        {
            int oldPriority = nextPriority;
            StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, MapStatusGivenEvent>> queue = new StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, MapStatusGivenEvent>>();

            MapStatusData entry = DataManager.Instance.GetMapStatus(ID);
            AddEventsToQueue<MapStatusGivenEvent>(queue, maxPriority, ref nextPriority, entry.OnMapStatusAdds);

            if (nextPriority != oldPriority)
                instructionQueue.Clear();
            while (queue.Count > 0)
            {
                GameEventPriority priority = queue.FrontPriority();
                Tuple<GameEventOwner, Character, MapStatusGivenEvent> effect = queue.Dequeue();
                instructionQueue.Enqueue(priority, effect.Item3.Apply(effect.Item1, effect.Item2, null, status, msg));
            }
        }

        public void EnqueueOnRemoveMapStatus(StablePriorityQueue<GameEventPriority, IEnumerator<YieldInstruction>> instructionQueue, int maxPriority, ref int nextPriority, MapStatus status, bool msg)
        {
            int oldPriority = nextPriority;
            StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, MapStatusGivenEvent>> queue = new StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, MapStatusGivenEvent>>();

            MapStatusData entry = DataManager.Instance.GetMapStatus(ID);
            AddEventsToQueue<MapStatusGivenEvent>(queue, maxPriority, ref nextPriority, entry.OnMapStatusRemoves);

            if (nextPriority != oldPriority)
                instructionQueue.Clear();
            while (queue.Count > 0)
            {
                GameEventPriority priority = queue.FrontPriority();
                Tuple<GameEventOwner, Character, MapStatusGivenEvent> effect = queue.Dequeue();
                instructionQueue.Enqueue(priority, effect.Item3.Apply(effect.Item1, effect.Item2, null, status, msg));
            }
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

