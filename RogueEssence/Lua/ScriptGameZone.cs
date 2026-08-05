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
        /// <summary>
        /// Gets the current ground map.
        /// </summary>
        /// <returns></returns>
        public GroundMap GetCurrentGround()
        {
            return ZoneManager.Instance.CurrentGround;
        }

        /// <summary>
        /// Gets the current dungeon map.
        /// </summary>
        /// <returns></returns>
        public Map GetCurrentFloor()
        {
            return ZoneManager.Instance.CurrentMap;
        }

        /// <summary>
        /// Gets the current zone, also known as dungeon.
        /// </summary>
        /// <returns></returns>
        public Zone GetCurrentDungeon()
        {
            return ZoneManager.Instance.CurrentZone;
        }

        /// <summary>
        /// Unlocks a specified dungeon.
        /// </summary>
        /// <param name="dungeonid">ID of the dungeon to unlock.</param>
        public void UnlockDungeon(string dungeonid)
        {
            DataManager.Instance.Save.UnlockDungeon(dungeonid);
        }

        /// <summary>
        /// Checks if a dungeon is unlocked.
        /// </summary>
        /// <param name="dungeonid">ID of the dungeon to check</param>
        /// <returns>True if unlocked, false otherwise.</returns>
        public bool DungeonUnlocked(string dungeonid)
        {
            return DataManager.Instance.Save.GetDungeonUnlock(dungeonid) != GameProgress.UnlockState.None;
        }


        /// <summary>
        /// Leave current map and load up the title screen.
        /// </summary>
        public void RestartToTitle()
        {
            GameManager.Instance.SceneOutcome = GameManager.Instance.RestartToTitle();
        }

        /// <summary>
        /// Restarts a Roguelocke run based on the configuration
        /// </summary>
        ///  <param name="config">The configuration of the roguelocke run</param>
        public void RestartRogue(RogueConfig config)
        {
            GameManager.Instance.SceneOutcome = GameManager.Instance.RestartToRogue(config);
        }


        /// <summary>
        /// Sets the game in cutscene mode. This prevents characters from taking idle action and hides certain UI.
        /// </summary>
        /// <param name="bon">If set to true, turns cutscene mode on. If set to false, turns it off.</param>
        public void CutsceneMode(bool bon)
        {
            int newIdle = bon ? 0 : Content.GraphicsManager.IdleAction;

            //only if switching off non-anim
            if (newIdle != Content.GraphicsManager.GlobalIdle && Content.GraphicsManager.GlobalIdle == 0)
            {
                //iterate all entities on the map that are in an idle anim, and reset their anim
                if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                {
                    GroundMap map = ZoneManager.Instance.CurrentGround;
                    foreach (GroundChar groundChar in map.IterateCharacters())
                    {
                        IdleGroundAction action = groundChar.GetCurrentAction() as IdleGroundAction;
                        if (action != null)
                            action.RestartAnim();
                    }
                }
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    Map map = ZoneManager.Instance.CurrentMap;
                    foreach (Character dungeonChar in map.IterateCharacters())
                    {
                        //TODO: dungeonChar.StartAnim?
                        //it's very protected right now.
                    }
                }
            }

            Content.GraphicsManager.GlobalIdle = newIdle;
            DataManager.Instance.Save.CutsceneMode = bon;
        }



    }
}
