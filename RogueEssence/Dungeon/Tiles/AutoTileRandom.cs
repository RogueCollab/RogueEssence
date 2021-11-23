using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class AutoTileRandom : AutoTileBase
    {
        public List<TileLayer> Ground;

        public AutoTileRandom()
        {
            Ground = new List<TileLayer>();
        }

        public override void AutoTileArea(ulong randSeed, Loc rectStart, Loc rectSize, Loc totalSize, PlacementMethod placementMethod, QueryMethod presenceMethod, QueryMethod queryMethod)
        {
            IRandom rand = new ReRandom(randSeed);
            for (int xx = 0; xx < rectStart.X + rectSize.X; xx++)
            {
                int yy = 0;
                for (; yy < rectStart.Y + rectSize.Y; yy++)
                {
                    ulong subSeed = rand.NextUInt64();
                    if (xx >= rectStart.X && yy >= rectStart.Y)
                    {
                        if (Collision.InBounds(totalSize.X, totalSize.Y, new Loc(xx, yy)) && presenceMethod(xx, yy))
                            placementMethod(xx, yy, GetVariantCode(new ReRandom(subSeed), 0));
                    }
                }
                while (yy < totalSize.Y)
                {
                    rand.NextUInt64();
                    yy++;
                }
            }
        }

        private int GetVariantCode(IRandom rand, int neighborCode)
        {
            List<TileLayer> tileVars = GetTileVariants(neighborCode);
            return SelectTileVariant(rand, tileVars.Count) << 8 | neighborCode;
        }


        public override List<TileLayer> GetLayers(int neighborCode)
        {
            if (neighborCode == -1)
                new List<TileLayer>() { Ground[0] };

            int lowerCode = neighborCode & Convert.ToInt32("11111111", 2);
            int upperCode = neighborCode >> 8 & Convert.ToInt32("11111111", 2);

            List<TileLayer> tileVars = GetTileVariants(lowerCode);
            List<TileLayer> tileList = new List<TileLayer>();
            AddBoundedLayer(tileList, tileVars, upperCode);

            return tileList;
        }

        private void AddBoundedLayer(List<TileLayer> results, List<TileLayer> variants, int variantCode)
        {
            if (variants.Count == 0)
                return;
            results.Add(variants[Math.Min(variantCode, variants.Count - 1)]);
        }


        private List<TileLayer> GetTileVariants(int neighborCode)
        {
            return Ground;
        }

    }
}
