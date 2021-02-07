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

namespace RogueEssence.Dungeon
{
    public interface IMobSpawnMap
    {
        IRandom Rand { get; }
        bool Begun { get; }

        int ID { get; }
    }

    //Contains all data within a dungeon map, and a few helper functions
    [Serializable]
    public class Map : BaseMap, IEntryData
    {
        public enum SightRange
        {
            Any = -1,
            Clear = 0,
            Dark = 1,
            Murky = 2,
            Blind = 3
        }

        /// <summary>
        /// Describes how to handle the map scrolling past the edge of the map
        /// </summary>
        public enum ScrollEdge
        {
            Blank = 0,//displays a black void, or the BlankBG texture
            Clamp,//does not scroll past the edge of the map
        }

        public enum DiscoveryState
        {
            None = 0,
            Hinted,//only shows geography, not tiles/items
            Traversed//shows all
        }

        public LocalText Name { get; set; }
        public string GetSingleLineName() { return Name.ToLocal().Replace('\n', ' '); }
        public bool Released { get; set; }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        /// <summary>
        /// the internal name of the map, no spaces or special characters, never localized.
        /// Used to refer to map data and script data for this map!
        /// </summary>
        public string AssetName { get; set; }

        public string Music;
        public SightRange TileSight;
        public SightRange CharSight;

        public ScrollEdge EdgeView;


        public Dictionary<int, MapStatus> Status;

        public List<SingleCharEvent> PrepareEvents;
        public List<SingleCharEvent> StartEvents;
        public List<SingleCharEvent> CheckEvents;

        //if maps are to be separated into their own chunks, these members would be specific to each chunk
        public int MaxFoes;
        public int RespawnTime;
        public SpawnList<TeamSpawner> TeamSpawns;

        public MoneySpawnRange MoneyAmount;
        public CategorySpawnChooser<InvItem> ItemSpawns;

        public AutoTile BlankBG;
        public AutoTile FloorBG;
        public Dictionary<int, AutoTile> TextureMap;
        public int Element;

        public MapBG Background;


        public Loc? ViewCenter;
        public Loc ViewOffset;

        [NonSerialized]
        public ExplorerTeam ActiveTeam;

        public Character CurrentCharacter
        {
            get
            {
                CharIndex turnChar = CurrentTurnMap.GetCurrentTurnChar();
                if (turnChar.Team > -1)
                    return MapTeams[turnChar.Team].Players[turnChar.Char];
                else
                    return ActiveTeam.Players[turnChar.Char];
            }
        }

        public int MapTurns;

        public TurnState CurrentTurnMap;

        public DiscoveryState[][] DiscoveryArray;

        public Map()
        {
            AssetName = "";
            Name = new LocalText();
            Comment = "";
            Music = "";

            TileSight = SightRange.Clear;
            CharSight = SightRange.Clear;

            TeamSpawns = new SpawnList<TeamSpawner>();
            ItemSpawns = new CategorySpawnChooser<InvItem>();

            BlankBG = new AutoTile();

            Background = new MapBG();

            PrepareEvents = new List<SingleCharEvent>();
            StartEvents = new List<SingleCharEvent>();
            CheckEvents = new List<SingleCharEvent>();

            Status = new Dictionary<int, MapStatus>();

            FloorBG = new AutoTile();
            TextureMap = new Dictionary<int, AutoTile>();

            CurrentTurnMap = new TurnState();
        }


        public override void CreateNew(int width, int height)
        {
            base.CreateNew(width, height);
            DiscoveryArray = new DiscoveryState[width][];
            for (int ii = 0; ii < width; ii++)
                DiscoveryArray[ii] = new DiscoveryState[height];
        }


