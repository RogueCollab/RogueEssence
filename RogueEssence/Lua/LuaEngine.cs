using System;
using System.Collections.Generic;
using System.Linq;
using RogueEssence.Menu;
using RogueEssence.Data;
using RogueEssence.Ground;
using RogueEssence.Dungeon;
using Microsoft.Xna.Framework;
using NLua;
using System.IO;
using System.Collections;
using Newtonsoft.Json;
using System.Text;
/*
* LuaEngine.cs
* 2017/06/24
* psycommando@gmail.com
* Description: Object managing the lua state, loading of scripts, and initializing the script interface.
*/

namespace RogueEssence.Script
{
    /// <summary>
    /// Class each components of the lua engine should implement
    /// </summary>
    public abstract class ILuaEngineComponent
    {
        /// <summary>
        /// Setups any extra functionalities for this object written on the lua side.
        /// </summary>
        public abstract void SetupLuaFunctions(LuaEngine state);
    }

    /**************************************************************************************
     * LuaEngine
     **************************************************************************************/
    /// <summary>
    /// Manager for the program-wide lua state. Init and de-init block!
    /// </summary>
    public partial class LuaEngine
    {
        #region ZONE_EVENTS
        /// <summary>
        /// The available callbacks a zone's lua script may receive from the engine.
        /// </summary>
        public enum EZoneCallbacks
        {
            Init = 0,    //When the zone is not yet active and being setup
            EnterSegment,          //When a segment is being entered
            ExitSegment,  //When a segment is exited by escape, defeat, completion, etc.
            //TODO: move these events to services
            Rescued,
            Invalid
        }

        public static readonly string ZoneCurrentScriptSym = "CURZONESCR";
        //The last one is optional, and is called before the map script is unloaded, so the script may do any needed cleanup
        public static readonly string ZoneCleanupFun = "{0}.Cleanup";

        /// <summary>
        /// Create the name of a map's expected callback function in its script.
        /// Each specifc callbacks has its own name and format.
        /// </summary>
        /// <param name="callbackformat"></param>
        /// <param name="mapname"></param>
        /// <returns></returns>
        public static string MakeZoneScriptCallbackName(string zonename, EZoneCallbacks callback)
        {
            if (callback < 0 && callback >= EZoneCallbacks.Invalid)
                throw new Exception("LuaEngine.MakeZoneScriptCallbackName(): Unknown callback!");
            return String.Format("{0}.{1}", zonename, callback.ToString());
        }

        #endregion


        #region MAP_EVENTS
        /// <summary>
        /// The available callbacks a map's lua script may receive from the engine.
        /// </summary>
        public enum EMapCallbacks
        {
            Init = 0,    //When the map is not yet displayed and being setup
            Enter,          //When the map is just being displayed an the game fades-in
            Exit,          //When the map has finished fading out before transition to the next
            Update,         //When the game script engine ticks
            GameSave,
            GameLoad,
            Invalid
        }
        //Name for common map callback functions
        public static readonly string MapCurrentScriptSym = "CURMAPSCR";
        //The last one is optional, and is called before the map script is unloaded, so the script may do any needed cleanup
        public static readonly string MapCleanupFun = "{0}.Cleanup";


        /// <summary>
        /// Utility function for the EMapCallbacks enum. Allows iterating all the enum's values.
        /// Meant to be used with a foreach loop
        /// </summary>
        /// <returns>One of the enum value.</returns>
        public static IEnumerable<EMapCallbacks> EnumerateCallbackTypes()
        {
            for (int ii = (int)EMapCallbacks.Init; ii < (int)EMapCallbacks.Invalid; ++ii)
                yield return (EMapCallbacks)ii;
        }

        /// <summary>
        /// Create the name of a map's expected callback function in its script.
        /// Each specifc callbacks has its own name and format.
        /// </summary>
        /// <param name="callbackformat"></param>
        /// <param name="mapname"></param>
        /// <returns></returns>
        public static string MakeMapScriptCallbackName(string mapname, EMapCallbacks callback)
        {
            if (callback < 0 && callback >= EMapCallbacks.Invalid)
                throw new Exception("LuaEngine.MakeMapScriptCallbackName(): Unknown callback!");
            return String.Format("{0}.{1}", mapname, callback.ToString());
        }
        #endregion



        #region DUNGEON_MAP_EVENTS

        /// <summary>
        /// Possible lua callbacks for a given dungeon floor
        /// </summary>
        public enum EDungeonMapCallbacks
        {
            Init = 0,   //When the map is not yet displayed and being setup
            Enter,      //When the map is just being displayed an the game fades-in
            Exit,       //When the player is leaving a dungeon floor

            Invalid,
        }

        //Name for common map callback functions
        public static readonly string DungeonMapCurrentScriptSym = "CURDUNMAPSCR";
        //The last one is optional, and is called before the map script is unloaded, so the script may do any needed cleanup
        public static readonly string DungeonMapCleanupFun = "{0}.Cleanup";

        /// <summary>
        /// Enumerate the values in the EDungeonFloorCallbacks enum
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EDungeonMapCallbacks> EnumerateDungeonFloorCallbackTypes()
        {
            for (int ii = (int)EDungeonMapCallbacks.Init; ii < (int)EDungeonMapCallbacks.Invalid; ++ii)
                yield return (EDungeonMapCallbacks)ii;
        }

        /// <summary>
        /// Returns the name string for a lua callback function for the given dungeon's floor
        /// </summary>
        /// <param name="zonename"></param>
        /// <param name="floornumber"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static string MakeDungeonMapScriptCallbackName(string zonename, int floornumber, EDungeonMapCallbacks callback)
        {
            return MakeDungeonMapScriptCallbackName(MakeDungeonFloorAssetName(zonename, floornumber), callback);
        }

        public static string MakeDungeonMapScriptCallbackName(string floorname, EDungeonMapCallbacks callback)
        {
            if (callback < 0 && callback >= EDungeonMapCallbacks.Invalid)
                throw new Exception("LuaEngine.MakeDungeonMapScriptCallbackName(): Unknown callback!");
            return String.Format("{0}.{1}", floorname, callback.ToString());
        }

        /// <summary>
        /// Generates an asset name for the given floor in the given dungeon. Mainly used for scripts.
        /// </summary>
        /// <param name="dungeonname"></param>
        /// <param name="floornumber"></param>
        /// <returns></returns>
        public static string MakeDungeonFloorAssetName(string dungeonname, int floornumber)
        {
            return String.Format("{0}.floor{1}", dungeonname, floornumber);
        }

        /// <summary>
        /// Return the name string for a lua callback for all the non-overriden dungeon floor events, for a given dungeon.
        /// </summary>
        /// <param name="dungeonname"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static string MakeDefaultDungeonFloorCallbackName(string dungeonname, EDungeonMapCallbacks callback)
        {
            if (callback < 0 && callback >= EDungeonMapCallbacks.Invalid)
                throw new Exception("LuaEngine.MakeDefaultDungeonFloorCallbackName(): Unknown callback!");
            return String.Format("{0}.floordefault.{1}", dungeonname, callback.ToString());
        }

        #endregion

        #region ENTITY_EVENT
        /// <summary>
        /// Types of events that an entity may have.
        /// </summary>
        public enum EEntLuaEventTypes
        {
            Action = 0,
            Touch = 1,
            Think = 2,
            Update = 3,
            EntSpawned = 4, //When a spawner spawned an entity
            Invalid,
        }
        
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EEntLuaEventTypes> IterateLuaEntityEvents()
        {
            for (int ii = (int)EEntLuaEventTypes.Action; ii < (int)EEntLuaEventTypes.Invalid; ++ii)
                yield return (EEntLuaEventTypes)ii;
        }

