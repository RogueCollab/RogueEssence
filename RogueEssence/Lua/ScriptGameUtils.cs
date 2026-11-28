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
        // Utils
        //===================================


        /// <summary>
        /// Checks if a player is making a certain physical keyboard input.
        /// </summary>
        /// <param name="keyid">The ID of the input</param>
        /// <returns>True if the button is currently pressed.  False otherwise.</returns>
        public bool IsKeyDown(int keyid)
        {
            Microsoft.Xna.Framework.Input.Keys curkey = (Microsoft.Xna.Framework.Input.Keys)keyid;
            return GameManager.Instance.MetaInputManager.BaseKeyDown(curkey);
        }

        /// <summary>
        /// Checks if a player is making a certain game input.
        /// </summary>
        /// <param name="inputid"></param>
        /// <returns>True if the input is currently pressed.  False otherwise.</returns>
        public bool IsInputDown(int inputid)
        {
            return GameManager.Instance.MetaInputManager[(FrameInput.InputType)inputid];
        }


        /// <summary>
        /// Prepares an event to execute on the next frame.
        /// </summary>
        /// <param name="obj"></param>
        public void QueueLeaderEvent(object obj)
        {
            IEnumerator<YieldInstruction> yields = null;
            if (obj is Coroutine)
            {
                Coroutine coro = obj as Coroutine;
                yields = CoroutineManager.Instance.YieldCoroutine(coro);
            }
            else if (obj is LuaFunction)
            {
                LuaFunction luaFun = obj as LuaFunction;
                yields = LuaEngine.Instance.CallScriptFunction(luaFun);
            }

            if (GameManager.Instance.CurrentScene == GroundScene.Instance)
            {
                GroundScene.Instance.PendingLeaderAction = yields;
            }
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                DungeonScene.Instance.PendingLeaderAction = yields;
            }
        }

        /// <summary>
        /// Waits for a specified number of frames before continuing.
        /// </summary>
        /// <example>
        /// GAME:WaitFrames(60)
        /// </example>
        public LuaFunction WaitFrames;

        /// <summary>
        /// [LuaFunction] WaitFrames
        /// </summary>
        /// <param name="frames">The number of frames to wait.  Each frame is 1/60th of a second.</param>
        /// <returns></returns>
        public YieldInstruction _WaitFrames(int frames)
        {
            return new WaitForFrames(frames);
        }

        /// <summary>
        /// Turns a vector (preferably a unit vector) into a cardinal or diagonal direction.
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <returns>The direction as one of 8 values.</returns>
        public Dir8 VectorToDirection(Loc v)
        {
            return VectorToDirection(v.X, v.Y);
        }

        /// <summary>
        /// Convenience function to get a vector's components from lua numbers(doubles)
        /// </summary>
        /// <param name="X">The X value of the vector</param>
        /// <param name="Y">The Y value of the vector</param>
        /// <returns>The direction the vector points to as one of 8 values.</returns>
        public Dir8 VectorToDirection(double X, double Y)
        {
            try
            {
                if (X < 0)
                {
                    if (Y > 0)
                        return Dir8.UpRight;
                    else if (Y < 0)
                        return Dir8.DownRight;
                    else
                        return Dir8.Right;
                }
                else if (X > 0)
                {
                    if (Y > 0)
                        return Dir8.UpLeft;
                    else if (Y < 0)
                        return Dir8.DownLeft;
                    else
                        return Dir8.Left;
                }
                else
                {
                    if (Y > 0)
                        return Dir8.Up;
                    else if (Y < 0)
                        return Dir8.Down;
                    else
                        return Dir8.None; //psy: If both X and Y are 0, well not much else fits ^^;
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
                return Dir8.None;
            }
        }

        /// <summary>
        /// Generates a random direction.
        /// </summary>
        /// <returns>An 8-directional direction.</returns>
        public Dir8 RandomDirection()
        {
            try
            {
                var rng = new Random();
                return (Dir8)rng.Next(0, 7);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
                return Dir8.None;
            }
        }


    }
}
