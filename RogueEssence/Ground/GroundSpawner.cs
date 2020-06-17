using System;
using System.Collections.Generic;
using RogueEssence.Script;
using RogueEssence.Dungeon;
using Microsoft.Xna.Framework;
using RogueElements;
using System.Runtime.Serialization;

namespace RogueEssence.Ground
{
    /// <summary>
    /// Class representing an NPC spawner on a ground map. Its meant to randomly create an NPC at runtime, and is controlled via script.
    /// </summary>
    [Serializable]
    public class GroundSpawner : GroundEntity
    {
        /// <summary>
        /// Name of the NPC that will be spawned by this spawner
        /// </summary>
        public string NPCName { get; set; }

        /// <summary>
        /// The "Character" entity from which the NPC will be generated.
        /// </summary>
        public Character NPCChar { get; set; }

        /// <summary>
        /// The NPC that was spawned by this spawner
        /// </summary>
        [NonSerialized]
        private GroundChar curNpc;
        public GroundChar CurrentNPC { get { return curNpc; } internal set { curNpc = value; } }

        /// <summary>
        /// The Callbacks that will be enabled on the spawned entity
        /// </summary>
        public HashSet<LuaEngine.EEntLuaEventTypes> EntityCallbacks;

        /// <summary>
        /// Script events available for this entity's instance.
        /// </summary>
        public Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent> ScriptEvents;


        public override EEntTypes GetEntityType()
        {
            return EEntTypes.Spawner;
        }

        /// <summary>
        /// Creates a NPC Spawner entity with the given name, NPC name and character entity.
        /// </summary>
        /// <param name="spawnername">name given to the spawner entity</param>
        /// <param name="npcname">name given to the spawned entity</param>
        /// <param name="npcchar">character entity from which the NPC will be generated from</param>
        public GroundSpawner(string spawnername, string npcname, Character npcchar )
            :base()
        {
            EntName = spawnername;
            NPCName = npcname;
            NPCChar = npcchar;
            DevEntColoring = DevEntColoring = Color.Salmon;
            Bounds = new Rect(Position.X, Position.Y, 10, 10); //Static size, so its easier to click on it!
            EntityCallbacks = new HashSet<LuaEngine.EEntLuaEventTypes>();
            ScriptEvents = new Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent>();
        }

        /// <summary>
        /// Generates the NPC and place it at the location the spawner is on the map.
        /// </summary>
        public virtual GroundChar Spawn(GroundMap currentmap)
        {
            if (!EntEnabled)
                return null;

            CurrentNPC = new GroundChar(NPCChar, Position, Direction, NPCName);

            //Setup callbacks on the spawned entity
            foreach (LuaEngine.EEntLuaEventTypes t in EntityCallbacks)
                CurrentNPC.AddScriptEvent(t);

            currentmap.AddTempChar(CurrentNPC);
            CurrentNPC.OnMapInit();

            //Run our script callback after we spawned
            CoroutineManager.Instance.StartCoroutine( RunEvent(LuaEngine.EEntLuaEventTypes.EntSpawned, CurrentNPC) );

            return CurrentNPC;
        }

        /// <summary>
        /// Add a script event to be assigned to the spawned entity.
        /// </summary>
        /// <param name="ev"></param>
        public virtual void AddSpawnedEntScriptEvent(LuaEngine.EEntLuaEventTypes ev)
        {
            EntityCallbacks.Add(ev);
        }

        /// <summary>
        /// Removes a script event from the list of events to be assigned to the spawned entity
        /// </summary>
        /// <param name="ev"></param>
        public virtual void RemoveSpawnedEntScriptEvent(LuaEngine.EEntLuaEventTypes ev)
        {
            EntityCallbacks.Remove(ev);
        }


        /// <summary>
        /// Iterates the list of entity callbacks enabled
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<LuaEngine.EEntLuaEventTypes> IterateSpawnedEntScriptEvents()
        {
            foreach (LuaEngine.EEntLuaEventTypes v in EntityCallbacks)
                yield return v;
        }



        public override void AddScriptEvent(LuaEngine.EEntLuaEventTypes ev)
        {
            if (!IsEventSupported(ev))
                return;
            ScriptEvents.Add(ev, new ScriptEvent(LuaEngine.MakeLuaEntityCallbackName(EntName, ev)));
        }

        public override void LuaEngineReload()
        {
            foreach (ScriptEvent scriptEvent in ScriptEvents.Values)
                scriptEvent.LuaEngineReload();
        }

        public override ScriptEvent FindEvent(string eventname)
        {
            foreach (var entry in ScriptEvents)
            {
                if (entry.Value.EventName().Contains(eventname))
                    return entry.Value;
            }
            return null;
        }

        public override void ReloadEvents()
        {
            foreach (var entry in ScriptEvents)
                entry.Value.ReloadEvent();
        }

        public override void DoCleanup()
        {
            foreach (var entry in ScriptEvents)
                entry.Value.DoCleanup();
            ScriptEvents.Clear();
        }


        public override void RemoveScriptEvent(LuaEngine.EEntLuaEventTypes ev)
        {
            if (ScriptEvents.ContainsKey(ev))
                ScriptEvents.Remove(ev);
        }

        public override IEnumerator<YieldInstruction> RunEvent(LuaEngine.EEntLuaEventTypes ev, params object[] parameters)
        {
            if (ScriptEvents.ContainsKey(ev))
            {
                //Since ScriptEvent.Apply takes a single variadic table, we have to concatenate our current variadic argument table
                // with the extra parameter we want to pass. Otherwise "parameters" will be passed as a table instead of its
                // individual elements, and things will crash left and right.
                List<object> partopass = new List<object>();
                partopass.Add(this);
                partopass.AddRange(parameters);
                yield return CoroutineManager.Instance.StartCoroutine(ScriptEvents[ev].Apply(partopass.ToArray()));
            }
            else
                yield break;
        }

        public override bool IsEventSupported(LuaEngine.EEntLuaEventTypes ev)
        {
            return ev == LuaEngine.EEntLuaEventTypes.EntSpawned;
        }


        [OnDeserialized]
        private void OnDeserialized(StreamingContext cntxt)
        {
            if (ScriptEvents == null)
                ScriptEvents = new Dictionary<LuaEngine.EEntLuaEventTypes, ScriptEvent>();
        }
    }
}
