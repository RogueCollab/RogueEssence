using System;
using RogueElements;
using System.Collections.Generic;
using RogueEssence.LevelGen;
using RogueEssence.Dev;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using Newtonsoft.Json;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Orients all already-placed compass tiles to point to points of interest.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class DetectItemStep<T> : GenStep<T>
        where T : StairsMapGenContext
    {
        /// <summary>
        /// Tile used as compass.
        /// </summary>
        [JsonConverter(typeof(TileConverter))]
        [DataType(0, DataManager.DataType.Item, false)]
        public string FindItem;

        public DetectItemStep()
        {
        }

        public DetectItemStep(string item)
        {
            FindItem = item;
        }

        public override void Apply(T map)
        {
            foreach(MapItem item in map.Items)
            {
                if (!item.IsMoney && item.Value == FindItem)
                    return;
            }

            throw new Exception("Did not find tile " + FindItem + "!");
        }
    }
}
