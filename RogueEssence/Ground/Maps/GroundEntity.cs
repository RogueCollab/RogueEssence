using Microsoft.Xna.Framework;
using RogueEssence.Script;
using RogueElements;
using System;
using System.Collections.Generic;

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
            EntEnabled = true;
            DevEntitySelected = false;
        }

        protected GroundEntity(GroundEntity other)
        {
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
            //Default does nothing
        }

        /// <summary>
        /// Method to be implemented by the child classes that accept script events.
        /// This allows to set the default callbacks for each types for the entity.
        /// The entity will decide whether the callback is compatible with itself, or will set an equivalent callback instead.
        /// It also ignores adding duplicate callbacks and simply does nothing when an existing callback is added!
        /// </summary>
        /// <param name="ev"></param>
        public virtual void AddScriptEvent(LuaEngine.EEntLuaEventTypes ev)
        {
            //Default does nothing
        }

        /// <summary>
        /// Returns the event with the same name as the string eventname.
        /// Or return null if the event can't be found.
        /// </summary>
        /// <param name="eventname"></param>
        /// <returns></returns>
        public virtual bool HasScriptEvent(LuaEngine.EEntLuaEventTypes ev)
        {
            return false;
        }

        /// <summary>
        /// Synchronizes the script call nams
        /// </summary>
        /// <param name="ev"></param>
        public virtual void SyncScriptEvents()
        {

        }

        /// <summary>
        /// This reset events  after they are unserialized for example
        /// </summary>
        public virtual void ReloadEvents() { }

        /// <summary>
        /// This reset events after a script reload. the execution will break, unlike ReloadEvents
        /// </summary>
        public virtual void LuaEngineReload() { }

        /// <summary>
        /// Method to be implemented by child classes that make use of script events.
        /// This will remove the default lua callback specified from the entity if possible.
        /// If its incompatible, or if the callback is not present nothing will happen.
        /// </summary>
        /// <param name="ev"></param>
        public virtual void RemoveScriptEvent(LuaEngine.EEntLuaEventTypes ev)
        {
            //Default does nothing
        }

        /// <summary>
        /// Run a lua event by type
        /// </summary>
        /// <param name="ev"></param>
        public virtual IEnumerator<YieldInstruction> RunEvent(LuaEngine.EEntLuaEventTypes ev)
        {
            //Default does nothing
            yield return CoroutineManager.Instance.StartCoroutine(RunEvent(ev, this));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual IEnumerator<YieldInstruction> RunEvent(LuaEngine.EEntLuaEventTypes ev, params object[] parameters)
        {
            //Default does nothing
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
            if(TriggerType != triggerty)
                RemoveTriggerCallback(); //Remove any leftover callback for the previous trigger type
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
        /// Removes the configured trigger script callback from the entity.
        /// Does nothing if it doesn't exists or the entity doesn't support callbacks.
        /// Does not change the trigger method type.
        /// </summary>
        public virtual void RemoveTriggerCallback()
        {
            switch (TriggerType)
            {
                case EEntityTriggerTypes.Action:
                    RemoveScriptEvent(LuaEngine.EEntLuaEventTypes.Action);
                    break;
                case EEntityTriggerTypes.Touch:
                    RemoveScriptEvent(LuaEngine.EEntLuaEventTypes.Touch);
                    break;
                case EEntityTriggerTypes.None:
                default:
                    break;
            }
        }

        /// <summary>
        /// Returns a list of all the lua callbacks this entity is subscribed to currently.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<LuaEngine.EEntLuaEventTypes> ActiveLuaCallbacks()
        {
            List<LuaEngine.EEntLuaEventTypes> callbacks = new List<LuaEngine.EEntLuaEventTypes>();
            var eventstypes = LuaEngine.IterateLuaEntityEvents();
            do
            {
                if (HasScriptEvent(eventstypes.Current))
                    callbacks.Add(eventstypes.Current);
            }
            while (eventstypes.MoveNext());
            return callbacks;
        }

        //!#NOTE: Ideally, when/if we have more a single interaction per object, we could add extra parameters to this.
        /// <summary>
        /// When something tries to interact with this entity, this method is called.
        /// </summary>
        /// <param name="activator"></param>
        public virtual IEnumerator<YieldInstruction> Interact(GroundEntity activator)
        {
            //default does nothing
            yield break;
        }


    }
}
