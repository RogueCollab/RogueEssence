using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class AutoTileBase
    {
        public delegate void PlacementMethod(int x, int y, List<TileLayer> tile);
        public delegate bool QueryMethod(int x, int y);
        //TODO: pass a seed instead, and choose variation based on the static aspect of the seed
        public abstract void AutoTileArea(ReRandom rand, Loc rectStart, Loc rectSize, PlacementMethod placementMethod, QueryMethod queryMethod);
        public abstract TileLayer[] Generic { get; }

        protected bool IsBlocked(QueryMethod queryMethod, int x, int y, Dir8 dir)
        {
            Loc loc = new Loc(x,y) + dir.GetLoc();

            return queryMethod(loc.X, loc.Y);
        }

        protected TileLayer SelectTile(ReRandom rand, List<TileLayer> anims)
        {

            if (anims.Count > 0)
            {
                int index = 0;
                for (int ii = 0; ii < anims.Count - 1; ii++)
                {
                    if (rand.Next() % 2 == 0)
                        index++;
                    else
                        break;
                }
                return anims[index];
            }
            return new TileLayer();
        }
    }
}
