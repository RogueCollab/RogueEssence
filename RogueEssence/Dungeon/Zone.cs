using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.LevelGen;
using RogueEssence.Ground;
using RogueEssence.Data;
using RogueEssence.Script;

namespace RogueEssence.Dungeon
{
    //A zone acts as a single dungeon, with both mapped and generated floors, ground or dungeon mode.
    [Serializable]
    public class Zone
    {
        public LocalText Name;

        public bool NoEXP;
        public int Level;
        public bool LevelCap;
        public bool TeamRestrict;
        public int TeamSize;
        public bool MoneyRestrict;
        public int BagRestrict;
        public int BagSize;
        public bool Persistent;

        //we want to be able to load and save maps in both ground and dungeon mode
        //we want to be able to create new maps and save them to a file
        //we want ground and dungeon mode to both be accessible via a structloc, whatever data that structloc may contain

        public List<ZoneSegmentBase> Segments;

        public List<string> GroundMaps;

        private IRandom rand;

        protected Dictionary<int, ZoneGenContext> structureContexts;
        protected Dictionary<SegLoc, Map> maps;

        public int MapCount { get { return maps.Count; } }

        public string ID { get; private set; }
        public SegLoc CurrentMapID { get; private set; }
        public Map CurrentMap { get; private set; }

        //include a current groundmap, with moveto methods included
        public GroundMap CurrentGround { get; private set; }

        public List<MapStatus> CarryOver;

        public int MapsLoaded;

        /// <summary>
        /// For containing entire dungeon-related events. (Since we can't handle some of those things inside the dungeon floors themselves)
        /// </summary>
        private Dictionary<LuaEngine.EZoneCallbacks, ScriptEvent> scriptEvents;

        public Zone(ulong seed, string zoneIndex)
        {
            DiagManager.Instance.LogInfo("Zone Seed: " + seed);
            rand = new ReRandom(seed);

            this.ID = zoneIndex;
            Name = new LocalText();

            CurrentMapID = SegLoc.Invalid;

            Level = -1;
            TeamSize = -1;
            BagRestrict = -1;
            BagSize = -1;

            structureContexts = new Dictionary<int, ZoneGenContext>();
            maps = new Dictionary<SegLoc, Map>();
            Segments = new List<ZoneSegmentBase>();

            CarryOver = new List<MapStatus>();

            scriptEvents = new Dictionary<LuaEngine.EZoneCallbacks, ScriptEvent>();
        }

        public string GetDisplayName()
        {
            return String.Format("[color=#FFC663]{0}[color]", Name.ToLocal());
        }

        public void LoadScriptEvents()
        {
            scriptEvents = new Dictionary<LuaEngine.EZoneCallbacks, ScriptEvent>();
            for (int ii = 0; ii < (int)LuaEngine.EZoneCallbacks.Invalid; ii++)
            {
                LuaEngine.EZoneCallbacks ev = (LuaEngine.EZoneCallbacks)ii;
                string assetName = this.ID;
                string callback = LuaEngine.MakeZoneScriptCallbackName(assetName, ev);
                if (!LuaEngine.Instance.DoesFunctionExists(callback))
                    continue;
                DiagManager.Instance.LogInfo(String.Format("Zone.LoadScriptEvents(): Added event {0} to zone {1}!", ev.ToString(), assetName));
                scriptEvents[ev] = new ScriptEvent(callback);
            }
        }

        public void LuaEngineReload()
        {
            LoadScriptEvents();
            if (CurrentMap != null)
                CurrentMap.LuaEngineReload();
            if (CurrentGround != null)
                CurrentGround.LuaEngineReload();
        }
        public void SaveLua()
        {
            if (CurrentMap != null)
                CurrentMap.SaveLua();
            if (CurrentGround != null)
                CurrentGround.SaveLua();
        }
        public void LoadLua()
        {
            if (CurrentMap != null)
                CurrentMap.LoadLua();
            if (CurrentGround != null)
                CurrentGround.LoadLua();
        }

        private void exitMap()
        {
            if (CurrentMap != null && CurrentMapID.IsValid())//only clean up maps that are valid (aka, not from editor mode)
                CurrentMap.DoCleanup();
            CurrentMap = null;
            if (CurrentGround != null && CurrentMapID.IsValid())//only clean up maps that are valid (aka, not from editor mode)
                CurrentGround.DoCleanup();
            CurrentGround = null;
        }

