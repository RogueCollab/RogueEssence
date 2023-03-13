using Avalonia;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using RogueEssence.Script;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public string Definition
        {
            get
            {
                return LuaEngine.MakeEntLuaEventDef(ZoneManager.Instance.CurrentGround.AssetName, baseEnt.EntName, EventType);
            }
            set
            {

            }
        }

        public async void mnuCopyFun_Click()
        {
            await Application.Current.Clipboard.SetTextAsync(Definition);
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
