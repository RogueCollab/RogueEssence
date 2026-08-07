using System;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueElements;

using System.Linq;
using RogueEssence.Content;
using NLua;
using System.Collections.Generic;

namespace RogueEssence.Script
{
    public class ScriptDungeon : ILuaEngineComponent
    {
        /// <summary>
        /// Makes a character turn to face another
        /// </summary>
        /// <param name="curch">The character that will turn.</param>
        /// <param name="turnto">The character being turned to.</param>
        public void CharTurnToChar(Character curch, Character turnto)
        {
            if (curch == null || turnto == null)
                return;
            Loc diff = turnto.CharLoc - curch.CharLoc;
            Dir8 dir = diff.ApproximateDir8();
            if (dir != Dir8.None)
                curch.CharDir = dir;
        }

        //===================================
        //  Dungeon Stats
        //===================================

        /// <summary>
        /// Gets the result of the last dungeon adventure.
        /// </summary>
        /// <returns></returns>
        public GameProgress.ResultType LastDungeonResult()
        {
            return DataManager.Instance.Save.Outcome;
        }

        /// <summary>
        /// Returns the floor number of the current dungeon.
        /// </summary>
        /// <returns></returns>
        public int DungeonCurrentFloor()
        {
            return ZoneManager.Instance.CurrentMapID.ID;
        }