        public static string MakeLuaEntityCallbackName(string entname, EEntLuaEventTypes type)
        {
            if (type < 0 && type >= EEntLuaEventTypes.Invalid)
                throw new Exception("LuaEngine.MakeLuaEntityCallbackName(): Invalid Lua entity event type!");
            return String.Format("{2}.{0}_{1}", entname, type.ToString(), MapCurrentScriptSym);
        }
#endregion


        #region SERVICES_EVENTS
        private static readonly string NameLuaServiceEventNames = "EngineServiceEvents";

        private enum EServiceEvents
        {
            Init = 0,
            Deinit,
            GraphicsLoad,
            GraphicsUnload,
            NewGame,
            Restart,
            Update,

            GroundEntityInteract,

            DungeonModeBegin,
            DungeonModeEnd,

            DungeonMapInit,
            DungeonFloorEnter,
            DungeonFloorExit,

            ZoneInit,
            DungeonSegmentStart,
            DungeonSegmentEnd,

            GroundModeBegin,
            GroundModeEnd,

            GroundMapInit,
            GroundMapEnter,
            GroundMapExit,

            //Keep last
            _NBEvents,
        };

        private IEnumerator<EServiceEvents> IterateServiceEvents()
        {
            for (int cntev = 0; cntev < (int)EServiceEvents._NBEvents; ++cntev)
                yield return (EServiceEvents)cntev;
        }
        #endregion

        #region MAIN_SCRIPTS

        const string SCRIPT_MAIN = "main.lua";
        const string SCRIPT_COMMON = "common.lua";
        const string SCRIPT_VARS = "scriptvars.lua";
        const string SCRIPT_EVENT = "event.lua";
        const string INCLUDE_EVENT = "include.lua";

        /// <summary>
        /// Assemble the path to the specified script
        /// </summary>
        /// <param name="script">Script to make the path for</param>
        /// <returns>The path to the script file.</returns>
        private string PathToScript(string script)
        {
            return String.Format("{0}{1}", PathMod.ModPath(SCRIPT_PATH), script);
        }
        #endregion

        //Paths
        public const string MAP_SCRIPT_PATTERN = "ground.{0}";
        public const string MAP_SCRIPT_DIR = SCRIPT_PATH + "ground/";
        public const string MAP_SCRIPT_ENTRY_POINT = "init"; //filename of the map's main script file that the engine will run when the map is loaded (by default lua package entrypoints are init.lua)

        public const string DUNGEON_MAP_SCRIPT_PATTERN = "map.{0}";
        public const string DUNGEON_MAP_SCRIPT_DIR = SCRIPT_PATH + "map/";
        public const string DUNGEON_MAP_SCRIPT_ENTRY_POINT = "init";

        public const string ZONE_SCRIPT_PATTERN = "zone.{0}";
        public const string ZONE_SCRIPT_DIR = SCRIPT_PATH + "zone/";
        public const string ZONE_SCRIPT_ENTRY_POINT = "init"; //filename of the zone's main script file that the engine will run when the zone is loaded (by default lua package entrypoints are init.lua)

        //Global lua symbol names
        private const string SCRIPT_VARS_NAME = "SV"; //Name of the table of script variables that gets loaded and saved with the game

        //Lua State
        public const string SCRIPT_PATH = DataManager.DATA_PATH + "Script/";  //Base script engine scripts path
        public const string SCRIPT_CPATH = DataManager.DATA_PATH + "Script/bin/"; //Base script engine libraries, for so and dlls
        private ScriptServices m_scrsvc;
        private ScriptSound m_scriptsound;
        private ScriptGround m_scriptground;
        private ScriptGame m_scriptgame;
        private ScriptUI m_scriptUI;
        private ScriptDungeon m_scriptdungeon;
        private ScriptStrings m_scriptstr;
        private ScriptTask m_scripttask;
        private ScriptAI m_scriptai;
        private ScriptXML m_scriptxml;

        //Engine time
        private TimeSpan m_nextUpdate;
        private GameTime m_curtime = new GameTime();

        //Properties
        public Lua      LuaState { get; set; }
        public GameTime Curtime { get { return m_curtime; } set { m_curtime = value; } }

        public bool Breaking { get; private set; }

        //Pre-compiled internal lua functions
        private LuaFunction m_MkCoIter;  //Instantiate a lua coroutine iterator function/state, for the ScriptEvent class mainly.
        private LuaFunction m_UnpackParamsAndRun;


        //==============================================================================
        // LuaEngine Initialization code
        //==============================================================================

        private static LuaEngine instance;
        public static void InitInstance()
        {
            instance = new LuaEngine();
        }
        public static LuaEngine Instance { get { return instance; } }

        /// <summary>
        /// Constructor private, since we don't want to instantiate this more than once! Otherwise bad things will happen.
        /// </summary>
        private LuaEngine()
        {
            Reset();
        }


