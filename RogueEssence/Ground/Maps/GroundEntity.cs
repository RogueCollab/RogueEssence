using Microsoft.Xna.Framework;
using RogueEssence.Script;
using RogueElements;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RogueEssence.Dungeon;

namespace RogueEssence.Ground
{
    /// <summary>
    /// Parent class meant to be used to access things common to all ground entities.
    /// </summary>
    [Serializable]
    public abstract class GroundEntity
    {

        /// <summary>
        /// The kind of trigger events available to entity.
        ///
        /// </summary>
        public enum EEntityTriggerTypes
        {
            None = 0,
            Action= 1,
            Touch = 2,
        }

        /// <summary>
        /// All sub-classes of the GroundEntity class have an enum entry so that their type can be obtained
        /// via the GetEntityType() method.
        /// </summary>
        public enum EEntTypes
        {
            Invalid = -1,
            Object = 0,
            Character = 1,
            Marker = 2,
            Spawner = 3,
            Count,
        }

//===========================================
// Common fields and properties
//===========================================
        public Rect Collider = new Rect();
        public string  EntName     { get; set; }
        public virtual Dir8    Direction   { get; set; }
        public virtual Loc     MapLoc      { get { return Collider.Start; } set { Collider.Start = value; } } //!FIXME: Not sure what to go with between maploc and position, since both are used, and maploc is kinda confusing
        public virtual Loc     Position    { get { return Collider.Start; } set { Collider.Start = value; } }
        public virtual Rect    Bounds      { get { return Collider; }       set { Collider = value; } }
        public virtual int     Height      { get { return Bounds.Height; } }
        public virtual int     Width       { get { return Bounds.Width; } }
        public virtual int     X           { get { return Bounds.X; } }
        public virtual int     Y           { get { return Bounds.Y; } }

        public virtual void SetMapLoc(Loc loc)
        {
            MapLoc = loc;
        }

        /// <summary>
        ///
        /// </summary>
        protected EEntityTriggerTypes triggerType;
        public virtual EEntityTriggerTypes TriggerType { get { return triggerType; } set {triggerType = value; } }

        //Moved script events to their own structure, to avoid duplicates and other issues
        [NonSerialized]
        protected Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent> scriptEvents;

        /// <summary>
        /// When this property is false, all processing of this entity is disabled.
        /// So it essentially becomes invisible and inactive on the map.
        /// Its mainly meant to be used with the mape editor so entities
        /// can be placed and activated only when needed by the map scenario.
        /// </summary>
        public virtual bool EntEnabled { get; set; }

        /// <summary>
        /// Returns an enum value indicating the actual type of this entity.
        /// </summary>
        /// <returns></returns>
        public abstract EEntTypes GetEntityType();

        /// <summary>
        /// Default base constructor
        /// </summary>
        public GroundEntity()
        {
            scriptEvents = new Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent>();
            EntEnabled = true;
            DevEntitySelected = false;
        }

        protected GroundEntity(GroundEntity other)
        {
            scriptEvents = new Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent>();
            foreach (LuaEngine.EEntLuaEventTypes ev in other.scriptEvents.Keys)
                scriptEvents.Add(ev, (ScriptEvent)other.scriptEvents[ev].Clone());
            EntEnabled = other.EntEnabled;
            Collider = other.Collider;
            EntName = other.EntName;
            Direction = other.Direction;
            triggerType = other.triggerType;
        }
        public abstract GroundEntity Clone();

        //==================================================
        // Map editor stuff
        //==================================================

        /// <summary>
        /// Whether the entity should be drawn in highlights
        /// </summary>
        [NonSerialized]
        public bool DevEntitySelected;

        /// <summary>
        /// The color of the boxes and etc around the entity
        /// </summary>
        public abstract Color DevEntColoring { get; }

        /// <summary>
        /// When the entity is selected by the map editor, this method is called
        /// </summary>
        public virtual void DevOnEntitySelected()
        {
            DevEntitySelected = true;
        }

        /// <summary>
        /// When the entity is de selected by the map editor, this method is called
        /// </summary>
        public virtual void DevOnEntityUnSelected()
        {
            DevEntitySelected = false;
        }

        /// <summary>
        /// Whether an entity has graphics that can be drawn. Aka, a marker has no graphics, but a character has.
        /// An object that's just a trigger has no graphics, but an object with a sprite attached has graphics.
        /// </summary>
        /// <returns></returns>
        public virtual bool DevHasGraphics()
        {
            return false;
        }

//==================================================
// Events stuff
//==================================================

        /// <summary>
        /// Function to be run when an entity is removed. Meant to cleanup events data.
        /// </summary>
        public virtual void DoCleanup()
        {
            foreach (var entry in scriptEvents)
                entry.Value.DoCleanup();
            scriptEvents.Clear();
        }
        
