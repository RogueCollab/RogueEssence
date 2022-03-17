using RogueEssence.Ground;
using RogueEssence.Script;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev.ViewModels
{
    public class EntScriptItem : ViewModelBase
    {
        public EntScriptItem() { }
        public EntScriptItem(LuaEngine.EEntLuaEventTypes eventType, GroundEntity ent)
        {
            EventType = eventType;
            baseEnt = ent;
        }
        public LuaEngine.EEntLuaEventTypes EventType { get; set; }
        public bool IsChecked
        {
            get
            {
                return baseEnt.HasScriptEvent(EventType);
            }
            set
            {

            }
        }

        private GroundEntity baseEnt;
    }

    public class SpawnScriptItem : ViewModelBase
    {
        public SpawnScriptItem() { }
        public SpawnScriptItem(LuaEngine.EEntLuaEventTypes eventType, GroundSpawner ent)
        {
            EventType = eventType;
            baseEnt = ent;
        }
        public LuaEngine.EEntLuaEventTypes EventType { get; set; }
        public bool IsChecked
        {
            get
            {
                return baseEnt.HasSpawnScriptEvent(EventType);
            }
            set
            {
                if (value)
                    baseEnt.AddSpawnedEntScriptEvent(EventType);
                else
                    baseEnt.AddSpawnedEntScriptEvent(EventType);
            }
        }

        private GroundSpawner baseEnt;
    }
}
