
using System;
using System.Linq;
using NLua;
using RogueEssence.Data;
using RogueEssence.LevelGen;
using RogueEssence.Script;

namespace RogueEssence.Dev
{
    public class MobSpawnStatusEditor : Editor<MobSpawnStatus>
    {
        public override string GetString(MobSpawnStatus obj, Type type, object[] attributes)
        {
            if (obj.Statuses.Count != 1)
                return string.Format("Status: [{0}]", obj.Statuses.Count.ToString());
            else
            {
                EntrySummary summary = DataManager.Instance.DataIndices[DataManager.DataType.Status].Get(obj.Statuses.GetSpawn(0).ID);
                return string.Format("Status: {0}", summary.Name.ToLocal());
            }
        }
        public override string GetTypeString()
        {
            return "Status";
        }
    }
    public class MobSpawnScriptEditor : Editor<MobSpawnScript>
    {
        public override string GetString(MobSpawnScript obj, Type type, object[] attributes)
        {
            LuaTable tbl = LuaEngine.Instance.RunString("return " + obj.ArgTable).First() as LuaTable;
            if (tbl is not null && tbl.Keys.Count > 0)
                return string.Format("Script: {0} [{1}]", obj.Script.ToString(), tbl.Keys.Count.ToString());
            else
                return string.Format("Script: {0}", obj.Script.ToString());
        }
        public override string GetTypeString()
        {
            return "Spawn Script";
        }
    }
}