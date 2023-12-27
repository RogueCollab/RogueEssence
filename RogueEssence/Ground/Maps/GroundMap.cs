﻿using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System.Runtime.Serialization;
using AABB;
using System.Linq;
using RogueEssence.Script;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using RogueEssence.Dev;

namespace RogueEssence.Ground
{
    [Serializable]
    public class GroundMap : IWorld, IEntryData
    {
        [NonSerialized]
        private AABB.Grid grid;

        private GroundWall[][] obstacles;

        protected IRandom rand;
        public IRandom Rand { get { return rand; } }

        [JsonConverter(typeof(MapStatusDictConverter))]
        public Dictionary<string, MapStatus> Status;

        [NonSerialized]
        private Dictionary<LuaEngine.EMapCallbacks, ScriptEvent> scriptEvents; //psy's notes: In order to get rid of duplicates and help make things more straightforward I moved script events to a dictionary

        public IBackgroundSprite Background;
        public AutoTile BlankBG;

        public List<MapLayer> Layers;

        /// <summary>
        /// Size in tex units (8x8 tiles)
        /// </summary>
        public int TexSize { get; private set; }
        public int TileSize { get { return TexSize * GraphicsManager.TEX_SIZE; } }

        /// <summary>
        /// Width of the map in tiles
        /// </summary>
        public int Width { get { return Layers[0].Tiles.Length; } }

        /// <summary>
        /// Height of the map in tiles
        /// </summary>
        public int Height { get { return Layers[0].Tiles[0].Length; } }

        /// <summary>
        /// Size of the map in tiles
        /// </summary>
        public Loc Size { get { return new Loc(Width, Height); } }

        public int TexWidth { get { return Width * TexSize; } }
        public int TexHeight { get { return Height * TexSize; } }

        /// <summary>
        /// Width of the map in pixels
        /// </summary>
        public int GroundWidth { get { return Width * TileSize; } }

        /// <summary>
        /// Height of the map in pixels
        /// </summary>
        public int GroundHeight { get { return Height * TileSize; } }

        /// <summary>
        /// Size of the map in pixels
        /// </summary>
        public Loc GroundSize { get { return new Loc(GroundWidth, GroundHeight); } }

        /// <summary>
        /// the internal name of the map, no spaces or special characters, never localized.
        /// Used to refer to map data and script data for this map.  This value is always set when loaded,
        /// But must remain serialized for state saving
        /// </summary>
        public string AssetName;

        public LocalText Name { get; set; }
        public string GetColoredName()
        {
            return String.Format("[color=#FFFFA5]{0}[color]", Name.ToLocal().Replace('\n', ' '));
        }

        public bool Released { get; set; }
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public string Music;
        public Map.ScrollEdge EdgeView;
        public bool NoSwitching;

        public Loc? ViewCenter;
        public Loc ViewOffset;

        public GroundChar ActiveChar;

        public List<AnimLayer> Decorations;
        //TODO: fully implement multilayered entities
        public List<EntityLayer> Entities;

        public GroundMap()
        {
            AssetName = "";
            rand = new ReRandom(0);
            scriptEvents = new Dictionary<LuaEngine.EMapCallbacks, ScriptEvent>();

            Entities = new List<EntityLayer>();

            Status = new Dictionary<string, MapStatus>();

            Background = new MapBG();
            BlankBG = new AutoTile();

            Name = new LocalText();
            Comment = "";
            Music = "";

            Layers = new List<MapLayer>();

            Decorations = new List<AnimLayer>();

        }

        [JsonConstructor]
        public GroundMap(bool init)
        {
            AssetName = "";
            scriptEvents = new Dictionary<LuaEngine.EMapCallbacks, ScriptEvent>();
            //dummy constructor for json serialization
        }

        public void LoadRand(IRandom rand)
        {
            this.rand = rand;
        }

        /// <summary>
        /// Call this so the map unregisters its events and delegates.
        ///
        /// </summary>
        public void DoCleanup()
        {
            DiagManager.Instance.LogInfo(String.Format("GroundMap.~GroundMap(): Finalizing {0}..", AssetName));

            foreach (var e in scriptEvents)
                e.Value.DoCleanup();
            scriptEvents.Clear();

            foreach (GroundEntity ent in IterateEntities())
            {
                if( ent != null )
                    ent.DoCleanup();
            }

            if (AssetName != "")
                LuaEngine.Instance.CleanMapScript(AssetName);
        }

