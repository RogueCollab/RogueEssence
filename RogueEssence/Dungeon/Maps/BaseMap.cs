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


        protected ReRandom rand;
        public ReRandom Rand { get { return rand; } }
        IRandom IMobSpawnMap.Rand { get { return rand; } }
        public bool Begun { get; set; }

        public int ID { get; set; }

        public bool NoRescue;
        public bool NoSwitching;
        public bool DropTitle;

        public List<MapLayer> Layers;

        public Tile[][] Tiles;
        
        //includes all start points
        public List<LocRay8> EntryPoints;
        public int Width { get { return Tiles.Length; } }
        public int Height { get { return Tiles[0].Length; } }

        public List<MapItem> Items;
        public List<Team> MapTeams;
        public List<Team> AllyTeams;


        public BaseMap()
        {
            rand = new ReRandom(0);
            EntryPoints = new List<LocRay8>();

            Layers = new List<MapLayer>();

            Items = new List<MapItem>();
            MapTeams = new List<Team>();
            AllyTeams = new List<Team>();
        }
        
        public void LoadRand(ReRandom rand)
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
            if (!Collision.InBounds(Width, Height, loc))
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
            if (!Collision.InBounds(Width, Height, loc))
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
                return false;

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
