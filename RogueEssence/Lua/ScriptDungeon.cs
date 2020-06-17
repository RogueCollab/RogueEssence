using System;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueElements;

using System.Linq;

namespace RogueEssence.Script
{
    class ScriptDungeon : ILuaEngineComponent
    {
        //===================================
        //
        //===================================
        public override void SetupLuaFunctions(LuaEngine state)
        {
            //TODO
        }

        /// <summary>
        /// Makes a character turn to face another
        /// </summary>
        /// <param name="curch"></param>
        /// <param name="turnto"></param>
        public void CharTurnToChar(Character curch, Character turnto)
        {
            if (curch == null || turnto == null)
                return;
            curch.CharDir = turnto.CharDir.Reverse();
        }

        //===================================
        //  Dungeon Stats
        //===================================
        public GameProgress.ResultType LastDungeonResult()
        {
            return DataManager.Instance.Save.Outcome;
        }
    }
}
