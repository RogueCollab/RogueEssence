using System;
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

namespace RogueEssence.Ground
{
    [Serializable]
    public class GroundMap : IWorld, IEntryData
    {
        [NonSerialized]
        private AABB.Grid grid;
        private GroundWall[][] obstacles;


        public Dictionary<int, MapStatus> Status;
        private Dictionary<LuaEngine.EMapCallbacks, ScriptEvent> ScriptEvents; //psy's notes: In order to get rid of duplicates and help make things more straightforward I moved script events to a dictionary

        public BGAnimData BGAnim;
        public Loc BGMovement;

        public List<MapLayer> Layers;

        /// <summary>
        /// Size in tex units (8x8 tiles)
        /// </summary>
        public int TexSize { get; private set; }
        public int TileSize { get { return TexSize * GraphicsManager.TEX_SIZE; } }
        public int Width { get { return Layers[0].Tiles.Length; } }
        public int Height { get { return Layers[0].Tiles[0].Length; } }

        public int TexWidth { get { return Width * TexSize; } }
        public int TexHeight { get { return Height * TexSize; } }

        public int GroundWidth { get { return Width * TileSize; } }
        public int GroundHeight { get { return Height * TileSize; } }

        /// <summary>
        /// the internal name of the map, no spaces or special characters, never localized.
        /// Used to refer to map data and script data for this map!
        /// </summary>
        public string AssetName { get; set; }

        public LocalText Name { get; set; }
        public string GetSingleLineName() { return Name.ToLocal().Replace('\n', ' '); }

        public bool Released { get; set; }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public string Music;
        public Map.ScrollEdge EdgeView;


        public Loc? ViewCenter;
        public Loc ViewOffset;

        public GroundChar ActiveChar;

        public List<AnimLayer> Decorations;
        //TODO: fully implement multilayered entities
        public List<EntityLayer> Entities;

        public GroundMap()
        {
            ScriptEvents = new Dictionary<LuaEngine.EMapCallbacks, ScriptEvent>();

            Entities = new List<EntityLayer>();

            Status = new Dictionary<int, MapStatus>();

            BGAnim = new BGAnimData();

            Name = new LocalText();
            Comment = "";
            Music = "";

            Layers = new List<MapLayer>();

            Decorations = new List<AnimLayer>();
            AssetName = "";

        }

        /// <summary>
        /// Call this so the map unregisters its events and delegates.
        ///
        /// </summary>
        public void DoCleanup()
        {
            DiagManager.Instance.LogInfo(String.Format("GroundMap.~GroundMap(): Finalizing {0}..", AssetName));

            foreach (var e in ScriptEvents)
                e.Value.DoCleanup();
            ScriptEvents.Clear();

            foreach (GroundEntity ent in IterateEntities())
            {
                if( ent != null )
                    ent.DoCleanup();
            }

            LuaEngine.Instance.CleanMapScript(AssetName);
        }

        /// <summary>
        /// Run this map's specified script event
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> RunScriptEvent(LuaEngine.EMapCallbacks ev)
        {
            if (ScriptEvents.ContainsKey(ev))
                yield return CoroutineManager.Instance.StartCoroutine(ScriptEvents[ev].Apply(this));
            else
                yield break;
        }

        /// <summary>
        /// Called by GroundScene as the map is being initialized.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> OnInit()
        {
            DiagManager.Instance.LogInfo("GroundMap.OnInit(): Initializing the map..");
            Script.LuaEngine.Instance.RunMapScript(AssetName);

            //Reload the map events
            foreach (var ev in ScriptEvents)
                ev.Value.ReloadEvent();

            foreach (GroundEntity entity in IterateEntities())
                entity.OnMapInit();

            //Do script event
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EMapCallbacks.Init));

