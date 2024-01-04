﻿using System;
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
using RogueEssence.Ground;

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

        public List<MapLayer> Layers;

        public List<AnimLayer> Decorations;

        public Tile[][] Tiles;

        /// <summary>
        /// Describes how to handle the map scrolling past the edge of the map
        /// </summary>
        public ScrollEdge EdgeView;

        //includes all start points
        public List<LocRay8> EntryPoints;

        /// <summary>
        /// Width in tiles
        /// </summary>
        public int Width { get { return Tiles.Length; } }

        /// <summary>
        /// Height in tiles
        /// </summary>
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
            Decorations = new List<AnimLayer>();

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
                    Tiles[ii][jj] = new Tile(DataManager.Instance.GenFloor, new Loc(ii, jj));
            }

            Layers.Clear();
            MapLayer layer = new MapLayer("New Layer");
            layer.CreateNew(width, height);
            Layers.Add(layer);

            Decorations.Clear();
            Decorations.Add(new AnimLayer("New Deco"));
        }

        public Tile GetTile(Loc loc)
        {
            if (!GetLocInMapBounds(ref loc))
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
            return TileBlocked(loc, inclusive ? TerrainData.Mobility.All : TerrainData.Mobility.Passable, diagonal);
        }

        public bool TileBlocked(Loc loc, TerrainData.Mobility mobility)
        {
            return TileBlocked(loc, mobility, false);
        }

        public bool TileBlocked(Loc loc, TerrainData.Mobility mobility, bool diagonal)
        {
            if (!GetLocInMapBounds(ref loc))
                return true;

            Tile tile = Tiles[loc.X][loc.Y];
            TerrainData terrain = tile.Data.GetData();
            if (TerrainBlocked(terrain, mobility, diagonal))
                return true;
            if (!String.IsNullOrEmpty(tile.Effect.ID))
            {
                TileData effect = DataManager.Instance.GetTile(tile.Effect.ID);
                if (EffectTileBlocked(effect, diagonal))
                    return true;
            }
            return false;
        }
        public bool TerrainBlocked(Loc loc, TerrainData.Mobility mobility)
        {
            if (!GetLocInMapBounds(ref loc))
                return true;

            Tile tile = Tiles[loc.X][loc.Y];
            TerrainData terrain = tile.Data.GetData();
            return TerrainBlocked(terrain, mobility, false);
        }

        public bool TerrainBlocked(TerrainData terrain, TerrainData.Mobility mobility, bool diagonal)
        {
            if (diagonal && !terrain.BlockDiagonal)
                return false;
            if (terrain.BlockType == TerrainData.Mobility.Impassable)
                return true;
            if (terrain.BlockType == TerrainData.Mobility.Passable)
                return false;

            return (terrain.BlockType & mobility) == 0;
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
            TerrainData.TileItemAllowance threshold = TerrainData.TileItemAllowance.Allow;
            if (!voluntary)
                threshold = TerrainData.TileItemAllowance.Force;

            if (!GetLocInMapBounds(ref loc))
                return false;

            Tile tile = Tiles[loc.X][loc.Y];
            TerrainData terrain = tile.Data.GetData();
            if (terrain.ItemAllow > threshold)
                return false;

            if (!String.IsNullOrEmpty(Tiles[loc.X][loc.Y].Effect.ID))
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="voluntary"></param>
        /// <returns>Unwrapped destination value.</returns>
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
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public Rect GetClampedSight(Rect rect)
        {
            if (EdgeView == ScrollEdge.Clamp)
            {
                Loc start = rect.Start;
                rect.Start = new Loc(Math.Max(0, Math.Min(start.X, Width - rect.Width)), Math.Max(0, Math.Min(start.Y, Height - rect.Height)));
                return rect;
            }
            return rect;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loc1">Reference loc</param>
        /// <param name="loc2">Loc to get the closest wrap of.  May be out of bounds.</param>
        /// <returns></returns>
        public Loc GetClosestUnwrappedLoc(Loc loc1, Loc loc2)
        {
            if (EdgeView == Map.ScrollEdge.Wrap)
                return WrappedCollision.GetClosestWrap(Size, loc1, loc2);
            else
                return loc2;
        }

        public int GetClosestDist8(Loc loc1, Loc loc2)
        {
            if (EdgeView == Map.ScrollEdge.Wrap)
                loc2 = WrappedCollision.GetClosestWrap(Size, loc1, loc2);
            return (loc1 - loc2).Dist8();
        }

        public Dir8 GetClosestDir8(Loc loc1, Loc loc2)
        {
            if (EdgeView == Map.ScrollEdge.Wrap)
                loc2 = WrappedCollision.GetClosestWrap(Size, loc1, loc2);
            return DirExt.GetDir(loc1, loc2);
        }

        public Dir8 ApproximateClosestDir8(Loc loc1, Loc loc2)
        {
            if (EdgeView == Map.ScrollEdge.Wrap)
                loc2 = WrappedCollision.GetClosestWrap(Size, loc1, loc2);
            return DirExt.ApproximateDir8(loc2 - loc1);
        }

        /// <summary>
        /// Checks if two characters are close together, accounting for wrapping
        /// </summary>
        /// <param name="loc1"></param>
        /// <param name="loc2"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public bool InRange(Loc loc1, Loc loc2, int range)
        {
            if (EdgeView == Map.ScrollEdge.Wrap)
                loc2 = WrappedCollision.GetClosestWrap(Size, loc1, loc2);
            return (loc1 - loc2).Dist8() <= range;
        }

        public bool InBounds(Rect rect, Loc loc)
        {
            if (EdgeView == Map.ScrollEdge.Wrap)
                return WrappedCollision.InBounds(Size, rect, loc);
            else
                return Collision.InBounds(rect, loc);
        }

        public bool Collides(Rect rect1, Rect rect2)
        {
            if (EdgeView == Map.ScrollEdge.Wrap)
                return WrappedCollision.Collides(Size, rect1, rect2);
            else
                return Collision.Collides(rect1, rect2);
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
            if (EdgeView != ScrollEdge.Wrap)
            {
                if (Collision.InBounds(rect, loc))
                    yield return loc;
                yield break;
            }

            foreach (Loc inLoc in WrappedCollision.IteratePointsInBounds(Size, rect, loc))
                yield return inLoc;
        }

        public void AddLayer(string name)
        {
            MapLayer layer = new MapLayer(name);
            layer.CreateNew(Width, Height);
            Layers.Add(layer);
        }


        public void DrawLoc(SpriteBatch spriteBatch, Loc drawPos, Loc loc, bool front)
        {
            if (!front)
            {
                foreach (MapLayer layer in Layers)
                {
                    if ((layer.Layer == DrawLayer.Under) && layer.Visible)
                        layer.Tiles[loc.X][loc.Y].Draw(spriteBatch, drawPos);
                }

                Tiles[loc.X][loc.Y].Data.TileTex.Draw(spriteBatch, drawPos);
            }

            foreach (MapLayer layer in Layers)
            {
                if (!layer.Visible)
                    continue;
                if (front)
                {
                    if (layer.Layer != DrawLayer.Top)
                        continue;
                }
                else
                {
                    if (layer.Layer < DrawLayer.Bottom || DrawLayer.Top <= layer.Layer)
                        continue;
                }
                layer.Tiles[loc.X][loc.Y].Draw(spriteBatch, drawPos);
            }
        }  
    }

}
