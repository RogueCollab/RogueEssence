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

        public override void AutoTileArea(INoise noise, Loc rectStart, Loc rectSize, Loc totalSize, PlacementMethod placementMethod, QueryMethod presenceMethod, QueryMethod queryMethod)
        {
            for (int xx = rectStart.X; xx < rectStart.X + rectSize.X; xx++)
            {
                for (int yy = rectStart.Y; yy < rectStart.Y + rectSize.Y; yy++)
                {
                    if (Collision.InBounds(totalSize.X, totalSize.Y, new Loc(xx, yy)) && presenceMethod(xx, yy))
                        placementMethod(xx, yy, GetVariantCode(noise.Get2DUInt64((ulong)xx, (ulong)yy), 0));
                }
            }
        }

        public override int GetVariantCode(ulong randCode, int neighborCode)
        {
            List<TileLayer> tileVars = GetTileVariants(neighborCode);
            return SelectTileVariant(randCode, tileVars.Count) << 8 | (neighborCode & 0xFF);
        }


        public override List<TileLayer> GetLayers(int variantCode)
        {
            if (variantCode == -1)
                new List<TileLayer>() { Ground[0] };

            int lowerCode = variantCode & 0xFF;
            int upperCode = variantCode >> 8 & 0xFF;

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


        public override IEnumerable<List<TileLayer>> IterateElements()
        {
            yield return Ground;
        }
    }
}