            //Notify script engine
            LuaEngine.Instance.OnGroundMapInit(AssetName, this);
        }


        /// <summary>
        /// Called when resuming a savegame on this map.
        /// Handles setting everything back in place.
        /// </summary>
        /// <returns></returns>
        public void OnResume()
        {
            //Load the map's script
            //LuaEngine.Instance.RunMapScript(AssetName);
            var iter = OnInit();
            while (iter.MoveNext());
        }

        /// <summary>
        /// Called by the GroundScene when the map is in "Begin" stage.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> OnBegin()
        {
            //Ensure the AI is enabled
            GroundAI.GlobalAIEnabled = true;

            //Do script event
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EMapCallbacks.Enter));

            //Notify script engine
            LuaEngine.Instance.OnGroundMapEnter(AssetName, this);
            yield break;
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

            this.grid = new AABB.Grid(width, height, GraphicsManager.TileSize);
        }


        
        public void SetPlayerChar(GroundChar mapChar)
        {
            //GroundChar groundChar = new GroundChar(ActiveTeam.Leader, entry.Loc, (entry.Dir != Dir8.None) ? entry.Dir : ActiveTeam.Leader.CharDir, "PLAYER");
            if (ActiveChar != null)
                grid.Remove(ActiveChar);
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
            grid.Remove(mapChar);
        }
        private void signCharToMap(GroundChar groundChar)
        {
            groundChar.Map = this;
            grid.Add(groundChar);
        }
        public void AddObject(GroundObject groundObj)
        {
            Entities[0].GroundObjects.Add(groundObj);
            grid.Add(groundObj);
        }

        /// <summary>
        /// Add a character entity that shouldn't be serialized!
        /// </summary>
        /// <param name="ch"></param>
        public void AddTempChar(GroundChar ch)
        {
            ch.Map = this;
            Entities[0].TemporaryChars.Add(ch);
            grid.Add(ch);
        }

        /// <summary>
        /// Removed a temp character entity from the list of temp characters
        /// </summary>
        /// <param name="ch"></param>
        public void RemoveTempChar(GroundChar ch)
        {
            Entities[0].TemporaryChars.Remove(ch);
            grid.Remove(ch);
        }

        public void RemoveObject(GroundObject groundObj)
        {
            Entities[0].GroundObjects.Remove(groundObj);
            grid.Remove(groundObj);
        }

        /// <summary>
        /// Lookup an object instance by name on the map.
        /// Returns the object if found, or null.
        /// </summary>
        /// <param name="instancename">Name of the object instance</param>
        /// <returns>Forund object, or null</returns>
        public GroundObject GetObj(string instancename)
        {
            GroundObject found = null;
            if ((found = Entities[0].GroundObjects.Find((GroundObject ch) => { return ch.EntName == instancename; })) == null)
            {
                //Maybe warn or something??
            }
            return found;
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

            foreach (GroundChar character in ZoneManager.Instance.CurrentGround.IterateCharacters())
            {
                Loc newLoc = character.MapLoc + texDiff * divSize;
                if (newLoc.X < 0)
                    newLoc.X = 0;
                else if (newLoc.X >= width)
                    newLoc.X = width - 1;
                if (newLoc.Y < 0)
                    newLoc.Y = 0;
                else if (newLoc.Y >= height)
                    newLoc.Y = height - 1;

                character.SetMapLoc(newLoc);
                character.UpdateFrame();
            }

            this.grid = new AABB.Grid(width, height, GraphicsManager.TileSize);
        }

        public void Retile(int texSize)
        {
            int newWidth = (Width * TexSize - 1) / texSize + 1;
            int newHeight = (Height * TexSize - 1) / texSize + 1;

            TexSize = texSize;

            foreach (MapLayer layer in Layers)
                layer.CreateNew(newWidth, newHeight);

            int divSize = GraphicsManager.TEX_SIZE;
            RogueElements.Grid.LocAction blockChangeOp = (Loc effectLoc) => { obstacles[effectLoc.X][effectLoc.Y].Bounds = new Rect(effectLoc.X * divSize, effectLoc.Y * divSize, divSize, divSize); };
            RogueElements.Grid.LocAction blocknewOp = (Loc effectLoc) => { obstacles[effectLoc.X][effectLoc.Y] = new GroundWall(effectLoc.X * divSize, effectLoc.Y * divSize, divSize, divSize); };

            RogueElements.Grid.ResizeJustified(ref obstacles,
                newWidth * TexSize, newHeight * TexSize, Dir8.DownRight, blockChangeOp, blocknewOp);

            this.grid = new AABB.Grid(newWidth, newHeight, GraphicsManager.TileSize);
        }

        public void AddLayer(string name)
        {
            MapLayer layer = new MapLayer("");
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
            x = Math.Max(0, Math.Min(x, this.GroundWidth - w));
            y = Math.Max(0, Math.Min(y, this.GroundHeight - h));
            w = Math.Max(0, Math.Min(w, this.GroundWidth - x));
            h = Math.Max(0, Math.Min(h, this.GroundHeight - y));


            foreach (IObstacle obstacle in this.grid.QueryBoxes(x, y, w, h))
                yield return obstacle;
            foreach (IObstacle obstacle in findWalls(x, y, w, h))
                yield return obstacle;

        }

        private IEnumerable<IObstacle> findWalls(int x, int y, int w, int h)
        {
            x = Math.Max(0, Math.Min(x, this.GroundWidth - w));
            y = Math.Max(0, Math.Min(y, this.GroundHeight - h));
            w = Math.Max(0, Math.Min(w, this.GroundWidth - x));
            h = Math.Max(0, Math.Min(h, this.GroundHeight - y));

            int divSize = GraphicsManager.TEX_SIZE;
            var minX = (x / divSize);
            var minY = (y / divSize);
            var maxX = ((x + w - 1) / divSize);
            var maxY = ((y + h - 1) / divSize);

            for (int ii = minX; ii <= maxX; ii++)
            {
                for (int jj = minY; jj <= maxY; jj++)
                {
                    if (obstacles[ii][jj].Tags > 0)
                        yield return obstacles[ii][jj];
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
            this.grid.Update(box, from);
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

        public void DrawLoc(SpriteBatch spriteBatch, Loc drawPos, Loc loc, bool front)
        {
            foreach (MapLayer layer in Layers)
            {
                if (layer.Front == front && layer.Visible)
                    layer.Tiles[loc.X][loc.Y].Draw(spriteBatch, drawPos);
            }
        }

        public void DrawBG(SpriteBatch spriteBatch)
        {
            if (BGAnim.AnimIndex != "")
            {
                DirSheet sheet = GraphicsManager.GetBackground(BGAnim.AnimIndex);

                Loc diff = BGMovement * (int)FrameTick.TickToFrames(GraphicsManager.TotalFrameTick) / 60;
                if (sheet.Width == 1 && sheet.Height == 1)
                    sheet.DrawTile(spriteBatch, new Rectangle(0, 0, GraphicsManager.ScreenWidth, GraphicsManager.ScreenHeight), 0, 0, Color.White);
                else
                {
                    for (int x = diff.X % sheet.TileWidth - sheet.TileWidth; x < GraphicsManager.ScreenWidth; x += sheet.TileWidth)
                    {
                        for (int y = diff.Y % sheet.TileHeight - sheet.TileHeight; y < GraphicsManager.ScreenHeight; y += sheet.TileHeight)
                            sheet.DrawDir(spriteBatch, new Vector2(x, y), BGAnim.GetCurrentFrame(GraphicsManager.TotalFrameTick, sheet.TotalFrames), BGAnim.AnimDir, Color.White);
                    }
                }
            }
        }

        public void DrawDebug(int x, int y, int w, int h,
                            Action<int, int, int, int, float> drawCell,
                            Action<IObstacle> drawBox,
                            Action<string, int, int, float> drawString)
        {
            // Drawing boxes
            IEnumerable<IObstacle> boxes = this.grid.QueryBoxes(x, y, w, h);
            foreach (IObstacle box in boxes)
                drawBox(box);

            IEnumerable<IObstacle> walls = findWalls(x,y,w,h);
            foreach (IObstacle box in walls)
                drawBox(box);

            // Drawing cells
            IEnumerable<AABB.Grid.Cell> cells = this.grid.QueryCells(x, y, w, h);
            foreach (AABB.Grid.Cell cell in cells)
            {
                int count = cell.Count();
                float alpha = count > 0 ? 1f : 0.4f;
                drawCell(cell.Bounds.X, cell.Bounds.Y, cell.Bounds.Width, cell.Bounds.Height, alpha);
                drawString(count.ToString(), cell.Bounds.Center.X, cell.Bounds.Center.Y, alpha);
            }

        }


        /// <summary>
        /// Finds a named marker in the marker table for this map.
        /// </summary>
        /// <param name="name">Name of the marker</param>
        /// <returns>The found marker, or null if not found.</returns>
        private GroundMarker FindMarker(string name)
        {
            int index = Entities[0].Markers.FindIndex(marker => marker.EntName == name);
            if (index > -1)
                return Entities[0].Markers[index];
            return null;
        }
        /// <summary>
        /// Returns the position and location of an entry point for the current map by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public LocRay8 GetEntryPoint(int index)
        {
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
            GroundMarker mark = FindMarker(name);
            if (mark == null)
                throw new KeyNotFoundException(String.Format("GroundMap.GetMarkerPosition({0}): Couldn't find the specified Marker!", name));

            return new LocRay8(mark.Position, mark.Direction);
        }

        /// <summary>
        /// Change the position of the specified named marker
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        public void SetMarkerPosition(string name, Loc pos)
        {
            GroundMarker mark = FindMarker(name);
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



        public string FindNonConflictingName(string inputStr)
        {
            //TODO: account for equivalent values such as with leading zeroes

            string prefix = inputStr;
            int origIndex = -1;
            int lastUnderscore = inputStr.LastIndexOf('_');
            if (lastUnderscore > -1)
            {
                string substr = inputStr.Substring(lastUnderscore + 1);
                if (int.TryParse(substr, out origIndex))
                    prefix = inputStr.Substring(0, lastUnderscore);
            }

            Dictionary<int, GroundEntity> found = new Dictionary<int, GroundEntity>();
            foreach (GroundEntity c in IterateEntities())
            {
                if (c.EntName == prefix)
                    found[-1] = c;
                else if (c.EntName.StartsWith(prefix + "_"))
                {
                    int val;
                    if (Int32.TryParse(c.EntName.Substring(prefix.Length+1), out val))
                        found[val] = c;
                }
            }

            if (!found.ContainsKey(origIndex))
                return inputStr;

            int copy_index = 1;
            while (copy_index < Int32.MaxValue)
            {
                if (!found.ContainsKey(copy_index))
                    break;

                copy_index++;
            }

            return prefix + "_" + copy_index.ToString();
        }

        public GroundEntity FindEntity(string name)
        {
            foreach(GroundEntity entity in IterateEntities())
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

        public void AddMapScriptEvent(LuaEngine.EMapCallbacks ev)
        {
            DiagManager.Instance.LogInfo(String.Format("GroundMap.AddMapScriptEvent(): Added event {0} to map {1}!", ev.ToString(), AssetName) );
            ScriptEvents[ev] = new ScriptEvent(LuaEngine.MakeMapScriptCallbackName(AssetName,ev));
        }

        public void LuaEngineReload()
        {
            foreach (ScriptEvent scriptEvent in ScriptEvents.Values)
                scriptEvent.LuaEngineReload();

            foreach (GroundEntity ent in IterateEntities())
            {
                if (ent != null)
                    ent.LuaEngineReload();
            }
        }

        public void RemoveMapScriptEvent(LuaEngine.EMapCallbacks ev)
        {
            DiagManager.Instance.LogInfo(String.Format("GroundMap.RemoveMapScriptEvent(): Removed event {0} from map {1}!", ev.ToString(), AssetName));
            if (ScriptEvents.ContainsKey(ev))
                ScriptEvents.Remove(ev);
        }

        public List<LuaEngine.EMapCallbacks> ActiveScriptEvent()
        {
            List<LuaEngine.EMapCallbacks> list = new List<LuaEngine.EMapCallbacks>();

            foreach( var e in ScriptEvents )
                list.Add(e.Key);

            return list;
        }

        
        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            
            
        }


        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //DiagManager.Instance.LogInfo(String.Format("GroundMap.OnDeserializedMethod(): Map {0} deserialized!", AssetName));

            //recompute the grid
            grid = new AABB.Grid(Width, Height, GraphicsManager.TileSize);

            //Because we clear those on save, we'll need to assign a new array here

            //reconnect characters and objects references
            foreach (GroundChar player in Entities[0].MapChars)
            {
                if (player != null)
                {
                    player.OnDeserializeMap(this);
                    signCharToMap(player);
                }
            }
            foreach (GroundObject groundObj in Entities[0].GroundObjects)
            {
                groundObj.OnDeserializeMap(this);
                grid.Add(groundObj);
            }

        }
    }
}

