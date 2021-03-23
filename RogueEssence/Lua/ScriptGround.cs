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
        public void Hide(string entityname)
        {
            GroundEntity found = ZoneManager.Instance.CurrentGround.FindEntity(entityname);
            if(found == null)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptGround.Hide({0}): Couldn't find entity to hide!", entityname));
                return;
            }
            //Hide the entity
            found.EntEnabled = false;
        }

        public void Unhide(string entityname)
        {
            GroundEntity found = ZoneManager.Instance.CurrentGround.FindEntity(entityname);
            if (found == null)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptGround.Unhide({0}): Couldn't find entity to un-hide!", entityname));
                return;
            }
            //Enable the entity
            found.EntEnabled = true;
        }

        //===================================
        //  Custscene stuff
        //===================================


        //===================================
        // Objects and Characters
        //===================================
        public object CreateObject(string objtype, string instancename, int x, int y, int w, int h )
        {
            GroundMap map = ZoneManager.Instance.CurrentGround;

            if (map == null)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptGround.CreateObject({0}, {1}, {2}, {3}, {4}, {5}) : No ground map loaded!", objtype, instancename, x, y, w, h) );
                return null;
            }

            GroundObject groundobject = null;
            var template = TemplateManager.Instance.FindTemplate(objtype); //Templates are created by the modders, and stored as data (This is handy, because its pretty certain a lot of characters and entities will be repeated throughout the maps)
            if (template == null)
                return null;

            groundobject = (GroundObject)template.create(instancename);
            groundobject.Bounds = new Rect(x,y,w,h);
            groundobject.AddScriptEvent(LuaEngine.EEntLuaEventTypes.Action);
            map.AddObject(groundobject);
            return groundobject; //Object's properties can be tweaked later on
        }

        public object CreateCharacter(string chartype, string instancename, int x, int y, string actionfun, string thinkfun)
        {
            GroundMap map = ZoneManager.Instance.CurrentGround;

            if (map == null)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptGround.CreateCharacter({0},{1},{2},{3})", chartype, instancename, x, y));
                return null;
            }

            GroundChar groundchar = null;


            //Ideally this is how we'd create characters :
            /*
                GroundCharTemplate template = CharacterTemplates.Find(chartype); //Templates are created by the modders, and stored as data (This is handy, because its pretty certain a lot of characters and entities will be repeated throughout the maps)
                if (template == null)
                    return null;

                groundchar = template.create(instancename, x, y);

                groundchar.SetRoutine(thinkfun); //Aka the code that makes the npc wander, or do stuff over and over again
                groundchar.SetAction(actionfun);

                map.AddMapChar(groundChar);
                return groundchar; //Object's properties can be tweaked later on
            */
            return groundchar;
        }

        public bool RemoveObject(string instancename)
        {
            GroundMap map = ZoneManager.Instance.CurrentGround;
            if (map == null)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptGround.RemoveObject({0}) : No ground map loaded!", instancename));
                return false;
            }

            DiagManager.Instance.LogInfo("ScriptGround.RemoveObject():FIXME"); //!#FIXME
            /*
            map.RemoveObject(instancename); //Removal by instance name, since lua can't do via .NET pointer reliably, and pointers to .NET aren't practical in lua
            */
            return true;
        }

        public bool RemoveCharacter(string instancename)
        {
            GroundMap map = ZoneManager.Instance.CurrentGround;
            if (map == null)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptGround.RemoveObject({0}) : No ground map loaded!", instancename));
                return false;
            }

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

            return false;
        }


        public GroundChar CreateCharacterFromCharData(string instancename, Character data, int x = 0, int y = 0, Dir8 direction = Dir8.Down)
        {
            GroundChar groundChar = new GroundChar(data, new Loc(x, y), direction, instancename);
            ZoneManager.Instance.CurrentGround.AddMapChar(groundChar);
            return groundChar;
        }


        public void RefreshPlayer()
        {
            GroundChar leaderChar = GroundScene.Instance.FocusedCharacter;
            ZoneManager.Instance.CurrentGround.SetPlayerChar(new GroundChar(DataManager.Instance.Save.ActiveTeam.Leader, leaderChar.MapLoc, leaderChar.CharDir, "PLAYER"));
        }

        /// <summary>
        /// Make the specified spawner run its spawn method.
        /// </summary>
        /// <param name="spawnername"></param>
        /// <returns></returns>
        public GroundChar SpawnerDoSpawn(string spawnername)
        {
            if (ZoneManager.Instance.CurrentGround == null)
                DiagManager.Instance.LogInfo("[SE]:ScriptGround.SpawnerDoSpawn(): No ground map loaded!!");
            else
            {
                GroundSpawner spwn = ZoneManager.Instance.CurrentGround.GetSpawner(spawnername);
                if (spwn != null)
                    return spwn.Spawn(ZoneManager.Instance.CurrentGround);
                else
                    DiagManager.Instance.LogInfo(String.Format("[SE]:ScriptGround.SpawnerDoSpawn(): Couldn't find spawner named {0} on map {1}!",
                                                                spawnername,
                                                                ZoneManager.Instance.CurrentGround.AssetName));
            }
            return null;
        }


        /// <summary>
        /// Sets the character to the specified spawner
        /// </summary>
        /// <param name="spawnername"></param>
        /// <param name="spawnChar"></param>
        /// <returns></returns>
        public void SpawnerSetSpawn(string spawnername, Character spawnChar)
        {
            if (ZoneManager.Instance.CurrentGround == null)
                DiagManager.Instance.LogInfo("[SE]:ScriptGround.SpawnerDoSpawn(): No ground map loaded!!");
            else
            {
                GroundSpawner spwn = ZoneManager.Instance.CurrentGround.GetSpawner(spawnername);
                if (spwn != null)
                    spwn.NPCChar = spawnChar;
                else
                    DiagManager.Instance.LogInfo(String.Format("[SE]:ScriptGround.SpawnerDoSpawn(): Couldn't find spawner named {0} on map {1}!",
                                                                spawnername,
                                                                ZoneManager.Instance.CurrentGround.AssetName));
            }
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
        public void CharSetEmote(GroundChar chara, int emoteid, int cycles)
        {
            if (chara != null)
            {
                EmoteData emote = DataManager.Instance.GetEmote(emoteid);
                chara.StartEmote(new Emote(emote.Anim, emote.LocHeight, cycles));
            }
        }

        /// <summary>
        /// Set a character's animation
        /// </summary>
        /// <param name="chara"></param>
        /// <param name="anim"></param>
        public void CharSetAnim(GroundChar chara, string anim, bool loop)
        {
            int animIndex = GraphicsManager.Actions.FindIndex((CharFrameType element) => element.Name == anim);
            chara.StartAction(new IdleAnimGroundAction(chara.Position, chara.Direction, animIndex, loop));
        }

        public void CharPoseAnim(GroundChar chara, string anim)
        {
            int animIndex = GraphicsManager.Actions.FindIndex((CharFrameType element) => element.Name == anim);
            chara.StartAction(new PoseGroundAction(chara.Position, chara.Direction, animIndex));
        }

        //===================================
        //  VFX
        //===================================



        /// <summary>
        /// Plays a VFX
        /// </summary>
        /// <param name="chara"></param>
        /// <param name="anim"></param>
        public void PlayVFX(FiniteEmitter emitter, int x, int y, Dir8 dir = Dir8.Down, int xTo = -1, int yTo = -1)
        {
            FiniteEmitter endEmitter = (FiniteEmitter)emitter.Clone();
            Loc endLoc = (x > -1) ? new Loc(x, y) : Loc.Zero;
            endEmitter.SetupEmit(new Loc(x, y), endLoc, dir);
            GroundScene.Instance.CreateAnim(endEmitter, DrawLayer.NoDraw);
        }

        public void MoveScreen(ScreenMover mover)
        {
            GroundScene.Instance.SetScreenShake(new ScreenMover(mover));
        }

        public void AddMapStatus(int statusIdx)
        {
            MapStatus status = new MapStatus(statusIdx);
            status.LoadFromData();
            GroundScene.Instance.AddMapStatus(status);
        }

        public void RemoveMapStatus(int statusIdx)
        {
            GroundScene.Instance.RemoveMapStatus(statusIdx);
        }




        //===================================
        //  Movement
        //===================================

        /// <summary>
        /// Makes a character turn to face another
        /// </summary>
        /// <param name="curch"></param>
        /// <param name="turnto"></param>
        public void CharTurnToChar(GroundChar curch, GroundChar turnto)
        {
            if (curch == null || turnto == null)
                return;
            Dir8 destDir = DirExt.ApproximateDir8(turnto.MapLoc - curch.MapLoc);
            if (destDir == Dir8.None)
                destDir = turnto.CharDir.Reverse();
            curch.CharDir = destDir;
        }



        /// <summary>
        ///
        /// </summary>
        /// <param name="curch"></param>
        /// <param name="turnto"></param>
        /// <param name="framedur"></param>
        /// <returns></returns>
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
        /// Make an entity immediately turn towards a direction
        /// </summary>
        /// <param name="curch"></param>
        /// <param name="direction"></param>
        public void EntTurn(GroundEntity ent, Dir8 direction)
        {
            if (ent == null || direction == Dir8.None)
                return;
            ent.Direction = direction;
        }

        /// <summary>
        /// Do an animated turn
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="direction"></param>
        /// <param name="framedur"></param>
        public LuaFunction CharAnimateTurn;
        public Coroutine _CharAnimateTurn(GroundChar ch, Dir8 direction, int framedur, bool ccw)
        {
            if (ch == null || direction == Dir8.None)
                return new Coroutine(LuaEngine._DummyWait());
            return new Coroutine(_DoAnimatedTurn(ch, _CountDirectionDifference(ch.CharDir, direction), framedur, ccw));
        }

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
            curch.StartAction(new IdleNoAnim(curch.MapLoc, curch.CharDir));
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
                Debug.Assert(curch.CharDir != Dir8.None, "ScriptGround._DoAnimatedTurn(): Result of turn was none! Something went wrong!");
                oldact.MapLoc = curch.MapLoc;
                oldact.CharDir = curch.CharDir;
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
        /// Make an entity move in a direction
        /// </summary>
        /// <returns></returns>
        public Coroutine _MoveInDirection(GroundChar chara, Dir8 direction, int duration)
        {
            return new Coroutine(__MoveInDirection(chara, direction, duration));
        }

        public LuaFunction MoveInDirection;

        private IEnumerator<YieldInstruction> __MoveInDirection(GroundChar chara, Dir8 direction, int duration)
        {
            if (chara == null)
            {
                DiagManager.Instance.LogInfo("[SE]:ScriptGround.__MoveInDirection(): Target character is null!");
                yield break;
            }

            for (int ii = 0; ii < duration; ++ii)
            {
                chara.CurrentCommand = new GameAction(GameAction.ActionType.Move, direction, 0);
                yield return new WaitForFrames(1);
            }
            yield break;
        }

        public void TeleportTo(object ent, int x, int y, Dir8 direction = Dir8.None)
        {
            if (ent is GroundChar)
            {
                GroundChar gent = ent as GroundChar;
                if (gent != null)
                {
                    gent.SetMapLoc(new Loc(x, y));
                    gent.UpdateFrame();
                    gent.Direction = direction;
                }
            }
            else
                DiagManager.Instance.LogInfo("ScriptGround.TeleportTo(): Got invalid entity!");
        }

        /// <summary>
        /// Makes an entity move to the selected position over a certain time.
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public LuaFunction MoveToPosition;
        public YieldInstruction _MoveToPosition(GroundEntity ent, int x, int y, bool run = false, int speed = 2)
        {
            if (speed < 1)
            {
                DiagManager.Instance.LogInfo("ScriptGround.MoveToPosition(): Got invalid walk speed!");
                return null;
            }

            if (ent is GroundChar)
            {
                GroundChar ch = (GroundChar)ent;
                FrameTick prevTime = new FrameTick();
                GroundAction prevAction = ch.GetCurrentAction();
                if (prevAction is WalkToPositionGroundAction)
                    prevTime = prevAction.ActionTime;
                WalkToPositionGroundAction newAction = new WalkToPositionGroundAction(ch.Position, ch.Direction, run, speed, prevTime, new Loc(x, y));
                ch.StartAction(newAction);
                return new WaitUntil(() =>
                {
                    return newAction.Complete;
                });
            }
            else
                DiagManager.Instance.LogInfo("ScriptGround.MoveToPosition(): Got invalid entity!");
            return null;
        }

        /// <summary>
        /// Makes an entity move to a marker.
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
        public LuaFunction MoveToMarker;
        public YieldInstruction _MoveToMarker(GroundEntity ent, GroundMarker mark, bool run = false, int speed = 2)
        {
            return _MoveToPosition(ent, mark.X, mark.Y, run, speed);
        }

        //
        //
        //


        public override void SetupLuaFunctions(LuaEngine state)
        {
            //Implement stuff that should be written in lua!
            MoveInDirection = state.RunString("return function(_,chara, direction, duration) return coroutine.yield(GROUND:_MoveInDirection(chara, direction, duration)) end", "MoveInDirection").First() as LuaFunction;
            CharAnimateTurn = state.RunString("return function(_,ch, direction, framedur, ccw) return coroutine.yield(GROUND:_CharAnimateTurn(ch, direction, framedur, ccw)) end", "CharAnimateTurn").First() as LuaFunction;
            CharAnimateTurnTo = state.RunString("return function(_,ch, direction, framedur) return coroutine.yield(GROUND:_CharAnimateTurnTo(ch, direction, framedur)) end", "CharAnimateTurn").First() as LuaFunction;
            CharTurnToCharAnimated = state.RunString("return function(_,curch, turnto, framedur) return coroutine.yield(GROUND:_CharTurnToCharAnimated(curch, turnto, framedur)) end", "CharTurnToCharAnimated").First() as LuaFunction;

            MoveToMarker = state.RunString("return function(_,ent, mark, shouldrun, speed) return coroutine.yield(GROUND:_MoveToMarker(ent, mark, shouldrun, speed)) end", "MoveToMarker").First() as LuaFunction;
            MoveToPosition = state.RunString("return function(_,ent, x, y, shouldrun, speed) return coroutine.yield(GROUND:_MoveToPosition(ent, x, y, shouldrun, speed)) end", "MoveToPosition").First() as LuaFunction;
        }
    }
}
