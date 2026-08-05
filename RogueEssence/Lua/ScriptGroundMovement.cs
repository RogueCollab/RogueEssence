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
        //  Movement
        //===================================

        /// <summary>
        /// Makes a character turn to face another character instantly.
        /// </summary>
        /// <param name="turnchar">The character that is turning.</param>
        /// <param name="turnto">The character to turn to.</param>
        public void CharTurnToChar(GroundChar turnchar, GroundChar turnto)
        {
            if (turnchar == null || turnto == null)
                return;
            Dir8 destDir = DirExt.ApproximateDir8(turnto.MapLoc - turnchar.MapLoc);
            if (destDir == Dir8.None)
                destDir = turnto.CharDir.Reverse();
            turnchar.CharDir = destDir;
        }

        /// <summary>
        /// Makes a character do an animated turn to face another character over the specified time.
        /// Clockwise or counter-clockwise are chosen based on the closest direction.
        /// Waits until the operation is completed.
        /// </summary>
        /// <example>
        /// GROUND:CharTurnToCharAnimated(charFrom, charTo, 3)
        /// </example>
        public LuaFunction CharTurnToCharAnimated;

        /// <summary>
        /// [LuaFunction] CharTurnToCharAnimated
        /// </summary>
        /// <param name="curch">Character that is turning</param>
        /// <param name="turnto">Character to turn to</param>
        /// <param name="framedur">Time spent on each direction, in frames</param>
        /// <returns></returns>
        public Coroutine _CharTurnToCharAnimated(GroundChar curch, GroundChar turnto, int framedur)
        {
            if (curch == null || turnto == null)
                return new Coroutine(LuaEngine._DummyWait());
            Dir8 destDir = DirExt.ApproximateDir8(turnto.MapLoc - curch.MapLoc);
            if (destDir == Dir8.None)
                destDir = turnto.CharDir.Reverse();
            int turn = _CountDirectionDifference(curch.CharDir, destDir);
            return new Coroutine(_DoAnimatedTurn(curch, turn, framedur, turn < 0));
        }

        /// <summary>
        /// Makes a character do an animated turn to face a chosen direction over the specified time.
        /// Must specify clockwise or counter-clockwise.
        /// Waits until the operation is completed.
        /// </summary>
        /// <example>
        /// GROUND:CharTurnToCharAnimated(charFrom, Dir8.Left, 3, true)
        /// </example>
        public LuaFunction CharAnimateTurn;

        /// <summary>
        /// [LuaFunction] CharAnimateTurn
        /// </summary>
        /// <param name="ch">The character to turn</param>
        /// <param name="direction">The direction to turn to</param>
        /// <param name="framedur">The time spent in each intermediate direction, in frames</param>
        /// <param name="ccw">false if clockwise, true if counter-clockwise</param>
        /// <returns></returns>
        public Coroutine _CharAnimateTurn(GroundChar ch, Dir8 direction, int framedur, bool ccw)
        {
            if (ch == null || direction == Dir8.None)
                return new Coroutine(LuaEngine._DummyWait());
            return new Coroutine(_DoAnimatedTurn(ch, _CountDirectionDifference(ch.CharDir, direction), framedur, ccw));
        }

        /// <summary>
        /// Makes a character do an animated turn to face a chosen direction over the specified time.
        /// Waits until the operation is completed.
        /// </summary>
        /// <example>
        /// GROUND:CharAnimateTurnTo(charFrom, Dir8.Left, 3)
        /// </example>
        public LuaFunction CharAnimateTurnTo;

        /// <summary>
        /// [LuaFunction] CharAnimateTurnTo
        /// </summary>
        /// <param name="ch">The character to turn</param>
        /// <param name="direction">The direction to turn to</param>
        /// <param name="framedur">The time spent in each intermediate direction, in frames</param>
        public Coroutine _CharAnimateTurnTo(GroundChar ch, Dir8 direction, int framedur)
        {
            if (ch == null || direction == Dir8.None)
                return new Coroutine(LuaEngine._DummyWait());
            int turn = _CountDirectionDifference(ch.CharDir, direction);
            return new Coroutine(_DoAnimatedTurn(ch, turn, framedur, turn < 0));
        }

        private IEnumerator<YieldInstruction> _DoAnimatedTurn(GroundChar curch, int turn, int framedur, bool ccw)
        {
            if (turn == 0)
                yield break;
            var oldact = curch.GetCurrentAction();
            curch.StartAction(new IdleNoAnim(curch.MapLoc, curch.LocHeight, curch.CharDir));
            Dir8 destDir = (Dir8)((8 + turn + (int)curch.CharDir) % 8);
            if (framedur <= 0) //instant turn
            {
                curch.CharDir = destDir;
                yield break;
            }
            else
            {
                while (curch.CharDir != destDir)
                {
                    if (ccw)
                        curch.CharDir = (Dir8)((7 + (int)curch.CharDir) % 8);
                    else
                        curch.CharDir = (Dir8)((1 + (int)curch.CharDir) % 8);
                    yield return new WaitForFrames(framedur);
                }
                oldact.MapLoc = curch.MapLoc;
                oldact.CharDir = curch.CharDir;
                oldact.LocHeight = curch.LocHeight;
                curch.StartAction(oldact);
                yield break;
            }
        }

        private int _CountDirectionDifference(Dir8 from, Dir8 to)
        {
            int i = (int)from;
            int cntclockwise = 0;
            for (; i != (int)to; i = ((i >= 7) ? 0 : i + 1)) //allow wrapping around
                cntclockwise++;

            if (cntclockwise > 4)
                return (8 - cntclockwise) * -1; //If a clockwise turn takes more than  half all directions, count-clockwise is the shortest turn
            else
                return cntclockwise; //If a clockwise turn is less or equal than half the nb of direction, then clockwise is the shortest turn!
        }

        /// <summary>
        /// Makes a ground entity turn to face a direction.
        /// Useful for non-character objects.
        /// </summary>
        /// <param name="ent">The ground entity.  Can be a character or object.</param>
        /// <param name="direction">The direction to face.</param>
        public void EntTurn(GroundEntity ent, Dir8 direction)
        {
            if (ent == null || direction == Dir8.None)
                return;
            ent.Direction = direction;
        }

        /// <summary>
        /// Repositions the ground entity in a specified location.
        /// </summary>
        /// <param name="ent">The ground entity to reposition</param>
        /// <param name="x">The X coordinate of the destination</param>
        /// <param name="y">The Y coordinate of the destination</param>
        /// <param name="direction">The direction to point the entity.  Defaults to Dir8.None, which leaves it untouched.</param>
        /// <param name="height"></param>
        public void TeleportTo(GroundEntity ent, int x, int y, Dir8 direction = Dir8.None, int height = 0)
        {
            try
            {
                if (ent is GroundChar)
                {
                    GroundChar gent = ent as GroundChar;
                    gent.SetMapLoc(new Loc(x, y));
                    gent.SetLocHeight(height);
                    gent.UpdateFrame();
                    if (direction > Dir8.None)
                        gent.Direction = direction;
                    return;
                }
                else if (ent is GroundObject)
                {
                    GroundObject gent = ent as GroundObject;
                    ent.SetMapLoc(new Loc(x, y));
                    if (direction > Dir8.None)
                        ent.Direction = direction;
                    return;
                }
                throw new ArgumentException("Entity is not a valid type.");
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Make ground character move in a direction.
        /// </summary>
        /// <example>
        /// GROUND:MoveInDirection(player, Dir8.Down, 24, false, 2)
        /// </example>
        public LuaFunction MoveInDirection;

        /// <summary>
        /// [LuaFunction] MoveInDirection
        /// </summary>
        /// <param name="chara">Character to move</param>
        /// <param name="direction">Direction to move in</param>
        /// <param name="duration">Duration of movement, in frames</param>
        /// <param name="run">True if using a running animation, false otherwise</param>
        /// <param name="speed">Speed in pixels per frame</param>
        /// <returns></returns>
        public YieldInstruction _MoveInDirection(GroundChar chara, Dir8 direction, int duration, bool run = false, float speed = 2)
        {
            Loc endLoc = chara.MapLoc + direction.GetLoc() * (duration * (int)speed);
            return _MoveToPosition(chara, endLoc.X, endLoc.Y, run, speed);
        }


        /// <summary>
        /// Make ground character move to a position.
        /// </summary>
        /// <example>
        /// GROUND:MoveInDirection(player, 200, 240, false, 2)
        /// </example>
        public LuaFunction MoveToPosition;

        /// <summary>
        /// [LuaFunction] MoveToPosition
        /// </summary>
        /// <param name="chara">Character to move</param>
        /// <param name="x">X coordinate of destination</param>
        /// <param name="y">Y  coordinate of destination</param>
        /// <param name="run">True if using a running animation, false otherwise</param>
        /// <param name="speed">Speed in pixels per frame</param>
        /// <returns></returns>
        public YieldInstruction _MoveToPosition(GroundEntity chara, int x, int y, bool run = false, float speed = 2)
        {
            try
            {
                if (speed <= 0f)
                    throw new ArgumentException(String.Format("Invalid Walk Speed: {0}", speed));

                if (chara is GroundChar)
                {
                    GroundChar ch = (GroundChar)chara;
                    FrameTick prevTime = new FrameTick();
                    GroundAction prevAction = ch.GetCurrentAction();
                    if (prevAction is AnimateToPositionGroundAction)
                        prevTime = prevAction.ActionTime;
                    Loc diff = new Loc(x, y) - ch.MapLoc;
                    Dir8 approxDir = diff.ApproximateDir8();
                    if (approxDir == Dir8.None)
                        approxDir = ch.Direction;

                    IdleGroundAction baseAction = new IdleGroundAction(ch.Position, ch.LocHeight, approxDir);
                    baseAction.Override = GraphicsManager.WalkAction;
                    AnimateToPositionGroundAction newAction = new AnimateToPositionGroundAction(baseAction, run ? 2 : 1, speed, prevTime, new Loc(x, y), ch.LocHeight);
                    ch.StartAction(newAction);
                    return new WaitUntil(() =>
                    {
                        return newAction.Complete || (ch.GetCurrentAction() != newAction);
                    });
                }

                throw new ArgumentException("Entity is not a valid type.");

            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return null;
        }


        /// <summary>
        /// Make ground character move to a ground marker.
        /// </summary>
        /// <example>
        /// GROUND:MoveInDirection(player, marker, false, 2)
        /// </example>
        public LuaFunction MoveToMarker;

        /// <summary>
        /// [LuaFunction] MoveToMarker
        /// </summary>
        /// <param name="chara">Character to move</param>
        /// <param name="mark">GroundMarker object ot move to</param>
        /// <param name="run">True if using a running animation, false otherwise</param>
        /// <param name="speed">Speed in pixels per frame</param>
        /// <returns></returns>
        public YieldInstruction _MoveToMarker(GroundEntity chara, GroundMarker mark, bool run = false, float speed = 2)
        {
            return _MoveToPosition(chara, mark.X, mark.Y, run, speed);
        }


        /// <summary>
        /// Make ground object move to a position.
        /// </summary>
        /// <example>
        /// GROUND:MoveInDirection(player, 200, 240, 2)
        /// </example>
        public LuaFunction MoveObjectToPosition;

        /// <summary>
        /// [LuaFunction] MoveObjectToPosition
        /// </summary>
        /// <param name="ent">Ground Entity to move</param>
        /// <param name="x">X coordinate of destination</param>
        /// <param name="y">Y  coordinate of destination</param>
        /// <param name="speed">Speed in pixels per frame</param>
        /// <returns></returns>
        public YieldInstruction _MoveObjectToPosition(GroundEntity ent, int x, int y, int speed)
        {
            try
            {
                if (speed < 1)
                    throw new ArgumentException(String.Format("Invalid Walk Speed: {0}", speed));

                if (ent is GroundObject)
                {
                    //is this really the best place to put this?
                    GroundObject obj = (GroundObject)ent;
                    return new Coroutine(obj.MoveToLoc(obj.Position, speed, new Loc(x, y)));
                }

                throw new ArgumentException("Entity is not a valid type.");

            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return null;
        }

        /// <summary>
        /// Make a ground character move in a direction with custom animation
        /// </summary>
        /// <example>
        /// GROUND:AnimateInDirection(player, "Hurt", Dir8.Down, 24, 0.5, 2)
        /// </example>
        public LuaFunction AnimateInDirection;

        /// <summary>
        /// [LuaFunction] AnimateInDirection
        /// </summary>
        /// <param name="chara">Character to move</param>
        /// <param name="anim">Name of the animation</param>
        /// <param name="animDir">Direction of animation</param>
        /// <param name="direction">Direction to move in</param>
        /// <param name="duration">Duration of movement, in frames</param>
        /// <param name="animSpeed">Speed of animation, where 1.0 represents normal speed</param>
        /// <param name="speed">Speed movement, in pixels per frame</param>
        /// <returns></returns>
        public YieldInstruction _AnimateInDirection(GroundChar chara, string anim, Dir8 animDir, Dir8 direction, int duration, float animSpeed, float speed)
        {
            Loc endLoc = chara.MapLoc + direction.GetLoc() * (duration * (int)speed);
            return _AnimateToPosition(chara, anim, animDir, endLoc.X, endLoc.Y, animSpeed, speed, 0);
        }

        /// <summary>
        /// Make a ground entity move to a position with custom animation
        /// </summary>
        /// <example>
        /// GROUND:AnimateToPosition(player, "Hurt", Dir8.Down, 200, 240, 0.5, 2)
        /// </example>
        public LuaFunction AnimateToPosition;

        /// <summary>
        /// [LuaFunction] AnimateToPosition
        /// </summary>
        /// <param name="ent">Entity to move</param>
        /// <param name="anim">Name of the animation</param>
        /// <param name="animDir">Direction of animation</param>
        /// <param name="x">X coordinate of the destination</param>
        /// <param name="y">Y coordinate of the destination</param>
        /// <param name="animSpeed">Speed of animation, where 1.0 represents normal speed</param>
        /// <param name="speed">Speed movement, in pixels per frame</param>
        /// <param name="height">Height of the destination</param>
        /// <returns></returns>
        public YieldInstruction _AnimateToPosition(GroundEntity ent, string anim, Dir8 animDir, int x, int y, float animSpeed, float speed, int height)
        {
            try
            {
                if (speed <= 0f)
                    throw new ArgumentException(String.Format("Invalid Walk Speed: {0}", speed));

                if (ent is GroundChar)
                {
                    GroundChar ch = (GroundChar)ent;
                    FrameTick prevTime = new FrameTick();
                    GroundAction prevAction = ch.GetCurrentAction();
                    int animIndex = GraphicsManager.GetAnimIndex(anim);
                    if (prevAction is AnimateToPositionGroundAction)
                    {
                        if (animIndex == prevAction.AnimFrameType)
                            prevTime = prevAction.ActionTime;
                    }
                    IdleGroundAction baseAction = new IdleGroundAction(ch.Position, ch.LocHeight, animDir);
                    baseAction.Override = animIndex;
                    AnimateToPositionGroundAction newAction = new AnimateToPositionGroundAction(baseAction, animSpeed, speed, prevTime, new Loc(x, y), height);
                    ch.StartAction(newAction);
                    return new WaitUntil(() =>
                    {
                        return newAction.Complete;
                    });
                }
                throw new ArgumentException("Entity is not a valid type.");

            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return null;
        }

        /// <summary>
        /// Make a ground entity action to a position with custom animation
        /// </summary>
        public LuaFunction ActionToPosition;

        /// <summary>
        /// [LuaFunction] ActionToPosition
        /// </summary>
        /// <param name="ent">Entity to move</param>
        /// <param name="baseAction">Action for the entity to perform</param>
        /// <param name="x">X coordinate of the destination</param>
        /// <param name="y">Y coordinate of the destination</param>
        /// <param name="animSpeed">Speed of animation, where 1.0 represents normal speed</param>
        /// <param name="speed">Speed movement, in pixels per frame</param>
        /// <param name="height">Height of the destination</param>
        /// <returns></returns>
        public YieldInstruction _ActionToPosition(GroundEntity ent, GroundAction baseAction, int x, int y, float animSpeed, float speed, int height)
        {
            try
            {
                if (speed <= 0f)
                    throw new ArgumentException(String.Format("Invalid Walk Speed: {0}", speed));

                if (ent is GroundChar)
                {
                    GroundChar ch = (GroundChar)ent;
                    FrameTick prevTime = new FrameTick();
                    GroundAction prevAction = ch.GetCurrentAction();
                    if (prevAction is AnimateToPositionGroundAction)
                    {
                        if (baseAction.AnimFrameType == prevAction.AnimFrameType)
                            prevTime = prevAction.ActionTime;
                    }
                    AnimateToPositionGroundAction newAction = new AnimateToPositionGroundAction(baseAction, animSpeed, speed, prevTime, new Loc(x, y), height);
                    ch.StartAction(newAction);
                    return new WaitUntil(() =>
                    {
                        return newAction.Complete;
                    });
                }
                throw new ArgumentException("Entity is not a valid type.");

            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return null;
        }
    }
}