        /// <summary>
        /// Returns the event with the same name as the string eventname.
        /// Or return null if the event can't be found.
        /// </summary>
        /// <param name="eventname"></param>
        /// <returns></returns>
        public virtual bool HasScriptEvent(LuaEngine.EEntLuaEventTypes ev)
        {
            return scriptEvents.ContainsKey(ev);
        }

        /// <summary>
        /// This reset events  after they are unserialized for example
        /// </summary>
        public virtual void ReloadEvents()
        {
            scriptEvents = new Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent>();
            foreach (LuaEngine.EEntLuaEventTypes ev in LuaEngine.IterateLuaEntityEvents())
            {
                if (!IsEventSupported(ev))
                    continue;
                string callback = LuaEngine.MakeLuaEntityCallbackName(EntName, ev);
                if (!LuaEngine.Instance.DoesFunctionExists(callback))
                    continue;
                DiagManager.Instance.LogInfo(String.Format("GroundEntity.ReloadEvents(): Added event {0} to entity {1}!", ev.ToString(), EntName));
                scriptEvents[ev] = new ScriptEvent(callback);
            }
        }

        /// <summary>
        /// This reset events after a script reload. the execution will break, unlike ReloadEvents
        /// </summary>
        public virtual void LuaEngineReload() { }

        /// <summary>
        /// Run a lua event by type
        /// </summary>
        /// <param name="ev"></param>
        public virtual IEnumerator<YieldInstruction> RunEvent(LuaEngine.EEntLuaEventTypes ev, TriggerResult result)
        {
            yield return CoroutineManager.Instance.StartCoroutine(RunEvent(ev, result, this));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual IEnumerator<YieldInstruction> RunEvent(LuaEngine.EEntLuaEventTypes ev, TriggerResult result, params object[] arguments)
        {
            if (scriptEvents.ContainsKey(ev))
            {
                //Since ScriptEvent.Apply takes a single variadic table, we have to concatenate our current variadic argument table
                // with the extra parameter we want to pass. Otherwise "parameters" will be passed as a table instead of its
                // individual elements, and things will crash left and right.
                List<object> partopass = new List<object>();
                partopass.Add(this);
                partopass.AddRange(arguments);
                yield return CoroutineManager.Instance.StartCoroutine(scriptEvents[ev].Apply(partopass.ToArray()));
                result.Success = true;
            }
            else
                yield break;
        }

        /// <summary>
        /// Used to check if an entity supports a lua event of the given kind.
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        public virtual bool IsEventSupported(LuaEngine.EEntLuaEventTypes ev)
        {
            return false;
        }

        public virtual void OnSerializeMap(GroundMap map)
        {
            //does nothing by default
        }

        /// <summary>
        /// This is called when the map owning the entity is deserialized.
        /// Its meant to help entities re-subscribe their events to the proper delegates,
        /// and load their assigned lua function.
        /// </summary>
        public virtual void OnDeserializeMap(GroundMap map)
        {
            //does nothing by default
        }

        /// <summary>
        /// This should be called when the map is initialized, either after loading a save, or when loading a map for the first time.
        /// </summary>
        public virtual void OnMapInit()
        {
            //Reload events
            ReloadEvents();
        }

        /// <summary>
        /// Setup the current trigger method for this entity.
        /// The trigger specifies how to interact with an entity.
        /// </summary>
        /// <param name="triggerty"></param>
        public virtual void SetTriggerType(EEntityTriggerTypes triggerty)
        {
            TriggerType = triggerty;
        }

        /// <summary>
        /// Returns the type of trigger method configured for this entity.
        /// The trigger specifies how to interact with an entity.
        /// </summary>
        /// <returns></returns>
        public virtual EEntityTriggerTypes GetTriggerType()
        {
            return TriggerType;
        }

        /// <summary>
        /// Returns a list of all the lua callbacks this entity is subscribed to currently.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LuaEngine.EEntLuaEventTypes> ActiveLuaCallbacks()
        {
            List<LuaEngine.EEntLuaEventTypes> callbacks = new List<LuaEngine.EEntLuaEventTypes>();
            foreach (LuaEngine.EEntLuaEventTypes ev in LuaEngine.IterateLuaEntityEvents())
            {
                if (HasScriptEvent(ev))
                    callbacks.Add(ev);
            }
            return callbacks;
        }

        //!#NOTE: Ideally, when/if we have more a single interaction per object, we could add extra parameters to this.
        /// <summary>
        /// When something tries to interact with this entity, this method is called.
        /// </summary>
        /// <param name="activator"></param>
        public virtual IEnumerator<YieldInstruction> Interact(GroundEntity activator, TriggerResult result)
        {
            //default does nothing
            yield break;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            scriptEvents = new Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent>();
        }
    }
}
