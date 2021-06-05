using NLua;
using RogueEssence.Script;
using System;

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
        public string Script;

        public SkillChangeScriptEvent() { Script = ""; }
        protected SkillChangeScriptEvent(SkillChangeScriptEvent other)
        {
            Script = other.Script;
        }
        public override GameEvent Clone() { return new SkillChangeScriptEvent(this); }

        public override void Apply(GameEventOwner owner, Character character, int[] skillIndices)
        {
            LuaFunction luafun = LuaEngine.Instance.LuaState.GetFunction(Script);

            if (luafun != null)
                luafun.Call(new object[] { owner, character, skillIndices });
        }
    }
}
