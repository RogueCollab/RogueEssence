using System;
using RogueEssence.Content;
using RogueEssence.Ground;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueElements;
using NLua;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace RogueEssence.Script
{
    /// <summary>
    /// Helper interface to regroup everything tied to ground mode under a single object
    /// </summary>
    public partial class ScriptGround : ILuaEngineComponent
    {

        /// <summary>
        /// Gives a character a set amount of EXP.
        /// Also handles leveling up and learning new moves.
        /// </summary>
        public LuaFunction HandoutEXP;

        /// <summary>
        /// [LuaFunction] HandoutEXP
        /// </summary>
        /// <param name="character">The characters to level up.</param>
        /// <param name="experience">The amount of EXP to gain.</param>
        /// <returns></returns>
        public Coroutine _HandoutEXP(Character character, int experience)
        {
            return new Coroutine(GroundScene.Instance.HandoutEXP(character, experience));
        }

        /// <summary>
        /// Levels up a character a certain amount of times all at once.
        /// Also handles learning new moves.
        /// </summary>       
        public LuaFunction LevelUpChar;

        /// <summary>
        /// [LuaFunction] HandoutEXP
        /// </summary>
        /// <param name="character">The characters to level up.</param>
        /// <param name="numLevelUps">The number of level ups.</param>
        /// <returns></returns>
        public Coroutine _LevelUpChar(Character character, int numLevelUps)
        {
            return new Coroutine(GroundScene.Instance.LevelUpChar(character, numLevelUps));
        }

        /// <summary>
        /// Adds a mapstatus to the ground map.  Map statuses only have an aesthetic effect in ground maps.
        /// </summary>
        /// <param name="statusIdx">The ID of the Map Status</param>
        public void AddMapStatus(string statusIdx)
        {
            MapStatus status = new MapStatus(statusIdx);
            status.LoadFromData();
            GroundScene.Instance.AddMapStatus(status);
        }

        /// <summary>
        /// Removes a map status from the ground map.
        /// </summary>
        /// <param name="statusIdx">The ID of the Map Status to remove.</param>
        public void RemoveMapStatus(string statusIdx)
        {
            GroundScene.Instance.RemoveMapStatus(statusIdx);
        }

        /// <summary>
        /// Initializes any LuaFunctions found in the class.
        /// Automatically on lua initialization.
        /// </summary>
        /// <param name="state">The lua engine to initialize with.</param>
        public override void SetupLuaFunctions(LuaEngine state)
        {
            //Implement stuff that should be written in lua!
            CharWaitAnim = state.RunString("return function(_, ent, anim) return coroutine.yield(GROUND:_CharWaitAnim(ent, anim)) end", "CharWaitAnim").First() as LuaFunction;
            ObjectWaitAnimFrame = state.RunString("return function(_, ent, animframe) return coroutine.yield(GROUND:_ObjectWaitAnimFrame(ent, animframe)) end", "ObjectWaitAnimFrame").First() as LuaFunction;

            MoveInDirection = state.RunString("return function(_, chara, direction, duration, shouldrun, speed) return coroutine.yield(GROUND:_MoveInDirection(chara, direction, duration, shouldrun, speed)) end", "MoveInDirection").First() as LuaFunction;
            AnimateInDirection = state.RunString("return function(_, chara, anim, animdir, direction, duration, animspeed, speed) return coroutine.yield(GROUND:_AnimateInDirection(chara, anim, animdir, direction, duration, animspeed, speed)) end", "AnimateInDirection").First() as LuaFunction;
            CharAnimateTurn = state.RunString("return function(_, ch, direction, framedur, ccw) return coroutine.yield(GROUND:_CharAnimateTurn(ch, direction, framedur, ccw)) end", "CharAnimateTurn").First() as LuaFunction;
            CharAnimateTurnTo = state.RunString("return function(_, ch, direction, framedur) return coroutine.yield(GROUND:_CharAnimateTurnTo(ch, direction, framedur)) end", "CharAnimateTurn").First() as LuaFunction;
            CharTurnToCharAnimated = state.RunString("return function(_, curch, turnto, framedur) return coroutine.yield(GROUND:_CharTurnToCharAnimated(curch, turnto, framedur)) end", "CharTurnToCharAnimated").First() as LuaFunction;



            MoveToMarker = state.RunString("return function(_, ent, mark, shouldrun, speed) return coroutine.yield(GROUND:_MoveToMarker(ent, mark, shouldrun, speed)) end", "MoveToMarker").First() as LuaFunction;
            MoveToPosition = state.RunString("return function(_, ent, x, y, shouldrun, speed) return coroutine.yield(GROUND:_MoveToPosition(ent, x, y, shouldrun, speed)) end", "MoveToPosition").First() as LuaFunction;
            AnimateToPosition = state.RunString("return function(_, ent, anim, animdir, x, y, animspeed, speed, height) return coroutine.yield(GROUND:_AnimateToPosition(ent, anim, animdir, x, y, animspeed, speed, height)) end", "AnimateToPosition").First() as LuaFunction;
            ActionToPosition = state.RunString("return function(_, ent, action, x, y, animspeed, speed, height) return coroutine.yield(GROUND:_ActionToPosition(ent, action, x, y, animspeed, speed, height)) end", "ActionToPosition").First() as LuaFunction;
            CharWaitAction = state.RunString("return function(_, ent, action) return coroutine.yield(GROUND:_CharWaitAction(ent, action)) end", "CharWaitAction").First() as LuaFunction;

            MoveObjectToPosition = state.RunString("return function(_, ent, x, y, speed) return coroutine.yield(GROUND:_MoveObjectToPosition(ent, x, y, speed)) end", "MoveObjectToPosition").First() as LuaFunction;
            HandoutEXP = state.RunString("return function(_, character, numlevelups) return coroutine.yield(GROUND:_HandoutEXP(character, experience)) end", "HandoutEXP").First() as LuaFunction;
            LevelUpChar = state.RunString("return function(_, character, numlevelups) return coroutine.yield(GROUND:_LevelUpChar(character, numlevelups)) end", "LevelUpChar").First() as LuaFunction;
        }
    }
}
