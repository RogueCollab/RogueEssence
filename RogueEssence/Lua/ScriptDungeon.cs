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
        /// <summary>
        /// Returns the floor number of the current dungeon
        /// </summary>
        /// <returns></returns>
        public int DungeonCurrentFloor()
        {
            return ZoneManager.Instance.CurrentMapID.ID;
        }

        /// <summary>
        /// Returns the internal name for the current dungeon
        /// </summary>
        /// <returns></returns>
        public string DungeonAssetName()
        {
            //if (ZoneManager.Instance.CurrentZone != null)
            //    return ZoneManager.Instance.CurrentZone.AssetName;
            //else
            //    return "NULL";
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the localized name of the current dungeon.
        /// </summary>
        /// <returns></returns>
        public string DungeonLocaleName()
        {
            if (ZoneManager.Instance.CurrentZone != null)
                return ZoneManager.Instance.CurrentZone.Name.ToLocal();
            else
                return "NULL";
        }
    }
}
