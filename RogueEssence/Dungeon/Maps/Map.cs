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


        public string GetColoredName()
        {
            return String.Format("[color=#FFC663]{0}[color]", Name.ToLocal().Replace('\n', ' '));
        }

        public bool Released { get; set; }
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        /// <summary>
        /// the internal name of the map, no spaces or special characters, never localized.
        /// Used to refer to map data and script data for this map!
        /// </summary>
        public string AssetName { get; set; }

        public Dictionary<LuaEngine.EDungeonMapCallbacks, ScriptEvent> ScriptEvents;

        public string Music;
        public SightRange TileSight;
        public SightRange CharSight;

        public ScrollEdge EdgeView;


        public Dictionary<int, MapStatus> Status;

        public ActiveEffect MapEffect;

        //if maps are to be separated into their own chunks, these members would be specific to each chunk
        public int MaxFoes;
        public int RespawnTime;
        public SpawnList<TeamSpawner> TeamSpawns;

        public MoneySpawnRange MoneyAmount;
        public CategorySpawnChooser<InvItem> ItemSpawns;

        public AutoTile BlankBG;
        public Dictionary<int, AutoTile> TextureMap;
        public int Element;

        public IBackgroundSprite Background;


        public Loc? ViewCenter;
        public Loc ViewOffset;

        [NonSerialized]
        private ExplorerTeam activeTeam;
        public ExplorerTeam ActiveTeam
        {
            get { return activeTeam; }
            set
            {
                if (activeTeam != null)
                    activeTeam.ContainingMap = null;
                activeTeam = value;
                activeTeam.ContainingMap = this;
            }
        }

        public bool NoRescue;
        public bool NoSwitching;

        public Character CurrentCharacter
        {
            get
            {
                CharIndex turnChar = CurrentTurnMap.GetCurrentTurnChar();
                return LookupCharIndex(turnChar);
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

            ScriptEvents = new Dictionary<LuaEngine.EDungeonMapCallbacks, ScriptEvent>();

            TileSight = SightRange.Clear;
            CharSight = SightRange.Clear;

            TeamSpawns = new SpawnList<TeamSpawner>();
            ItemSpawns = new CategorySpawnChooser<InvItem>();

            Background = new MapBG();
            BlankBG = new AutoTile();

            MapEffect = new ActiveEffect();

            Status = new Dictionary<int, MapStatus>();

            TextureMap = new Dictionary<int, AutoTile>();

            CurrentTurnMap = new TurnState();

            setTeamEvents();
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
            foreach (MapLayer layer in Layers)
                layer.ResizeJustified(width, height, anchorDir);

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
                EntryPoints[ii] = new LocRay8(Collision.ClampToBounds(width, height, EntryPoints[ii].Loc + diff), EntryPoints[ii].Dir);
        }


        private void baseRefresh()
        {
            NoRescue = false;
            NoSwitching = false;
        }

        private void OnRefresh()
        {
            DungeonScene.EventEnqueueFunction<RefreshEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<RefreshEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnMapRefresh, null);
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, MapEffect.OnMapRefresh, null);

                foreach (MapStatus status in Status.Values)
                {
                    MapStatusData mapStatusData = DataManager.Instance.GetMapStatus(status.ID);
                    PassiveContext effectContext = new PassiveContext(status, mapStatusData, GameEventPriority.USER_PORT_PRIORITY, null);
                    effectContext.AddEventsToQueue<RefreshEvent>(queue, maxPriority, ref nextPriority, mapStatusData.OnMapRefresh, null);
                }
            };
            foreach (EventQueueElement<RefreshEvent> effect in DungeonScene.IterateEvents<RefreshEvent>(function))
                effect.Event.Apply(effect.Owner, effect.OwnerChar, null);
        }

        public void RefreshTraits()
        {
            baseRefresh();

            OnRefresh();
        }

        public List<Loc> GetFreeToSpawnTiles()
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

                    foreach (Team team in AllyTeams)
                    {
                        foreach (Character character in team.EnumerateChars())
                        {
                            if (!character.Dead && character.CharLoc == testLoc)
                                return;
                        }
                    }

                    foreach (Team team in MapTeams)
                    {
                        foreach (Character character in team.EnumerateChars())
                        {
                            if (!character.Dead && character.CharLoc == testLoc)
                                return;
                        }
                    }
                    freeTiles.Add(testLoc);
                },
                EntryPoints[0].Loc);
            return freeTiles;
        }

        public List<Character> RespawnMob()
        {
            List<Character> respawns = new List<Character>();
            if (TeamSpawns.CanPick)
            {
                List<Loc> freeTiles = GetFreeToSpawnTiles();
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


                        if (resultLocs.Count >= newTeam.Players.Count + newTeam.Guests.Count)
                        {
                            for (int jj = 0; jj < newTeam.Players.Count; jj++)
                                newTeam.Players[jj].CharLoc = resultLocs[jj];
                            for (int jj = 0; jj < newTeam.Guests.Count; jj++)
                                newTeam.Guests[jj].CharLoc = resultLocs[newTeam.Players.Count + jj];

                            MapTeams.Add(newTeam);

                            foreach (Character member in newTeam.EnumerateChars())
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


        public Team GetTeam(Faction faction, int teamIndex)
        {
            switch (faction)
            {
                case Faction.Foe:
                    return MapTeams[teamIndex];
                case Faction.Friend:
                    return AllyTeams[teamIndex];
                default:
                    return ActiveTeam;
            }
        }

        public void EnterMap(ExplorerTeam activeTeam, LocRay8 entryPoint)
        {
            ActiveTeam = activeTeam;

            //place characters around in order
            ActiveTeam.Leader.CharLoc = entryPoint.Loc;
            if (entryPoint.Dir != Dir8.None)
                ActiveTeam.Leader.CharDir = entryPoint.Dir;
            foreach (Character character in ActiveTeam.EnumerateChars())
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
            Team team = GetTeam(charIndex.Faction, charIndex.Team);
            EventedList<Character> playerList;
            if (charIndex.Guest)
                playerList = team.Guests;
            else
                playerList = team.Players;

            return playerList[charIndex.Char];
        }

        public CharIndex GetCharIndex(Character character)
        {
            {
                CharIndex charIndex = ActiveTeam.GetCharIndex(character);
                if (charIndex != CharIndex.Invalid)
                    return new CharIndex(Faction.Player, 0, charIndex.Guest, charIndex.Char);
            }

            for (int ii = 0; ii < AllyTeams.Count; ii++)
            {
                CharIndex charIndex = AllyTeams[ii].GetCharIndex(character);
                if (charIndex != CharIndex.Invalid)
                    return new CharIndex(Faction.Friend, ii, charIndex.Guest, charIndex.Char);
            }

            for (int ii = 0; ii < MapTeams.Count; ii++)
            {
                CharIndex charIndex = MapTeams[ii].GetCharIndex(character);
                if (charIndex != CharIndex.Invalid)
                    return new CharIndex(Faction.Foe, ii, charIndex.Guest, charIndex.Char);
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
            foreach (MapLayer layer in Layers)
                layer.CalculateAutotiles(this.rand.FirstSeed, rectStart, rectSize);
        }

        public void CalculateTerrainAutotiles(Loc rectStart, Loc rectSize)
        {
            ReNoise noise = new ReNoise(rand.FirstSeed);
            //does not calculate floor tiles.
            //in all known use cases, there is no need to autotile floor tiles.
            //if a use case is brought up that does, this can be changed.
            HashSet<int> blocktilesets = new HashSet<int>();
            for (int ii = rectStart.X; ii < rectStart.X + rectSize.X; ii++)
            {
                for (int jj = rectStart.Y; jj < rectStart.Y + rectSize.Y; jj++)
                {
                    if (Collision.InBounds(Width, Height, new Loc(ii, jj)))
                    {
                        //Only color empty tiles
                        if (!Tiles[ii][jj].Data.StableTex)
                        {
                            AutoTile outTile;
                            if (TextureMap.TryGetValue(Tiles[ii][jj].Data.ID, out outTile))
                            {
                                Tiles[ii][jj].Data.TileTex = outTile.Copy();

                                if (Tiles[ii][jj].Data.TileTex.AutoTileset > -1)
                                    blocktilesets.Add(Tiles[ii][jj].Data.TileTex.AutoTileset);
                            }
                        }
                    }
                }
            }
            foreach (int tileset in blocktilesets)
            {
                AutoTileData entry = DataManager.Instance.GetAutoTile(tileset);
                entry.Tiles.AutoTileArea(noise, rectStart, rectSize, new Loc(Width, Height),
                    (int x, int y, int neighborCode) =>
                    {
                        Tiles[x][y].Data.TileTex.NeighborCode = neighborCode;
                    },
                    (int x, int y) =>
                    {
                        if (!Collision.InBounds(Width, Height, new Loc(x, y)))
                            return true;
                        return Tiles[x][y].Data.TileTex.AutoTileset == tileset;
                    },
                    (int x, int y) =>
                    {
                        if (!Collision.InBounds(Width, Height, new Loc(x, y)))
                            return true;
                        return Tiles[x][y].Data.TileTex.AutoTileset == tileset || Tiles[x][y].Data.TileTex.Associates.Contains(tileset);
                    });
            }
        }

        public void MapModified(Loc startLoc, Loc sizeLoc)
        {
            CalculateAutotiles(startLoc, sizeLoc);
            CalculateTerrainAutotiles(startLoc, sizeLoc);

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

        public IEnumerable<Character> IterateCharacters(bool ally = true, bool foe = true)
        {
            if (ally)
            {
                if (ActiveTeam != null)
                {
                    foreach (Character player in ActiveTeam.EnumerateChars())
                        yield return player;
                }

                foreach (Team team in AllyTeams)
                {
                    foreach (Character character in team.EnumerateChars())
                        yield return character;
                }
            }

            if (foe)
            {
                foreach (Team team in MapTeams)
                {
                    foreach (Character character in team.EnumerateChars())
                        yield return character;
                }
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

        /// <summary>
        /// Gets the the acceptable warp destination for a character, as close as possible to the ideal warp destination.
        /// </summary>
        /// <param name="character">The character being moved. Null if not a character currently on the map.</param>
        /// <param name="loc">The ideal warp destination.</param>
        /// <returns></returns>
        public Loc? GetClosestTileForChar(Character character, Loc loc)
        {
            return Grid.FindClosestConnectedTile(new Loc(), new Loc(Width, Height),
                (Loc testLoc) =>
                {
                    if (character == null)
                    {
                        if (TileBlocked(testLoc))
                            return false;
                    }
                    else
                    {
                        if (TileBlocked(testLoc, character.Mobility))
                            return false;
                    }

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

        public void DrawDefaultTile(SpriteBatch spriteBatch, Loc drawPos, Loc mapPos)
        {
            INoise noise = new ReNoise(rand.FirstSeed);
            BlankBG.DrawBlank(spriteBatch, drawPos, noise.Get2DUInt64((ulong)mapPos.X, (ulong)mapPos.Y));
        }

        private void setTeamEvents()
        {
            AllyTeams.ItemAdding += addingTeam;
            MapTeams.ItemAdding += addingTeam;
            AllyTeams.ItemChanging += settingAllies;
            MapTeams.ItemChanging += settingFoes;
            AllyTeams.ItemRemoving += removingTeam;
            MapTeams.ItemRemoving += removingTeam;
            AllyTeams.ItemsClearing += clearingAllies;
            MapTeams.ItemsClearing += clearingFoes;
        }

        private void settingAllies(int index, Team team)
        {
            AllyTeams[index].ContainingMap = null;
            team.ContainingMap = this;
            //TODO: update location caches
        }
        private void settingFoes(int index, Team team)
        {
            MapTeams[index].ContainingMap = null;
            team.ContainingMap = this;
            //TODO: update location caches
        }
        private void addingTeam(int index, Team team)
        {
            team.ContainingMap = this;
            //TODO: update location caches
        }
        private void removingTeam(int index, Team team)
        {
            team.ContainingMap = null;
            //TODO: update location caches
        }
        private void clearingAllies()
        {
            foreach (Team team in AllyTeams)
                team.ContainingMap = null;
            //TODO: update location caches
        }
        private void clearingFoes()
        {
            foreach (Team team in MapTeams)
                team.ContainingMap = null;
            //TODO: update location caches
        }

        //========================
        //  Script Stuff
        //========================

        /// <summary>
        /// Called before the map is displayed to run script events and etc.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> OnInit()
        {
            DiagManager.Instance.LogInfo("Map.OnInit(): Initializing the map..");
            if (AssetName != "")
                LuaEngine.Instance.RunDungeonMapScript(AssetName);

            //Check for floor specific events in the current dungeon's package.
            //Reload the map events
            foreach (var ev in ScriptEvents)
                ev.Value.ReloadEvent();


            //!TODO: Handle entity callbacks maybe?

            //Do script event
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EDungeonMapCallbacks.Init));

            //Notify script engine
            LuaEngine.Instance.OnDungeonMapInit(AssetName, this);
        }

        public IEnumerator<YieldInstruction> OnEnter()
        {
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EDungeonMapCallbacks.Enter));

            LuaEngine.Instance.OnDungeonMapEnter(AssetName, this);
        }

        public IEnumerator<YieldInstruction> OnExit()
        {
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EDungeonMapCallbacks.Exit));

            LuaEngine.Instance.OnDungeonMapExit(AssetName, this);
        }

        /// <summary>
        /// Search the current dungeon's lua package for defined floor callbacks functions, and add those that were found.
        /// </summary>
        private void LoadScriptEvents()
        {
            foreach (var ev in LuaEngine.EnumerateDungeonFloorCallbackTypes())
            {
                string cbackn = LuaEngine.MakeDungeonMapScriptCallbackName(AssetName, ev);
                if (LuaEngine.Instance.DoesFunctionExists(cbackn))
                    ScriptEvents[ev] = new ScriptEvent(cbackn);
            }
        }

        /// <summary>
        /// Runs the specified script event for the current dungeon floor map if it exists. Or fallbacks to the dungeon set default floor event.
        /// </summary>
        /// <param name="ev">The event to run.</param>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> RunScriptEvent(LuaEngine.EDungeonMapCallbacks ev)
        {
            //If we have a floor specific script event to run, do it, or run the default dungeon specified script event for this floor event if there is one defined.
            if (ScriptEvents.ContainsKey(ev))
                yield return CoroutineManager.Instance.StartCoroutine(ScriptEvents[ev].Apply(this));
        }

        public void LuaEngineReload()
        {
            LoadLua();
        }

        public void LoadLua()
        {
            foreach (Team team in AllyTeams)
                team.LoadLua();
            foreach (Team team in MapTeams)
                team.LoadLua();
        }
        public void SaveLua()
        {
            foreach (Team team in AllyTeams)
                team.SaveLua();
            foreach (Team team in MapTeams)
                team.SaveLua();
        }

        /// <summary>
        /// Call this so the map unregisters its events and delegates.
        ///
        /// </summary>
        public void DoCleanup()
        {
            foreach (var e in ScriptEvents)
                e.Value.DoCleanup();
            ScriptEvents.Clear();

            if (ActiveTeam != null)
            {
                foreach (Character c in IterateCharacters())
                    c.DoCleanup();
            }
        }



        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //No need to set team events since they'd already be set during the class construction phase of deserialization
            ReconnectMapReference();
        }

        protected virtual void ReconnectMapReference()
        {
            //reconnect Teams' references
            foreach (Team team in AllyTeams)
                team.ContainingMap = this;
            foreach (Team team in MapTeams)
                team.ContainingMap = this;
        }
    }


}
