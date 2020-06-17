using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class TerrainTile : GameEventOwner
    {
        public override GameEventPriority.EventCause GetEventCause()
        {
            return GameEventPriority.EventCause.Terrain;
        }

        public override int GetID() { return ID; }

        public TerrainData GetData()
        {
            return DataManager.Instance.GetTerrain(ID);
        }
        public override string GetName() { return GetData().Name.ToLocal(); }

        public int ID;
        public AutoTile TileTex;
        public TerrainTile()
        {
            TileTex = new AutoTile();
        }
        public TerrainTile(int index) : this()
        {
            ID = index;
        }
        protected TerrainTile(TerrainTile other)
        {
            ID = other.ID;
            TileTex = other.TileTex.Copy();
        }
        public TerrainTile Copy() { return new TerrainTile(this); }

        public IEnumerator<YieldInstruction> LandedOnTile(Character character)
        {
            DungeonScene.EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, SingleCharEvent>> queue, int maxPriority, ref int nextPriority) =>
            {
                TerrainData entry = DataManager.Instance.GetTerrain(ID);
                AddEventsToQueue(queue, maxPriority, ref nextPriority, entry.LandedOnTiles);
            };
            foreach (Tuple<GameEventOwner, Character, SingleCharEvent> effect in DungeonScene.IterateEvents(function))
                yield return CoroutineManager.Instance.StartCoroutine(effect.Item3.Apply(effect.Item1, effect.Item2, character));
        }
    }
}
