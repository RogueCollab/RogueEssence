using System;
using RogueElements;
using System.Linq;
using System.Collections.Generic;
using RogueEssence.Content;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class MapLayer : IMapLayer
    {
        public string Name { get; set; }
        public DrawLayer Layer { get; set; }
        public bool Visible { get; set; }

        public AutoTile[][] Tiles;

        public int Width => Tiles.Length;
        public int Height => Tiles[0].Length;

        public MapLayer(string name)
        {
            Name = name;
            Visible = true;
        }

        protected MapLayer(MapLayer other)
        {
            Name = other.Name;
            Layer = other.Layer;
            Visible = other.Visible;


            Tiles = new AutoTile[other.Tiles.Length][];
            for (int ii = 0; ii < other.Tiles.Length; ii++)
            {
                Tiles[ii] = new AutoTile[other.Tiles[0].Length];
                for (int jj = 0; jj < other.Tiles[0].Length; jj++)
                    Tiles[ii][jj] = other.Tiles[ii][jj].Copy();
            }
        }

        public IMapLayer Clone() { return new MapLayer(this); }

        public void Merge(IMapLayer other)
        {
            MapLayer otherLayer = (MapLayer)other;
            for (int xx = 0; xx < Width; xx++)
            {
                for (int yy = 0; yy < Height; yy++)
                {
                    AutoTile tile = otherLayer.Tiles[xx][yy];
                    if (!tile.IsEmpty())
                        Tiles[xx][yy] = tile.Copy();
                }
            }
        }

        public void CreateNew(int width, int height)
        {
            Tiles = new AutoTile[width][];
            for (int ii = 0; ii < width; ii++)
            {
                Tiles[ii] = new AutoTile[height];
                for (int jj = 0; jj < height; jj++)
                    Tiles[ii][jj] = new AutoTile();
            }
        }

        public void ResizeJustified(int width, int height, Dir8 anchorDir)
        {
            Grid.LocAction changeOp = (Loc effectLoc) => { };
            Grid.LocAction newOp = (Loc effectLoc) => { Tiles[effectLoc.X][effectLoc.Y] = new AutoTile(); };

            Loc diff = Grid.ResizeJustified(ref Tiles,
                width, height, anchorDir.Reverse(), changeOp, newOp);
        }


        public void CalculateAutotiles(ulong randSeed, Loc rectStart, Loc rectSize, bool wrap)
        {
            ReNoise noise = new ReNoise(randSeed);
            HashSet<string> floortilesets = new HashSet<string>();
            for (int ii = rectStart.X; ii < rectStart.X + rectSize.X; ii++)
            {
                for (int jj = rectStart.Y; jj < rectStart.Y + rectSize.Y; jj++)
                {
                    Loc destLoc = new Loc(ii, jj);
                    if (wrap)
                        destLoc = WrapLoc(destLoc);

                    if (!Collision.InBounds(Width, Height, destLoc))
                        continue;

                    if (!String.IsNullOrEmpty(Tiles[destLoc.X][destLoc.Y].AutoTileset))
                        floortilesets.Add(Tiles[destLoc.X][destLoc.Y].AutoTileset);
                }
            }
            foreach (string tileset in floortilesets)
            {
                Data.AutoTileData entry = Data.DataManager.Instance.GetAutoTile(tileset);
                entry.Tiles.AutoTileArea(noise, rectStart, rectSize,
                    (int x, int y, int neighborCode) =>
                    {
                        Loc checkLoc = new Loc(x, y);
                        if (wrap)
                            checkLoc = WrapLoc(checkLoc);
                        else if (!Collision.InBounds(Width, Height, checkLoc))
                            return;
                        Tiles[checkLoc.X][checkLoc.Y].NeighborCode = neighborCode;
                    },
                    (int x, int y) =>
                    {
                        Loc checkLoc = new Loc(x, y);
                        if (wrap)
                            checkLoc = WrapLoc(checkLoc);
                        else if (!Collision.InBounds(Width, Height, checkLoc))
                            return true;
                        return Tiles[checkLoc.X][checkLoc.Y].AutoTileset == tileset;
                    },
                    (int x, int y) =>
                    {
                        Loc checkLoc = new Loc(x, y);
                        if (wrap)
                            checkLoc = WrapLoc(checkLoc);
                        else if (!Collision.InBounds(Width, Height, checkLoc))
                            return true;
                        return Tiles[checkLoc.X][checkLoc.Y].AutoTileset == tileset || Tiles[checkLoc.X][checkLoc.Y].Associates.Contains(tileset);
                    });
            }
        }

        public Loc WrapLoc(Loc loc)
        {
            return (loc + new Loc(Width, Height)) % new Loc(Width, Height);
        }

        public override string ToString()
        {
            return (Layer == DrawLayer.Top ? "[Top] " : "") + Name;
        }
    }
}