        /// <summary>
        /// Run this map's specified script event
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> RunScriptEvent(LuaEngine.EMapCallbacks ev)
        {
            if (scriptEvents.ContainsKey(ev))
                yield return CoroutineManager.Instance.StartCoroutine(scriptEvents[ev].Apply(this));
        }

        public void OnEditorInit()
        {
            if (AssetName != "")
                LuaEngine.Instance.RunGroundMapScript(AssetName);

            //Reload the map events
            LoadScriptEvents();

            foreach (GroundEntity entity in IterateEntities())
                entity.ReloadEvents();
        }

        /// <summary>
        /// Called by GroundScene as the map is being initialized.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> OnInit()
        {
            DiagManager.Instance.LogInfo("GroundMap.OnInit(): Initializing the map..");
            if (AssetName != "")
                LuaEngine.Instance.RunGroundMapScript(AssetName);

            //Reload the map events
            LoadScriptEvents();

            foreach (GroundEntity entity in IterateEntities())
                entity.OnMapInit();

            //Do script event
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EMapCallbacks.Init));

            //Notify script engine
            LuaEngine.Instance.OnGroundMapInit(AssetName, this);
        }

        public IEnumerator<YieldInstruction> OnGameLoad()
        {
            //Do script event
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EMapCallbacks.GameLoad));

            //Notify script engine?
        }

        public IEnumerator<YieldInstruction> OnGameSave()
        {
            //Do script event
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EMapCallbacks.GameSave));

            //Notify script engine?
        }


        /// <summary>
        /// Called by the GroundScene when the map is in "Begin" stage.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> OnEnter()
        {
            //Do script event
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EMapCallbacks.Enter));

            //Notify script engine
            LuaEngine.Instance.OnGroundMapEnter(AssetName, this);
        }

        public IEnumerator<YieldInstruction> OnExit()
        {
            //Do script event
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EMapCallbacks.Exit));

            //Notify script engine
            LuaEngine.Instance.OnGroundMapExit(AssetName, this);
        }

        /// <summary>
        /// Called by ProcessInput to update the focused character using events stored in the map.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> OnCheck()
        {
            //Do script event
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EMapCallbacks.Update));
        }


        public void CreateNew(int width, int height, int texSize)
        {
            TexSize = texSize;

            Layers.Clear();
            MapLayer layer = new MapLayer("New Layer");
            layer.CreateNew(width, height);
            Layers.Add(layer);

            Decorations.Clear();
            Decorations.Add(new AnimLayer("New Deco"));

            Entities.Clear();
            Entities.Add(new EntityLayer("New EntLayer"));

            int divSize = GraphicsManager.TEX_SIZE;
            obstacles = new GroundWall[width * TexSize][];
            for (int ii = 0; ii < obstacles.Length; ii++)
            {
                obstacles[ii] = new GroundWall[height * TexSize];
                for (int jj = 0; jj < obstacles[ii].Length; jj++)
                    obstacles[ii][jj] = new GroundWall(ii * divSize, jj * divSize, divSize, divSize);
            }

            this.grid = new AABB.Grid(MathUtils.DivUp(GroundWidth, GraphicsManager.TileSize), MathUtils.DivUp(GroundHeight, GraphicsManager.TileSize), GraphicsManager.TileSize);
        }


        
        public void SetPlayerChar(GroundChar mapChar)
        {
            //GroundChar groundChar = new GroundChar(ActiveTeam.Leader, entry.Loc, (entry.Dir != Dir8.None) ? entry.Dir : ActiveTeam.Leader.CharDir, "PLAYER");
            if (ActiveChar != null)
                grid.Remove(ActiveChar, EdgeView == BaseMap.ScrollEdge.Wrap);
            ActiveChar = mapChar;

            if (ActiveChar != null)
                signCharToMap(mapChar);
        }

        public void AddMapChar(GroundChar mapChar)
        {
            Entities[0].MapChars.Add(mapChar);
            signCharToMap(mapChar);
        }

        public void AddSpawner(GroundSpawner spwn)
        {
            Entities[0].Spawners.Add(spwn);
        }

        public void RemoveSpawner(GroundSpawner spwner)
        {
            //Delete the spawned NPC if any!
            if (spwner.CurrentNPC != null)
                RemoveTempChar(spwner.CurrentNPC);
            Entities[0].Spawners.Remove(spwner);
        }

        /// <summary>
        /// Lookup a character of any type by instance name on the map, and return it if found, or returns null if not found.
        /// </summary>
        /// <param name="instancename">The name of the instance of this character</param>
        /// <returns>Character instance or null if not found</returns>
        public GroundChar GetChar(string instancename)
        {
            if (instancename == "PLAYER")
                return this.ActiveChar;

            GroundChar found = Entities[0].MapChars.Find((GroundChar ch) => { return ch.EntName == instancename; });
            if (found != null)
                return found;

            found = Entities[0].TemporaryChars.Find((GroundChar ch) => { return ch.EntName == instancename; });
            return found;
        }


        /// <summary>
        /// Lookup a map character by instance name on the map, and return it if found, or returns null if not found.
        /// </summary>
        /// <param name="instancename">The name of the instance of this character</param>
        /// <returns>Character instance or null if not found</returns>
        public GroundChar GetMapChar(string instancename)
        {
            GroundChar found = Entities[0].MapChars.Find((GroundChar ch) => { return ch.EntName == instancename; });
            return found;
        }

        /// <summary>
        /// Lookup a temp character by instance name on the map, and return it if found, or returns null if not found.
        /// </summary>
        /// <param name="instancename">The name of the instance of this character</param>
        /// <returns>Character instance or null if not found</returns>
        public GroundChar GetTempChar(string instancename)
        {
            GroundChar found = Entities[0].TemporaryChars.Find((GroundChar ch) => { return ch.EntName == instancename; });
            return found;
        }

        public void RemoveMapChar(GroundChar mapChar)
        {
            Entities[0].MapChars.Remove(mapChar);
            grid.Remove(mapChar, EdgeView == BaseMap.ScrollEdge.Wrap);
        }
        private void signCharToMap(GroundChar groundChar)
        {
            groundChar.Map = this;
            grid.Add(groundChar, EdgeView == BaseMap.ScrollEdge.Wrap);
        }
        public void AddObject(GroundObject groundObj)
        {
            Entities[0].GroundObjects.Add(groundObj);
            grid.Add(groundObj, EdgeView == BaseMap.ScrollEdge.Wrap);
        }

        public void RemoveObject(GroundObject groundObj)
        {
            Entities[0].GroundObjects.Remove(groundObj);
            grid.Remove(groundObj, EdgeView == BaseMap.ScrollEdge.Wrap);
        }

        /// <summary>
        /// Add a character entity that shouldn't be serialized!
        /// </summary>
        /// <param name="ch"></param>
        public void AddTempChar(GroundChar ch)
        {
            ch.Map = this;
            Entities[0].TemporaryChars.Add(ch);
            grid.Add(ch, EdgeView == BaseMap.ScrollEdge.Wrap);
        }

        /// <summary>
        /// Removed a temp character entity from the list of temp characters
        /// </summary>
        /// <param name="ch"></param>
        public void RemoveTempChar(GroundChar ch)
        {
            Entities[0].TemporaryChars.Remove(ch);
            grid.Remove(ch, EdgeView == BaseMap.ScrollEdge.Wrap);
        }

        public void AddTempObject(GroundObject groundObj)
        {
            Entities[0].TemporaryObjects.Add(groundObj);
            grid.Add(groundObj, EdgeView == BaseMap.ScrollEdge.Wrap);
        }

        public void RemoveTempObject(GroundObject groundObj)
        {
            Entities[0].TemporaryObjects.Remove(groundObj);
            grid.Remove(groundObj, EdgeView == BaseMap.ScrollEdge.Wrap);
        }

        /// <summary>
        /// Lookup an object instance by name on the map.
        /// Returns the object if found, or null.
        /// </summary>
        /// <param name="instancename">Name of the object instance</param>
        /// <returns>Forund object, or null</returns>
        public GroundObject GetObj(string instancename)
        {
            GroundObject found = Entities[0].GroundObjects.Find((GroundObject ch) => { return ch.EntName == instancename; });
            if (found != null)
                return found;

            return null;
        }

        /// <summary>
        /// Lookup a temporary object instance by name on the map.
        /// Returns the object if found, or null.
        /// </summary>
        /// <param name="instancename">Name of the object instance</param>
        /// <returns>Forund object, or null</returns>
        public GroundObject GetTempObj(string instancename)
        {
            GroundObject found = Entities[0].TemporaryObjects.Find((GroundObject ch) => { return ch.EntName == instancename; });
            if (found != null)
                return found;

            return null;
        }



        /// <summary>
        /// Create a new marker with the specified parameters, and add it to the map.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        public void AddMarker(string name, Loc pos, Dir8 dir)
        {
            Entities[0].Markers.Add(new GroundMarker(name, pos, dir));
        }

        /// <summary>
        /// Add the marker entity specified to the map
        /// </summary>
        /// <param name="mark"></param>
        public void AddMarker(GroundMarker mark)
        {
            Entities[0].Markers.Add(mark);
        }

        /// <summary>
        /// Remove the named marker if present, otherwise does nothing.
        /// </summary>
        /// <param name="name"></param>
        public void RemoveMarker(string name)
        {
            int index = Entities[0].Markers.FindIndex(marker => marker.EntName == name);
            if (index > -1)
                Entities[0].Markers.RemoveAt(index);
        }

        /// <summary>
        /// Remove the specific marker entity from the map. If not present does nothing.
        /// </summary>
        /// <param name="mark"></param>
        public void RemoveMarker(GroundMarker mark)
        {
            if (Entities[0].Markers.Contains(mark))
                Entities[0].Markers.Remove(mark);
        }


        /// <summary>
        /// Finds a named marker in the marker table for this map.
        /// </summary>
        /// <param name="name">Name of the marker</param>
        /// <returns>The found marker, or null if not found.</returns>
        public GroundMarker GetMarker(string name)
        {
            int index = Entities[0].Markers.FindIndex(marker => marker.EntName == name);
            if (index > -1)
                return Entities[0].Markers[index];
            return null;
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
        public bool InMapBounds(Loc loc)
        {
            if (EdgeView == Map.ScrollEdge.Wrap)
                return true;
            return RogueElements.Collision.InBounds(Width, Height, loc);
        }

        public bool InBounds(Rect rect, Loc loc)
        {
            if (EdgeView == Map.ScrollEdge.Wrap)
                return WrappedCollision.InBounds(Size, rect, loc);
            else
                return RogueElements.Collision.InBounds(rect, loc);
        }

        public IEnumerable<GroundChar> IterateCharacters()
        {
            if (ActiveChar != null)
                yield return ActiveChar;

            foreach (EntityLayer layer in Entities)
            {
                if (layer.Visible)
                {
                    foreach (GroundChar v in layer.IterateCharacters())
                        yield return v;
                }
            }
        }

        public void WrapEntities()
        {
            if (EdgeView == BaseMap.ScrollEdge.Wrap)
            {
                if (ActiveChar != null)
                    ActiveChar.SetMapLoc(WrapGroundLoc(ActiveChar.MapLoc));

                foreach (GroundEntity ent in IterateEntities())
                    ent.SetMapLoc(WrapGroundLoc(ent.MapLoc));
            }
        }

        public uint GetObstacle(int x, int y)
        {
            return obstacles[x][y].Tags;
        }
        public void SetObstacle(int x, int y, uint tags)
        {
            obstacles[x][y].Tags = tags;
        }

        public void ResizeJustified(int width, int height, Dir8 anchorDir)
        {
            foreach (MapLayer layer in Layers)
                layer.ResizeJustified(width, height, anchorDir);

            int divSize = GraphicsManager.TEX_SIZE;
            RogueElements.Grid.LocAction blockChangeOp = (Loc effectLoc) => { obstacles[effectLoc.X][effectLoc.Y].Bounds = new Rect(effectLoc.X * divSize, effectLoc.Y * divSize, divSize, divSize); };
            RogueElements.Grid.LocAction blocknewOp = (Loc effectLoc) => { obstacles[effectLoc.X][effectLoc.Y] = new GroundWall(effectLoc.X * divSize, effectLoc.Y * divSize, divSize, divSize); };

            Loc texDiff = RogueElements.Grid.ResizeJustified(ref obstacles,
                width * TexSize, height * TexSize, anchorDir.Reverse(), blockChangeOp, blocknewOp);

            foreach (EntityLayer layer in Entities)
            {
                foreach (GroundEntity obj in layer.IterateEntities())
                {
                    obj.SetMapLoc(RogueElements.Collision.ClampToBounds(width * TileSize - obj.Width, height * TileSize - obj.Height, obj.MapLoc + texDiff * divSize));
                    if (obj is GroundChar)
                    {
                        GroundChar character = (GroundChar)obj;
                        character.UpdateFrame();
                    }
                }
            }

            this.grid = new AABB.Grid(MathUtils.DivUp(GroundWidth, GraphicsManager.TileSize), MathUtils.DivUp(GroundHeight, GraphicsManager.TileSize), GraphicsManager.TileSize);
        }

        public void Retile(int texSize)
        {
            int newWidth = MathUtils.DivUp(Width * TexSize, texSize);
            int newHeight = MathUtils.DivUp(Height * TexSize, texSize);

            TexSize = texSize;

            foreach (MapLayer layer in Layers)
                layer.CreateNew(newWidth, newHeight);

            int divSize = GraphicsManager.TEX_SIZE;
            RogueElements.Grid.LocAction blockChangeOp = (Loc effectLoc) => { obstacles[effectLoc.X][effectLoc.Y].Bounds = new Rect(effectLoc.X * divSize, effectLoc.Y * divSize, divSize, divSize); };
            RogueElements.Grid.LocAction blocknewOp = (Loc effectLoc) => { obstacles[effectLoc.X][effectLoc.Y] = new GroundWall(effectLoc.X * divSize, effectLoc.Y * divSize, divSize, divSize); };

            RogueElements.Grid.ResizeJustified(ref obstacles,
                newWidth * TexSize, newHeight * TexSize, Dir8.DownRight, blockChangeOp, blocknewOp);

            this.grid = new AABB.Grid(MathUtils.DivUp(GroundWidth, GraphicsManager.TileSize), MathUtils.DivUp(GroundHeight, GraphicsManager.TileSize), GraphicsManager.TileSize);
        }

        public void AddLayer(string name)
        {
            MapLayer layer = new MapLayer(name);
            layer.CreateNew(Width, Height);
            Layers.Add(layer);
        }

        public GroundSpawner GetSpawner(string name)
        {
            GroundSpawner found = Entities[0].Spawners.Find((GroundSpawner ch) => { return ch.EntName == name; });
            if (found == null)
            {
                //Maybe warn or something??
            }
            return found;
        }

        /// <summary>
        /// Finds all objects and walls in the grid cells that the rectangle intersects.  This is not a true collision test!
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public IEnumerable<IObstacle> FindPossible(int x, int y, int w, int h)
        {
            foreach (IObstacle obstacle in this.grid.QueryBoxes(x, y, w, h, EdgeView == BaseMap.ScrollEdge.Wrap))
                yield return obstacle;
            foreach (IObstacle obstacle in findWalls(x, y, w, h))
                yield return obstacle;
        }

        private IEnumerable<IObstacle> findWalls(int x, int y, int w, int h)
        {
            int divSize = GraphicsManager.TEX_SIZE;
            var minX = MathUtils.DivDown(x, divSize);
            var minY = MathUtils.DivDown(y, divSize);
            var maxX = MathUtils.DivUp(x + w, divSize);
            var maxY = MathUtils.DivUp(y + h, divSize);
            Loc texSize = new Loc(TexWidth, TexHeight);

            for (int ii = minX; ii < maxX; ii++)
            {
                for (int jj = minY; jj < maxY; jj++)
                {
                    Loc testLoc = new Loc(ii, jj);
                    if (EdgeView == BaseMap.ScrollEdge.Wrap)
                        testLoc = Loc.Wrap(testLoc, texSize);
                    else if (!RogueElements.Collision.InBounds(TexWidth, TexHeight, testLoc))
                        continue;

                    if (obstacles[testLoc.X][testLoc.Y].Tags > 0)
                        yield return obstacles[testLoc.X][testLoc.Y];
                }
            }
        }

        /// <summary>
        /// Finds all objects and walls in the grid cells that the rectangle intersects.  This is not a true collision test!
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public IEnumerable<IObstacle> FindPossible(Rect area)
        {
            return this.FindPossible(area.X, area.Y, area.Width, area.Height);
        }

        /// <summary>
        /// Finds the objects that intersect the area exactly.
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public IEnumerable<IObstacle> Find(Rect area)
        {
            return this.FindPossible(area.X, area.Y, area.Width, area.Height).Where((box) => area.Intersects(box.Bounds));
        }

        public void Update(IBox box, Rect from)
        {
            this.grid.Update(box, from, EdgeView == BaseMap.ScrollEdge.Wrap);
        }

        public IHit Hit(Loc point, IEnumerable<IObstacle> ignoring = null)
        {
            IEnumerable<IObstacle> boxes = this.FindPossible(point.X, point.Y, 0, 0);

            if (ignoring != null)
            {
                boxes = boxes.Except(ignoring);
            }

            foreach (var other in boxes)
            {
                //TODO: detect collision in wrap-arounds
                var hit = AABB.Hit.Resolve(point, other);

                if (hit != null)
                {
                    return hit;
                }
            }

            return null;
        }

        public IHit Hit(Loc origin, Loc destination, IEnumerable<IObstacle> ignoring = null)
        {
            var min = Loc.Min(origin, destination);
            var max = Loc.Max(origin, destination);

            var wrap = new Rect(min, max - min);
            IEnumerable<IObstacle> boxes = this.FindPossible(wrap.X, wrap.Y, wrap.Width, wrap.Height);

            if (ignoring != null)
            {
                boxes = boxes.Except(ignoring);
            }

            IHit nearest = null;

            foreach (IObstacle other in boxes)
            {
                //TODO: detect collision in wrap-arounds
                var hit = AABB.Hit.Resolve(origin, destination, other);

                if (hit != null && (nearest == null || hit.IsNearest(nearest, origin)))
                    nearest = hit;
            }

            return nearest;
        }

        public IHit Hit(Rect origin, Rect destination, IEnumerable<IObstacle> ignoring = null)
        {
            Rect wrap = new Rect(origin, destination);
            IEnumerable<IObstacle> boxes = this.FindPossible(wrap.X, wrap.Y, wrap.Width, wrap.Height);

            if (ignoring != null)
                boxes = boxes.Except(ignoring);

            IHit nearest = null;

            foreach (IObstacle other in boxes)
            {
                //TODO: detect collision in wrap-arounds
                IHit hit = AABB.Hit.Resolve(origin, destination, other);

                if (hit != null && (nearest == null || hit.IsNearest(nearest, origin.Start)))
                    nearest = hit;
            }

            return nearest;
        }

        public IMovement Simulate(IBox box, int x, int y, Func<ICollision, ICollisionResponse> filter)
        {
            Rect origin = box.Bounds;
            Rect destination = new Rect(x, y, box.Width, box.Height);

            List<IHit> hits = new List<IHit>();

            Movement result = new Movement()
            {
                Origin = origin,
                Goal = destination,
                Destination = this.Simulate(hits, new List<IObstacle>() { box }, box, origin, destination, filter),
                Hits = hits,
            };

            return result;
        }

        private Rect Simulate(List<IHit> hits, List<IObstacle> ignoring, IBox box, Rect origin, Rect destination, Func<ICollision, ICollisionResponse> filter)
        {
            IHit nearest = this.Hit(origin, destination, ignoring);

            if (nearest != null)
            {
                hits.Add(nearest);

                Rect impact = new Rect(nearest.Position, origin.Size);
                AABB.Collision collision = new AABB.Collision() { Box = box, Hit = nearest, Goal = destination, Origin = origin };
                ICollisionResponse response = filter(collision);


                ignoring.Add(nearest.Box);
                if (response != null && destination != response.Destination)
                    return this.Simulate(hits, ignoring, box, impact, response.Destination, filter);//hit something; estimate based on the new trajectory
                else
                    return this.Simulate(hits, ignoring, box, origin, destination, filter);//didn't hit something; estimate based on the current trajectory again
            }

            return destination;
        }


        public void DrawDefaultTile(SpriteBatch spriteBatch, Loc drawPos, Loc mapPos)
        {
            INoise noise = new ReNoise(rand.FirstSeed);
            BlankBG.DrawBlank(spriteBatch, drawPos, noise.Get2DUInt64((ulong)mapPos.X, (ulong)mapPos.Y));
        }

        public void DrawLoc(SpriteBatch spriteBatch, Loc drawPos, Loc loc, bool front)
        {
            loc = WrapLoc(loc);
            foreach (MapLayer layer in Layers)
            {
                if ((layer.Layer == DrawLayer.Top) == front && layer.Visible)
                    layer.Tiles[loc.X][loc.Y].Draw(spriteBatch, drawPos);
            }
        }

        public void DrawDebug(int x, int y, int w, int h,
                            Action<int, int, int, int, float> drawCell,
                            Action<IObstacle> drawBox,
                            Action<string, int, int, float> drawString)
        {
            // Drawing boxes
            IEnumerable<IObstacle> boxes = this.grid.QueryBoxes(x, y, w, h, EdgeView == BaseMap.ScrollEdge.Wrap);
            foreach (IObstacle box in boxes)
                drawBox(box);

            IEnumerable<IObstacle> walls = findWalls(x,y,w,h);
            foreach (IObstacle box in walls)
                drawBox(box);

            // Drawing cells
            IEnumerable<AABB.Grid.Cell> cells = this.grid.QueryCells(x, y, w, h, EdgeView == BaseMap.ScrollEdge.Wrap);
            foreach (AABB.Grid.Cell cell in cells)
            {
                int count = cell.Count();
                float alpha = count > 0 ? 1f : 0.4f;
                drawCell(cell.Bounds.X, cell.Bounds.Y, cell.Bounds.Width, cell.Bounds.Height, alpha);
                drawString(count.ToString(), cell.Bounds.Center.X, cell.Bounds.Center.Y, alpha);
            }

        }

        /// <summary>
        /// Returns the position and location of an entry point for the current map by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public LocRay8 GetEntryPoint(int index)
        {
            if (index >= Entities[0].Markers.Count)
                return new LocRay8(Loc.Zero, Dir8.Down);
            GroundMarker mark = Entities[0].Markers[index];
            return new LocRay8(mark.Position, mark.Direction);
        }


        /// <summary>
        /// Returns the position and location of an entry point for the current map by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public LocRay8 GetEntryPoint(string name)
        {
            GroundMarker mark = GetMarker(name);
            if (mark == null)
                throw new KeyNotFoundException(String.Format("GroundMap.GetMarkerPosition({0}): Couldn't find the specified Marker!", name));

            return new LocRay8(mark.Position, mark.Direction);
        }

        /// <summary>
        /// Returns the index of an entry point for the current map by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public int GetEntryPointIdx(string name)
        {
            int idx = Entities[0].Markers.FindIndex(marker => marker.EntName == name);
            if (idx > -1)
                return idx;

            throw new KeyNotFoundException(String.Format("GroundMap.GetEntryPointIdx({0}): Couldn't find the specified Marker!", name));
        }

        /// <summary>
        /// Change the position of the specified named marker
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        public void SetMarkerPosition(string name, Loc pos)
        {
            GroundMarker mark = GetMarker(name);
            if (mark == null)
                throw new KeyNotFoundException(String.Format("GroundMap.SetMarkerPosition({0},{1}): Couldn't find the specified Marker!", name, pos));
            mark.Position = pos;
        }

        /// <summary>
        /// Convenience method for locating an entity on that map at the
        /// position specified.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public List<GroundEntity> FindEntitiesAtPosition(Loc pos)
        {
            List<GroundEntity> found = new List<GroundEntity>();
            foreach (GroundEntity c in IterateEntities())
            {
                if (RogueElements.Collision.InBounds(c.Bounds, pos))
                    found.Add(c);
            }

            return found;
        }


        public bool EntityNameExists(string name)
        {
            //TODO: account for equivalent values such as with leading zeroes
            foreach (GroundEntity c in IterateEntities())
            {
                if (String.Compare(c.EntName, name, true) == 0)
                    return true;
            }
            return false;
        }

        public string FindNonConflictingName(string inputStr)
        {
            return Text.GetNonConflictingName(inputStr, EntityNameExists);
        }

        public GroundEntity FindEntity(string name)
        {
            if (name == "PLAYER")
                return this.ActiveChar;

            foreach (GroundEntity entity in IterateEntities())
            {
                if (entity.EntName == name)
                    return entity;
            }

            return null;
        }

        /// <summary>
        /// Allow iterating through all entities on the map,
        /// characters, objects, markers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GroundEntity> IterateEntities()
        {
            foreach (EntityLayer layer in Entities)
            {
                if (layer.Visible)
                {
                    foreach (GroundEntity v in layer.IterateEntities())
                        yield return v;
                }
            }
        }

        public IEnumerable<GroundAnim> IterateDecorations()
        {
            foreach (AnimLayer layer in Decorations)
            {
                if (layer.Visible)
                {
                    foreach (GroundAnim v in layer.Anims)
                        yield return v;
                }
            }
        }


        public void LoadScriptEvents()
        {
            scriptEvents = new Dictionary<LuaEngine.EMapCallbacks, ScriptEvent>();
            if (!String.IsNullOrEmpty(AssetName))
            {
                for (int ii = 0; ii < (int)LuaEngine.EMapCallbacks.Invalid; ii++)
                {
                    LuaEngine.EMapCallbacks ev = (LuaEngine.EMapCallbacks)ii;
                    string callback = LuaEngine.MakeMapScriptCallbackName(LuaEngine.MapCurrentScriptSym, ev);
                    if (!LuaEngine.Instance.DoesFunctionExists(callback))
                        continue;
                    DiagManager.Instance.LogInfo(String.Format("GroundMap.LoadScriptEvents(): Added event {0} to map {1}!", ev.ToString(), AssetName));
                    scriptEvents[ev] = new ScriptEvent(callback);
                }
            }
        }

        public void LuaEngineReload()
        {
            LoadScriptEvents();

            foreach (GroundEntity ent in IterateEntities())
            {
                if (ent != null)
                    ent.LuaEngineReload();
            }
            LoadLua();
        }

        public void SaveLua()
        {
            foreach (EntityLayer layer in Entities)
            {
                if (layer.Visible)
                {
                    foreach (GroundChar v in layer.IterateCharacters())
                        v.Data.SaveLua();
                }
            }
        }

        public void LoadLua()
        {
            foreach (EntityLayer layer in Entities)
            {
                if (layer.Visible)
                {
                    foreach (GroundChar v in layer.IterateCharacters())
                        v.Data.LoadLua();
                }
            }
        }
        public List<LuaEngine.EMapCallbacks> ActiveScriptEvent()
        {
            List<LuaEngine.EMapCallbacks> list = new List<LuaEngine.EMapCallbacks>();

            foreach (var e in scriptEvents)
                list.Add(e.Key);

            return list;
        }


        /// <summary>
        /// Returns true if there exists and event with the same name as the string eventname.
        /// The script event doesn't need to be loaded.
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        public bool HasScriptEvent(LuaEngine.EMapCallbacks ev)
        {
            string callback = LuaEngine.MakeMapScriptCallbackName(LuaEngine.MapCurrentScriptSym, ev);
            return LuaEngine.Instance.DoesFunctionExists(callback);
        }

        public void ReloadEntLayer(int layer)
        {

            //reconnect characters and objects references
            foreach (GroundChar player in Entities[layer].MapChars)
            {
                if (player != null)
                {
                    player.OnDeserializeMap(this);
                    signCharToMap(player);
                }
            }
            foreach (GroundObject groundObj in Entities[layer].GroundObjects)
            {
                groundObj.OnDeserializeMap(this);
                grid.Add(groundObj, EdgeView == BaseMap.ScrollEdge.Wrap);
            }
        }

        public void PreSaveEntLayer(int layer)
        {
            //reconnect characters and objects references
            foreach (GroundChar player in Entities[layer].MapChars)
            {
                if (player != null)
                    player.OnSerializeMap(this);
            }
            foreach (GroundObject groundObj in Entities[layer].GroundObjects)
                groundObj.OnSerializeMap(this);
        }


        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            PreSaveEntLayer(0);
        }


        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //recompute the grid
            grid = new AABB.Grid(MathUtils.DivUp(GroundWidth, GraphicsManager.TileSize), MathUtils.DivUp(GroundHeight, GraphicsManager.TileSize), GraphicsManager.TileSize);

            if (ActiveChar != null)
            {
                ActiveChar.OnDeserializeMap(this);
                signCharToMap(ActiveChar);
            }

            ReloadEntLayer(0);

            scriptEvents = new Dictionary<LuaEngine.EMapCallbacks, ScriptEvent>();
        }
    }
}

