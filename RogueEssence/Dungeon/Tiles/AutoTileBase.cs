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
        //TODO: pass a seed instead, and choose variation based on the static aspect of the seed
        public abstract void AutoTileArea(ulong randSeed, Loc rectStart, Loc rectSize, Loc totalSize, PlacementMethod placementMethod, QueryMethod presenceMethod, QueryMethod queryMethod);

        public abstract List<TileLayer> GetLayers(int neighborCode);
        protected bool IsBlocked(QueryMethod queryMethod, int x, int y, Dir8 dir)
        {
            Loc loc = new Loc(x,y) + dir.GetLoc();

            return queryMethod(loc.X, loc.Y);
        }


        protected int SelectTileVariant(IRandom rand, int count)
        {
            int index = 0;
            for (int ii = 0; ii < count - 1; ii++)
            {
                if (rand.Next() % 2 == 0)
                    index++;
                else
                    break;
            }
            return index;
        }
    }
}
