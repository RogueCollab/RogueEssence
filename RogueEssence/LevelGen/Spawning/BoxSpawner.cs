using RogueElements;
using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;
using Newtonsoft.Json;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Spawns a box with a random item in it.
    /// </summary>
    /// <typeparam name="TGenContext"></typeparam>
    [Serializable]
    public class BoxSpawner<TGenContext> : IStepSpawner<TGenContext, MapItem>
        where TGenContext : IGenContext
    {
        public BoxSpawner()
        {
        }

        public BoxSpawner(string id, IStepSpawner<TGenContext, MapItem> spawner)
        {
            this.BaseSpawner = spawner;
            this.BoxID = id;
        }

        /// <summary>
        /// The item ID of the box containing the item.
        /// </summary>
        [JsonConverter(typeof(ItemConverter))]
        [DataType(0, Data.DataManager.DataType.Item, false)]
        public string BoxID { get; set; }

        /// <summary>
        /// The spawner that decides what item the box holds.
        /// </summary>
        public IStepSpawner<TGenContext, MapItem> BaseSpawner { get; set; }

        public List<MapItem> GetSpawns(TGenContext map)
        {
            if (this.BaseSpawner is null)
                return new List<MapItem>();

            List<MapItem> baseItems = this.BaseSpawner.GetSpawns(map);
            List<MapItem> copyResults = new List<MapItem>();

            foreach (MapItem item in baseItems)
                copyResults.Add(MapItem.CreateBox(BoxID, item.Value));

            return copyResults;
        }

        public override string ToString()
        {
            string baseSpawnerString = "NULL";
            if (this.BaseSpawner != null)
                baseSpawnerString = this.BaseSpawner.ToString();
            return string.Format("{0}: {1}, {2}", this.GetType().GetFormattedTypeName(), this.BoxID.ToString(), baseSpawnerString);
        }
    }
}
