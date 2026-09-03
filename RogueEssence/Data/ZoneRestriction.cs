using System;
using System.Collections.Generic;
using System.Linq;
using NLua;
using RogueEssence.Dungeon;
using RogueEssence.Script;

namespace RogueEssence.Data
{
    public abstract class ZoneRestriction : GameEvent
    {
        public abstract bool ApplyBeforeAutosave();
        public abstract string GetDisplayName(string index, ZoneEntrySummary zoneEntry, bool showRestrict);
        public abstract IEnumerator<YieldInstruction> Apply(string zoneID, ZoneEntrySummary zoneSummary, bool noRestrict);
    }

    /// <summary>
    /// A zone restriction entirely governed by lua scripts
    /// </summary>
    [Serializable]
    public class CustomRestriction : ZoneRestriction
    {
        public bool BeforeAutosave;
        [Dev.Sanitize(0)]
        public string ZoneRuleNameScript;
        [Dev.Multiline(0)]
        public string ZoneRuleNameArgs;
        [Dev.Sanitize(0)]
        public string ZoneRuleEffectScript;
        [Dev.Multiline(0)]
        public string ZoneRuleEffectArgs;

        public CustomRestriction() { ZoneRuleNameScript = ""; ZoneRuleNameArgs = "{}"; ZoneRuleEffectScript = ""; ZoneRuleEffectArgs = "{}"; }
        public CustomRestriction(bool beforeAutosave, string nameScript, string effectScript) {
            BeforeAutosave = beforeAutosave; ZoneRuleNameScript = nameScript; ZoneRuleNameArgs = "{}"; ZoneRuleEffectScript = effectScript; ZoneRuleEffectArgs = "{}";
        }
        public CustomRestriction(bool beforeAutosave, string nameScript, string nameArgs, string effectScript, string effectArgs) {
            BeforeAutosave = beforeAutosave; ZoneRuleNameScript = nameScript; ZoneRuleNameArgs = nameArgs; ZoneRuleEffectScript = effectScript; ZoneRuleEffectArgs = effectArgs;
        }
        public override GameEvent Clone()
        {
            return new CustomRestriction(BeforeAutosave, ZoneRuleNameScript, ZoneRuleNameArgs, ZoneRuleEffectScript, ZoneRuleEffectArgs);
        }

        public override bool ApplyBeforeAutosave() => BeforeAutosave;
        public override string GetDisplayName(string index, ZoneEntrySummary zoneEntry, bool showRestrict) {
            LuaTable args = LuaEngine.Instance.RunString("return " + ZoneRuleNameArgs).First() as LuaTable;
            string name = LuaEngine.ZONE_RULE_NAME_NAME + "." + ZoneRuleNameScript;
            LuaFunction lua_func = LuaEngine.Instance.LuaState.GetFunction(name);
            if (lua_func != null)
            {
                object[] parameters = new object[] { index, zoneEntry, showRestrict, args };
                object result = lua_func.Call(parameters).First();
                if(result.ToString() == null)
                    return "";
                return result.ToString();
            }
            return "";
        }

        public override IEnumerator<YieldInstruction> Apply(string zoneID, ZoneEntrySummary zoneSummary, bool noRestrict)
        {
            LuaTable args = LuaEngine.Instance.RunString("return " + ZoneRuleEffectArgs).First() as LuaTable;
            object[] parameters = new object[] { zoneID, zoneSummary, noRestrict, args };
            string name = LuaEngine.ZONE_RULE_SCRIPT_NAME + "." + ZoneRuleEffectScript;
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(name, parameters);

            yield return CoroutineManager.Instance.StartCoroutine(ScriptEvent.ApplyFunc(name, func_iter));
        }

    }
}