        public void ResizeJustified(int width, int height, Dir8 anchorDir)
        {
            //tiles
            Grid.LocAction changeOp = (Loc loc) => { Tiles[loc.X][loc.Y].Effect.UpdateTileLoc(loc); };
            Grid.LocAction newOp = (Loc loc) => { Tiles[loc.X][loc.Y] = new Tile(0, loc); };

            Loc diff = Grid.ResizeJustified(ref Tiles, width, height, anchorDir.Reverse(), changeOp, newOp);

            //discovery array
            changeOp = (Loc loc) => { };
            newOp = (Loc loc) => { };

            Grid.ResizeJustified(ref DiscoveryArray, width, height, anchorDir.Reverse(), changeOp, newOp);

            foreach (Character character in IterateCharacters())
            {
                character.CharLoc = Collision.ClampToBounds(width, height, character.CharLoc + diff);
                character.UpdateFrame();
                UpdateExploration(character);
            }

            //items
            foreach (MapItem item in Items)
                item.TileLoc = Collision.ClampToBounds(width, height, item.TileLoc + diff);

            //entry points
            for (int ii = 0; ii < EntryPoints.Count; ii++)
                EntryPoints[ii] = new LocRay8(EntryPoints[ii].Loc + diff, EntryPoints[ii].Dir);
        }

        public List<Character> RespawnMob()
        {
            List<Character> respawns = new List<Character>();
            if (TeamSpawns.Count > 0)
            {
                bool[][] traversedGrid = new bool[Width][];
                for (int xx = 0; xx < Width; xx++)
                    traversedGrid[xx] = new bool[Height];

                List<Loc> freeTiles = new List<Loc>();
                Grid.FloodFill(new Rect(new Loc(), new Loc(Width, Height)),
                    (Loc testLoc) =>
                    {
                        if (traversedGrid[testLoc.X][testLoc.Y])
                            return true;
                        return TileBlocked(testLoc);
                    },
                    (Loc testLoc) =>
                    {
                        if (traversedGrid[testLoc.X][testLoc.Y])
                            return true;
                        return TileBlocked(testLoc, true);
                    },
                    (Loc testLoc) =>
                    {
                        traversedGrid[testLoc.X][testLoc.Y] = true;

                        if (Grid.GetForkDirs(testLoc, TileBlocked, TileBlocked).Count >= 2)
                            return;
                        //must be walkable, not have a nonwalkable on at least 3 cardinal directions, not be within eyesight of any of the player characters
                        foreach (Character character in ActiveTeam.Players)
                        {
                            if (character.IsInSightBounds(testLoc))
                                return;
                        }

                        foreach (Team team in MapTeams)
                        {
                            foreach (Character character in team.Players)
                            {
                                if (!character.Dead && character.CharLoc == testLoc)
                                    return;
                            }
                        }
                        freeTiles.Add(testLoc);
                    },
                    EntryPoints[0].Loc);

                if (freeTiles.Count > 0)
                {
                    for (int ii = 0; ii < 10; ii++)
                    {
                        Team newTeam = TeamSpawns.Pick(Rand).Spawn(this);
                        if (newTeam == null)
                            continue;
                        Loc trialLoc = freeTiles[Rand.Next(freeTiles.Count)];
                        //find a way to place all members- needs to fit all of them in, or else fail the spawn

                        Grid.LocTest checkOpen = (Loc testLoc) =>
                        {
                            if (TileBlocked(testLoc))
                                return false;

                            Character locChar = GetCharAtLoc(testLoc);
                            if (locChar != null)
                                return false;
                            return true;
                        };
                        Grid.LocTest checkBlock = (Loc testLoc) =>
                        {
                            return TileBlocked(testLoc, true);
                        };
                        Grid.LocTest checkDiagBlock = (Loc testLoc) =>
                        {
                            return TileBlocked(testLoc, true, true);
                        };

                        List<Loc> resultLocs = new List<Loc>();
                        foreach (Loc loc in Grid.FindClosestConnectedTiles(new Loc(), new Loc(Width, Height),
                            checkOpen, checkBlock, checkDiagBlock, trialLoc, newTeam.Players.Count))
                        {
                            resultLocs.Add(loc);
                        }


                        if (resultLocs.Count >= newTeam.Players.Count)
                        {
                            for (int jj = 0; jj < newTeam.Players.Count; jj++)
                                newTeam.Players[jj].CharLoc = resultLocs[jj];

                            MapTeams.Add(newTeam);

                            foreach (Character member in newTeam.Players)
                            {
                                member.RefreshTraits();
                                respawns.Add(member);
                            }
                            break;
                        }
                    }
                }
            }
            return respawns;
        }

