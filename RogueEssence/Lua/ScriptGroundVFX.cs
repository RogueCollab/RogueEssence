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

        //===================================
        //  VFX
        //===================================


        /// <summary>
        /// Plays a VFX using a finite emitter that generates BaseAnims.
        /// </summary>
        /// <param name="emitter">The VFX emitter</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y Position</param>
        /// <param name="dir">Direction to orient the VFX, defaults to Down</param>
        public void PlayVFX(FiniteEmitter emitter, int x, int y, Dir8 dir = Dir8.Down)
        {
            FiniteEmitter endEmitter = (FiniteEmitter)emitter.Clone();
            endEmitter.SetupEmit(new Loc(x, y), new Loc(x, y), dir);
            GroundScene.Instance.CreateAnim(endEmitter, DrawLayer.NoDraw);
        }

        /// <summary>
        /// Plays a VFX that has a start position and an end position.  It uses a finite emitter that generates BaseAnims.
        /// </summary>
        /// <param name="emitter">The VFX emitter</param>
        /// <param name="x">Start X position</param>
        /// <param name="y">Start Y Position</param>
        /// <param name="dir">Direction to orient the VFX, defaults to Down.</param>
        /// <param name="xTo">End X position</param>
        /// <param name="yTo">End Y position</param>
        public void PlayVFX(FiniteEmitter emitter, int x, int y, Dir8 dir, int xTo, int yTo)
        {
            FiniteEmitter endEmitter = (FiniteEmitter)emitter.Clone();
            endEmitter.SetupEmit(new Loc(x, y), new Loc(xTo, yTo), dir);
            GroundScene.Instance.CreateAnim(endEmitter, DrawLayer.NoDraw);
        }

        /// <summary>
        /// Plays a VFX using just a BaseAnim
        /// </summary>
        /// <param name="anim">The animation to play</param>
        /// <param name="layer">The layer to put it on</param>
        public void PlayVFXAnim(BaseAnim anim, DrawLayer layer)
        {
            GroundScene.Instance.CreateAnim(anim, layer);
        }

        /// <summary>
        /// Plays a screen-moving effect.
        /// </summary>
        /// <param name="mover">The screen mover.</param>
        public void MoveScreen(ScreenMover mover)
        {
            GroundScene.Instance.SetScreenShake(new ScreenMover(mover));
        }
    }
}
