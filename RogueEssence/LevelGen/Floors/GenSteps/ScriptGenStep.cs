
using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.LevelGen;
using RogueEssence.Dev;
using RogueEssence.Script;
using NLua;
using System.Linq;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Calls a lua script function that acts as the map gen step.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ScriptGenStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        /// <summary>
        /// The name of the script.  The resulting function will be FLOOR_GEN_SCRIPT.[Script Name]
        /// </summary>
        [Dev.Sanitize(0)]
        public string Script;

        /// <summary>
        /// Additional arguments that will be passed into the script.
        /// </summary>
        [Multiline(0)]
        public string ArgTable;

        public ScriptGenStep() { Script = ""; ArgTable = "{}"; }
        public ScriptGenStep(string script) { Script = script; ArgTable = "{}"; }

        public override void Apply(T map)
        {
            LuaFunction luafun = LuaEngine.Instance.LuaState.GetFunction("FLOOR_GEN_SCRIPT." + Script);

            if (luafun != null)
            {
                LuaTable args = LuaEngine.Instance.RunString("return " + ArgTable).First() as LuaTable;
                luafun.Call(new object[] { map, args });
            }
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", this.GetType().Name, Script);
        }
    }
}
