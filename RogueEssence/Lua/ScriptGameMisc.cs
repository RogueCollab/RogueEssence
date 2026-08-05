using RogueEssence.Ground;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Linq;

using NLua;
using System.Collections.Generic;
using RogueElements;
using System;

namespace RogueEssence.Script
{
    public partial class ScriptGame : ILuaEngineComponent
    {
        /// <summary>
        /// The game's random object.  Is not recorded in replays.
        /// </summary>
        public IRandom Rand { get { return MathUtils.Rand; } }

        /// <summary>
        /// Saves the game while in ground mode.
        /// </summary>
        /// <example>
        /// GAME:GroundSave()
        /// </example>
        public LuaFunction GroundSave;

        /// <summary>
        /// [LuaFunction] GroundSave
        /// </summary>
        /// <returns></returns>
        public Coroutine _GroundSave()
        {
            return new Coroutine(GroundScene.Instance.SaveGame());
        }

        /// <summary>
        /// Checks to see if the specified mod has differences from the current save file.
        /// </summary>
        /// <param name="uuidStr">The UUID of the mod to check for differences.</param>
        /// <returns></returns>
        public ModDiff GetModDiff(string uuidStr)
        {
            Guid uuid = new Guid(uuidStr);
            List<ModDiff> diffs = DataManager.Instance.Save.GetModDiffs();
            foreach (ModDiff diff in diffs)
            {
                if (diff.UUID == uuid)
                    return diff;
            }
            return new ModDiff("", uuid, null, null);
        }



        /// <summary>
        /// Initializes any LuaFunctions found in the class.
        /// Automatically on lua initialization.
        /// </summary>
        /// <param name="state">The lua engine to initialize with.</param>
        public override void SetupLuaFunctions(LuaEngine state)
        {
            GroundSave = state.RunString("return function(_) return coroutine.yield(GAME:_GroundSave()) end").First() as LuaFunction;
            CheckLevelSkills = state.RunString("return function(_,chara, oldLevel) return coroutine.yield(GAME:_CheckLevelSkills(chara, oldLevel)) end").First() as LuaFunction;
            TryLearnSkill = state.RunString("return function(_,chara, skill) return coroutine.yield(GAME:_TryLearnSkill(chara, skill)) end").First() as LuaFunction;
            EnterRescue = state.RunString("return function(_, sosPath) return coroutine.yield(GAME:_EnterRescue(sosPath)) end").First() as LuaFunction;
            EnterDungeon = state.RunString("return function(_, dungeonid, structureid, mapid, entryid, stakes, recorded, silentRestrict) return coroutine.yield(GAME:_EnterDungeon(dungeonid, structureid, mapid, entryid, stakes, recorded, silentRestrict)) end").First() as LuaFunction;
            ContinueDungeon = state.RunString("return function(_, dungeonid, structureid, mapid, entryid) return coroutine.yield(GAME:_ContinueDungeon(dungeonid, structureid, mapid, entryid)) end").First() as LuaFunction;
            EndDungeonRun = state.RunString("return function(_, result, destzoneid, structureid, mapid, entryid, display, fanfare, completedZone) return coroutine.yield(GAME:_EndDungeonRun(result, destzoneid, structureid, mapid, entryid, display, fanfare, completedZone)) end").First() as LuaFunction;
            FadeOutFront = state.RunString("return function(_, bwhite, duration) return coroutine.yield(GAME:_FadeOutFront(bwhite, duration)) end").First() as LuaFunction;
            FadeInFront = state.RunString("return function(_, duration) return coroutine.yield(GAME:_FadeInFront(duration)) end").First() as LuaFunction;
            FadeOut = state.RunString("return function(_, bwhite, duration) return coroutine.yield(GAME:_FadeOut(bwhite, duration)) end").First() as LuaFunction;
            FadeIn = state.RunString("return function(_, duration) return coroutine.yield(GAME:_FadeIn(duration)) end").First() as LuaFunction;
            MoveCamera = state.RunString("return function(_, x, y, duration, toPlayer) return coroutine.yield(GAME:_MoveCamera(x, y, duration, toPlayer)) end").First() as LuaFunction;
            MoveCameraToChara = state.RunString("return function(_, x, y, duration, chara) return coroutine.yield(GAME:_MoveCameraToChara(x, y, duration, chara)) end").First() as LuaFunction;
            WaitFrames      = state.RunString("return function(_, frames) return coroutine.yield(GAME:_WaitFrames(frames)) end").First() as LuaFunction;
        }


    }
}