        public void SetCurrentMap(SegLoc map)
        {
            exitMap();

            if (map.IsValid())
            {
                if (map.Segment > -1)
                    CurrentMap = GetMap(map);
                else
                    CurrentGround = GetGround(map);
            }
            CurrentMapID = map;
        }

        /// <summary>
        /// Finds the mapname in this zone's map list, and loads it.
        /// </summary>
        /// <param name="mapname"></param>
        public void SetCurrentGround(string mapname)
        {
            exitMap();

            int index = GroundMaps.FindIndex((str) => (str == mapname));

            if (index > -1)
                CurrentGround = GetGround(new SegLoc(-1, index));
            else
                throw new Exception(String.Format("Cannot find ground map of name {0} in {1}.", mapname, Name.DefaultText));

            CurrentMapID = new SegLoc(-1, index);
        }


        /// <summary>
        /// Creates a new map of the specified name into the current zone (temporarily) for dev usage.
        /// </summary>
        public void DevNewMap()
        {
            exitMap();

            CurrentMap = new Map();
            CurrentMap.CreateNew(10, 10);

            CurrentMapID = new SegLoc(0, 0);
        }


        /// <summary>
        /// Loads a new map of the specified name into the current zone (temporarily) for dev usage.
        /// </summary>
        /// <param name="mapname"></param>
        public void DevLoadMap(string mapname)
        {
            exitMap();

            CurrentMap = DataManager.Instance.GetMap(mapname);
            CurrentMapID = new SegLoc(0, 0);
        }


        /// <summary>
        /// Loads a new ground map of the specified name into the current zone (temporarily) for dev usage.
        /// </summary>
        /// <param name="mapname"></param>
        public void DevLoadGround(string mapname)
        {
            exitMap();

            CurrentGround = DataManager.Instance.GetGround(mapname);
            CurrentMapID = new SegLoc(-1, 0);
        }

        /// <summary>
        /// Creates a new ground map of the specified name into the current zone (temporarily) for dev usage.
        /// </summary>
        public void DevNewGround()
        {
            exitMap();

            CurrentGround = new GroundMap();
            CurrentGround.CreateNew(16, 16, Content.GraphicsManager.DungeonTexSize);
            CurrentMapID = new SegLoc(-1, 0);
        }


        public Map GetMap(SegLoc id)
        {
            if (!maps.ContainsKey(id))
            {
                DiagManager.Instance.LogInfo("Zone Seed: " + rand.FirstSeed);
                ReNoise totalNoise = new ReNoise(rand.FirstSeed);
                ulong[] doubleSeed = totalNoise.GetTwoUInt64((ulong)id.Segment);
                ulong structSeed = doubleSeed[0];
                INoise structNoise = new ReNoise(structSeed);
                INoise idNoise = new ReNoise(doubleSeed[1]);

                //load the struct context if it isn't present yet
                if (!structureContexts.ContainsKey(id.Segment))
                {
                    ZoneGenContext newContext = new ZoneGenContext();
                    newContext.CurrentZone = ID;
                    newContext.CurrentSegment = id.Segment;
                    foreach (ZoneStep zoneStep in Segments[id.Segment].ZoneSteps)
                    {
                        //TODO: find a better way to feed ZoneSteps into full zone segments.
                        //Is there a way for them to be stateless?
                        //Additionally, the ZoneSteps themselves sometimes hold IGenSteps that are copied over to the layouts.
                        //Is that really OK? (I would guess yes because there is no chance by design for them to be mutated when generating...)
                        ZoneStep newStep = zoneStep.Instantiate(structNoise.GetUInt64((ulong)newContext.ZoneSteps.Count));
                        newContext.ZoneSteps.Add(newStep);
                    }
                    structureContexts[id.Segment] = newContext;
                }
                ZoneGenContext zoneContext = structureContexts[id.Segment];
                zoneContext.CurrentID = id.ID;
                ulong finalSeed = (ulong)id.ID;
                finalSeed <<= 32;
                finalSeed |= (ulong)MapsLoaded;

                DiagManager.Instance.LogInfo("Map Count: " + MapsLoaded);
                DiagManager.Instance.LogInfo("Map Seed: " + finalSeed);
                zoneContext.Seed = idNoise.GetUInt64(finalSeed);

                //TODO: remove the need for this explicit cast
                //make a parameterized version of zonestructure and then make zonestructure itself put in basemapgencontext as the parameter
                for (int ii = 0; ii < 5; ii++)
                {
                    try
                    {
                        IGenContext context = Segments[id.Segment].GetMap(zoneContext);
                        Map map = ((BaseMapGenContext)context).Map;

                        if (Persistent)
                            maps.Add(id, map);
                        else
                            MapsLoaded++;
                        return map;
                    }
                    catch (Exception ex)
                    {
                        DiagManager.Instance.LogError(ex);
                        DiagManager.Instance.LogInfo(String.Format("{0}th attempt.", ii+1));
                        ulong subSeed = (ulong)id.ID;
                        subSeed <<= 32;
                        subSeed |= (ulong)(MapsLoaded + ii);
                        zoneContext.Seed = structNoise.GetUInt64(subSeed);
                    }
                }

                DiagManager.Instance.LogInfo(String.Format("Falling back to an empty map!"));
                Map bak = new Map();
                bak.CreateNew(10, 10);
                return bak;
            }
            return maps[id];
        }

