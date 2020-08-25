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

        public override void AutoTileArea(ReRandom rand, Loc rectStart, Loc rectSize, PlacementMethod placementMethod, QueryMethod queryMethod)
        {
            for (int ii = 0; ii < rectSize.X; ii++)
            {
                for (int jj = 0; jj < rectSize.Y; jj++)
                {
                    if (queryMethod(rectStart.X + ii, rectStart.Y + jj))
                        placementMethod(rectStart.X + ii, rectStart.Y + jj, GetTile(rand));
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
