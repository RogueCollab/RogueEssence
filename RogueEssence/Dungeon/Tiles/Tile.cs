using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class Tile : ITile
    {
        //ground, water, lava that can be changed
        [Dev.SubGroup]
        public TerrainTile Data;
        //traps, wonder tiles, stairs, etc. that can be removed
        public EffectTile Effect;

        public string ID { get { return Data.ID; } set { Data.ID = value; } }

        public Tile()
        {
            Data = new TerrainTile();
            Effect = new EffectTile();
        }

        public Tile(string type)
        {
            Data = new TerrainTile(type);
            Effect = new EffectTile();
        }

        public Tile(string type, bool stableTex)
        {
            Data = new TerrainTile(type, stableTex);
            Effect = new EffectTile();
        }

        public Tile(string type, Loc loc)
        {
            Data = new TerrainTile(type);
            Effect = new EffectTile(loc);
        }

        protected Tile(Tile other)
        {
            Data = other.Data.Copy();
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

        public override string ToString()
        {
            List<string> values = new List<string>();

            if (!String.IsNullOrEmpty(Data.ID))
                values.Add(DataManager.Instance.DataIndices[DataManager.DataType.Terrain].Get(Data.ID).Name.ToLocal());
            if (!String.IsNullOrEmpty(Effect.ID))
                values.Add(DataManager.Instance.DataIndices[DataManager.DataType.Tile].Get(Effect.ID).Name.ToLocal());
            string features = string.Join("/", values);
            return string.Format("{0}: {1}", this.GetType().Name, features);
        }
    }
}
