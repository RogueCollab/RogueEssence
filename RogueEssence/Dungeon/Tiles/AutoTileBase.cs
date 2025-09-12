using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class AutoTileBase
    {
        public delegate void PlacementMethod(int x, int y, int neighborCode);
        public delegate bool QueryMethod(int x, int y);
        public abstract void AutoTileArea(INoise noise, Loc rectStart, Loc rectSize, PlacementMethod placementMethod, QueryMethod presenceMethod, QueryMethod queryMethod);

        public abstract IEnumerable<List<TileLayer>> IterateElements();

        /// <summary>
        /// Gets a list of tiles to draw, based on a variant code (neighborcode + variant)
        /// </summary>
        /// <param name="variantCode"></param>
        /// <returns></returns>
        public abstract List<TileLayer> GetLayers(int variantCode);
        protected bool IsBlocked(QueryMethod queryMethod, int x, int y, Dir8 dir)
        {
            Loc loc = new Loc(x,y) + dir.GetLoc();

            return queryMethod(loc.X, loc.Y);
        }

        /// <summary>
        /// Gets a variant code based on a randomly given code and the base neighborcode.
        /// If the input is already a variant code (ie, upper bits are nonzero), it recomputes it.
        /// </summary>
        /// <param name="randCode"></param>
        /// <param name="neighborCode"></param>
        /// <returns></returns>
        public abstract int GetVariantCode(ulong randCode, int neighborCode);

        /// <summary>
        /// Every variant is half as likely as the variant before it.
        /// </summary>
        /// <param name="randCode"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected int SelectTileVariant(ulong randCode, int count)
        {
            int index = 0;
            for (int ii = 0; ii < count - 1; ii++)
            {
                if (randCode % 2 == 0)
                {
                    index++;
                    randCode = randCode >> 1;
                }
                else
                    break;
            }
            return index;
        }
    }
}
