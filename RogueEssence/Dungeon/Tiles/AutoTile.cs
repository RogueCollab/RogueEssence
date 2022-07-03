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
        /// <summary>
        /// If AutoTileSet is set to -1, this variable can be specified to make a hand-crafted texture.
        /// </summary>
        public List<TileLayer> Layers;

        [Dev.DataType(0, DataManager.DataType.AutoTile, true)]
        public int AutoTileset { get; private set; }

        /// <summary>
        /// Associates are autotiles that will be considered the same as the autotile this object is using.
        /// Only used for texture computation, and only relevant for edge cases involving when two different autotiles meet.
        /// </summary>
        [Dev.DataType(1, DataManager.DataType.AutoTile, false)]
        public HashSet<int> Associates { get; private set; }

        [Dev.NonEdited]
        public int NeighborCode;

        public AutoTile()
        {
            Layers = new List<TileLayer>();
            AutoTileset = -1;
            NeighborCode = -1;
            Associates = new HashSet<int>();
        }
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
            draw(spriteBatch, pos, false, 0);
        }

        public void DrawBlank(SpriteBatch spriteBatch, Loc pos, ulong randCode)
        {
            draw(spriteBatch, pos, true, randCode);
        }

        private void draw(SpriteBatch spriteBatch, Loc pos, bool neighborCodeOverride, ulong randCode)
        {
            List<TileLayer> layers;
            if (AutoTileset > -1)
            {
                AutoTileData entry = DataManager.Instance.GetAutoTile(AutoTileset);
                int neighborCode = NeighborCode;
                if (neighborCodeOverride)
                    neighborCode = entry.Tiles.GetVariantCode(randCode, neighborCode);
                layers = entry.Tiles.GetLayers(neighborCode);
            }
            else
                layers = Layers;
            foreach (TileLayer anim in layers)
                anim.Draw(spriteBatch, pos, GraphicsManager.TotalFrameTick);
        }

        public bool IsEmpty()
        {
            if (AutoTileset > -1)
                return false;

            if (Layers.Count > 0)
                return false;
            return true;
        }

        public bool Equals(AutoTile other)
        {
            if (other == null)
                return false;
            if (AutoTileset != other.AutoTileset)
                return false;

            if (AutoTileset > -1)
            {
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

        public override string ToString()
        {
            //TODO: String Assets
            if (AutoTileset > -1 && AutoTileset < DataManager.Instance.DataIndices[DataManager.DataType.AutoTile].Count)
                return String.Format("AutoTile {0}", DataManager.Instance.DataIndices[DataManager.DataType.AutoTile].Entries[AutoTileset.ToString()].Name.ToLocal());
            else
            {
                if (Layers.Count > 0)
                {
                    TileLayer layer = Layers[0];
                    if (layer.Frames.Count > 0)
                    {
                        TileFrame frame = layer.Frames[0];
                        return String.Format("AutoTile {0}: {1}", frame.Sheet, frame.TexLoc.ToString());
                    }
                }
            }
            return "[EMPTY]";
        }
    }
}