        public GroundMap GetGround(SegLoc id)
        {
            return DataManager.Instance.GetGround(GroundMaps[id.ID]);
        }

        public void UnloadMap(SegLoc id)
        {
            if (maps.ContainsKey(id))
                maps.Remove(id);
        }

        public IEnumerator<YieldInstruction> OnInit()
        {
            string assetName = ZoneManager.Instance.CurrentZoneID;

            DiagManager.Instance.LogInfo("Zone.OnInit(): Initializing the zone..");
            if (!String.IsNullOrEmpty(ZoneManager.Instance.CurrentZoneID))
                LuaEngine.Instance.RunZoneScript(assetName);

            LoadScriptEvents();

            //Reload the map events
            foreach (var ev in scriptEvents)
                ev.Value.ReloadEvent();

            //Do script event
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EZoneCallbacks.Init, this));

            //Notify script engine
            LuaEngine.Instance.OnZoneInit(/*assetName, this*/);
        }

        public IEnumerator<YieldInstruction> OnEnterSegment(bool rescuing)
        {
            string assetName = ZoneManager.Instance.CurrentZoneID;

            //Do script event
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EZoneCallbacks.EnterSegment, this, rescuing, CurrentMapID.Segment, CurrentMapID.ID));

            //Notify script engine
            LuaEngine.Instance.OnZoneSegmentStart(/*assetName, this*/);
        }

        public IEnumerator<YieldInstruction> OnExitSegment(GameProgress.ResultType result, bool rescuing)
        {
            string assetName = ZoneManager.Instance.CurrentZoneID;

            //Do script event
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EZoneCallbacks.ExitSegment, this, result, rescuing, CurrentMapID.Segment, CurrentMapID.ID));

            //Notify script engine
            LuaEngine.Instance.OnZoneSegmentEnd(/*assetName, this*/);
        }

        public IEnumerator<YieldInstruction> OnRescued(string name, SOSMail mail)
        {
            string assetName = ZoneManager.Instance.CurrentZoneID;

            //Do script event
            yield return CoroutineManager.Instance.StartCoroutine(RunScriptEvent(LuaEngine.EZoneCallbacks.Rescued, this, name, mail));

        }


        public IEnumerator<YieldInstruction> RunScriptEvent(LuaEngine.EZoneCallbacks ev, params object[] parms)
        {
            if (scriptEvents.ContainsKey(ev))
                yield return CoroutineManager.Instance.StartCoroutine(scriptEvents[ev].Apply(parms));
        }


        public void DoCleanup()
        {
            exitMap();

            string assetName = ID;
            DiagManager.Instance.LogInfo(String.Format("Zone.~Zone(): Finalizing {0}..", assetName));

            if (!String.IsNullOrEmpty(ID))
                LuaEngine.Instance.CleanZoneScript(assetName);
        }
    }
}