        public void EnterMap(ExplorerTeam activeTeam, LocRay8 entryPoint)
        {
            ActiveTeam = activeTeam;
            //place characters around in order
            ActiveTeam.Leader.CharLoc = entryPoint.Loc;
            if (entryPoint.Dir != Dir8.None)
                ActiveTeam.Leader.CharDir = entryPoint.Dir;
            foreach (Character character in ActiveTeam.Players)
            {
                //TODO: there may be a problem here; the method for finding a free space searches through all characters already in the map
                //since the active team has already been added to the map, all characters are counted and can block themselves when reassigning locations
                //warp all active team allies next to the player of this floor
                Loc? endLoc = GetClosestTileForChar(character, entryPoint.Loc);
                if (endLoc == null)
                    endLoc = entryPoint.Loc;

                character.CharLoc = endLoc.Value;
                if (entryPoint.Dir != Dir8.None)
                    character.CharDir = entryPoint.Dir;

                if (!character.Dead)
                    UpdateExploration(character);
            }
        }


        public Character LookupCharIndex(CharIndex charIndex)
        {
            Character character = null;
            if (charIndex.Team == -1)
                character = ActiveTeam.Players[charIndex.Char];
            else
                character = MapTeams[charIndex.Team].Players[charIndex.Char];
            return character;
        }

        public CharIndex GetCharIndex(Character character)
        {
            int charIndex = ActiveTeam.GetCharIndex(character);
            if (charIndex > -1)
                return new CharIndex(-1, charIndex);

            for (int ii = 0; ii < MapTeams.Count; ii++)
            {
                charIndex = MapTeams[ii].GetCharIndex(character);
                if (charIndex > -1)
                    return new CharIndex(ii, charIndex);
            }

            return CharIndex.Invalid;
        }

        public EffectTile.TileOwner GetTileOwner(Character target)
        {
            if (target == null)
                return EffectTile.TileOwner.None;
            if (target.MemberTeam == ActiveTeam)
                return EffectTile.TileOwner.Player;
            return EffectTile.TileOwner.Enemy;
        }


        public void CalculateAutotiles(Loc rectStart, Loc rectSize)
        {
            HashSet<int> floortilesets = new HashSet<int>();
            HashSet<int> blocktilesets = new HashSet<int>();
            for (int ii = rectStart.X; ii < rectStart.X + rectSize.X; ii++)
            {
                for (int jj = rectStart.Y; jj < rectStart.Y + rectSize.Y; jj++)
                {
                    if (Collision.InBounds(Width, Height, new Loc(ii, jj)))
                    {
                        AutoTile outTile;
                        if (Tiles[ii][jj].FloorTile.Layers.Count == 0)
                            Tiles[ii][jj].FloorTile = FloorBG.Copy();
                        if (TextureMap.TryGetValue(Tiles[ii][jj].Data.ID, out outTile))
                            Tiles[ii][jj].Data.TileTex = outTile.Copy();

                        if (Tiles[ii][jj].FloorTile.AutoTileset > -1)
                            floortilesets.Add(Tiles[ii][jj].FloorTile.AutoTileset);
                        if (Tiles[ii][jj].Data.TileTex.AutoTileset > -1)
                            blocktilesets.Add(Tiles[ii][jj].Data.TileTex.AutoTileset);
                    }
                }
            }
            foreach (int tileset in floortilesets)
            {
                AutoTileData entry = DataManager.Instance.GetAutoTile(tileset);
                entry.Tiles.AutoTileArea(Rand.FirstSeed, rectStart, rectSize, new Loc(Width, Height),
                    (int x, int y, List<TileLayer> tile) =>
                    {
                        if (Collision.InBounds(Width, Height, new Loc(x, y)))
                            Tiles[x][y].FloorTile.Layers = tile;
                    },
                    (int x, int y) =>
                    {
                        if (!Collision.InBounds(Width, Height, new Loc(x, y)))
                            return true;
                        if (Tiles[x][y].Data.TileTex.AutoTileset != -1 && Tiles[x][y].Data.TileTex.AutoTileset == Tiles[x][y].FloorTile.BorderTileset)
                            return false;
                        return Tiles[x][y].FloorTile.AutoTileset == tileset;
                    });
            }
            foreach (int tileset in blocktilesets)
            {
                AutoTileData entry = DataManager.Instance.GetAutoTile(tileset);
                entry.Tiles.AutoTileArea(Rand.FirstSeed, rectStart, rectSize, new Loc(Width, Height),
                    (int x, int y, List<TileLayer> tile) =>
                    {
                        if (Collision.InBounds(Width, Height, new Loc(x, y)))
                            Tiles[x][y].Data.TileTex.Layers = tile;
                    },
                    (int x, int y) =>
                    {
                        if (!Collision.InBounds(Width, Height, new Loc(x, y)))
                            return true;
                        return Tiles[x][y].Data.TileTex.AutoTileset == tileset;
                    });
            }
        }

