using System;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueElements;
using NLua;
using RogueEssence.Script;
using System.Linq;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public abstract class MobSpawnExtra
    {
        public abstract MobSpawnExtra Copy();
        public abstract void ApplyFeature(IMobSpawnMap map, Character newChar);
    }


    /// <summary>
    /// Spawns the mob with a status problem.
    /// </summary>
    [Serializable]
    public class MobSpawnStatus : MobSpawnExtra
    {
        /// <summary>
        /// The possible statuses.  Picks one.
        /// </summary>
        public SpawnList<StatusEffect> Statuses;

        public MobSpawnStatus()
        {
            Statuses = new SpawnList<StatusEffect>();
        }
        public MobSpawnStatus(MobSpawnStatus other) : this()
        {
            for (int ii = 0; ii < other.Statuses.Count; ii++)
                Statuses.Add(other.Statuses.GetSpawn(ii).Clone(), other.Statuses.GetSpawnRate(ii));
        }
        public override MobSpawnExtra Copy() { return new MobSpawnStatus(this); }

        public override void ApplyFeature(IMobSpawnMap map, Character newChar)
        {
            StatusEffect status = Statuses.Pick(map.Rand).Clone();//Clone Use Case; convert to Instantiate?
            status.LoadFromData();
            StatusData entry = (StatusData)status.GetData();
            if (!entry.Targeted)//no targeted statuses allowed
            {
                //need to also add the additional status states
                newChar.StatusEffects.Add(status.ID, status);
            }
        }

        public override string ToString()
        {
            if (Statuses.Count != 1)
                return string.Format("{0}[{1}]", this.GetType().GetFormattedTypeName(), Statuses.Count.ToString());
            else
            {
                EntrySummary summary = DataManager.Instance.DataIndices[DataManager.DataType.Status].Get(Statuses.GetSpawn(0).ID);
                return string.Format("{0}: {1}", this.GetType().GetFormattedTypeName(), summary.Name.ToLocal());
            }
        }
    }

    /// <summary>
    /// Runs a script to modify the spawning mob.
    /// </summary>
    [Serializable]
    public class MobSpawnScript : MobSpawnExtra
    {
        [Dev.Sanitize(0)]
        public string Script;
        [Dev.Multiline(0)]
        public string ArgTable;


        public MobSpawnScript() { Script = ""; ArgTable = ""; }
        public MobSpawnScript(string script)  { Script = script; ArgTable = ""; }
        public MobSpawnScript(string script, string argTable)  { Script = script; ArgTable = argTable; }
        public MobSpawnScript(MobSpawnScript other) : this()
        {
            Script = other.Script;
            ArgTable = other.ArgTable;
        }
        public override MobSpawnExtra Copy() { return new MobSpawnScript(this); }

        public override void ApplyFeature(IMobSpawnMap map, Character newChar)
        {
            LuaTable args = LuaEngine.Instance.RunString("return " + ArgTable).First() as LuaTable;
            object[] parameters = new object[] { map, newChar, args };
            string name = LuaEngine.EVENT_SPAWN_NAME + "." + Script;

            LuaEngine.Instance.CallLuaFunctions(name, parameters);
        }

        public override string ToString()
        {
            string name = LuaEngine.EVENT_SPAWN_NAME + "." + Script;
            return string.Format("{0}: {1}", this.GetType().GetFormattedTypeName(), name);
        }
    }

}
