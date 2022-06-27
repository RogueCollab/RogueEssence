using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Dev;
using RogueEssence.LevelGen;
using RogueEssence.Data;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Generates a room by loading a map as the room.
    /// Includes tiles, items, enemies, and mapstarts.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class RoomGenLoadMap<T> : RoomGen<T> where T : BaseMapGenContext
    {
        /// <summary>
        /// Map file to load.
        /// </summary>
        [Dev.DataFolder(0, "Map/")]
        public string MapID;

        /// <summary>
        /// The terrain that counts as room.  Halls will only attach to room tiles, or tiles specified with Borders.
        /// </summary>
        public ITile RoomTerrain { get; set; }

        /// <summary>
        /// Determines which tiles of the border are open for halls.
        /// </summary>
        //public Dictionary<Dir4, bool[]> Borders { get; set; }

        /// <summary>
        /// Determines if connecting hallways should continue digging inward after they hit the room bounds, until a walkable tile is found.
        /// </summary>
        //public bool FulfillAll { get; set; }

        public PostProcType PreventChanges { get; set; }

        [NonSerialized]
        private Map map;

        public RoomGenLoadMap()
        {
            MapID = "";
        }


        protected RoomGenLoadMap(RoomGenLoadMap<T> other)
        {
            MapID = other.MapID;
            this.RoomTerrain = other.RoomTerrain;
            this.PreventChanges = other.PreventChanges;
            //this.FulfillAll = other.FulfillAll;

            //this.Borders = new Dictionary<Dir4, bool[]>();
            //foreach (Dir4 dir in DirExt.VALID_DIR4)
            //{
            //    this.Borders[dir] = new bool[other.Borders[dir].Length];
            //    for (int jj = 0; jj < other.Borders[dir].Length; jj++)
            //        this.Borders[dir][jj] = other.Borders[dir][jj];
            //}

        }
        public override RoomGen<T> Copy() { return new RoomGenLoadMap<T>(this); }


        public override Loc ProposeSize(IRandom rand)
        {
            map = DataManager.Instance.GetMap(MapID);
            return new Loc(this.map.Width, this.map.Height);
        }


        public override void DrawOnMap(T map)
        {
            if (this.Draw.Width != this.map.Width || this.Draw.Height != this.map.Height)
            {
                this.DrawMapDefault(map);
                return;
            }

            //no copying is needed here since the map is disposed of after use

            //add needed layers
            Dictionary<int, int> layerMap = new Dictionary<int, int>();
            Dictionary<Content.DrawLayer, int> drawOrderDict = new Dictionary<Content.DrawLayer, int>();
            for (int ii = 0; ii < this.map.Layers.Count; ii++)
            {
                if (!this.map.Layers[ii].Visible)
                    continue;

                // find the next layer that has the same draw layer as this one
                int layerStart;
                if (!drawOrderDict.TryGetValue(this.map.Layers[ii].Layer, out layerStart))
                    layerStart = 0;
                for (; layerStart < map.Map.Layers.Count; layerStart++)
                {
                    if (map.Map.Layers[layerStart].Layer == this.map.Layers[ii].Layer)
                        break;
                }
                //add it if it doesn't exist
                if (layerStart == map.Map.Layers.Count)
                {
                    map.Map.AddLayer(this.map.Layers[ii].Name);
                    map.Map.Layers[map.Map.Layers.Count - 1].Layer = this.map.Layers[ii].Layer;
                }
                //set the new layer start variable for which to continue checking from
                layerMap[ii] = layerStart;
                drawOrderDict[this.map.Layers[ii].Layer] = layerStart + 1;

                map.Map.AddLayer(this.map.Layers[ii].Name);
            }

            //draw the tiles
            for (int xx = 0; xx < this.Draw.Width; xx++)
            {
                for (int yy = 0; yy < this.Draw.Height; yy++)
                {
                    map.SetTile(new Loc(this.Draw.X + xx, this.Draw.Y + yy), this.map.Tiles[xx][yy]);
                    for (int ii = 0; ii < this.map.Layers.Count; ii++)
                    {
                        int layerTo;
                        if (layerMap.TryGetValue(ii, out layerTo))
                            map.Map.Layers[layerTo].Tiles[this.Draw.X + xx][this.Draw.Y + yy] = this.map.Layers[ii].Tiles[xx][yy];
                    }
                }
            }

            //place items
            foreach (MapItem item in this.map.Items)
            {
                item.TileLoc = item.TileLoc + this.Draw.Start;
                map.Items.Add(item);
            }

            //place mobs
            foreach (Team team in this.map.MapTeams)
            {
                foreach (Character member in team.EnumerateChars())
                    member.CharLoc = member.CharLoc + this.Draw.Start;
                map.MapTeams.Add(team);
            }

            //place map entrances
            foreach (LocRay8 entrance in this.map.EntryPoints)
                map.Map.EntryPoints.Add(new LocRay8(entrance.Loc + this.Draw.Start, entrance.Dir));

            //this.FulfillRoomBorders(map, this.FulfillAll);
            this.SetRoomBorders(map);

            for (int xx = 0; xx < Draw.Width; xx++)
            {
                for (int yy = 0; yy < Draw.Height; yy++)
                    map.GetPostProc(new Loc(Draw.X + xx, Draw.Y + yy)).AddMask(new PostProcTile(PreventChanges));
            }
        }

        protected override void PrepareFulfillableBorders(IRandom rand)
        {
            // NOTE: Because the context is not passed in when preparing borders,
            // the tile ID representing an opening must be specified on this class instead.
            if (this.Draw.Width != this.map.Width || this.Draw.Height != this.map.Height)
            {
                foreach (Dir4 dir in DirExt.VALID_DIR4)
                {
                    for (int jj = 0; jj < this.FulfillableBorder[dir].Length; jj++)
                        this.FulfillableBorder[dir][jj] = true;
                }
            }
            else
            {
                for (int ii = 0; ii < this.Draw.Width; ii++)
                {
                    this.FulfillableBorder[Dir4.Up][ii] = this.map.Tiles[ii][0].TileEquivalent(this.RoomTerrain);// || this.Borders[Dir4.Up][ii];
                    this.FulfillableBorder[Dir4.Down][ii] = this.map.Tiles[ii][this.Draw.Height - 1].TileEquivalent(this.RoomTerrain);// || this.Borders[Dir4.Down][ii];
                }

                for (int ii = 0; ii < this.Draw.Height; ii++)
                {
                    this.FulfillableBorder[Dir4.Left][ii] = this.map.Tiles[0][ii].TileEquivalent(this.RoomTerrain);// || this.Borders[Dir4.Left][ii];
                    this.FulfillableBorder[Dir4.Right][ii] = this.map.Tiles[this.Draw.Width - 1][ii].TileEquivalent(this.RoomTerrain);// || this.Borders[Dir4.Right][ii];
                }
            }
        }
    }
}