        public void MapModified(Loc startLoc, Loc sizeLoc)
        {
            CalculateAutotiles(startLoc, sizeLoc);

            //update exploration for every character that sees the change
            if (ActiveTeam != null)
            {
                foreach (Character character in ActiveTeam.Players)
                {
                    if (!character.Dead)
                    {
                        Loc seenBounds = Character.GetSightDims();
                        if (Collision.Collides(startLoc, sizeLoc, character.CharLoc - seenBounds, seenBounds * 2 + new Loc(1)))
                            UpdateExploration(character);
                    }
                }
            }
        }

        private void discoveryLightOp(int x, int y, float light)
        {
            DiscoveryArray[x][y] = DiscoveryState.Traversed;
        }

        public void UpdateExploration(Character character)
        {
            //called whenever terrain changes, character moves or changes (including dies, sent home, recruited, summoned)
            //TODO: include when its vision changes/map vision changes
            if (character.MemberTeam == ActiveTeam)
            {
                //updateCharSight = true;
                if (!character.Dead)
                    character.UpdateTileSight(discoveryLightOp);
            }
        }

        public IEnumerable<Character> IterateCharacters()
        {
            if (ActiveTeam != null)
            {
                foreach (Character player in ActiveTeam.Players)
                    yield return player;
            }

            foreach (Team team in MapTeams)
            {
                foreach (Character character in team.Players)
                    yield return character;
            }
        }

        public Character GetCharAtLoc(Loc loc, Character exclude = null)
        {
            foreach (Character character in IterateCharacters())
            {
                if (!character.Dead && character.CharLoc == loc && exclude != character)
                    return character;
            }
            return null;
        }


        public List<Loc> FindNearLocs(Character character, Loc charLoc, int radius)
        {
            //warp search is capable of warping through walls (but not impassable ones) even if the player himself cannot pass them
            return Grid.FindConnectedTiles(charLoc - new Loc(radius), new Loc(radius * 2 + 1),
                (Loc testLoc) =>
                {
                    if (TileBlocked(testLoc, character.Mobility))
                        return false;
                    foreach (Character blockChar in IterateCharacters())
                    {
                        if (blockChar != character && blockChar.CharLoc == testLoc)
                            return false;
                    }
                    return true;
                },
                (Loc testLoc) =>
                {
                    return TileBlocked(testLoc, true);
                },
                (Loc testLoc) =>
                {
                    return TileBlocked(testLoc, true, true);
                },
                charLoc);
        }

        public Loc? GetClosestTileForChar(Character character, Loc loc)
        {
            return Grid.FindClosestConnectedTile(new Loc(), new Loc(Width, Height),
                (Loc testLoc) =>
                {
                    if (TileBlocked(testLoc, character.Mobility))
                        return false;

                    Character locChar = GetCharAtLoc(testLoc, character);
                    if (locChar != null)
                        return false;
                    return true;
                },
                (Loc testLoc) =>
                {
                    return TileBlocked(testLoc, true);
                },
                (Loc testLoc) =>
                {
                    return TileBlocked(testLoc, true, true);
                },
                loc);
        }

        public bool IsBlocked(Loc loc, uint mobility)
        {
            return IsBlocked(loc, mobility, true, false);
        }

        public bool IsBlocked(Loc loc, uint mobility, bool checkPlayer, bool checkDiagonal)
        {
            if (TileBlocked(loc, mobility, checkDiagonal))
                return true;

            if (checkPlayer && !checkDiagonal)
            {
                if (GetCharAtLoc(loc) != null)
                    return true;
            }

            //map object blocking

            return false;
        }


        public bool DirBlocked(Dir8 dir, Loc loc, uint mobility)
        {
            return DirBlocked(dir, loc, mobility, 1, true, true);
        }

