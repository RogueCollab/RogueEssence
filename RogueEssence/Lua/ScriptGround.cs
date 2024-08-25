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
    class ScriptGround : ILuaEngineComponent
    {
        //===================================
        // Objects and Characters
        //===================================

        /// <summary>
        /// Hides an entity.
        /// </summary>
        /// <param name="entityname">The name of the entity to hide.</param>
        public void Hide(string entityname)
        {
            try
            {
                GroundEntity found = ZoneManager.Instance.CurrentGround.FindEntity(entityname);
                if (found == null)
                    throw new ArgumentException(String.Format("ScriptGround.Hide({0}): Couldn't find entity to hide!", entityname));
                //Hide the entity
                found.EntEnabled = false;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Unhides an entity.
        /// </summary>
        /// <param name="entityname">The name of the entity to unhide.</param>
        public void Unhide(string entityname)
        {
            try
            {
                GroundEntity found = ZoneManager.Instance.CurrentGround.FindEntity(entityname);
                if (found == null)
                    throw new ArgumentException(String.Format("ScriptGround.Unhide({0}): Couldn't find entity to un-hide!", entityname));
                //Enable the entity
                found.EntEnabled = true;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// TODO: WIP
        /// </summary>
        /// <param name="objtype"></param>
        /// <param name="instancename"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public object CreateObject(string objtype, string instancename, int x, int y, int w, int h)
        {
            try
            {
                GroundMap map = ZoneManager.Instance.CurrentGround;

                GroundObject groundobject = null;
                var template = TemplateManager.Instance.FindTemplate(objtype); //Templates are created by the modders, and stored as data (This is handy, because its pretty certain a lot of characters and entities will be repeated throughout the maps)
                if (template == null)
                    return null;

                groundobject = (GroundObject)template.create(instancename);
                groundobject.Bounds = new Rect(x, y, w, h);
                groundobject.ReloadEvents();
                map.AddObject(groundobject);
                return groundobject; //Object's properties can be tweaked later on

            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return null;
        }

        /// <summary>
        /// TODO: WIP
        /// </summary>
        /// <param name="chartype"></param>
        /// <param name="instancename"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="actionfun"></param>
        /// <param name="thinkfun"></param>
        /// <returns></returns>
        public object CreateCharacter(string chartype, string instancename, int x, int y, string actionfun, string thinkfun)
        {
            try
            {
                GroundMap map = ZoneManager.Instance.CurrentGround;

                //Ideally this is how we'd create characters :
                /*
                GroundChar groundchar = null;
                    GroundCharTemplate template = CharacterTemplates.Find(chartype); //Templates are created by the modders, and stored as data (This is handy, because its pretty certain a lot of characters and entities will be repeated throughout the maps)
                    if (template == null)
                        return null;

                    groundchar = template.create(instancename, x, y);

                    groundchar.SetRoutine(thinkfun); //Aka the code that makes the npc wander, or do stuff over and over again
                    groundchar.SetAction(actionfun);

                    map.AddMapChar(groundChar);
                    return groundchar; //Object's properties can be tweaked later on
                */
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return null;
        }

        /// <summary>
        /// Deletes an object from the ground map, identified by its instance name.
        /// </summary>
        /// <param name="instancename">The instance name of the object.</param>
        /// <returns>Returns true if succeeded, false otherwise.</returns>
        public bool RemoveObject(string instancename)
        {
            try
            {
                GroundMap map = ZoneManager.Instance.CurrentGround;

                throw new NotImplementedException();
                /*
                map.RemoveObject(instancename); //Removal by instance name, since lua can't do via .NET pointer reliably, and pointers to .NET aren't practical in lua
                */
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return false;
        }

        /// <summary>
        /// Deletes a character from the ground map, identified by its instance name.
        /// </summary>
        /// <param name="instancename">The instance name of the object.</param>
        /// <returns>Returns true if succeeded, false otherwise.</returns>
        public bool RemoveCharacter(string instancename)
        {
            try
            {
                GroundMap map = ZoneManager.Instance.CurrentGround;

                //Removal by instance name, since lua can't do via .NET pointer reliably, and pointers to .NET aren't practical in lua
                GroundChar charToRemove = map.GetMapChar(instancename);
                if (charToRemove != null)
                {
                    map.RemoveMapChar(charToRemove);
                    return true;
                }
                charToRemove = map.GetTempChar(instancename);
                if (charToRemove != null)
                {
                    map.RemoveTempChar(charToRemove);
                    return true;
                }

            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return false;
        }

        /// <summary>
        /// Creates a ground character, given a dungeon character.
        /// </summary>
        /// <param name="instancename">The instance name to give the character</param>
        /// <param name="data">Character data to create from</param>
        /// <param name="x">X coordinate of the character</param>
        /// <param name="y">Y coordinate of the character</param>
        /// <param name="direction">Direction the character will face, defaults to Dir8.Down</param>
        /// <returns></returns>
        public GroundChar CreateCharacterFromCharData(string instancename, Character data, int x = 0, int y = 0, Dir8 direction = Dir8.Down)
        {
            GroundChar groundChar = new GroundChar(data, new Loc(x, y), direction, instancename);
            ZoneManager.Instance.CurrentGround.AddMapChar(groundChar);
            return groundChar;
        }

        /// <summary>
        /// Reloads the controllable player's character data to be the current team's leader.
        /// </summary>
        public void RefreshPlayer()
        {
            GroundChar leaderChar = GroundScene.Instance.FocusedCharacter;
            ZoneManager.Instance.CurrentGround.SetPlayerChar(new GroundChar(DataManager.Instance.Save.ActiveTeam.Leader, leaderChar.MapLoc, leaderChar.CharDir, "PLAYER"));
        }

        /// <summary>
        /// Sets the controllable player to use new character data
        /// </summary>
        /// <param name="charData">The new character data</param>
        public void SetPlayer(CharData charData)
        {
            GroundChar leaderChar = GroundScene.Instance.FocusedCharacter;
            ZoneManager.Instance.CurrentGround.SetPlayerChar(new GroundChar(charData, leaderChar.MapLoc, leaderChar.CharDir, "PLAYER"));
        }

        /// <summary>
        /// Make the specified spawner run its spawn method.
        /// </summary>
        /// <param name="spawnername"></param>
        /// <returns>The ground character spawned.</returns>
        public GroundChar SpawnerDoSpawn(string spawnername)
        {
            try
            {
                GroundSpawner spwn = ZoneManager.Instance.CurrentGround.GetSpawner(spawnername);
                if (spwn == null)
                    throw new ArgumentException(String.Format("ScriptGround.SpawnerDoSpawn({0}):  Couldn't find spawner!", spawnername));
                return spwn.Spawn(ZoneManager.Instance.CurrentGround);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }

            return null;
        }


        /// <summary>
        /// Sets the character to the specified spawner
        /// </summary>
        /// <param name="spawnername">The spawner to set the character to, by name</param>
        /// <param name="spawnChar">The character to spawn.</param>
        /// <returns></returns>
        public void SpawnerSetSpawn(string spawnername, CharData spawnChar)
        {
            try
            {
                GroundSpawner spwn = ZoneManager.Instance.CurrentGround.GetSpawner(spawnername);
                if (spwn == null)
                    throw new ArgumentException(String.Format("ScriptGround.SpawnerSetSpawn({0}):  Couldn't find spawner!", spawnername));
                spwn.NPCChar = spawnChar;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
        }


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
        /// <param name="curch">Character that is turning</param>
        /// <param name="turnto">Character to turn to</param>
        /// <param name="framedur">Time spent on each direction, in frames</param>
        /// <example>
        /// CharTurnToCharAnimated(charFrom, charTo, 3)
        /// </example>
        public LuaFunction CharTurnToCharAnimated;

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
        /// Makes a ground entity turn to face a direction.
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
        /// Makes a character do an animated turn to face a chosen direction over the specified time.
        /// Must specify clockwise or counter-clockwise.
        /// Waits until the operation is completed.
        /// </summary>
        /// <param name="ch">The character to turn</param>
        /// <param name="direction">The direction to turn to</param>
        /// <param name="framedur">The time spent in each intermediate direction, in frames</param>
        /// <param name="ccw">false if clockwise, true if counter-clockwise</param>
        /// <example>
        /// CharTurnToCharAnimated(charFrom, Dir8.Left, 3, true)
        /// </example>
        public LuaFunction CharAnimateTurn;
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
        /// <param name="ch">The character to turn</param>
        /// <param name="direction">The direction to turn to</param>
        /// <param name="framedur">The time spent in each intermediate direction, in frames</param>
        /// <example>
        /// CharTurnToCharAnimated(charFrom, Dir8.Left, 3)
        /// </example>
        public LuaFunction CharAnimateTurnTo;
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
        /// <param name="chara">Character to move</param>
        /// <param name="direction">Direction to move in</param>
        /// <param name="duration">Duration of movement, in frames</param>
        /// <param name="run">True if using a running animation, false otherwise</param>
        /// <param name="speed">Speed in pixels per frame</param>
        /// <example>
        /// GROUND:MoveInDirection(player, Dir8.Down, 24, false, 2)
        /// </example>
        public LuaFunction MoveInDirection;

        public YieldInstruction _MoveInDirection(GroundChar chara, Dir8 direction, int duration, bool run = false, int speed = 2)
        {
            Loc endLoc = chara.MapLoc + direction.GetLoc() * (duration * speed);
            return _MoveToPosition(chara, endLoc.X, endLoc.Y, run, speed);
        }


        /// <summary>
        /// Make ground character move to a position.
        /// </summary>
        /// <param name="chara">Character to move</param>
        /// <param name="x">X coordinate of destination</param>
        /// <param name="y">Y  coordinate of destination</param>
        /// <param name="run">True if using a running animation, false otherwise</param>
        /// <param name="speed">Speed in pixels per frame</param>
        /// <example>
        /// GROUND:MoveInDirection(player, 200, 240, false, 2)
        /// </example>
        public LuaFunction MoveToPosition;
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
        /// <param name="chara">Character to move</param>
        /// <param name="mark">GroundMarker object ot move to</param>
        /// <param name="run">True if using a running animation, false otherwise</param>
        /// <param name="speed">Speed in pixels per frame</param>
        /// <example>
        /// GROUND:MoveInDirection(player, marker, false, 2)
        /// </example>
        public LuaFunction MoveToMarker;
        public YieldInstruction _MoveToMarker(GroundEntity chara, GroundMarker mark, bool run = false, int speed = 2)
        {
            return _MoveToPosition(chara, mark.X, mark.Y, run, speed);
        }


        /// <summary>
        /// Make ground object move to a position.
        /// </summary>
        /// <param name="ent">Ground Entity to move</param>
        /// <param name="x">X coordinate of destination</param>
        /// <param name="y">Y  coordinate of destination</param>
        /// <param name="speed">Speed in pixels per frame</param>
        /// <example>
        /// GROUND:MoveInDirection(player, 200, 240, 2)
        /// </example>
        public LuaFunction MoveObjectToPosition;
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
        /// <param name="chara">Character to move</param>
        /// <param name="anim">Name of the animation</param>
        /// <param name="animDir">Direction of animation</param>
        /// <param name="direction">Direction to move in</param>
        /// <param name="duration">Duration of movement, in frames</param>
        /// <param name="animSpeed">Speed of animation, where 1.0 represents normal speed</param>
        /// <param name="speed">Speed movement, in pixels per frame</param>
        /// <example>
        /// GROUND:AnimateInDirection(player, "Hurt", Dir8.Down, 24, 0.5, 2)
        /// </example>
        public LuaFunction AnimateInDirection;
        public YieldInstruction _AnimateInDirection(GroundChar chara, string anim, Dir8 animDir, Dir8 direction, int duration, float animSpeed, int speed)
        {
            Loc endLoc = chara.MapLoc + direction.GetLoc() * (duration * speed);
            return _AnimateToPosition(chara, anim, animDir, endLoc.X, endLoc.Y, animSpeed, speed, 0);
        }

        /// <summary>
        /// Make a ground entity move to a position with custom animation
        /// </summary>
        /// <param name="ent">Entity to move</param>
        /// <param name="anim">Name of the animation</param>
        /// <param name="animDir">Direction of animation</param>
        /// <param name="x">X coordinate of the destination</param>
        /// <param name="y">Y coordinate of the destination</param>
        /// <param name="animSpeed">Speed of animation, where 1.0 represents normal speed</param>
        /// <param name="speed">Speed movement, in pixels per frame</param>
        /// <param name="height">Height of the destination</param>
        /// <example>
        /// GROUND:AnimateToPosition(player, "Hurt", Dir8.Down, 200, 240, 0.5, 2)
        /// </example>
        public LuaFunction AnimateToPosition;
        public YieldInstruction _AnimateToPosition(GroundEntity ent, string anim, Dir8 animDir, int x, int y, float animSpeed, int speed, int height)
        {
            try
            {
                if (speed < 1)
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


        public LuaFunction ActionToPosition;
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

        //===================================
        //  Animation
        //===================================


        /// <summary>
        /// Make a character emote on the ground map.
        /// </summary>
        /// <param name="chara">Character to emote</param>
        /// <param name="emoteid">ID of the emote</param>
        /// <param name="cycles">The number of times to play the emote.</param>
        public void CharSetEmote(GroundChar chara, string emoteid, int cycles)
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
        /// Sets the ground character's draw effect to become invisible, shaking, still, etc.
        /// </summary>
        /// <param name="chara">Target ground character.</param>
        /// <param name="effect">The draw effect.</param>
        public void CharSetDrawEffect(GroundChar chara, DrawEffect effect)
        {
            if (chara != null)
            {
                chara.SetDrawEffect(effect);
            }
        }

        /// <summary>
        /// Sets the ground character's draw effect to become invisible, shaking, still, etc.
        /// </summary>
        /// <param name="chara">Target ground character.</param>
        /// <param name="effect">The draw effect.</param>
        public void CharEndDrawEffect(GroundChar chara, DrawEffect effect)
        {
            if (chara != null)
            {
                chara.RemoveDrawEffect(effect);
            }
        }

        /// <summary>
        /// Gets the fallback animation for the character.
        /// </summary>
        /// <param name="chara"></param>
        /// <param name="anim">The anim to get the fallback anim of.</param>
        /// <returns>The fallback animation, as a string.  Blank if there is none.  Will return anim if anim already exists.</returns>
        public string CharGetAnimFallback(GroundChar chara, string anim)
        {
            int animIndex = GraphicsManager.GetAnimIndex(anim);
            CharSheet sheet = GraphicsManager.GetChara(chara.CurrentForm.ToCharID());
            int fallbackIndex = sheet.GetReferencedAnimIndex(animIndex);
            if (fallbackIndex < 0)
                return "";
            return GraphicsManager.Actions[fallbackIndex].Name;
        }


        /// <summary>
        /// Gets a character's current animation as a string.
        /// </summary>
        /// <param name="chara"></param>
        public string CharGetAnim(GroundChar chara)
        {
            GroundAction charaAction = chara.GetCurrentAction();
            int frameType = charaAction.AnimFrameType;
            if (frameType < 0)
                return "";
            return GraphicsManager.Actions[frameType].Name;
        }

        /// <summary>
        /// Gets the chosen action point of the character at this specific frame.
        /// </summary>
        /// <param name="chara"></param>
        /// <param name="actionPoint">The ype of action point to retrieve the coordinates for.</param>
        /// <returns>The location of the action point in absolute coordinates on the map.</returns>
        public Loc CharGetAnimPoint(GroundChar chara, ActionPointType actionPoint)
        {
            GroundAction charaAction = chara.GetCurrentAction();
            CharSheet sheet = GraphicsManager.GetChara(chara.CurrentForm.ToCharID());
            return charaAction.GetActionPoint(sheet, actionPoint);
        }

        /// <summary>
        /// Set a character's animation.
        /// </summary>
        /// <param name="chara">Character to animate</param>
        /// <param name="anim">Name of the animation</param>
        /// <param name="loop">Whether to loop the animation</param>
        public void CharSetAnim(GroundChar chara, string anim, bool loop)
        {
            int animIndex = GraphicsManager.GetAnimIndex(anim);
            chara.StartAction(new IdleAnimGroundAction(chara.Position, chara.LocHeight, chara.Direction, animIndex, loop));
        }

        /// <summary>
        /// Stops a character's current animation, reverting them to default idle.
        /// </summary>
        /// <param name="chara">Character to stop animating</param>
        public void CharEndAnim(GroundChar chara)
        {
            chara.StartAction(new IdleGroundAction(chara.Position, chara.LocHeight, chara.Direction));
        }

        /// <summary>
        /// Makes the character perform an animation and waits until it's over.
        /// </summary>
        /// <param name="ent">Character to animate</param>
        /// <param name="anim">Animation to play</param>
        /// <example>
        /// GROUND:CharWaitAnim(player, "Hurt")
        /// </example>
        public LuaFunction CharWaitAnim;
        public YieldInstruction _CharWaitAnim(GroundEntity ent, string anim)
        {
            try
            {
                if (ent is GroundChar)
                {
                    GroundChar ch = (GroundChar)ent;
                    int animIndex = GraphicsManager.GetAnimIndex(anim);
                    IdleAnimGroundAction action = new IdleAnimGroundAction(ch.Position, ch.LocHeight, ch.Direction, animIndex, false);
                    ch.StartAction(action);
                    return new WaitUntil(() =>
                    {
                        return action.Complete || (ch.GetCurrentAction() != action);
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
        /// Set a character's action.
        /// </summary>
        /// <param name="chara">Character to perfom the action</param>
        /// <param name="action">The action to perform</param>
        public void CharSetAction(GroundChar chara, GroundAction action)
        {
            chara.StartAction(action);
        }


        /// <summary>
        /// Makes the character perform an action and waits until it's over.
        /// </summary>
        /// <param name="ent">Character to animate</param>
        /// <param name="action">Action to perform</param>
        /// <example>
        /// GROUND:CharWaitAction(player, action)
        /// </example>
        public LuaFunction CharWaitAction;
        public YieldInstruction _CharWaitAction(GroundEntity ent, GroundAction action)
        {
            try
            {
                if (ent is GroundChar)
                {
                    GroundChar ch = (GroundChar)ent;
                    ch.StartAction(action);
                    return new WaitUntil(() =>
                    {
                        return action.Complete || (ch.GetCurrentAction() != action);
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
        /// Sets a ground object's animation.  After it finishes, it will return to the default animation.
        /// </summary>
        /// <param name="obj">The object to animate</param>
        /// <param name="frameTime">The duration of each frame of animation</param>
        /// <param name="startFrame">The start frame of animation</param>
        /// <param name="endFrame">The end frame of animation</param>
        /// <param name="dir">The direction of the animation</param>
        /// <param name="cycles">The number of times to repeat the animation</param>
        public void ObjectSetAnim(GroundObject obj, int frameTime, int startFrame, int endFrame, Dir8 dir, int cycles)
        {
            obj.StartAction(new ObjAnimData(obj.ObjectAnim.AnimIndex, frameTime, startFrame, endFrame, 255, dir), cycles);
        }

        /// <summary>
        /// Sets a ground object's default animation.
        /// </summary>
        /// <param name="obj">The object to animate</param>
        /// <param name="animName">The name of the animation</param>
        /// <param name="frameTime">The duration of each frame of animation</param>
        /// <param name="startFrame">The start frame of animation</param>
        /// <param name="endFrame">The end frame of animation</param>
        /// <param name="dir">The direction of the animation</param>
        public void ObjectSetDefaultAnim(GroundObject obj, string animName, int frameTime, int startFrame, int endFrame, Dir8 dir)
        {
            obj.ObjectAnim = new ObjAnimData(animName, frameTime, startFrame, endFrame, 255, dir);
        }


        /// <summary>
        /// Waits for the object to reach a specific frame before continuing.
        /// </summary>
        /// <param name="obj">The object ot wait on</param>
        /// <param name="frame">The frame of animation to wait on.</param>
        /// <example>
        /// GROUND:WaitObjectAnim(fountain, 3)
        /// </example>
        public LuaFunction ObjectWaitAnimFrame;
        public YieldInstruction _ObjectWaitAnimFrame(GroundObject obj, int frame)
        {
            try
            {
                return new WaitUntil(() =>
                {
                    return obj.GetCurrentFrame() == frame;
                });
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return null;
        }

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

        /// <summary>
        /// Gives a character a set amount of EXP.
        /// Also handles leveling up and learning new moves.
        /// </summary>
        /// <param name="character">The characters to level up.</param>
        /// <param name="experience">The amount of EXP to gain.</param>
       
        public LuaFunction HandoutEXP;
        
        public Coroutine _HandoutEXP(Character character, int experience)
        {
            return new Coroutine(GroundScene.Instance.HandoutEXP(character, experience));
        }

        /// <summary>
        /// Levels up a character a certain amount of times all at once.
        /// Also handles learning new moves.
        /// </summary>
        /// <param name="character">The characters to level up.</param>
        /// <param name="numLevelUps">The number of level ups.</param>
       
        public LuaFunction LevelUpChar;
        
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

        //
        //
        //


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
