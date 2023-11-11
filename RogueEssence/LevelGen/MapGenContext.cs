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

        public ITile RoomTerrain { get { return new Tile(DataManager.Instance.GenFloor); } }
        public ITile WallTerrain { get { return new Tile(DataManager.Instance.GenWall); } }
        public ITile UnbreakableTerrain { get { return new Tile(DataManager.Instance.GenUnbreakable); } }

        public int ID { get { return Map.ID; } }

        public int Width { get { return Map.Width; } }
        public int Height { get { return Map.Height; } }
        public bool Wrap { get { return Map.EdgeView == BaseMap.ScrollEdge.Wrap; } }
        public SpawnList<TeamSpawner> TeamSpawns { get { return Map.TeamSpawns; } }

        public SpawnList<EffectTile> TileSpawns { get; set; }
        IRandPicker<EffectTile> ISpawningGenContext<EffectTile>.Spawner { get { return TileSpawns; } }
        public MoneySpawnRange MoneyAmount { get { return Map.MoneyAmount; } set { Map.MoneyAmount = value; } }
        IRandPicker<MoneySpawn> ISpawningGenContext<MoneySpawn>.Spawner { get { return Map.MoneyAmount; } }
        public CategorySpawnChooser<InvItem> ItemSpawns { get { return Map.ItemSpawns; } }
        IRandPicker<InvItem> ISpawningGenContext<InvItem>.Spawner { get { return Map.ItemSpawns; } }


        public IRandom Rand { get { return Map.Rand; } }
        public bool Begun { get { return Map.Begun; } }
        public Tile[][] Tiles { get { return Map.Tiles; } }

        public ITile GetTile(Loc loc) { return Map.GetTile(loc); }
        public virtual bool CanSetTile(Loc loc, ITile tile)
        {
            if (UnbreakableTerrain.TileEquivalent(Map.GetTile(loc)))
            {
                if (!UnbreakableTerrain.TileEquivalent(tile))
                    return false;
            }
            PostProcTile postProc = GetPostProc(loc);
            if ((postProc.Status & PostProcType.Terrain) != PostProcType.None)
                return false;
            return true;
        }
        public bool TrySetTile(Loc loc, ITile tile)
        {
            loc = Map.WrapLoc(loc);
            if (!CanSetTile(loc, tile)) return false;
            Map.Tiles[loc.X][loc.Y] = (Tile)tile;
            return true;
        }
        public void SetTile(Loc loc, ITile tile)
        {
            loc = Map.WrapLoc(loc);
            Map.Tiles[loc.X][loc.Y] = (Tile)tile;
        }

        public bool TilesInitialized { get { return Map.Tiles != null; } }

        public List<MapItem> Items { get { return Map.Items; } }
        public EventedList<Team> AllyTeams { get { return Map.AllyTeams; } }
        public EventedList<Team> MapTeams { get { return Map.MapTeams; } }

        public PostProcTile GetPostProc(Loc loc)
        {
            if (!Map.GetLocInMapBounds(ref loc))
                return null;
            return PostProcGrid[loc.X][loc.Y];
        }

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
        public void LoadRand(IRandom rand)
        {
            Map.LoadRand(rand);
        }

        public virtual void CreateNew(int width, int height, bool wrap = false)
        {
            Map.CreateNew(width, height);
            if (wrap)
                Map.EdgeView = BaseMap.ScrollEdge.Wrap;
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
            Tile tile = Map.GetTile(loc);
            if (tile == null)
                return false;
            return !String.IsNullOrEmpty(tile.Effect.ID);
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
            Tile tile = Map.GetTile(loc);
            TerrainData data = tile.Data.GetData();
            return (data.BlockType != TerrainData.Mobility.Passable || HasTileEffect(loc));
        }

        bool IPlaceableGenContext<MoneySpawn>.CanPlaceItem(Loc loc) { return canPlaceItemTile(loc); }
        bool IPlaceableGenContext<InvItem>.CanPlaceItem(Loc loc) { return canPlaceItemTile(loc); }
        bool IPlaceableGenContext<MapItem>.CanPlaceItem(Loc loc) { return canPlaceItemTile(loc); }
        bool IPlaceableGenContext<EffectTile>.CanPlaceItem(Loc loc) { return canPlaceItemTile(loc); }

        protected virtual bool canPlaceItemTile(Loc loc)
        {
            if (isObstructed(loc))
                return false;
            if ((GetPostProc(loc).Status & (PostProcType.Panel | PostProcType.Item)) != PostProcType.None)
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
            if (!Map.GetLocInMapBounds(ref loc))
                throw new ArgumentException("Loc out of bounds.");
            MapItem newItem = MapItem.CreateMoney(item.Amount);
            newItem.TileLoc = loc;
            Items.Add(newItem);
        }
        void IPlaceableGenContext<InvItem>.PlaceItem(Loc loc, InvItem item)
        {
            if (!Map.GetLocInMapBounds(ref loc))
                throw new ArgumentException("Loc out of bounds.");
            MapItem newItem = new MapItem(item);
            newItem.TileLoc = loc;
            Items.Add(newItem);
        }
        void IPlaceableGenContext<MapItem>.PlaceItem(Loc loc, MapItem item)
        {
            if (!Map.GetLocInMapBounds(ref loc))
                throw new ArgumentException("Loc out of bounds.");
            MapItem newItem = new MapItem(item);
            newItem.TileLoc = loc;
            Items.Add(newItem);
        }
        void IPlaceableGenContext<EffectTile>.PlaceItem(Loc loc, EffectTile item)
        {
            if (!Map.GetLocInMapBounds(ref loc))
                throw new ArgumentException("Loc out of bounds.");
            Tile tile = Map.GetTile(loc);
            tile.Effect = new EffectTile(item);
            //remove foliage, only foliage.
            //TODO: refactor to tell apart trap placement (remove terrain) and other effect tile placement (keep terrain)
            //This is a general problem, in which placeables of the same data type want to be placed, but placed differently
            //other example: money placement  instead of item placement, mob placement being multiple entities
            TerrainData data = tile.Data.GetData();
            if (data.BlockType == TerrainData.Mobility.Passable)
            {
                Tile tmpTile = (Tile)RoomTerrain.Copy();
                tile.Data = tmpTile.Data;
            }
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

        public virtual bool CanPlaceTeam(Loc loc)
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
                {
                    Loc loc = locs[ii];
                    if (!Map.GetLocInMapBounds(ref loc))
                        throw new ArgumentException("Loc out of bounds.");
                    itemBatch.Team.Players[ii].CharLoc = loc;
                }
                for (int ii = 0; ii < itemBatch.Team.Guests.Count; ii++)
                {
                    Loc loc = locs[itemBatch.Team.Players.Count + ii];
                    if (!Map.GetLocInMapBounds(ref loc))
                        throw new ArgumentException("Loc out of bounds.");
                    itemBatch.Team.Guests[ii].CharLoc = loc;
                }
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
                return RoomTerrain.TileEquivalent(GetTile(testLoc));
            };

            tiles = Grid.FindTilesInBox(rect.Start, rect.Size, checkLenient);
            return tiles;
        }
        bool IPlaceableGenContext<MapGenEntrance>.CanPlaceItem(Loc loc) { return canPlaceItemTile(loc); }
        bool IPlaceableGenContext<MapGenExit>.CanPlaceItem(Loc loc) { return canPlaceItemTile(loc); }


        public override bool CanPlaceTeam(Loc loc)
        {
            if (!base.CanPlaceTeam(loc))
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

        void IPlaceableGenContext<MapGenEntrance>.PlaceItem(Loc loc, MapGenEntrance item)
        {
            if (!Map.GetLocInMapBounds(ref loc))
                throw new ArgumentException("Loc out of bounds.");
            Tile tile = Map.GetTile(loc);
            tile.Data = ((Tile)RoomTerrain).Data.Copy();
            MapGenEntrance newItem = new MapGenEntrance(loc, item.Dir);
            GenEntrances.Add(newItem);
        }
        void IPlaceableGenContext<MapGenExit>.PlaceItem(Loc loc, MapGenExit item)
        {
            if (!Map.GetLocInMapBounds(ref loc))
                throw new ArgumentException("Loc out of bounds.");
            Tile tile = Map.GetTile(loc);
            tile.Data = ((Tile)RoomTerrain).Data.Copy();
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
                if (Map.InRange(entrance.Loc, loc, MIN_DIST_FROM_START))
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
