using NLua;
using RogueEssence.Script;
using System;
using System.Linq;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class SkillChangeEvent : GameEvent
    {
        public abstract void Apply(GameEventOwner owner, Character character, int[] skillIndices);
    }

    [Serializable]
    public class SkillChangeScriptEvent : SkillChangeEvent
    {
        [Dev.Sanitize(0)]
        public string Script;
        [Dev.Multiline(0)]
        public string ArgTable;

        public SkillChangeScriptEvent() { Script = ""; ArgTable = "{}"; }
        public SkillChangeScriptEvent(string script) { Script = script; ArgTable = "{}"; }
        public SkillChangeScriptEvent(string script, string argTable) { Script = script; ArgTable = argTable; }
        protected SkillChangeScriptEvent(SkillChangeScriptEvent other)
        {
            Script = other.Script;
            ArgTable = other.ArgTable;
        }
        public override GameEvent Clone() { return new SkillChangeScriptEvent(this); }

        public override void Apply(GameEventOwner owner, Character character, int[] skillIndices)
        {
            LuaFunction luafun = LuaEngine.Instance.LuaState.GetFunction("SKILL_CHANGE_SCRIPT." + Script);

            if (luafun != null)
            {
                LuaTable args = LuaEngine.Instance.RunString("return " + ArgTable).First() as LuaTable;
                luafun.Call(new object[] { owner, character, skillIndices, args });
            }
        }
    }
}