        /// <summary>
        /// Returns the internal name for the current dungeon.
        /// </summary>
        /// <returns></returns>
        public string DungeonAssetName()
        {
            //return ZoneManager.Instance.CurrentZone.AssetName;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the localized name of the current dungeon.
        /// </summary>
        /// <returns></returns>
        public string DungeonDisplayName()
        {
            return ZoneManager.Instance.CurrentZone.Name.ToLocal();
        }

        /// <summary>
        /// Used to force-disable the minimap.
        /// </summary>
        /// <param name="visible">If false, will force-disable the minimap.</param>
        public void SetMinimapVisible(bool visible)
        {
            ZoneManager.Instance.CurrentMap.HideMinimap = !visible;
        }

        //===================================
        //  Animation
        //===================================

        /// <summary>
        /// Set a character's emote in a dungeon map.
        /// </summary>
        /// <param name="chara">Character to emote</param>
        /// <param name="emoteid">ID of the emote. Use a blank string to clear the current emote.</param>
        /// <param name="cycles">The number of times to play the emote.</param>
        public void CharSetEmote(Character chara, string emoteid, int cycles)
        {
            if (chara != null)
            {
                if (!String.IsNullOrEmpty(emoteid))
                {
                    EmoteData emote = DataManager.Instance.GetEmote(emoteid);
                    chara.StartEmote(new Emote(emote.Anim, emote.LocHeight, cycles));
                }
                else
                    chara.StartEmote(null);
            }
        }

        /// <summary>
        /// Set a character's animation.
        /// </summary>
        /// <example>
        /// DUNGEON:CharStartAnim(player, anim, false)
        /// </example>
        public LuaFunction CharStartAnim;

        /// <summary>
        /// [LuaFunction] CharStartAnim
        /// </summary>
        /// <param name="chara">Character to animate.</param>
        /// <param name="anim">Name of the animation</param>
        /// <param name="loop">Whether to loop the animation</param>
        /// <returns></returns>
        public Coroutine _CharStartAnim(Character chara, string anim, bool loop)
        {
            int animIndex = GraphicsManager.GetAnimIndex(anim);
            if (loop)
                return new Coroutine(chara.StartAnim(new IdleAnimAction(chara.CharLoc, chara.CharDir, animIndex)));
            else
                return new Coroutine(chara.StartAnim(new CharAnimAction(chara.CharLoc, chara.CharDir, animIndex)));
        }

        /// <summary>
        /// Stops a character's current animation, reverting them to default idle.
        /// </summary>
        /// <example>
        /// DUNGEON:CharEndAnim(player)
        /// </example>
        public LuaFunction CharEndAnim;

        /// <summary>
        /// [LuaFunction] CharEndAnim
        /// </summary>
        /// <param name="chara">Character to stop animating</param>
        /// <returns></returns>
        public Coroutine _CharEndAnim(Character chara)
        {
            return new Coroutine(chara.StartAnim(new CharAnimIdle(chara.CharLoc, chara.CharDir)));
        }

        /// <summary>
        /// Set a character's animation, and waits until it completed before continue.
        /// </summary>
        /// <example>
        /// DUNGEON:CharWaitAnim(player, anim)
        /// </example>
        public LuaFunction CharWaitAnim;

        /// <summary>
        /// [LuaFunction] CharWaitAnim
        /// </summary>
        /// <param name="chara">Character to animate</param>
        /// <param name="anim">Name of the animation</param>
        /// <returns></returns>
        public Coroutine _CharWaitAnim(Character chara, string anim)
        {
            return new Coroutine(_charWaitAnim(chara, anim));
        }

        private IEnumerator<YieldInstruction> _charWaitAnim(Character chara, string anim)
        {
            int animIndex = GraphicsManager.GetAnimIndex(anim);
            yield return CoroutineManager.Instance.StartCoroutine(chara.StartAnim(new CharAnimAction(chara.CharLoc, chara.CharDir, animIndex)));
            yield return new WaitWhile(chara.OccupiedwithAction);
        }


        /// <summary>
        /// Sets a character's character-animation, taking a CharAnimation object instead of an animation name.
        /// </summary>
        public LuaFunction CharSetAction;

        /// <summary>
        /// [LuaFunction] CharSetAction
        /// </summary>
        /// <param name="chara">The character to animate</param>
        /// <param name="anim">The animation for the character</param>
        /// <returns></returns>
        public Coroutine _CharSetAction(Character chara, CharAnimation anim)
        {
            return new Coroutine(chara.StartAnim(anim));
        }

        //===================================
        //  VFX
        //===================================

        /// <summary>
        /// Plays a VFX in the dungeon map
        /// </summary>
        /// <param name="emitter">The VFX emitter</param>
        /// <param name="x">X position in pixels</param>
        /// <param name="y">Y Position in pixels</param>
        /// <param name="dir">Direction to orient the VFX, defaults to Down</param>
        public void PlayVFX(FiniteEmitter emitter, int x, int y, Dir8 dir = Dir8.Down)
        {
            FiniteEmitter endEmitter = (FiniteEmitter)emitter.Clone();
            endEmitter.SetupEmit(new Loc(x, y), new Loc(x, y), dir);
            DungeonScene.Instance.CreateAnim(endEmitter, DrawLayer.NoDraw);
        }

        /// <summary>
        /// Plays a VFX that has a start position and an end position.  It uses a finite emitter that generates BaseAnims.
        /// </summary>
        /// <param name="emitter">The VFX emitter</param>
        /// <param name="x">Start X position in pixels</param>
        /// <param name="y">Start Y Position in pixels</param>
        /// <param name="dir">Direction to orient the VFX, defaults to Down.</param>
        /// <param name="xTo">End X position in pixels</param>
        /// <param name="yTo">End Y position in pixels</param>
        public void PlayVFX(FiniteEmitter emitter, int x, int y, Dir8 dir, int xTo, int yTo)
        {
            FiniteEmitter endEmitter = (FiniteEmitter)emitter.Clone();
            endEmitter.SetupEmit(new Loc(x, y), new Loc(xTo, yTo), dir);
            DungeonScene.Instance.CreateAnim(endEmitter, DrawLayer.NoDraw);
        }


        /// <summary>
        /// Plays a VFX using just a BaseAnim
        /// </summary>
        /// <param name="anim">The animation to play</param>
        /// <param name="layer">The layer to put it on</param>
        public void PlayVFXAnim(BaseAnim anim, DrawLayer layer)
        {
            DungeonScene.Instance.CreateAnim(anim, layer);
        }

        /// <summary>
        /// Plays a screen-moving effect.
        /// </summary>
        /// <param name="mover">The screen mover.</param>
        public void MoveScreen(ScreenMover mover)
        {
            DungeonScene.Instance.SetScreenShake(new ScreenMover(mover));
        }


        /// <summary>
        /// Adds a Map Status to the current map.
        /// </summary>
        /// <example>
        /// DUNGEON:AddMapStatus("rain")
        /// </example>
        public LuaFunction AddMapStatus;

        /// <summary>
        /// [LuaFunction] AddMapStatus
        /// </summary>
        /// <param name="statusId">The ID of the map status to add to the map.</param>
        /// <returns></returns>
        public Coroutine _AddMapStatus(string statusId)
        {
            MapStatus status = new MapStatus(statusId);
            status.LoadFromData();
            return new Coroutine(DungeonScene.Instance.AddMapStatus(status));
        }

        /// <summary>
        /// Removes a Map Status from the current map.
        /// </summary>
        /// <example>
        /// DUNGEON:RemoveMapStatus("rain")
        /// </example>
        public LuaFunction RemoveMapStatus;

        /// <summary>
        /// [LuaFunction] RemoveMapStatus
        /// </summary>
        /// <param name="statusId">The ID of the map status to remove from the map.</param>
        /// <returns></returns>
        public Coroutine _RemoveMapStatus(string statusId)
        {
            return new Coroutine(DungeonScene.Instance.RemoveMapStatus(statusId));
        }

        /// <summary>
        /// Initializes any LuaFunctions found in the class.
        /// Automatically on lua initialization.
        /// </summary>
        /// <param name="state">The lua engine to initialize with.</param>
        public override void SetupLuaFunctions(LuaEngine state)
        {
            //Implement stuff that should be written in lua!
            CharStartAnim = state.RunString("return function(_, chara, anim, loop) return coroutine.yield(DUNGEON:_CharStartAnim(chara, anim, loop)) end", "CharStartAnim").First() as LuaFunction;
            CharEndAnim = state.RunString("return function(_, chara) return coroutine.yield(DUNGEON:_CharEndAnim(chara)) end", "CharEndAnim").First() as LuaFunction;
            CharWaitAnim = state.RunString("return function(_, chara, anim) return coroutine.yield(DUNGEON:_CharWaitAnim(chara, anim)) end", "CharWaitAnim").First() as LuaFunction;
            CharSetAction = state.RunString("return function(_, chara, anim) return coroutine.yield(DUNGEON:_CharSetAction(chara, anim)) end", "CharSetAction").First() as LuaFunction;

            AddMapStatus = state.RunString("return function(_, status_id) return coroutine.yield(DUNGEON:_AddMapStatus(status_id)) end", "AddMapStatus").First() as LuaFunction;
            RemoveMapStatus = state.RunString("return function(_, status_id) return coroutine.yield(DUNGEON:_RemoveMapStatus(status_id)) end", "RemoveMapStatus").First() as LuaFunction;

        }
    }
}
