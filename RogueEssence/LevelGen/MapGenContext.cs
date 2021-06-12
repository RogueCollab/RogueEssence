using System;
using System.Collections;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    public class TeamSpawn : IGroupSpawnable
    {
        public Team Team;
        public bool AllyFaction;

        public TeamSpawn(Team team, bool ally)
        {
            Team = team;
            AllyFaction = ally;
        }
    }


    public abstract class BaseMapGenContext : IPostProcGenContext, IUnbreakableGenContext, IMobSpawnMap,
        ISpawningGenContext<InvItem>, ISpawningGenContext<MoneySpawn>, ISpawningGenContext<EffectTile>,
        IPlaceableGenContext<InvItem>, IPlaceableGenContext<MoneySpawn>, IPlaceableGenContext<MapItem>, IPlaceableGenContext<EffectTile>,
        IGroupPlaceableGenContext<TeamSpawn>
    {
        public Map Map { get; set; }

        public ITile RoomTerrain { get { return new Tile(0); } }
        public ITile WallTerrain { get { return new Tile(2); } }
        public ITile UnbreakableTerrain { get { return new Tile(1); } }

        public int ID { get { return Map.ID; } }

        public int Width { get { return Map.Width; } }
        public int Height { get { return Map.Height; } }

        public int MaxFoes { get { return Map.MaxFoes; } set { Map.MaxFoes = value; } }
        public int RespawnTime { get { return Map.RespawnTime; } set { Map.RespawnTime = value; } }
        public SpawnList<TeamSpawner> TeamSpawns { get { return Map.TeamSpawns; } }

        public SpawnList<EffectTile> TileSpawns { get; set; }
        IRandPicker<EffectTile> ISpawningGenContext<EffectTile>.Spawner { get { return TileSpawns; } }
        public MoneySpawnRange MoneyAmount { get { return Map.MoneyAmount; } set { Map.MoneyAmount = value; } }
        IRandPicker<MoneySpawn> ISpawningGenContext<MoneySpawn>.Spawner { get { return Map.MoneyAmount; } }
        public CategorySpawnChooser<InvItem> ItemSpawns { get { return Map.ItemSpawns; } }
        IRandPicker<InvItem> ISpawningGenContext<InvItem>.Spawner { get { return Map.ItemSpawns; } }


        public IRandom Rand { get { return Map.Rand; } }
        public bool Begun { get { return Map.Begun; } }
        public bool NoRescue { get { return Map.NoRescue; } set { Map.NoRescue = value; } }
        public bool DropTitle { get { return Map.DropTitle; } set { Map.DropTitle = value; } }

        public Tile[][] Tiles { get { return Map.Tiles; } }
        public MapLayer Floor { get { return Map.Layers[0]; } }

        public ITile GetTile(Loc loc) { return Map.Tiles[loc.X][loc.Y]; }
        public virtual bool CanSetTile(Loc loc, ITile tile)
        {
            return !Map.Tiles[loc.X][loc.Y].TileEquivalent(UnbreakableTerrain);
        }
        public bool TrySetTile(Loc loc, ITile tile)
        {
            if (!CanSetTile(loc, tile)) return false;
            Map.Tiles[loc.X][loc.Y] = (Tile)tile;
            return true;
        }
        public void SetTile(Loc loc, ITile tile)
        {
            if (!TrySetTile(loc, tile))
                throw new InvalidOperationException("Can't place tile!");
        }

        public bool TilesInitialized { get { return Map.Tiles != null; } }

        public List<MapItem> Items { get { return Map.Items; } }
        public List<Team> AllyTeams { get { return Map.AllyTeams; } }
        public List<Team> MapTeams { get { return Map.MapTeams; } }

        public PostProcTile[][] PostProcGrid { get; private set; }


        public BaseMapGenContext()
        {
            Map = new Map();
            TileSpawns = new SpawnList<EffectTile>();
        }

        public void InitSeed(ulong seed)
        {
            Map.LoadRand(new ReRandom(seed));
        }
        public void LoadRand(ReRandom rand)
        {
            Map.LoadRand(rand);
        }

        public virtual void CreateNew(int width, int height)
        {
            Map.CreateNew(width, height);
            PostProcGrid = new PostProcTile[width][];
            for (int ii = 0; ii < width; ii++)
            {
                PostProcGrid[ii] = new PostProcTile[height];
                for (int jj = 0; jj < height; jj++)
                    PostProcGrid[ii][jj] = new PostProcTile();
            }
        }

        public bool TileBlocked(Loc loc)
        {
            return Map.TileBlocked(loc);
        }

        public bool TileBlocked(Loc loc, bool diagonal)
        {
            return Map.TileBlocked(loc, false, diagonal);
        }


        public bool HasTileEffect(Loc loc)
        {
            return Map.Tiles[loc.X][loc.Y].Effect.ID > -1;
        }

        List<Loc> IPlaceableGenContext<MoneySpawn>.GetAllFreeTiles() { return getAllFreeTiles(getOpenItemTiles); }
        List<Loc> IPlaceableGenContext<InvItem>.GetAllFreeTiles() { return getAllFreeTiles(getOpenItemTiles); }
        List<Loc> IPlaceableGenContext<MapItem>.GetAllFreeTiles() { return getAllFreeTiles(getOpenItemTiles); }
        List<Loc> IPlaceableGenContext<EffectTile>.GetAllFreeTiles() { return getAllFreeTiles(getOpenItemTiles); }


        protected delegate List<Loc> getOpen(Rect rect);
        protected virtual List<Loc> getAllFreeTiles(getOpen func)
        {
            return func(new Rect(0, 0, Width, Height));
        }


        List<Loc> IPlaceableGenContext<MoneySpawn>.GetFreeTiles(Rect rect) { return getOpenItemTiles(rect); }
        List<Loc> IPlaceableGenContext<InvItem>.GetFreeTiles(Rect rect) { return getOpenItemTiles(rect); }
        List<Loc> IPlaceableGenContext<MapItem>.GetFreeTiles(Rect rect) { return getOpenItemTiles(rect); }
        List<Loc> IPlaceableGenContext<EffectTile>.GetFreeTiles(Rect rect) { return getOpenItemTiles(rect); }

        protected List<Loc> getOpenItemTiles(Rect rect)
        {
            Grid.LocTest checkOp = (Loc testLoc) =>
            {
                return canPlaceItemTile(testLoc);
            };

            return Grid.FindTilesInBox(rect.Start, rect.Size, checkOp);
        }

        protected bool isObstructed(Loc loc)
        {
            if (!Collision.InBounds(Width, Height, loc))
                return true;
            return (!Tiles[loc.X][loc.Y].TileEquivalent(RoomTerrain) || HasTileEffect(loc));
        }

        bool IPlaceableGenContext<MoneySpawn>.CanPlaceItem(Loc loc) { return canPlaceItemTile(loc); }
        bool IPlaceableGenContext<InvItem>.CanPlaceItem(Loc loc) { return canPlaceItemTile(loc); }
        bool IPlaceableGenContext<MapItem>.CanPlaceItem(Loc loc) { return canPlaceItemTile(loc); }
        bool IPlaceableGenContext<EffectTile>.CanPlaceItem(Loc loc) { return canPlaceItemTile(loc); }

        protected virtual bool canPlaceItemTile(Loc loc)
        {
            if (isObstructed(loc))
                return false;
            if (PostProcGrid[loc.X][loc.Y].Status[(int)PostProcType.Panel] || PostProcGrid[loc.X][loc.Y].Status[(int)PostProcType.Item])
                return false;

            if (Grid.GetForkDirs(loc, isObstructed, isObstructed).Count >= 2)
                return false;
            foreach (MapItem item in Items)
            {
                if (item.TileLoc == loc)
                    return false;
            }
            return true;
        }

        void IPlaceableGenContext<MoneySpawn>.PlaceItem(Loc loc, MoneySpawn item)
        {
            MapItem newItem = new MapItem(true, item.Amount);
            newItem.TileLoc = loc;
            Items.Add(newItem);
        }
        void IPlaceableGenContext<InvItem>.PlaceItem(Loc loc, InvItem item)
        {
            MapItem newItem = new MapItem(item);
            newItem.TileLoc = loc;
            Items.Add(newItem);
        }
        void IPlaceableGenContext<MapItem>.PlaceItem(Loc loc, MapItem item)
        {
            MapItem newItem = new MapItem(item);
            newItem.TileLoc = loc;
            Items.Add(newItem);
        }
        void IPlaceableGenContext<EffectTile>.PlaceItem(Loc loc, EffectTile item)
        {
            Map.Tiles[loc.X][loc.Y].Effect = new EffectTile(item);
        }



        List<Loc> IGroupPlaceableGenContext<TeamSpawn>.GetFreeTiles(Rect rect)
        {
            Grid.LocTest checkOp = (Loc testLoc) =>
            {
                return canPlaceItemTile(testLoc);
            };

            return Grid.FindTilesInBox(rect.Start, rect.Size, checkOp);
        }

        bool IGroupPlaceableGenContext<TeamSpawn>.CanPlaceItem(Loc loc) { return canPlaceTeam(loc); }

        protected virtual bool canPlaceTeam(Loc loc)
        {
            if (TileBlocked(loc))
                return false;

            foreach (Team team in AllyTeams)
            {
                foreach (Character character in team.EnumerateChars())
                {
                    if (!character.Dead && character.CharLoc == loc)
                        return false;
                }
            }
            foreach (Team team in MapTeams)
            {
                foreach (Character character in team.EnumerateChars())
                {
                    if (!character.Dead && character.CharLoc == loc)
                        return false;
                }
            }
            return true;
        }

        void IGroupPlaceableGenContext<TeamSpawn>.PlaceItems(TeamSpawn itemBatch, Loc[] locs)
        {
            if (locs != null)
            {
                if (locs.Length != itemBatch.Team.MemberGuestCount)
                    throw new Exception("Team members not matching locations!");
                for (int ii = 0; ii < itemBatch.Team.Players.Count; ii++)
                    itemBatch.Team.Players[ii].CharLoc = locs[ii];
                for (int ii = 0; ii < itemBatch.Team.Guests.Count; ii++)
                    itemBatch.Team.Guests[ii].CharLoc = locs[itemBatch.Team.Players.Count + ii];
            }

            if (itemBatch.AllyFaction)
                AllyTeams.Add(itemBatch.Team);
            else
                MapTeams.Add(itemBatch.Team);
        }

        public virtual void FinishGen()
        {
            for (int xx = 0; xx < Width; xx++)
            {
                for (int yy = 0; yy < Height; yy++)
                    Map.Tiles[xx][yy].Effect.UpdateTileLoc(new Loc(xx, yy));
            }

            Map.CalculateAutotiles(new Loc(), new Loc(Width, Height));
            Map.CalculateTerrainAutotiles(new Loc(), new Loc(Width, Height));
        }

    }

    public class MapLoadContext : BaseMapGenContext
    {

    }


    public class MapGenContext : ListMapGenContext, IRoomGridGenContext
    {
        public void InitGrid(GridPlan plan)
        {
            GridPlan = plan;
        }
        public GridPlan GridPlan { get; private set; }

    }


    public class StairsMapGenContext : BaseMapGenContext, IViewPlaceableGenContext<MapGenEntrance>, IViewPlaceableGenContext<MapGenExit>
    {
        const int MIN_DIST_FROM_START = 5;


        public StairsMapGenContext()
        {
            GenEntrances = new List<MapGenEntrance>();
            GenExits = new List<MapGenExit>();
        }


        public override bool CanSetTile(Loc loc, ITile tile)
        {
            for (int ii = 0; ii < GenEntrances.Count; ii++)
            {
                if (GenEntrances[ii].Loc == loc)
                    return false;
            }
            for (int ii = 0; ii < GenExits.Count; ii++)
            {
                if (GenExits[ii].Loc == loc)
                    return false;
            }
            return base.CanSetTile(loc, tile);
        }

        public List<MapGenEntrance> GenEntrances { get; set; }
        public List<MapGenExit> GenExits { get; set; }

        List<Loc> IPlaceableGenContext<MapGenEntrance>.GetAllFreeTiles() { return getAllFreeTiles(((IPlaceableGenContext<MapGenEntrance>)this).GetFreeTiles); }
        List<Loc> IPlaceableGenContext<MapGenExit>.GetAllFreeTiles() { return getAllFreeTiles(((IPlaceableGenContext<MapGenExit>)this).GetFreeTiles); }

        List<Loc> IPlaceableGenContext<MapGenEntrance>.GetFreeTiles(Rect rect) { return getOpenEntranceExitTiles(rect); }
        List<Loc> IPlaceableGenContext<MapGenExit>.GetFreeTiles(Rect rect) { return getOpenEntranceExitTiles(rect); }

        private List<Loc> getOpenEntranceExitTiles(Rect rect)
        {
            List<Loc> tiles = getOpenItemTiles(rect);

            if (tiles.Count > 0)
                return tiles;

            Grid.LocTest checkLenient = (Loc testLoc) =>
            {
                return Tiles[testLoc.X][testLoc.Y].TileEquivalent(RoomTerrain);
            };

            tiles = Grid.FindTilesInBox(rect.Start, rect.Size, checkLenient);
            return tiles;
        }
        bool IPlaceableGenContext<MapGenEntrance>.CanPlaceItem(Loc loc) { return canPlaceItemTile(loc); }
        bool IPlaceableGenContext<MapGenExit>.CanPlaceItem(Loc loc) { return canPlaceItemTile(loc); }

        void IPlaceableGenContext<MapGenEntrance>.PlaceItem(Loc loc, MapGenEntrance item)
        {
            MapGenEntrance newItem = new MapGenEntrance(loc, item.Dir);
            GenEntrances.Add(newItem);
        }
        void IPlaceableGenContext<MapGenExit>.PlaceItem(Loc loc, MapGenExit item)
        {
            MapGenExit newItem = new MapGenExit(loc, new EffectTile(item.Tile));
            GenExits.Add(newItem);
        }

        int IViewPlaceableGenContext<MapGenEntrance>.Count { get { return GenEntrances.Count; } }
        int IViewPlaceableGenContext<MapGenExit>.Count { get { return GenExits.Count; } }
        MapGenEntrance IViewPlaceableGenContext<MapGenEntrance>.GetItem(int index) { return GenEntrances[index]; }
        MapGenExit IViewPlaceableGenContext<MapGenExit>.GetItem(int index) { return GenExits[index]; }
        Loc IViewPlaceableGenContext<MapGenEntrance>.GetLoc(int index) { return GenEntrances[index].Loc; }
        Loc IViewPlaceableGenContext<MapGenExit>.GetLoc(int index) { return GenExits[index].Loc; }


        protected override bool canPlaceItemTile(Loc loc)
        {
            if (!base.canPlaceItemTile(loc))
                return false;

            for (int ii = 0; ii < GenEntrances.Count; ii++)
            {
                if (GenEntrances[ii].Loc == loc)
                    return false;
            }
            for (int ii = 0; ii < GenExits.Count; ii++)
            {
                if (GenExits[ii].Loc == loc)
                    return false;
            }

            return true;
        }

        protected override bool canPlaceTeam(Loc loc)
        {
            if (!base.canPlaceTeam(loc))
                return false;

            foreach (MapGenEntrance entrance in GenEntrances)
            {
                if ((entrance.Loc - loc).Dist8() <= MIN_DIST_FROM_START)
                    return false;
            }
            return true;
        }

        public override void FinishGen()
        {
            foreach (MapGenEntrance entrance in GenEntrances)
                Map.EntryPoints.Add(new LocRay8(entrance.Loc, entrance.Dir));
            foreach (MapGenExit exit in GenExits)
                ((IPlaceableGenContext<EffectTile>)this).PlaceItem(exit.Loc, exit.Tile);

            base.FinishGen();
        }
    }

    public class ListMapGenContext : StairsMapGenContext, IFloorPlanGenContext
    {
        public void InitPlan(FloorPlan plan)
        {
            RoomPlan = plan;
        }
        public FloorPlan RoomPlan { get; private set; }

        protected override List<Loc> getAllFreeTiles(getOpen func)
        {
            List<Loc> freeTiles = new List<Loc>();
            //get all places that items are eligible
            for (int ii = 0; ii < RoomPlan.RoomCount; ii++)
            {
                IRoomGen room = RoomPlan.GetRoom(ii);

                List<Loc> tiles = func(room.Draw);

                freeTiles.AddRange(tiles);
            }
            return freeTiles;
        }
    }
}
