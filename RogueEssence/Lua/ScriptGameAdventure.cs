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
        /// Leave current map, and enter specified ground map within the current zone
        /// </summary>
        /// <param name="id">The index of the ground map in the zone</param>
        /// <param name="idxentrypoint">The index of the entry point in the ground map</param>
        /// <param name="preserveMusic">If set to true, does not change the music when moving to the new ground map.</param>
        public void EnterGroundMap(int id, int idxentrypoint, bool preserveMusic = false)
        {
            //Leave current map and enter specific groundmap at the specified entry point
            GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToZone(new ZoneLoc(ZoneManager.Instance.CurrentZoneID, new SegLoc(ZoneManager.Instance.CurrentMapID.Segment, id), idxentrypoint), false, preserveMusic);
        }

        /// <summary>
        /// Leave current map, and enter specified ground map within the current zone
        /// </summary>
        /// <param name="name">The name of the ground map.  It must exist within in the zone.</param>
        /// <param name="entrypoint">The name of the entry point in the ground map</param>
        /// <param name="preserveMusic">If set to true, does not change the music when moving to the new ground map.</param>
        public void EnterGroundMap(string name, string entrypoint, bool preserveMusic = false)
        {
            GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToGround(ZoneManager.Instance.CurrentZoneID, name, entrypoint, preserveMusic);
        }

        /// <summary>
        /// Leave current map, and enter specified ground map within a new zone.
        /// </summary>
        /// <param name="zone">The name of the destination zone.</param>
        /// <param name="name">The name of the ground map.  It must exist within in the zone.</param>
        /// <param name="entrypoint">The name of the entry point in the ground map</param>
        /// <param name="preserveMusic">If set to true, does not change the music when moving to the new ground map.</param>
        public void EnterGroundMap(string zone, string name, string entrypoint, bool preserveMusic = false)
        {
            GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToGround(zone, name, entrypoint, preserveMusic);
        }


        /// <summary>
        /// Enters a zone and begins a new adventure.
        /// </summary>
        /// <example>
        /// GAME:EnterDungeon(1, 0, 0, 0, RogueEssence.Data.GameProgress.DungeonStakes.Risk, true, false)
        /// </example>
        public LuaFunction EnterDungeon;

        /// <summary>
        /// [LuaFunction] EnterDungeon
        /// </summary>
        /// <param name="dungeonid">The id of the dungeon to travel to.</param>
        /// <param name="structureid">The segment within the dungeon to start in.  -1 represents ground maps.</param>
        /// <param name="mapid">The id of the ground map or dungeon map within the dungeon segment.</param>
        /// <param name="entry">The entry point on the resulting map</param>
        /// <param name="stakes">Decides what happens when the adventure fails/succeeds.</param>
        /// <param name="recorded">Record the adventure in a replay</param>
        /// <param name="noRestrict">Do not apply dungeon restrictions</param>
        /// <returns></returns>
        public Coroutine _EnterDungeon(string dungeonid, int structureid, int mapid, int entry, GameProgress.DungeonStakes stakes, bool recorded, bool noRestrict)
        {
            return new Coroutine(GameManager.Instance.BeginGameInSegment(new ZoneLoc(dungeonid, new SegLoc(structureid, mapid), entry), stakes, recorded, noRestrict));
        }

        /// <summary>
        /// Enters a zone and continues the current adventure.  Often used in midpoint rest areas.
        /// </summary>
        /// <example>
        /// GAME:ContinueDungeon(1, 1, 0, 0)
        /// </example>
        public LuaFunction ContinueDungeon;

        /// <summary>
        /// [LuaFunction] ContinueDungeon
        /// </summary>
        /// <param name="dungeonid">The id of the dungeon to travel to.</param>
        /// <param name="structureid">The segment within the dungeon to start in.  -1 represents ground maps.</param>
        /// <param name="mapid">The id of the ground map or dungeon map within the dungeon segment.</param>
        /// <param name="entry">The entry point on the resulting map</param>
        /// <returns></returns>
        public Coroutine _ContinueDungeon(string dungeonid, int structureid, int mapid, int entry)
        {
            return new Coroutine(GameManager.Instance.BeginSegment(new ZoneLoc(dungeonid, new SegLoc(structureid, mapid), entry), false));
        }


        /// <summary>
        /// Ends the current adventure, sending the player to a specified destination.
        /// </summary>
        /// <example>
        /// GAME:EndDungeonRun(GameProgress.ResultType.Cleared, 0, -1, 1, 0, true, true)
        /// </example>
        public LuaFunction EndDungeonRun;

        /// <summary>
        /// [LuaFunction] EndDungeonRun
        /// </summary>
        /// <param name="result">The result of the adventure.</param>
        /// <param name="destzoneid">The id of the dungeon to travel to.</param>
        /// <param name="structureid">The segment within the dungeon to start in.  -1 represents ground maps.</param>
        /// <param name="mapid">The id of the ground map or dungeon map within the dungeon segment.</param>
        /// <param name="entryid">The entry point on the resulting map</param>
        /// <param name="display">Display an epitaph marking the end of the adventure.</param>
        /// <param name="fanfare">Play a fanfare.</param>
        /// <param name="completedZone">Zone to mark as completed. Defaults to current zone.</param>
        /// <returns></returns>
        public Coroutine _EndDungeonRun(GameProgress.ResultType result, string destzoneid, int structureid, int mapid, int entryid, bool display, bool fanfare, string completedZone = null)
        {
            if (String.IsNullOrEmpty(completedZone))
                completedZone = ZoneManager.Instance.CurrentZoneID;
            return new Coroutine(DataManager.Instance.Save.EndGame(result, new ZoneLoc(destzoneid, new SegLoc(structureid, mapid), entryid), display, fanfare, completedZone));
        }

        /// <summary>
        /// Enters a zone and begins a new adventure.
        /// </summary>
        /// <param name="dungeonid">The id of the dungeon to travel to.</param>
        /// <param name="structureid">The segment within the dungeon to start in.  -1 represents ground maps.</param>
        /// <param name="mapid">The id of the ground map or dungeon map within the dungeon segment.</param>
        /// <param name="entry">The entry point on the resulting map</param>
        public void EnterZone(string dungeonid, int structureid, int mapid, int entry)
        {
            GameManager.Instance.SceneOutcome = GameManager.Instance.MoveToZone(new ZoneLoc(dungeonid, new SegLoc(structureid, mapid), entry));
        }


        /// <summary>
        /// Checks if the current game is in rogue mode.
        /// </summary>
        /// <returns>True if in rogue mode, false otherwise.</returns>
        public bool InRogueMode()
        {
            return DataManager.Instance.Save is RogueProgress;
        }

        /// <summary>
        /// Gets the random seed for the current adventure.
        /// </summary>
        /// <returns>The current adventure's seed.</returns>
        public ulong GetDailySeed()
        {
            return DataManager.Instance.Save.Rand.FirstSeed;
        }

    }
}