        public static void InitScriptFolders(string baseFolder)
        {
            string sourcePath = Path.Join(PathMod.ASSET_PATH, SCRIPT_PATH);
            string destPath = Path.Join(baseFolder, SCRIPT_PATH);
            //TODO: do we need scripts to be in their own folder?
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destPath), true);
        }

        /// <summary>
        /// Handles importing the various .NET namespaces used in the project to lua.
        /// It uses reflection to do so.
        /// </summary>
        private void ImportDotNet()
        {
            DiagManager.Instance.LogInfo("[SE]:Importing .NET packages..");
            LuaState.LoadCLRPackage();

            LuaState.DoFile(PathToScript(INCLUDE_EVENT));
        }

        /// <summary>
        /// Clean the lua state
        /// Must call init methods manually again!!
        /// </summary>
        public void Reset()
        {
            //init lua
            LuaState = new Lua();
            //LuaState.UseTraceback = true;
            LuaState.State.Encoding = Encoding.UTF8;
            m_nextUpdate = new TimeSpan(-1);
            DiagManager.Instance.LogInfo(String.Format("[SE]:Initialized {0}", LuaState["_VERSION"] as string));

            ImportDotNet();
            m_scriptstr = new ScriptStrings();

            //Disable the import command, we could also rewrite it to only allow importing some specific things!
            //LuaState.DoString("import = function() end");
            //!#FIXME: disable  import again

            //Instantiate components
            m_nextUpdate = new TimeSpan(0);
            m_scrsvc = new ScriptServices(this);
            m_scriptsound = new ScriptSound();
            m_scriptgame = new ScriptGame();
            m_scriptground = new ScriptGround();
            m_scriptUI = new ScriptUI();
            m_scriptdungeon = new ScriptDungeon();
            m_scripttask = new ScriptTask();
            m_scriptai = new ScriptAI();
            m_scriptxml = new ScriptXML();

            //Expose symbols
            ExposeInterface();
            SetupGlobals();
            CacheMainScripts();

            DiagManager.Instance.LogInfo("[SE]: **- Lua engine ready! -**");
        }

        public void BreakScripts()
        {
            Breaking = true;
        }

        public void SceneOver()
        {
            Breaking = false;
        }

        /// <summary>
        /// Calling this sends the OnInit event to the script engine.
        /// Use this if you just reset the script state, and want to force it to do its initialization.
        /// </summary>
        public void ReInit()
        {
            Breaking = true;
            DiagManager.Instance.LogInfo("[SE]:Re-initializing scripts!");
            DiagManager.Instance.LogInfo("[SE]:Loading last serialized script variables!");
            if (DataManager.Instance.Save != null)
                LoadSavedData(DataManager.Instance.Save);
            if (ZoneManager.Instance != null)
            {
                ZoneManager.Instance.LuaEngineReload();
                if (ZoneManager.Instance.CurrentZone != null && 
                    (GameManager.Instance.CurrentScene == GroundScene.Instance || GameManager.Instance.CurrentScene == DungeonScene.Instance))
                    GameManager.Instance.SceneOutcome = ReInitZone();
            }

        }

        public IEnumerator<YieldInstruction> ReInitZone()
        {
            yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentZone.OnInit());
            if (ZoneManager.Instance.CurrentGround != null)
            {
                OnGroundMapInit(ZoneManager.Instance.CurrentGround.AssetName, ZoneManager.Instance.CurrentGround);
                OnGroundModeBegin();

                //process events before the map fades in
                yield return CoroutineManager.Instance.StartCoroutine(ZoneManager.Instance.CurrentGround.OnInit());
            }
            else if (ZoneManager.Instance.CurrentMap != null)
            {
                //!#FIXME : We'll need to call the method on map entry too if a map is running!
                //OnDungeonFloorInit();
            }
        }


        /// <summary>
        /// Set lua package paths to the ones in the game's files.
        /// </summary>
        private void SetLuaPaths()
        {
            DiagManager.Instance.LogInfo("[SE]:Setting up lua paths..");
            
            //windows-specific: set the encoding to code page 1252, just for this block
            //This is because the filesystem uses cp1252, and if the search paths arent passed in with cp1252 encoding,
            //then searching for lua includes will fail when paths contain unicode letters.
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                LuaState.State.Encoding = Encoding.GetEncoding(1252);
            
            //Add the current script directory to the lua searchpath for loading modules!
            LuaState["package.path"] = LuaState["package.path"] + ";" +
                                        Path.GetFullPath(PathMod.ModPath(SCRIPT_PATH)) + "lib/?.lua;" +
                                        Path.GetFullPath(PathMod.ModPath(SCRIPT_PATH)) + "?.lua;" +
                                        Path.GetFullPath(PathMod.ModPath(SCRIPT_PATH)) + "?/init.lua";

            //Add lua binary path
            string cpath = LuaState["package.cpath"] + ";" + Path.GetFullPath(PathMod.ModPath(SCRIPT_CPATH));
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                cpath += "?.dll";
            else
                cpath += "?.so";
            LuaState["package.cpath"] = cpath;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                LuaState.State.Encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Expose the list of available service callbacks names to Lua. It should make it easier for script devs to get a list of them to fiddle with.
        /// </summary>
        private void FillServiceEventsTable()
        {
            LuaTable availables = RunString("return {}").First() as LuaTable;
            LuaFunction fnAddToTable = RunString("return function(tbl, name, item) tbl[name] = item end").First() as LuaFunction;
            var svcev = IterateServiceEvents();
            while (svcev.MoveNext())
                fnAddToTable.Call(availables, svcev.Current.ToString(), svcev.Current.ToString());
            LuaState[NameLuaServiceEventNames] = availables;
        }

        /// <summary>
        /// Add all the required global variables to the lua environment!
        /// </summary>
        private void SetupGlobals()
        {
            DiagManager.Instance.LogInfo("[SE]:Setting up lua globals..");

            SetLuaPaths();

            //Setup some globabl vars
            LuaState["_SCRIPT_PATH"] = Path.GetFullPath(PathMod.ModPath(SCRIPT_PATH)); //Share with the script engine the path to the root of the script files

            RunString(ZoneCurrentScriptSym + " = nil");
            RunString(MapCurrentScriptSym + " = nil");
            RunString(DungeonMapCurrentScriptSym + " = nil");

            //Make empty script variable table
            LuaState.NewTable(SCRIPT_VARS_NAME);
            LuaState[SCRIPT_VARS_NAME + ".__ALL_SET"] = "OK"; //This is just a debug variable, to make sure the table isn't overwritten at runtime by something else, and help track down issues linked to that.

            //Add the list of available Callbacknames
            FillServiceEventsTable();

            RunString("require 'CLRPackage'");

            RunString(@"
                __GetLevel = function()
                    local curground = RogueEssence.Dungeon.ZoneManager.Instance.CurrentGround
                    local curmap    = RogueEssence.Dungeon.ZoneManager.Instance.CurrentMap

                    if curground then
                      return curground
                    elseif curmap then
                      return curmap
                    end
                    return nil
                  end
            ", "__GetLevel function init");

            //Setup lookup functions
            //Character lookup
            RunString(@"
                function CH(charname)
                    local curlvl = __GetLevel()
                    if curlvl then
                      return curlvl:GetChar(charname)
                    end
                    return nil
                  end
            ", "CH function init");

            //Object lookup
            RunString(@"
                OBJ = function(name)
                    if _ZONE.CurrentGround then
                      return _ZONE.CurrentGround:GetObj(name)
                    elseif _ZONE.CurrentMap then
                      assert(false, 'OBJ(' .. name .. '): unimplemented on dungeon maps!!!!')
                    end
                    return nil
                  end
            ", "OBJ function init");

            //Marker lookup
            RunString(@"
                MRKR = function(name)
                    if _ZONE.CurrentGround then
                      return _ZONE.CurrentGround:GetMarker(name)
                    elseif _ZONE.CurrentMap then
                      assert(false, 'MRKR(' .. name .. '): unimplemented on dungeon maps!!!!')
                    end
                    return nil
                  end
            ", "OBJ function init");

            //Character localized name
            RunString(@"
                CHName = function(name)
                    if _ZONE.CurrentGround then
                      local ch = _ZONE.CurrentGround:GetChar(name)
                      if ch then
                          return ch:GetDisplayName()
                      end
                    elseif _ZONE.CurrentMap then
                      assert(false, 'CHName(' .. name .. '): unimplemented on dungeon maps!!!!')
                    end
                    return nil
                  end
            ", "CHName function init");

            //This returns a MonsterID
            RunString(@"
                IDX = function(id, ...)
                        local param = {...}
                        local form  = param[0]
                        local skin = param[1]
                        local gender= param[2]

                        if not form then form = 0 end
                        if not skin then skin = 0 end
                        if not gender then gender = Gender.Male end
                        return MonsterID(id, form, skin, gender)
                    end
            ", "IDX function init");

            //LTBL is a wrapper to access the LuaTable that some entiies have
            RunString(@"
                function LTBL(entity)
                    if not entity then return nil end
                    if not entity.LuaData then return nil end
                    return entity.LuaData
                end
                ",
                "LTBL");

            //Function to lookup a NPC spawner by name on the current level
            RunString(@"
                SPWN = function(spawnername)
                    local curlvl = __GetLevel()
                    if curlvl then
                      return curlvl:GetSpanwer(spawnername)
                    end
                    return nil
                end
            ", "SPWN function init");


            //Expose the lua system
            LuaState["LUA_ENGINE"] = this;

        }


        /// <summary>
        /// Call this on map changes to ensure that the script engine has access to those .NET globals!
        /// </summary>
        private void ExposeInterface()
        {
            DiagManager.Instance.LogInfo("[SE]:Exposing game engine components instances..");
            //Expose directly parts of the engine
            LuaState[ScriptServices.SInterfaceInstanceName] = m_scrsvc;

            LuaState["_GROUND"] = GroundScene.Instance;
            LuaState["_DUNGEON"] = DungeonScene.Instance;
            LuaState["_ZONE"] = ZoneManager.Instance;
            LuaState["_GAME"] = GameManager.Instance;
            LuaState["_DATA"] = DataManager.Instance;
            LuaState["_MENU"] = MenuManager.Instance;
            LuaState["_DIAG"] = DiagManager.Instance;

            DiagManager.Instance.LogInfo("[SE]:Exposing script interface..");
            //Expose script interface  objects
            LuaState["GROUND"] = m_scriptground;
            LuaState["DUNGEON"] = m_scriptdungeon;
            LuaState["GAME"] = m_scriptgame;
            m_scriptgame.SetupLuaFunctions(this); //Need to do this at this very moment.
            LuaState["SOUND"] = m_scriptsound;
            LuaState["UI"] = m_scriptUI;
            LuaState["STRINGS"] = m_scriptstr;
            LuaState["TASK"] = m_scripttask;
            LuaState["AI"] = m_scriptai;
            LuaState["XML"] = m_scriptxml;

        }

        public void UpdateZoneInstance()
        {
            LuaState["_ZONE"] = ZoneManager.Instance;
        }


        private void SetupLuaFunctions()
        {
            //Add the function to make iterators on coroutines
            m_MkCoIter = RunString(
                @"return function(fun,...)
                      local arguments = {...}
                      local co = coroutine.create(function() xpcall( fun, PrintStack, table.unpack(arguments)) end)
                      return function ()   -- iterator
                        local code, res = coroutine.resume(co)
                        if code == false then --This means there was an error in there
                            assert(false,'Error running coroutine ' .. tostring(fun) .. '! :\n' .. PrintStack(res))
                        end
                        return res
                      end
                    end",
            "MakeCoroutineIterator").First() as LuaFunction;

            m_UnpackParamsAndRun = RunString(
                @"return function(fun, params)
                    local size = params.Length
                    local transittbl = {}
                    print('Length == ' .. tostring(params.Length))
                    local i = 0
                    while i < size do
                        table.insert(transittbl, params[i])
                        i = i + 1
                    end
                    return fun(table.unpack(transittbl))
                end",
                "UnpackParamsAndRun").First() as LuaFunction;
        }

        /// <summary>
        /// Preload the script files we expect to be there!
        /// </summary>
        private void CacheMainScripts()
        {
            DiagManager.Instance.LogInfo("[SE]:Caching scripts..");
            m_scrsvc.SetupLuaFunctions(this);
            //Cache default script variables
            LuaState.DoFile(PathToScript(SCRIPT_VARS));
            //Cache common lib
            LuaState.LoadFile(PathToScript(SCRIPT_COMMON));
            //load events
            LuaState.DoFile(PathToScript(SCRIPT_EVENT));

            //Install misc lua functions each interfaces needs
            DiagManager.Instance.LogInfo("[SE]:Installing game interface functions..");
            SetupLuaFunctions();
            //m_scrco.SetupLuaFunctions(this);
            m_scriptstr.SetupLuaFunctions(this);
            m_scriptsound.SetupLuaFunctions(this);
            m_scriptground.SetupLuaFunctions(this);
            m_scriptUI.SetupLuaFunctions(this);
            m_scriptdungeon.SetupLuaFunctions(this);
            m_scripttask.SetupLuaFunctions(this);
            m_scriptai.SetupLuaFunctions(this);
            m_scriptxml.SetupLuaFunctions(this);

            //If script vars aren't initialized in the save, do it now!
            //DiagManager.Instance.LogInfo("[SE]:Checking if we need to initialize the script variables saved state..");
            //if (DataManager.Instance != null && DataManager.Instance.Save != null && String.IsNullOrEmpty(DataManager.Instance.Save.ScriptVars))
            //    SaveData(DataManager.Instance.Save);

            //Run main script
            DiagManager.Instance.LogInfo(String.Format("[SE]:Running {0} script..", SCRIPT_MAIN));
            LuaState.DoFile(PathToScript(SCRIPT_MAIN));
        }


        /// <summary>
        /// Use this to un-serialize the script variables and load them.
        /// </summary>
        public void LoadSavedData(GameProgress loaded)
        {
            //Do stuff when resuming from a save!
            DiagManager.Instance.LogInfo("LuaEngine.LoadSavedData()..");
            if ( loaded == null || loaded.ScriptVars == null)
            {
                LuaState.DoFile(PathToScript(SCRIPT_VARS));
            }
            else
            {
                LuaTable tbl = DictToLuaTable(loaded.ScriptVars);
                LuaState[SCRIPT_VARS_NAME] = tbl;
            }
            if (loaded != null)
            {
                loaded.ActiveTeam.LoadLua();
            }
            //Tell the script we've just resumed a save!
            m_scrsvc.Publish("LoadSavedData");
        }

        /// <summary>
        /// Use this to serialize the script variables and place the serialized data into the current GameProgress.
        /// </summary>
        public void SaveData(GameProgress save)
        {
            //First tell the script we're gonna save
            m_scrsvc.Publish("SaveData");

            //Save script engine stuff here!
            DiagManager.Instance.LogInfo("LuaEngine.SaveData()..");
            LuaTable tbl = LuaState.GetTable(SCRIPT_VARS_NAME);
            save.ScriptVars = LuaTableToDict(tbl);
            save.ActiveTeam.SaveLua();
        }


        /// <summary>
        /// Creates a deep-copy conversion of the input lua table to a dictionary of objects.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public LuaTableContainer LuaTableToDict(LuaTable table)
        {
            Dictionary<object, object> dict = LuaState.GetTableDict(table);
            List<object[]> tbl_list = new List<object[]>();
            foreach (object key in dict.Keys)
            {
                object val = dict[key];
                if (val is LuaTable)
                {
                    LuaTableContainer subDict = LuaTableToDict(val as LuaTable);
                    tbl_list.Add(new object[] { key, subDict });
                }
                else
                    tbl_list.Add(new object[] { key, val });

            }
            return new LuaTableContainer(tbl_list);
        }

        public LuaTable DictToLuaTable(LuaTableContainer dict)
        {
            if (dict == null)
                return null;

            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, key, itm) tbl[key] = itm end").First() as LuaFunction;
            foreach (object[] entry in dict.Table)
            {
                object key = entry[0];
                object val = entry[1];
                if (val is LuaTableContainer)
                    val = DictToLuaTable((LuaTableContainer)val);
                addfn.Call(tbl, key, val);
            }
            return tbl;
        }

        /// <summary>
        /// Dumps the specified lua table to a string, for serialization!
        /// </summary>
        /// <param name="tbl">lua table itself</param>
        /// <returns>A string representation of the lua table.Returns null if failed!</returns>
        public string SerializeLuaTable(LuaTable tbl)
        {
            object[] result = CallLuaFunctions("Serpent.dump", tbl);
            if (result != null && result[0] != null)
                return result[0] as string;
            else
                return null;
        }

        /// <summary>
        /// Deserialize a lua table that was dumped to a string using the SerializeLuaTable method.
        /// </summary>
        /// <param name="serializedtbl"></param>
        /// <returns></returns>
        public LuaTable DeserializedLuaTable(string serializedtbl)
        {
            //Load returns a boolean and the result. The boolean is true if it succeeded.
            object[] result = CallLuaFunctions("Serpent.load", serializedtbl).ToArray();
            if ((bool)result[0])
                return result[1] as LuaTable;
            else
                return null;
        }

        /// <summary>
        /// Struct used for defining an entry to be imported by the script engine.
        /// </summary>
        private struct ImportEntry
        {
            String m_Assembly;
            String m_Namespace;

            public ImportEntry(String asm, String nspace)
            {
                m_Assembly = asm;
                m_Namespace = nspace;
            }

            public bool hasAssembly()
            {
                return (m_Assembly!= null && m_Assembly.Length > 0) &&
                       (m_Namespace != null && m_Namespace.Length > 0);
            }

            public string Namespace { get { return m_Namespace;} set { m_Namespace = value;} }
            public string Assembly { get { return m_Assembly; } set { m_Assembly = value; } }
        }

        /// <summary>
        /// Instantiate a lua module's Class table using its metatable's "__call" definition
        /// </summary>
        /// <param name="classpath"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public LuaTable InstantiateLuaModule(string modulepath, params object[] args)
        {
            try
            {
                LuaTable luaclass = RunString(String.Format("return require '{0}'", modulepath)).First() as LuaTable;
                //LuaTable tbl = LuaState.GetTable(classpath);
                if (luaclass != null)
                {
                    //LuaFunction DoInstantiate =   RunString("return function(srcclass, ...) return srcclass(...) end").First() as LuaFunction;
                    //LuaFunction fn =  luaclass["__call"] as LuaFunction;
                    //if (fn != null)
                    //    return fn.Call(args).First() as LuaTable;
                    //if(DoInstantiate != null)
                    //{
                        return m_UnpackParamsAndRun.Call(luaclass, args).First() as LuaTable;
                    //}
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogInfo(String.Format("LuaEngine.InstantiateLuaClass(): Error instantiating class \"{0}\"!\n{1}", modulepath, ex.Message));
            }
            return null;
        }

        /// <summary>
        /// Allow calling a lua function of a lua table/object with the specified parameters, and returns the result.
        /// </summary>
        /// <param name="objname">Path of the lua object instance whose method we'll call.</param>
        /// <param name="funname">Name of the method of the lua object instance to call.</param>
        /// <param name="args">Parameters to pass the method (excluding "self")</param>
        /// <returns>Returns the array of objects that the lua interpreter returns after executing the method.</returns>
        public object[] CallLuaMemberFun(string objname, string funname, params object[] args)
        {
            try
            {
                string fpath = objname + "." + funname;
                LuaFunction fun = LuaState.GetFunction(fpath);
                if (fun == null)
                {
                    DiagManager.Instance.LogInfo("[SE]:LuaEngine.CallLuaMemberFun(): Tried to call undefined method " + fpath + "!");
                    return null;
                }
                List<object> ar = (args == null) ? new List<object>() : new List<object>(args);
                ar.Insert(0, LuaState[objname]);

                return m_UnpackParamsAndRun.Call(fun, ar.ToArray()); //fun.Call(ar.ToArray()); //We need to pass the "self" parameter first
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo("[SE]:LuaEngine.CallLuaMemberFun(): Error calling member function:\n" + e.Message);
            }
            return null;
        }


        /// <summary>
        /// Calls a lua function of the given name, with the given arguments, and returns its return value(s).
        /// </summary>
        /// <param name="path">Path of the function to call.</param>
        /// <param name="args">Parameters to pass the function being called.</param>
        /// <returns>Returns the array of objects that the lua interpreter returns after executing the method.</returns>
        public object[] CallLuaFunctions(string path, params object[] args)
        {

            try
            {
                var scriptFunc = LuaState[path] as LuaFunction;
                if (scriptFunc == null)
                {
                    DiagManager.Instance.LogInfo("[SE]:LuaEngine.CallLuaFunctions(): Tried to call undefined function " + path + "!");
                    return null;
                }

                return m_UnpackParamsAndRun.Call(scriptFunc, args);
                //return scriptFunc.Call(args);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo("[SE]:LuaEngine.CallLuaFunctions(): Error calling function :\n" + e.Message);
            }
            return null;
        }

        /// <summary>
        /// Makes the lua interpreters execute the given string as lua code.
        /// </summary>
        /// <param name="luatxt">Lua code  to execute.</param>
        /// <returns>Object array containing the return value of the string's execution.</returns>
        public object[] RunString(string luatxt, string chunkname = "chunk")
        {
            //Pretty basic so far, but ideally this should be a lot more sophisticated.
            // Might need to inject things in the lua stack too
            try
            {
                return LuaState.DoString(luatxt, chunkname);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo("[SE]:LuaEngine.RunString(): Error executing string:\n" + e.Message + "\nContent:\n" + luatxt);
            }
            return null;
        }

        /// <summary>
        /// Makes the full absolute path to the directory a map's script should be in.
        /// </summary>
        /// <param name="mapname">AssetName of the map to look for.</param>
        /// <returns>Absolute path to the map's script directory.</returns>
        public static string MakeZoneScriptPath(string zonename)
        {
            return Path.GetFullPath(PathMod.ModPath(SCRIPT_PATH)) +
                   string.Format(ZONE_SCRIPT_PATTERN, zonename).Replace('.', '/'); //The physical path to the map's script dir
        }

        /// <summary>
        /// Load and execute the script of a zone.
        /// </summary>
        /// <param name="zoneassetname">The AssetName of the zone for which we have to load the script of</param>
        public void RunZoneScript(string zoneassetname)
        {
            string zonepath = MakeZoneScriptPath(zoneassetname);
            try
            {
                string abspath = Path.GetFullPath(zonepath + "/init.lua");
                LuaState.LoadFile(abspath);
                RunString(String.Format("{2} = require('{0}'); {1} = {2}", string.Format(ZONE_SCRIPT_PATTERN, zoneassetname), ZoneCurrentScriptSym, zoneassetname),
                          abspath);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo("[SE]:LuaEngine.RunZoneScript(): Error running zone script!:\n" + e.Message + "\npath:\n" + zonepath);
            }
        }

        /// <summary>
        /// Use this to clean up the traces left behind by a zone package.
        /// Also collects garbages.
        /// </summary>
        /// <param name="zoneassetname"></param>
        public void CleanZoneScript(string zoneassetname)
        {
            RunString(@"
                local tbllen = 0
                for k,v in pairs(_ENV) do
                    tbllen = tbllen + 1
                end
                print('=>_ENV pre-cleanup:' .. tostring(tbllen))
            ");

            RunString(
                    String.Format(@"
                        if {1} and {2} then
                            xpcall( {2}, PrintStack)
                        end
                        package.loaded.{0} = nil
                        {1} = nil
                        {0} = nil
                        collectgarbage()", zoneassetname, ZoneCurrentScriptSym, String.Format(ZoneCleanupFun, ZoneCurrentScriptSym)),
                      "CleanZoneScript");

            RunString(@"
                local tbllen = 0
                for k,v in pairs(_ENV) do
                    tbllen = tbllen + 1
                end
                print('=>_ENV post cleanup:' .. tostring(tbllen))
            ");
        }




        /// <summary>
        /// Makes the full absolute path to the directory a map's script should be in.
        /// </summary>
        /// <param name="mapname">AssetName of the map to look for.</param>
        /// <returns>Absolute path to the map's script directory.</returns>
        public static string MakeDungeonMapScriptPath(string mapname)
        {
            return Path.GetFullPath(PathMod.ModPath(SCRIPT_PATH)) +
                   string.Format(DUNGEON_MAP_SCRIPT_PATTERN, mapname).Replace('.', '/'); //The physical path to the map's script dir
        }

        /// <summary>
        /// Load and execute the script of a zone.
        /// </summary>
        /// <param name="mapassetname">The AssetName of the zone for which we have to load the script of</param>
        public void RunDungeonMapScript(string mapassetname)
        {
            string mappath = MakeDungeonMapScriptPath(mapassetname);
            try
            {
                string abspath = Path.GetFullPath(mappath + "/init.lua");
                LuaState.LoadFile(abspath);
                RunString(String.Format("{2} = require('{0}'); {1} = {2}", string.Format(DUNGEON_MAP_SCRIPT_PATTERN, mapassetname), DungeonMapCurrentScriptSym, mapassetname),
                          abspath);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo("[SE]:LuaEngine.RunDungeonMapScript(): Error running dungeon map script!:\n" + e.Message + "\npath:\n" + mappath);
            }
        }

        /// <summary>
        /// Use this to clean up the traces left behind by a zone package.
        /// Also collects garbages.
        /// </summary>
        /// <param name="zoneassetname"></param>
        public void CleanDungeonMapScript(string mapassetname)
        {
            RunString(@"
                local tbllen = 0
                for k,v in pairs(_ENV) do
                    tbllen = tbllen + 1
                end
                print('=>_ENV pre-cleanup:' .. tostring(tbllen))
            ");

            RunString(
                    String.Format(@"
                        if {1} and {2} then
                            xpcall( {2}, PrintStack)
                        end
                        package.loaded.{0} = nil
                        {1} = nil
                        {0} = nil
                        collectgarbage()", mapassetname, DungeonMapCurrentScriptSym, String.Format(DungeonMapCleanupFun, DungeonMapCurrentScriptSym)),
                      "CleanDungeonMapScript");

            RunString(@"
                local tbllen = 0
                for k,v in pairs(_ENV) do
                    tbllen = tbllen + 1
                end
                print('=>_ENV post cleanup:' .. tostring(tbllen))
            ");
        }

        /// <summary>
        /// Makes the full absolute path to the directory a map's script should be in.
        /// </summary>
        /// <param name="mapname">AssetName of the map to look for.</param>
        /// <returns>Absolute path to the map's script directory.</returns>
        public static string MakeMapScriptPath(string mapname)
        {
            return Path.GetFullPath(PathMod.ModPath(SCRIPT_PATH)) +
                   string.Format(MAP_SCRIPT_PATTERN, mapname).Replace('.', '/'); //The physical path to the map's script dir
        }

        /// <summary>
        /// Load and execute the script of a map.
        /// </summary>
        /// <param name="mapassetname">The AssetName of the map for which we have to load the script of</param>
        public void RunMapScript(string mapassetname)
        {
            string mappath = MakeMapScriptPath(mapassetname);
            try
            {
                string abspath = Path.GetFullPath(mappath + "/init.lua");
                LuaState.LoadFile(abspath);
                RunString(String.Format("{2} = require('{0}'); {1} = {2}", string.Format(MAP_SCRIPT_PATTERN, mapassetname), MapCurrentScriptSym, mapassetname),
                          abspath);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo("[SE]:LuaEngine.RunMapScript(): Error running map script!:\n" + e.Message + "\npath:\n" + mappath);
            }
        }

        /// <summary>
        /// Use this to clean up the traces left behind by a map package.
        /// Also collects garbages.
        /// </summary>
        /// <param name="mapassetname"></param>
        public void CleanMapScript(string mapassetname)
        {
            RunString(@"
                local tbllen = 0
                for k,v in pairs(_ENV) do
                    tbllen = tbllen + 1
                end
                print('=>_ENV pre-cleanup:' .. tostring(tbllen))
            ");

            RunString(
                    String.Format(@"
                        if {1} and {2} then
                            xpcall( {2}, PrintStack)
                        end
                        package.loaded.{0} = nil
                        {1} = nil
                        {0} = nil
                        collectgarbage()", mapassetname, MapCurrentScriptSym, String.Format(MapCleanupFun, MapCurrentScriptSym)),
                      "CleanMapScript");

            RunString(@"
                local tbllen = 0
                for k,v in pairs(_ENV) do
                    tbllen = tbllen + 1
                end
                print('=>_ENV post cleanup:' .. tostring(tbllen))
            ");
        }

        /// <summary>
        /// Creates the bare minimum script and map folder for a ground map.
        /// </summary>
        /// <param name="mapassetname"></param>
        public void CreateNewMapScriptDir(string mapassetname)
        {
            string mappath = MakeMapScriptPath(mapassetname);
            try
            {
                //Check if files exists already
                if (!Directory.Exists(mappath))
                    Directory.CreateDirectory(mappath);

                string packageentry = String.Format("{1}/{0}.lua", MAP_SCRIPT_ENTRY_POINT, mappath);
                if (!File.Exists(packageentry))
                {
                    using (var fstream = File.CreateText(packageentry))
                    {
                        //Insert comment header
                        fstream.WriteLine(
                        "--[[\n" +
                        "    {0}\n" +
                        "    Created: {2}\n" +
                        "    Description: Autogenerated script file for the map {1}.\n" +
                        "]]--\n" +
                        "-- Commonly included lua functions and data\n" +
                        "require 'common'\n\n" +

                        "-- Package name\n" +
                        "local {1} = {{}}\n\n" +

                        "-- Local, localized strings table\n" +
                        "-- Use this to display the named strings you added in the strings files for the map!\n" +
                        "-- Ex:\n" +
                        "--      local localizedstring = MapStrings['SomeStringName']\n" +
                        "local MapStrings = {{}}\n\n" +

                        "-------------------------------\n" +
                        "-- Map Callbacks\n" +
                        "-------------------------------",
                        MAP_SCRIPT_ENTRY_POINT + ".lua", mapassetname, DateTime.Now.ToString());

                        //Insert the default map functions and comment header
                        foreach (EMapCallbacks fn in EnumerateCallbackTypes())
                        {
                            string callbackname = MakeMapScriptCallbackName(mapassetname, fn);
                            fstream.WriteLine("---{0}\n--Engine callback function\nfunction {0}(map)\n", callbackname);
                            if (fn == EMapCallbacks.Init)
                            {
                                //Add the map string loader
                                fstream.WriteLine(
                                "  --This will fill the localized strings table automatically based on the locale the game is \n" +
                                "  -- currently in. You can use the MapStrings table after this line!\n" +
                                "  MapStrings = COMMON.AutoLoadLocalizedStrings()");
                            }
                            else if (fn == EMapCallbacks.Enter || fn == EMapCallbacks.GameLoad)
                            {
                                fstream.WriteLine(
                                "  GAME:FadeIn(20)");
                            }
                            fstream.WriteLine("\nend\n");
                        }

                        fstream.WriteLine(
                        "-------------------------------\n" +
                        "-- Entities Callbacks\n" +
                        "-------------------------------\n\n\n" +


                        "return {0}\n",
                        mapassetname);

                        fstream.Flush();
                        fstream.Close();
                    }
                }


            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo("[SE]:LuaEngine.CreateNewMapScriptDir(): Error creating map package!!:\n" + e.Message + "\npath:\n" + mappath);
            }
        }


        public IEnumerator<YieldInstruction> CallScriptFunction(LuaFunction luaFun)
        {
            //Create a lua iterator function for the lua coroutine
            LuaFunction iter = CreateCoroutineIterator(luaFun);

            //Then call it until it returns null!
            object[] allres = iter.Call();
            object res = allres.First();
            while (res != null)
            {
                if (res.GetType() == typeof(Coroutine)) //This handles waiting on coroutines
                    yield return CoroutineManager.Instance.StartCoroutine(res as Coroutine, false);
                else if (res.GetType().IsSubclassOf(typeof(YieldInstruction)))
                    yield return res as YieldInstruction;

                //Pick another yield from the lua coroutine
                allres = iter.Call();
                res = allres.First();
            }
        }

        /// <summary>
        /// Creates a wrapped coroutine that works like a lua iterator.
        /// Each call to the returned function will call resume on the wrapped coroutine, and resume from where it left off.
        /// </summary>
        /// <param name="luapath">Path to the lua function. Ex: "Mytable.ObjectInstance.luafunction"</param>
        /// <param name="arguments">Arguments to pass the function on call. Those will be wrapped into the lua iterator function.</param>
        /// <returns></returns>
        public LuaFunction CreateCoroutineIterator(string luapath, params object[] arguments)
        {
            if (!LuaEngine.Instance.DoesFunctionExists(luapath))
                return null;
            LuaFunction luafun = LuaState.GetFunction(luapath);
            if (luafun != null)
                return CreateCoroutineIterator(luafun, arguments);
            else
                return null;
        }

        /// <summary>
        /// Creates a wrapped coroutine that works like a lua iterator.
        /// Each call to the returned function will call resume on the wrapped coroutine, and resume from where it left off.
        /// </summary>
        /// <param name="luafun">LuaFunction the iterator should run!</param>
        /// <param name="arguments">Arguments to pass the function on call. Those will be wrapped into the lua iterator function.</param>
        /// <returns></returns>
        public LuaFunction CreateCoroutineIterator(LuaFunction luafun, params object[] arguments)
        {
            object iter = null;
            try
            {
                //!NOTE: I had to make this because the arguments were being passed as a single table of userdata. Which isn't practical at all.
                //!      So I made this to try to unpack up to 10 parameters for a lua function. I doubt we'll ever need that many parameters, but, at least its all there!
                if (arguments.Count() == 0)
                    iter = m_MkCoIter.Call(luafun).First();
                else if (arguments.Count() == 1)
                    iter = m_MkCoIter.Call(luafun, arguments[0]).First();
                else if (arguments.Count() == 2)
                    iter = m_MkCoIter.Call(luafun, arguments[0], arguments[1]).First();
                else if (arguments.Count() == 3)
                    iter = m_MkCoIter.Call(luafun, arguments[0], arguments[1], arguments[2]).First();
                else if (arguments.Count() == 4)
                    iter = m_MkCoIter.Call(luafun, arguments[0], arguments[1], arguments[2], arguments[3]).First();
                else if (arguments.Count() == 5)
                    iter = m_MkCoIter.Call(luafun, arguments[0], arguments[1], arguments[2], arguments[3], arguments[4]).First();
                else if (arguments.Count() == 6)
                    iter = m_MkCoIter.Call(luafun, arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5]).First();
                else if (arguments.Count() == 7)
                    iter = m_MkCoIter.Call(luafun, arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6]).First();
                else if (arguments.Count() == 8)
                    iter = m_MkCoIter.Call(luafun, arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7]).First();
                else if (arguments.Count() == 9)
                    iter = m_MkCoIter.Call(luafun, arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8]).First();
                else if (arguments.Count() == 10)
                    iter = m_MkCoIter.Call(luafun, arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7], arguments[8], arguments[9]).First();
                else if (arguments.Count() > 10)
                {
                    throw new OverflowException("Function has too many paramters!!!");
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo("LuaEngine.CreateCoroutineIterator(): Failed to create coroutine : " + e.Message);
            }
            return iter as LuaFunction;
        }


        /// <summary>
        /// Checks if a Lua function exists in the current state.
        /// </summary>
        /// <param name="luapath"></param>
        /// <returns></returns>
        public bool DoesFunctionExists(string luapath)
        {
            try
            {
                if (String.IsNullOrEmpty(luapath))
                {
                    DiagManager.Instance.LogInfo("[SE]:LuaEngine.DoesFunctionExists(): Empty function path!");
                    return false;
                }

                string[] splitted = luapath.Split('.');
                string curp = "";
                foreach (string s in splitted )
                {
                    curp += s;
                    if (LuaState[curp] == null)
                        return false;
                    curp += ".";
                }

                return true;
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo("[SE]:LuaEngine.DoesFunctionExists(): Error looking for function!: " + luapath + "\npath:\n" + e.Message);
                return false;
            }
        }


        /// <summary>
        /// Makes a .net Action to be used in lua
        /// </summary>
        /// <param name="fun"></param>
        /// <returns></returns>
        public Action MakeLuaAction( LuaFunction fun, params object[] param )
        {
            return new Action( ()=>{ fun.Call(param); } );
        }


        private Type[] parseTypeArgs(LuaTable table)
        {
            List<Type> types = new List<Type>();
            foreach (object val in table.Values)
            {
                if (val is ProxyType)
                    types.Add(((ProxyType)val).UnderlyingSystemType);
                else if (val is LuaTable)
                {
                    throw new NotImplementedException("I haven't decided now to do nested generic types yet...");
                    //probably make a recursive call to parseTypeArgs
                }
            }
            return types.ToArray();
        }

        public object MakeGenericType(ProxyType class_type, LuaTable class_arg_table, LuaTable arg_table)
        {
            try
            {
                Type class_to_make = class_type.UnderlyingSystemType;

                Type[] class_args = parseTypeArgs(class_arg_table);
                List<object> inst_args = new List<object>();
                foreach (object val in arg_table.Values)
                    inst_args.Add(val);

                return makeGenericType(class_to_make, class_args, inst_args.ToArray());
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo("[SE]:LuaEngine.MakeGenericType(): Error creating type: " + class_type.ToString() + "\npath:\n" + e.Message);
                return null;
            }
        }

        private object makeGenericType(Type class_type, Type[] class_args, object[] args)
        {
            Type[] needed_args = class_type.GetGenericArguments();
            if (needed_args.Length != class_args.Length)
                throw new ArgumentException("Argument types not equal to needed amount.");

            Type filled_type = class_type.MakeGenericType(class_args);

            if (args.Length > 0)
                return Activator.CreateInstance(filled_type, args);
            else
                return Activator.CreateInstance(filled_type);
        }

        public dynamic LuaCast(object val, object t)
        {
            Type destTy;
            if (t.GetType().IsEquivalentTo(typeof(ProxyType)))
            {
                //Type orig = val.GetType();
                //var castedorig = Convert.ChangeType(val, orig);
                destTy = ((ProxyType)t).UnderlyingSystemType;
            }
            else if (t.GetType().IsEquivalentTo(typeof(Type)))
                destTy = (Type)t;
            else
                destTy = t.GetType();
            
            if (destTy.IsEnum)
                return Enum.ToObject(destTy, val);
            else
                return Convert.ChangeType(val, destTy);
        }

        public dynamic EnumToNumeric(object val)
        {
            Type underlying = Enum.GetUnderlyingType(val.GetType());
            return Convert.ChangeType(val, underlying);
        }

        public Type TypeOf(object v)
        {
            if (v.GetType().IsEquivalentTo(typeof(ProxyType)))
            {
                return ((ProxyType)v).UnderlyingSystemType;
            }
            else if (v.GetType() == typeof(ProxyType))
            {
                return ((ProxyType)v).UnderlyingSystemType;
            }
            else
                return v.GetType();
        }

        public string DumpStack()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //int level = 0;
            //KeraLua.LuaDebug info = new KeraLua.LuaDebug();
            //while (LuaState.GetStack(level, ref info) != 0)
            //{
            //    LuaState.GetInfo("nSl", ref info);
            //    string name = "<unknown>";
            //    if (!string.IsNullOrEmpty(info.Name))
            //        name = info.Name;

            //    sb.AppendFormat("[{0}] {1}:{2} -- {3} [{4}]\n",
            //        level, info.ShortSource, info.CurrentLine,
            //        name, info.NameWhat);
            //    level++;
            //}

            sb.Append(LuaState.GetDebugTraceback() + "\n");
            return sb.ToString();
        }

        /// <summary>
        /// Utility function for returning a dummy yield through the lua layer.
        /// </summary>
        /// <returns></returns>
        public static IEnumerator<YieldInstruction> _DummyWait()
        {
            yield break;
        }


        //
        // Common Casts
        //
        public GroundChar CastToGroundChar(GroundEntity o)
        {
            return (GroundChar)o;
        }

        public GroundAIUser CastToGroundAIUser(GroundEntity o)
        {
            return (GroundAIUser)o;
        }

        public GroundObject CastToGroundObject(GroundEntity o)
        {
            return (GroundObject)o;
        }

        public BaseTaskUser CastToBaseTaskUser(object o)
        {
            return (BaseTaskUser)o;
        }



        /// <summary>
        /// Call this when DataManager is being initialized
        /// </summary>
        public void OnDataLoad()
        {
            m_scrsvc.Publish(EServiceEvents.Init.ToString());
        }

        /// <summary>
        /// Call this when DataManager is being de-initialized
        /// </summary>
        public void OnDataUnload()
        {
            m_scrsvc.Publish(EServiceEvents.Deinit.ToString());
        }

        /// <summary>
        /// Call this when GraphicsManager is being loaded.
        /// </summary>
        public void OnGraphicsLoad()
        {
            m_scrsvc.Publish(EServiceEvents.GraphicsLoad.ToString());
        }

        /// <summary>
        /// Call this when GraphicsManager is being unloaded.
        /// </summary>
        public void OnGraphicsUnload()
        {
            //Do stuff..
            DiagManager.Instance.LogInfo("LuaEngine.OnGraphicsUnload()..");
            m_scrsvc.Publish(EServiceEvents.GraphicsUnload.ToString());
        }

        public void OnNewGame()
        {
            m_scrsvc.Publish(EServiceEvents.NewGame.ToString());
        }

        /// <summary>
        /// Called when the game mode switches to GroundMode!
        /// </summary>
        public void OnGroundModeBegin()
        {
            m_scrsvc.Publish(EServiceEvents.GroundModeBegin.ToString());
        }

        /// <summary>
        /// Called when the game mode switches to another mode from ground mode!
        /// </summary>
        public void OnGroundModeEnd()
        {
            m_scrsvc.Publish(EServiceEvents.GroundModeEnd.ToString());
        }


        public void OnGroundMapInit(string mapname, GroundMap map)
        {
            m_scriptUI.Reset();
            DiagManager.Instance.LogInfo("LuaEngine.OnGroundMapInit()..");
            m_scrsvc.Publish(EServiceEvents.GroundMapInit.ToString(), mapname, map);

        }

        /// <summary>
        /// #TODO: Call this when a ground map is entered!
        /// </summary>
        public void OnGroundMapEnter(string mapname, GroundMap mapobj)
        {
            //Do stuff..
            DiagManager.Instance.LogInfo("LuaEngine.OnGroundMapEnter()..");
            m_scrsvc.Publish(EServiceEvents.GroundMapEnter.ToString(), mapname);
        }

        /// <summary>
        /// #TODO: Call this when a ground map is exited!
        /// </summary>
        public void OnGroundMapExit(string mapname, GroundMap mapobj)
        {
            //Do stuff..
            DiagManager.Instance.LogInfo("LuaEngine.OnGroundMapExit()..");
            m_scrsvc.Publish(EServiceEvents.GroundMapExit.ToString(), mapname, mapobj);
        }

        /// <summary>
        /// Called when the game switches to DungeonMode
        /// </summary>
        public void OnDungeonModeBegin()
        {
            m_scrsvc.Publish(EServiceEvents.DungeonModeBegin.ToString());
        }

        /// <summary>
        /// Called when the game switches to another mode from DungeonMode
        /// </summary>
        public void OnDungeonModeEnd()
        {
            m_scrsvc.Publish(EServiceEvents.DungeonModeEnd.ToString());
        }

        /// <summary>
        /// #TODO: Call this when a dungeon map starts!
        /// </summary>
        public void OnDungeonMapInit(string mapname, Map mapobj)
        {
            //Stop lua execution, and save stack or something?
            DiagManager.Instance.LogInfo("LuaEngine.OnDungeonMapInit()..");
            m_scrsvc.Publish(EServiceEvents.DungeonMapInit.ToString());
        }

        /// <summary>
        /// When entering a new dungeon floor this is called
        /// </summary>
        public void OnDungeonMapEnter(string mapname, Map mapobj)
        {
            DiagManager.Instance.LogInfo("LuaEngine.OnDungeonMapEnter()..");
            m_scrsvc.Publish(EServiceEvents.DungeonFloorEnter.ToString(), mapname);
        }

        /// <summary>
        /// When leaving a dungeon floor this is called.
        /// </summary>
        /// <param name="floor">Floor on which was just exited</param>
        public void OnDungeonMapExit(string mapname, Map mapobj)
        {
            m_scrsvc.Publish(EServiceEvents.DungeonFloorExit.ToString());
        }

        public void OnZoneInit()
        {
            m_scrsvc.Publish(EServiceEvents.ZoneInit.ToString());
        }

        public void OnZoneSegmentStart()
        {
            m_scrsvc.Publish(EServiceEvents.DungeonSegmentStart.ToString());
        }

        public void OnZoneSegmentEnd()
        {
            m_scrsvc.Publish(EServiceEvents.DungeonSegmentEnd.ToString());
        }

        /// <summary>
        /// Called when an entity activates another.
        /// </summary>
        /// <param name="activator">The entity that activates the target</param>
        /// <param name="target">The entity that is being activated</param>
        /// <param name="info">The context of the activation</param>
        public void OnActivate(GroundEntity activator, GroundEntity target )
        {
            //CallLuaMemberFun(MainScriptInstanceName, "OnActivate", activator, target, info);
            m_scrsvc.Publish(EServiceEvents.GroundEntityInteract.ToString(), activator, target);
        }

        /// <summary>
        /// Call this so the LuaEngine calls the main script's update method.
        /// </summary>
        /// <param name="gametime">Time elapsed since launch in game time.</param>
        /// <param name="frametime">Value between 0 and 1 indicating the current time fraction of a frame the call is taking place at.</param>
        public void Update(GameTime gametime)
        {
            m_curtime = gametime;
            if (m_nextUpdate.Ticks < 0)
                return;

            if (m_nextUpdate < gametime.TotalGameTime)
            {
                //The lua engine handles processing things as coroutines!
                m_scrsvc.Publish(EServiceEvents.Update.ToString(), gametime);


                m_nextUpdate = gametime.TotalGameTime + TimeSpan.FromMilliseconds(20); //Schedule next update
            }
        }
    }

    [Serializable]
    public class LuaTableContainer
    {
        /// <summary>
        /// We're using a List<object[]> instead of a dictionary because some quirk in json serialization causes integer keys to be written as strings.
        /// this data type is the next best thing in terms of internal storage
        /// </summary>
        [JsonConverter(typeof(Dev.LuaTableContainerDictConverter))]
        public List<object[]> Table;

        public LuaTableContainer() { Table = new List<object[]>(); }
        public LuaTableContainer(List<object[]> table) { Table = table; }
    }

}
