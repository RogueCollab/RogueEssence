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
    class ScriptDungeon : ILuaEngineComponent
    {
        /// <summary>
        /// Makes a character turn to face another
        /// </summary>
        /// <param name="curch"></param>
        /// <param name="turnto"></param>
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


        //===================================
        //  Animation
        //===================================

        /// <summary>
        /// Set a character's emote
        /// </summary>
        /// <param name="chara"></param>
        /// <param name="emoteid"></param>
        /// <param name="cycles"></param>
        public void CharSetEmote(Character chara, int emoteid, int cycles)
        {
            if (chara != null)
            {
                if (emoteid >= 0)
                {
                    EmoteData emote = DataManager.Instance.GetEmote(emoteid);
                    chara.StartEmote(new Emote(emote.Anim, emote.LocHeight, cycles));
                }
                else
                    chara.StartEmote(null);
            }
        }

        public LuaFunction CharStartAnim;

        /// <summary>
        /// Set a character's animation
        /// </summary>
        /// <param name="chara"></param>
        /// <param name="anim"></param>
        public Coroutine _CharStartAnim(Character chara, string anim, bool loop)
        {
            int animIndex = GraphicsManager.Actions.FindIndex((CharFrameType element) => element.Name == anim);
            if (loop)
                return new Coroutine(chara.StartAnim(new IdleAnimAction(chara.CharLoc, chara.CharDir, animIndex)));
            else
                return new Coroutine(chara.StartAnim(new CharAnimAction(chara.CharLoc, chara.CharDir, animIndex)));
        }

        public LuaFunction CharEndAnim;

        public Coroutine _CharEndAnim(Character chara)
        {
            return new Coroutine(chara.StartAnim(new CharAnimIdle(chara.CharLoc, chara.CharDir)));
        }

        public LuaFunction CharWaitAnim;
        public Coroutine _CharWaitAnim(Character chara, string anim)
        {
            return new Coroutine(__CharWaitAnim(chara, anim));
        }
        public IEnumerator<YieldInstruction> __CharWaitAnim(Character chara, string anim)
        {
            int animIndex = GraphicsManager.Actions.FindIndex((CharFrameType element) => element.Name == anim);
            yield return CoroutineManager.Instance.StartCoroutine(chara.StartAnim(new CharAnimAction(chara.CharLoc, chara.CharDir, animIndex)));
            yield return new WaitWhile(chara.OccupiedwithAction);
        }


        //===================================
        //  VFX
        //===================================

        /// <summary>
        /// Plays a VFX
        /// </summary>
        /// <param name="chara"></param>
        /// <param name="anim"></param>
        public void PlayVFX(FiniteEmitter emitter, int x, int y, Dir8 dir = Dir8.Down)
        {
            FiniteEmitter endEmitter = (FiniteEmitter)emitter.Clone();
            endEmitter.SetupEmit(new Loc(x, y), new Loc(x, y), dir);
            DungeonScene.Instance.CreateAnim(endEmitter, DrawLayer.NoDraw);
        }
        public void PlayVFX(FiniteEmitter emitter, int x, int y, Dir8 dir, int xTo, int yTo)
        {
            FiniteEmitter endEmitter = (FiniteEmitter)emitter.Clone();
            endEmitter.SetupEmit(new Loc(x, y), new Loc(xTo, yTo), dir);
            DungeonScene.Instance.CreateAnim(endEmitter, DrawLayer.NoDraw);
        }
        public void PlayVFXAnim(BaseAnim anim, DrawLayer layer)
        {
            DungeonScene.Instance.CreateAnim(anim, layer);
        }

        public void MoveScreen(ScreenMover mover)
        {
            DungeonScene.Instance.SetScreenShake(new ScreenMover(mover));
        }


        public override void SetupLuaFunctions(LuaEngine state)
        {
            //Implement stuff that should be written in lua!
            CharStartAnim = state.RunString("return function(_, chara, anim, loop) return coroutine.yield(DUNGEON:_CharStartAnim(chara, anim, loop)) end", "CharStartAnim").First() as LuaFunction;
            CharEndAnim = state.RunString("return function(_, chara) return coroutine.yield(DUNGEON:_CharEndAnim(chara)) end", "CharEndAnim").First() as LuaFunction;
            CharWaitAnim = state.RunString("return function(_, chara, anim) return coroutine.yield(DUNGEON:_CharWaitAnim(chara, anim)) end", "CharWaitAnim").First() as LuaFunction;
        }
    }
}
