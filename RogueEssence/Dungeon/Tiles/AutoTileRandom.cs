using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class AutoTileRandom : AutoTileBase
    {
        public override TileLayer[] Generic { get { return Ground.ToArray(); } }

        public List<TileLayer> Ground;
        public List<TileLayer> Variations;


        public AutoTileRandom()
        {
            Ground = new List<TileLayer>();
            Variations = new List<TileLayer>();

        }

        public override void AutoTileArea(ulong randSeed, Loc rectStart, Loc rectSize, Loc totalSize, PlacementMethod placementMethod, QueryMethod queryMethod)
        {
            ReRandom rand = new ReRandom(randSeed);
            for (int xx = 0; xx < rectStart.X + rectSize.X; xx++)
            {
                int yy = 0;
                for (; yy < rectStart.Y + rectSize.Y; yy++)
                {
                    ulong subSeed = rand.NextUInt64();
                    if (xx >= rectStart.X && yy >= rectStart.Y)
                    {
                        if (queryMethod(xx, yy))
                            placementMethod(xx, yy, GetTile(new ReRandom(subSeed)));
                    }
                }
                while (yy < totalSize.Y)
                {
                    rand.NextUInt64();
                    yy++;
                }
            }
        }


        private List<TileLayer> GetTile(ReRandom rand)
        {
            List<TileLayer> tileList = new List<TileLayer>();

            tileList.Add(new TileLayer(SelectTile(rand, Ground)));
            if (Variations.Count > 0)
                tileList.Add(new TileLayer(SelectTile(rand, Variations)));

            return tileList;
        }


    }
}
