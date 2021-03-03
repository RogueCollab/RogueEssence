using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Data;
using System.Runtime.Serialization;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class AutoTile
    {
        public List<TileLayer> Layers;

        [Dev.DataType(0, DataManager.DataType.AutoTile, true)]
        public int AutoTileset { get; private set; }

        /// <summary>
        /// Tilesets that are considered this tileset for texture computing purposes.  Only used for Autotiles
        /// </summary>
        [Dev.DataType(1, DataManager.DataType.AutoTile, false)]
        public HashSet<int> Associates { get; private set; }
        public int NeighborCode;

        public AutoTile(params TileLayer[] layers)
        {
            Layers = new List<TileLayer>();
            Layers.AddRange(layers);
            AutoTileset = -1;
            NeighborCode = -1;
            Associates = new HashSet<int>();
        }

        public AutoTile(int autotile, params int[] associates)
        {
            Layers = new List<TileLayer>();
            AutoTileset = autotile;
            NeighborCode = -1;
            Associates = new HashSet<int>();
            foreach (int tile in associates)
                Associates.Add(tile);
        }
        public AutoTile(int autotile, HashSet<int> associates)
        {
            Layers = new List<TileLayer>();
            AutoTileset = autotile;
            NeighborCode = -1;
            Associates = associates;
        }
        protected AutoTile(AutoTile other) : this()
        {
            foreach (TileLayer layer in other.Layers)
                Layers.Add(new TileLayer(layer));
            AutoTileset = other.AutoTileset;
            NeighborCode = other.NeighborCode;
            foreach (int tile in other.Associates)
                Associates.Add(tile);
        }
        public AutoTile Copy() { return new AutoTile(this); }

        public void Draw(SpriteBatch spriteBatch, Loc pos)
        {
            List<TileLayer> layers;
            if (AutoTileset > -1)
            {
                AutoTileData entry = DataManager.Instance.GetAutoTile(AutoTileset);
                layers = entry.Tiles.GetLayers(NeighborCode);
            }
            else
                layers = Layers;
            foreach (TileLayer anim in layers)
                anim.Draw(spriteBatch, pos, GraphicsManager.TotalFrameTick);
        }


        public bool Equals(AutoTile other)
        {
            if (other == null)
                return false;

            if (AutoTileset > -1)
            {
                if (AutoTileset != other.AutoTileset)
                    return false;
                if (Associates.Count != other.Associates.Count)
                    return false;
                foreach (int tile in Associates)
                {
                    if (!other.Associates.Contains(tile))
                        return false;
                }
                return true;
            }
            else
            {
                if (Layers.Count != other.Layers.Count)
                    return false;

                for (int ii = 0; ii < Layers.Count; ii++)
                {
                    if (Layers[ii] != other.Layers[ii])
                        return false;
                }

                return true;
            }
        }

        public override bool Equals(object obj)
        {
            return (obj != null) && Equals(obj as AutoTile);
        }

        public override int GetHashCode()
        {
            return AutoTileset.GetHashCode() ^ Associates.GetHashCode() ^ Layers.GetHashCode();
        }
    }
}
