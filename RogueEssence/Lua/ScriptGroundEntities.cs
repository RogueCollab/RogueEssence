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
        // Entities
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

        ///// <summary>
        ///// TODO: WIP
        ///// </summary>
        ///// <param name="objtype"></param>
        ///// <param name="instancename"></param>
        ///// <param name="x"></param>
        ///// <param name="y"></param>
        ///// <param name="w"></param>
        ///// <param name="h"></param>
        ///// <returns></returns>
        //public object CreateObject(string objtype, string instancename, int x, int y, int w, int h)
        //{
        //    try
        //    {
        //        GroundMap map = ZoneManager.Instance.CurrentGround;

        //        GroundObject groundobject = null;
        //        var template = TemplateManager.Instance.FindTemplate(objtype); //Templates are created by the modders, and stored as data (This is handy, because its pretty certain a lot of characters and entities will be repeated throughout the maps)
        //        if (template == null)
        //            return null;

        //        groundobject = (GroundObject)template.create(instancename);
        //        groundobject.Bounds = new Rect(x, y, w, h);
        //        groundobject.ReloadEvents();
        //        map.AddObject(groundobject);
        //        return groundobject; //Object's properties can be tweaked later on

        //    }
        //    catch (Exception ex)
        //    {
        //        DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// TODO: WIP
        ///// </summary>
        ///// <param name="chartype"></param>
        ///// <param name="instancename"></param>
        ///// <param name="x"></param>
        ///// <param name="y"></param>
        ///// <param name="actionfun"></param>
        ///// <param name="thinkfun"></param>
        ///// <returns></returns>
        //public object CreateCharacter(string chartype, string instancename, int x, int y, string actionfun, string thinkfun)
        //{
        //    try
        //    {
        //        GroundMap map = ZoneManager.Instance.CurrentGround;

        //        //Ideally this is how we'd create characters :
        //        /*
        //        GroundChar groundchar = null;
        //            GroundCharTemplate template = CharacterTemplates.Find(chartype); //Templates are created by the modders, and stored as data (This is handy, because its pretty certain a lot of characters and entities will be repeated throughout the maps)
        //            if (template == null)
        //                return null;

        //            groundchar = template.create(instancename, x, y);

        //            groundchar.SetRoutine(thinkfun); //Aka the code that makes the npc wander, or do stuff over and over again
        //            groundchar.SetAction(actionfun);

        //            map.AddMapChar(groundChar);
        //            return groundchar; //Object's properties can be tweaked later on
        //        */
        //        throw new NotImplementedException();
        //    }
        //    catch (Exception ex)
        //    {
        //        DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
        //    }
        //    return null;
        //}

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

                GroundObject objectToRemove = map.FindObject(instancename);
                if (objectToRemove != null)
                {
                    map.RemoveObject(objectToRemove);
                    return true;
                }

                objectToRemove = map.FindTempObject(instancename);
                if (objectToRemove != null)
                {
                    map.RemoveTempObject(objectToRemove);
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
        /// Sets the controllable player to use new character data.
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


    }
}
