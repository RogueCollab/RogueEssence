using System;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class Tile : ITile
    {
        //ground, water, lava that can be changed
        public TerrainTile Data;
        //traps, wonder tiles, stairs, etc. that can be removed
        public EffectTile Effect;
        //ground tiles
        public AutoTile FloorTile;

        public int ID { get { return Data.ID; } set { Data.ID = value; } }

        public Tile()
        {
            Data = new TerrainTile();
            FloorTile = new AutoTile();
            Effect = new EffectTile();
        }

        public Tile(int type)
        {
            Data = new TerrainTile(type);
            FloorTile = new AutoTile();
            Effect = new EffectTile();
        }

        public Tile(int type, Loc loc)
        {
            Data = new TerrainTile(type);
            FloorTile = new AutoTile();
            Effect = new EffectTile(loc);
        }

        protected Tile(Tile other)
        {
            Data = other.Data.Copy();
            FloorTile = other.FloorTile.Copy();
            Effect = new EffectTile(other.Effect);
        }
        public ITile Copy() { return new Tile(this); }

        public bool TileEquivalent(ITile other)
        {
            Tile tile = other as Tile;
            if (tile == null)
                return false;
            return tile.ID == ID;
        }
    }
}
