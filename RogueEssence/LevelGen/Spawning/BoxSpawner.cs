using RogueElements;
using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{

    [Serializable]
    public class BoxSpawner<TGenContext> : IStepSpawner<TGenContext, MapItem>
        where TGenContext : IGenContext
    {
        public BoxSpawner()
        {
        }

        public BoxSpawner(int id, IStepSpawner<TGenContext, MapItem> spawner)
        {
            this.BaseSpawner = spawner;
            this.BoxID = id;
        }

        [Dev.DataType(0, Data.DataManager.DataType.Item, false)]
        public int BoxID { get; set; }

        public IStepSpawner<TGenContext, MapItem> BaseSpawner { get; set; }

        public List<MapItem> GetSpawns(TGenContext map)
        {
            if (this.BaseSpawner is null)
                return new List<MapItem>();

            List<MapItem> baseItems = this.BaseSpawner.GetSpawns(map);
            List<MapItem> copyResults = new List<MapItem>();

            foreach (MapItem item in baseItems)
                copyResults.Add(new MapItem(BoxID, item.Value));

            return copyResults;
        }
    }
}
