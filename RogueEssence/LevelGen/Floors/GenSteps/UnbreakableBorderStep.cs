using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class UnbreakableBorderStep<T> : GenStep<T> where T : class, IUnbreakableGenContext
    {
        public int Thickness;

        public UnbreakableBorderStep() { }
            
        public UnbreakableBorderStep(int thickness)
        {
            Thickness = thickness;
        }

        public override void Apply(T map)
        {
            for (int ii = 0; ii < map.Width; ii++)
            {
                for (int jj = 0; jj < map.Height; jj++)
                {
                    if (ii < Thickness || jj < Thickness || ii >= map.Width - Thickness || jj >= map.Height - Thickness)
                        map.SetTile(new Loc(ii, jj), map.UnbreakableTerrain.Copy());
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: Thick:{1}", this.GetType().Name, this.Thickness);
        }
    }


    [Serializable]
    public class TileBorderStep<T> : GenStep<T> where T : class, ITiledGenContext
    {
        public int Thickness;

        /// <summary>
        /// Tile representing the water terrain to paint with.
        /// </summary>
        public ITile Terrain { get; set; }

        public TileBorderStep() { }

        public TileBorderStep(int thickness, ITile terrain)
        {
            Thickness = thickness;
            Terrain = terrain;
        }

        public override void Apply(T map)
        {
            for (int ii = 0; ii < map.Width; ii++)
            {
                for (int jj = 0; jj < map.Height; jj++)
                {
                    if (ii < Thickness || jj < Thickness || ii >= map.Width - Thickness || jj >= map.Height - Thickness)
                        map.SetTile(new Loc(ii, jj), Terrain.Copy());
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: Thick:{1}", this.GetType().Name, this.Thickness);
        }
    }
}
