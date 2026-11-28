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


        //===================================
        // Camera Control
        //===================================


        /// <summary>
        /// Fade out the screen. Waits to complete before continuing.
        /// This fade specifically comes in front of the menu.
        /// </summary>
        /// <example>
        /// GAME:FadeOutFront(false, 60)
        /// </example>
        public LuaFunction FadeOutFront;

        /// <summary>
        /// [LuaFunction] FadeOutFront
        /// </summary>
        /// <param name="white">Fade to white if set to true.  Fades to black otherwise.</param>
        /// <param name="duration">The amount of time to fade in frames.</param>
        /// <returns></returns>
        public Coroutine _FadeOutFront(bool white, int duration)
        {
            return new Coroutine(GameManager.Instance.FadeOutFront(white, duration));
        }


        /// <summary>
        /// Fade in the screen. Waits to complete before continuing.
        /// This fade specifically comes in front of the menu.
        /// </summary>
        /// <example>
        /// GAME:FadeOutFront(false, 60)
        /// </example>
        public LuaFunction FadeInFront;

        /// <summary>
        /// [LuaFunction] FadeInFront
        /// </summary>
        /// <param name="duration">The amount of time to fade in frames.</param>
        /// <returns></returns>
        public Coroutine _FadeInFront(int duration)
        {
            return new Coroutine(GameManager.Instance.FadeInFront(duration));
        }


        /// <summary>
        /// Fade out the screen. Waits to complete before continuing.
        /// </summary>
        /// <example>
        /// GAME:FadeOut(false, 60)
        /// </example>
        public LuaFunction FadeOut;

        /// <summary>
        /// [LuaFunction] FadeOut
        /// </summary>
        /// <param name="white">Fade to white if set to true.  Fades to black otherwise.</param>
        /// <param name="duration">The amount of time to fade in frames.</param>
        /// <returns></returns>
        public Coroutine _FadeOut(bool white, int duration)
        {
            return new Coroutine(GameManager.Instance.FadeOut(white, duration));
        }

        /// <summary>
        /// Fade into the screen. Waits to complete before continuing.
        /// </summary>
        /// <example>
        /// GAME:FadeIn(false, 60)
        /// </example>
        public LuaFunction FadeIn;

        /// <summary>
        /// [LuaFunction] FadeIn
        /// </summary>
        /// <param name="duration">The amount of time to fade in frames.</param>
        /// <returns></returns>
        public Coroutine _FadeIn(int duration)
        {
            return new Coroutine(GameManager.Instance.FadeIn(duration));
        }

        /// <summary>
        /// Centers the camera on a position.
        /// </summary>
        /// <example>
        /// GAME:MoveCamera(200, 240, 60, false)
        /// </example>
        public LuaFunction MoveCamera;

        /// <summary>
        /// [LuaFunction] MoveCamera
        /// </summary>
        /// <param name="x">X coordinate of the camera center</param>
        /// <param name="y">Y coordinate of the camera center</param>
        /// <param name="duration">The amount of time it takes ot move to the destination</param>
        /// <param name="toPlayer">Destination is in absolute coordinates if false, and relative to the player character if set to true.</param>
        /// <returns></returns>
        public Coroutine _MoveCamera(int x, int y, int duration, bool toPlayer = false)
        {
            return new Coroutine(GroundScene.Instance.MoveCamera(new Loc(x, y), duration, toPlayer));
        }

        /// <summary>
        /// Centers the camera on a character.
        ///
        /// As we are simply moving the camera to a character, this will simply set ViewCenter and not ViewOffset.
        /// </summary>
        /// <example>
        /// GAME:MoveCameraToChara(200, 240, 60, false)
        /// </example>
        public LuaFunction MoveCameraToChara;

        /// <summary>
        /// [LuaFunction] MoveCameraToChara
        /// </summary>
        /// <param name="x">X coordinate of the camera center, as an offset for the chara</param>
        /// <param name="y">Y coordinate of the camera center, as an offset for the chara</param>
        /// <param name="duration">The amount of time it takes ot move to the destination</param>
        /// <param name="chara">The character to center on.</param>
        /// <returns></returns>
        public Coroutine _MoveCameraToChara(int x, int y, int duration, GroundChar chara)
        {
            return new Coroutine(GroundScene.Instance.MoveCameraToChara(new Loc(x, y), duration, chara));
        }


        /// <summary>
        /// Gets the current center of the camera.
        /// </summary>
        /// <returns>A Loc object representing the center of the camera.</returns>
        public Loc GetCameraCenter()
        {
            return GroundScene.Instance.GetFocusedLoc();
        }

        /// <summary>
        /// Determines whether the camera is centered relative to the player.
        /// </summary>
        /// <returns>Returns true if the camera is relative to the player, false otherwise.</returns>
        public bool IsCameraOnChar()
        {
            return !ZoneManager.Instance.CurrentGround.ViewCenter.HasValue;
        }



    }
}
