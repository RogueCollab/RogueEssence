using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dev;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class TerrainTile : GameEventOwner
    {
        public override GameEventPriority.EventCause GetEventCause()
        {
            return GameEventPriority.EventCause.Terrain;
        }

        public override string GetID() { return ID; }

        public TerrainData GetData() { return DataManager.Instance.GetTerrain(ID); }
        public override string GetDisplayName() { return GetData().GetColoredName(); }


        [JsonConverter(typeof(TerrainConverter))]
        [DataType(0, DataManager.DataType.Terrain, false)]
        public string ID;
        public AutoTile TileTex;
        
        //TODO: make this an editable value in map editor... when someone wants it
        /// <summary>
        /// Prevents the texture from being overridden by the map's texturemap
        /// </summary>
        public bool StableTex;

        public TerrainTile()
        {
            ID = "";
            TileTex = new AutoTile();
        }
        public TerrainTile(string index) : this()
        {
            ID = index;
        }
        public TerrainTile(string index, bool stableTex) : this()
        {
            ID = index;
            StableTex = stableTex;
        }
        public TerrainTile(string index, AutoTile tex)
        {
            ID = index;
            TileTex = tex;
        }
        public TerrainTile(string index, AutoTile tex, bool stableTex)
        {
            ID = index;
            TileTex = tex;
            StableTex = stableTex;
        }
        protected TerrainTile(TerrainTile other)
        {
            ID = other.ID;
            TileTex = other.TileTex.Copy();
            StableTex = other.StableTex;
        }
        public TerrainTile Copy() { return new TerrainTile(this); }

        public IEnumerator<YieldInstruction> LandedOnTile(Character character)
        {
            SingleCharContext context = new SingleCharContext(character);
            DungeonScene.EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<SingleCharEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                TerrainData entry = DataManager.Instance.GetTerrain(ID);
                AddEventsToQueue(queue, maxPriority, ref nextPriority, entry.LandedOnTiles, character);
            };
            foreach (EventQueueElement<SingleCharEvent> effect in DungeonScene.IterateEvents(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                if (context.CancelState.Cancel)
                    yield break;
            }
        }

        public bool Equals(TerrainTile other)
        {
            if (other == null)
                return false;

            if (!TileTex.Equals(other.TileTex))
                return false;

            if (ID != other.ID)
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            return (obj != null) && Equals(obj as TerrainTile);
        }

        public override int GetHashCode()
        {
            return TileTex.GetHashCode() ^ ID.GetHashCode();
        }

        public override string ToString()
        {
            if (!String.IsNullOrEmpty(ID))
                return DataManager.Instance.DataIndices[DataManager.DataType.Terrain].Get(ID).Name.ToLocal();
            else
                return "[EMPTY]";
        }
    }
}
