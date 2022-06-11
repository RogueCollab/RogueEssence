using System;
using System.Collections;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.LevelGen;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using RogueEssence.Script;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class BaseMap : IMobSpawnMap
    {
        public enum ScrollEdge
        {
            /// <summary>
            /// Displays the BlankBG texture, or a black void if there is none
            /// </summary>
            Blank = 0,
            /// <summary>
            /// Does not scroll past the edge of the map.
            /// </summary>
            Clamp,
            /// <summary>
            /// The map is wrapped around.
            /// </summary>
            Wrap,
        }

        protected IRandom rand;
        public IRandom Rand { get { return rand; } }
        IRandom IMobSpawnMap.Rand { get { return rand; } }
        public bool Begun { get; set; }

        public int ID { get; set; }

        public bool DropTitle;

        public List<MapLayer> Layers;

        public Tile[][] Tiles;

        /// <summary>
        /// Describes how to handle the map scrolling past the edge of the map
        /// </summary>
        public ScrollEdge EdgeView;

        //includes all start points
        public List<LocRay8> EntryPoints;
        public int Width { get { return Tiles.Length; } }
        public int Height { get { return Tiles[0].Length; } }
        public Loc Size { get { return new Loc(Width, Height); } }

        public int GroundWidth { get { return Width * GraphicsManager.TileSize; } }
        public int GroundHeight { get { return Height * GraphicsManager.TileSize; } }
        public Loc GroundSize { get { return Size * GraphicsManager.TileSize; } }

        public List<MapItem> Items;
        public EventedList<Team> MapTeams { get; set; }
        public EventedList<Team> AllyTeams { get; set; }


        public BaseMap()
        {
            rand = new ReRandom(0);
            EntryPoints = new List<LocRay8>();

            Layers = new List<MapLayer>();

            Items = new List<MapItem>();
            MapTeams = new EventedList<Team>();
            AllyTeams = new EventedList<Team>();
        }
        
        public void LoadRand(IRandom rand)
        {
            this.rand = rand;
        }

        public virtual void CreateNew(int width, int height)
        {
            Tiles = new Tile[width][];
            for (int ii = 0; ii < width; ii++)
            {
                Tiles[ii] = new Tile[height];
                for (int jj = 0; jj < height; jj++)
                    Tiles[ii][jj] = new Tile(0, new Loc(ii, jj));
            }

            Layers.Clear();
            MapLayer layer = new MapLayer("New Layer");
            layer.CreateNew(width, height);
            Layers.Add(layer);
        }

        public Tile GetTile(Loc loc)
        {
            if (!Collision.InBounds(Width, Height, loc))
                return null;
            return Tiles[loc.X][loc.Y];
        }


        public int GetItem(Loc loc)
        {
            for (int ii = 0; ii < Items.Count; ii++)
            {
                if (Items[ii].TileLoc == loc)
                    return ii;
            }
            return -1;
        }

        public bool TileBlocked(Loc loc)
        {
            return TileBlocked(loc, false);
        }

        public bool TileBlocked(Loc loc, bool inclusive)
        {
            return TileBlocked(loc, inclusive, false);
        }

        public bool TileBlocked(Loc loc, bool inclusive, bool diagonal)
        {
            return TileBlocked(loc, inclusive ? UInt32.MaxValue : 0, diagonal);
        }

        public bool TileBlocked(Loc loc, uint mobility)
        {
            return TileBlocked(loc, mobility, false);
        }

        public bool TileBlocked(Loc loc, uint mobility, bool diagonal)
        {
            if (!GetLocInMapBounds(ref loc))
                return true;

            Tile tile = Tiles[loc.X][loc.Y];
            TerrainData terrain = tile.Data.GetData();
            if (TerrainBlocked(terrain, mobility, diagonal))
                return true;
            if (tile.Effect.ID > -1)
            {
                TileData effect = DataManager.Instance.GetTile(tile.Effect.ID);
                if (EffectTileBlocked(effect, diagonal))
                    return true;
            }
            return false;
        }
        public bool TerrainBlocked(Loc loc, uint mobility)
        {
            if (!GetLocInMapBounds(ref loc))
                return true;

            Tile tile = Tiles[loc.X][loc.Y];
            TerrainData terrain = tile.Data.GetData();
            return TerrainBlocked(terrain, mobility, false);
        }

        public bool TerrainBlocked(TerrainData terrain, uint mobility, bool diagonal)
        {
            if (diagonal && !terrain.BlockDiagonal)
                return false;
            if (terrain.BlockType == TerrainData.Mobility.Impassable)
                return true;
            if (terrain.BlockType == TerrainData.Mobility.Passable)
                return false;

            return ((1U << (int)terrain.BlockType) & mobility) == 0;
        }

        public bool EffectTileBlocked(TileData effect, bool diagonal)
        {
            //doesn't apply here; for now, assume all effects block diagonal if they block at all
            //if (diagonal && !effect.BlockDiagonal)
            //    return false;
            if (effect.StepType == TileData.TriggerType.Unlockable || effect.StepType == TileData.TriggerType.Blocker)
                return true;

            return false;
        }



        public bool CanItemLand(Loc loc, bool voluntary, bool ignoreItem)
        {
            uint mobility = 0;
            mobility |= (1U << (int)TerrainData.Mobility.Water);
            if (!voluntary)
            {
                mobility |= (1U << (int)TerrainData.Mobility.Lava);
                mobility |= (1U << (int)TerrainData.Mobility.Abyss);
            }
            if (TileBlocked(loc, mobility, false))
                return false;
            if (Tiles[loc.X][loc.Y].Effect.ID > -1)
            {
                TileData tileData = DataManager.Instance.GetTile(Tiles[loc.X][loc.Y].Effect.ID);
                if (tileData.BlockItem)
                    return false;
            }

            if (ignoreItem)
                return true;

            return (GetItem(loc) == -1);
        }

        public Loc? FindItemlessTile(Loc origin, int range)
        {
            return FindItemlessTile(origin, range, false);
        }

        public Loc? FindItemlessTile(Loc origin, int range, bool voluntary)
        {
            return FindItemlessTile(origin, origin - new Loc(range), new Loc(range * 2 + 1), voluntary);
        }
        public Loc? FindItemlessTile(Loc origin, Loc start, Loc end, bool voluntary)
        {
            return Grid.FindClosestConnectedTile(start, end,
                (Loc testLoc) =>
                {
                    return CanItemLand(testLoc, voluntary, false);
                },
                (Loc testLoc) =>
                {
                    Tile tile = GetTile(testLoc);
                    if (tile == null)
                        return true;
                    if (tile.Data.GetData().BlockType == TerrainData.Mobility.Impassable)
                        return true;
                    return false;
                },
                (Loc testLoc) =>
                {
                    Tile tile = GetTile(testLoc);
                    if (tile == null)
                        return true;
                    TerrainData terrain = tile.Data.GetData();
                    if (!terrain.BlockDiagonal)
                        return false;
                    if (terrain.BlockType == TerrainData.Mobility.Impassable)
                        return true;
                    return false;
                },
                origin);
        }


        /// <summary>
        /// Converts out of bounds coords to wrapped-around coords.
        /// Based on tiles.
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public Loc WrapLoc(Loc loc)
        {
            return Loc.Wrap(loc, Size);
        }

        /// <summary>
        /// Converts out of bounds coords to wrapped-around coords.
        /// Based on pixels.
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public Loc WrapGroundLoc(Loc loc)
        {
            return Loc.Wrap(loc, GroundSize);
        }

        /// <summary>
        /// Checks to see if the loc is in map bounds.
        /// If it's not wrapped, expect normal results.
        /// If it's normally out of bounds but wrapped, the loc will be changed and the result will be true.
        /// </summary>
        /// <param name="loc">The location to test.  Will be wrapped if the map is wrapped.</param>
        /// <returns></returns>
        public bool GetLocInMapBounds(ref Loc loc)
        {
            if (EdgeView == ScrollEdge.Wrap)
            {
                loc = WrapLoc(loc);
                return true;
            }
            return Collision.InBounds(Width, Height, loc);
        }

        public bool InBounds(Rect rect, Loc loc)
        {
            if (EdgeView == Map.ScrollEdge.Wrap)
                return WrappedCollision.InBounds(Size, rect, loc);
            else
                return Collision.InBounds(rect, loc);
        }


        /// <summary>
        /// Gets all wrapped locations that fit in the specified bounds.
        /// In tiles.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="loc"></param>
        /// <returns></returns>
        public IEnumerable<Loc> IterateLocInBounds(Rect rect, Loc loc)
        {
            if (EdgeView != BaseMap.ScrollEdge.Wrap)
            {
                if (RogueElements.Collision.InBounds(rect, loc))
                    yield return loc;
                yield break;
            }

            //take the topmost Y of the map, subtract the height of the sprite, round down to the lowest whole map.  this is the topmost map to check
            //take the bottom-most Y of the map, round up to the highest whole map.  this is the bottom-most (exclusive) map to check
            //do the same for X
            Loc topLeftBounds = new Loc(MathUtils.DivDown(rect.X, Size.X), MathUtils.DivDown(rect.Y, Size.Y));
            Loc bottomRightBounds = new Loc(MathUtils.DivUp(rect.End.X, Size.X), MathUtils.DivUp(rect.End.Y, Size.Y));
            Loc wrapLoc = Loc.Wrap(loc, Size);

            for (int xx = topLeftBounds.X; xx < bottomRightBounds.X; xx++)
            {
                for (int yy = topLeftBounds.Y; yy < bottomRightBounds.Y; yy++)
                {
                    Loc mapStart = new Loc(xx, yy) * Size;
                    Loc testLoc = mapStart + wrapLoc;
                    if (RogueElements.Collision.InBounds(rect, testLoc))
                        yield return testLoc;
                }
            }
        }

        public void AddLayer(string name)
        {
            MapLayer layer = new MapLayer(name);
            layer.CreateNew(Width, Height);
            Layers.Add(layer);
        }


        public void DrawLoc(SpriteBatch spriteBatch, Loc drawPos, Loc loc, bool front)
        {
            foreach (MapLayer layer in Layers)
            {
                if ((layer.Layer == DrawLayer.Top) == front && layer.Visible)
                    layer.Tiles[loc.X][loc.Y].Draw(spriteBatch, drawPos);
            }

            if (!front)
                Tiles[loc.X][loc.Y].Data.TileTex.Draw(spriteBatch, drawPos);
        }  
    }

}