        public bool DirBlocked(Dir8 dir, Loc loc, uint mobility, int distance, bool blockedByPlayer, bool blockedByDiagonal)
        {
            return Grid.IsDirBlocked(loc, dir,
                (Loc testLoc) =>
                {
                    return IsBlocked(testLoc, mobility, blockedByPlayer, false);
                },
                (Loc testLoc) =>
                {
                    return blockedByDiagonal && IsBlocked(testLoc, mobility, blockedByPlayer, true);
                },
                distance);
        }

        public void DrawDefaultTile(SpriteBatch spriteBatch, Loc drawPos)
        {
            BlankBG.Draw(spriteBatch, drawPos);
        }

        /// <summary>
        /// Call this so the map unregisters its events and delegates.
        ///
        /// </summary>
        public void DoCleanup()
        {

            foreach (Character c in IterateCharacters())
                c.DoCleanup();
        }
    }

    [Serializable]
    public abstract class BaseMap : IMobSpawnMap
    {

        //includes all start points
        public List<LocRay8> EntryPoints;

        protected ReRandom rand;
        public ReRandom Rand { get { return rand; } }
        IRandom IMobSpawnMap.Rand { get { return rand; } }
        public bool Begun { get; set; }

        public int ID { get; set; }

        public bool NoRescue;
        public bool NoSwitching;
        public bool DropTitle;

        public Tile[][] Tiles;

        public int Width { get { return Tiles.Length; } }
        public int Height { get { return Tiles[0].Length; } }

        public List<MapItem> Items;
        public List<Team> MapTeams;
        

        public BaseMap()
        {
            rand = new ReRandom(0);
            EntryPoints = new List<LocRay8>();

            Items = new List<MapItem>();
            MapTeams = new List<Team>();
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


        public void DrawLoc(SpriteBatch spriteBatch, Loc drawPos, Loc loc)
        {
            Tiles[loc.X][loc.Y].FloorTile.Draw(spriteBatch, drawPos);
            Tiles[loc.X][loc.Y].Data.TileTex.Draw(spriteBatch, drawPos);
        }  
    }

    // TODO: probably make this more generic; this is made specifically for item categories in maps at present.
    [Serializable]
    public class CategorySpawnChooser<T> : IRandPicker<T>
    {
        public SpawnDict<string, SpawnList<T>> Spawns;

        public CategorySpawnChooser()
        {
            Spawns = new SpawnDict<string, SpawnList<T>>();
        }
        public CategorySpawnChooser(SpawnDict<string, SpawnList<T>> spawns)
        {
            Spawns = spawns;
        }
        public CategorySpawnChooser(CategorySpawnChooser<T> other)
        {
            Spawns = new SpawnDict<string, SpawnList<T>>();
            foreach (string key in other.Spawns.GetKeys())
            {
                SpawnList<T> list = new SpawnList<T>();
                SpawnList<T> otherList = other.Spawns.GetSpawn(key);
                for (int ii = 0; ii < otherList.Count; ii++)
                    list.Add(otherList.GetSpawn(ii), otherList.GetSpawnRate(ii));
                Spawns.Add(key, list, other.Spawns.GetSpawnRate(key));
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (SpawnList<T> element in Spawns)
            {
                foreach (T item in element)
                    yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public T Pick(IRandom rand)
        {
            SpawnDict<string, SpawnList<T>> tempSpawn = new SpawnDict<string, SpawnList<T>>();
            foreach (string key in Spawns.GetKeys())
            {
                SpawnList<T> otherList = Spawns.GetSpawn(key);
                if (!otherList.CanPick)
                    continue;
                tempSpawn.Add(key, otherList, Spawns.GetSpawnRate(key));
            }
            SpawnList<T> choice = tempSpawn.Pick(rand);
            return choice.Pick(rand);
        }

        public bool ChangesState => false;
        public bool CanPick
        {
            get
            {
                if (!Spawns.CanPick)
                    return false;
                foreach (SpawnList<T> spawn in Spawns)
                {
                    if (spawn.CanPick)
                        return true;
                }
                return false;
            }
        }

        public IRandPicker<T> CopyState()
        {
            return new CategorySpawnChooser<T>(this);
        }
    }
}
