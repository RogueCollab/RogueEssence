using System;
using RogueElements;
using System.Linq;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class MapLayer
    {
        public string Name { get; set; }
        public bool Front { get; set; }
        public bool Visible { get; set; }

        public AutoTile[][] Tiles;

        public MapLayer(string name)
        {
            Name = name;
            Visible = true;
        }

        protected MapLayer(MapLayer other)
        {
            Name = other.Name;
            Front = other.Front;
            Visible = other.Visible;


            Tiles = new AutoTile[other.Tiles.Length][];
            for (int ii = 0; ii < other.Tiles.Length; ii++)
            {
                Tiles[ii] = new AutoTile[other.Tiles[0].Length];
                for (int jj = 0; jj < other.Tiles[0].Length; jj++)
                    Tiles[ii][jj] = other.Tiles[ii][jj].Copy();
            }
        }

        public MapLayer Clone() { return new MapLayer(this); }

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
            RogueElements.Grid.LocAction changeOp = (Loc effectLoc) => { };
            RogueElements.Grid.LocAction newOp = (Loc effectLoc) => { Tiles[effectLoc.X][effectLoc.Y] = new AutoTile(); };

            Loc diff = RogueElements.Grid.ResizeJustified(ref Tiles,
                width, height, anchorDir.Reverse(), changeOp, newOp);
        }

        public override string ToString()
        {
            return (Front ? "[Front] " : "") + Name;
        }
    }
}

