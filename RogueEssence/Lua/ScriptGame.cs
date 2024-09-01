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
    class ScriptGame : ILuaEngineComponent
    {
        /// <summary>
        /// The game's random object.  Is not recorded in replays.
        /// </summary>
        public IRandom Rand { get { return MathUtils.Rand; } }

        /// <summary>
        /// Saves the game while in ground mode.
        /// </summary>
        /// <example>
        /// GAME:GroundSave()
        /// </example>
        public LuaFunction GroundSave;

        public Coroutine _GroundSave()
        {
            return new Coroutine(GroundScene.Instance.SaveGame());
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="uuidStr"></param>
        /// <returns></returns>
        public ModDiff GetModDiff(string uuidStr)
        {
            Guid uuid = new Guid(uuidStr);
            List<ModDiff> diffs = DataManager.Instance.Save.GetModDiffs();
            foreach (ModDiff diff in diffs)
            {
                if (diff.UUID == uuid)
                    return diff;
            }
            return new ModDiff("", uuid, null, null);
        }

        //===================================
        // Current Map
        //===================================

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


        //===================================
        // Enter/leave methods
        //===================================

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
        /// <param name="dungeonid">The id of the dungeon to travel to.</param>
        /// <param name="structureid">The segment within the dungeon to start in.  -1 represents ground maps.</param>
        /// <param name="mapid">The id of the ground map or dungeon map within the dungeon segment.</param>
        /// <param name="entry">The entry point on the resulting map</param>
        /// <param name="stakes">Decides what happens when the adventure fails/succeeds.</param>
        /// <param name="recorded">Record the adventure in a replay</param>
        /// <param name="silentRestrict">Make the dungeon restrictions silently</param>
        /// <example>
        /// GAME:EnterDungeon(1, 0, 0, 0, RogueEssence.Data.GameProgress.DungeonStakes.Risk, true, false)
        /// </example>
        public LuaFunction EnterDungeon;

        public Coroutine _EnterDungeon(string dungeonid, int structureid, int mapid, int entry, GameProgress.DungeonStakes stakes, bool recorded, bool silentRestrict)
        {
            return new Coroutine(GameManager.Instance.BeginGameInSegment(new ZoneLoc(dungeonid, new SegLoc(structureid, mapid), entry), stakes, recorded, silentRestrict));
        }

        /// <summary>
        /// Enters a zone and continues the current adventure.
        /// </summary>
        /// <param name="dungeonid">The id of the dungeon to travel to.</param>
        /// <param name="structureid">The segment within the dungeon to start in.  -1 represents ground maps.</param>
        /// <param name="mapid">The id of the ground map or dungeon map within the dungeon segment.</param>
        /// <param name="entry">The entry point on the resulting map</param>
        /// <example>
        /// GAME:ContinueDungeon(1, 1, 0, 0)
        /// </example>
        public LuaFunction ContinueDungeon;
        public Coroutine _ContinueDungeon(string dungeonid, int structureid, int mapid, int entry)
        {
            return new Coroutine(GameManager.Instance.BeginSegment(new ZoneLoc(dungeonid, new SegLoc(structureid, mapid), entry), false));
        }


        /// <summary>
        /// Ends the current adventure, sending the player to a specified destination.
        /// </summary>
        /// <param name="result">The result of the adventure.</param>
        /// <param name="destzoneid">The id of the dungeon to travel to.</param>
        /// <param name="structureid">The segment within the dungeon to start in.  -1 represents ground maps.</param>
        /// <param name="mapid">The id of the ground map or dungeon map within the dungeon segment.</param>
        /// <param name="entryid">The entry point on the resulting map</param>
        /// <param name="display">Display an epitaph marking the end of the adventure.</param>
        /// <param name="fanfare">Play a fanfare.</param>
        /// <param name="completedZone">Zone to mark as completed. Defaults to current zone.</param>
        /// <example>
        /// GAME:EndDungeonRun(GameProgress.ResultType.Cleared, 0, -1, 1, 0, true, true)
        /// </example>
        public LuaFunction EndDungeonRun;

        public Coroutine _EndDungeonRun(GameProgress.ResultType result, string destzoneid, int structureid, int mapid, int entryid, bool display, bool fanfare, string completedZone = null)
        {
            if (String.IsNullOrEmpty(completedZone))
                completedZone = ZoneManager.Instance.CurrentZoneID;
            return new Coroutine(DataManager.Instance.Save.EndGame(result, new ZoneLoc(destzoneid, new SegLoc(structureid, mapid), entryid), display, fanfare, completedZone));
        }

        /// <summary>
        /// Enters a zone and begins a rescue adventure.
        /// </summary>
        /// <param name="sosPath">The path of the sos mail.</param>
        /// <example>
        /// GAME:EnterRescue("RESCUE/INBOX/SOS/example.sosmail")
        /// </example>
        public LuaFunction EnterRescue;
        public Coroutine _EnterRescue(string sosPath)
        {
            return new Coroutine(GameManager.Instance.BeginRescue(sosPath));
        }

        /// <summary>
        /// TODO: WIP
        /// </summary>
        /// <param name="remarkIndex"></param>
        public void AddAOKRemark(int remarkIndex)
        {
            AOKMail aok = null;
            if (DataManager.Instance.Save.GeneratedAOK != null)
                aok = DataManager.LoadRescueMail(PathMod.FromApp(DataManager.RESCUE_OUT_PATH + DataManager.AOK_FOLDER + DataManager.Instance.Save.GeneratedAOK)) as AOKMail;
            if (aok != null)
            {
                aok.FinalStatement = remarkIndex;
                DataManager.SaveRescueMail(DataManager.Instance.Save.GeneratedAOK, aok);
            }
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
        /// Fade out the screen. Waits to complete before continuing.
        /// </summary>
        /// <param name="white">Fade to white if set to true.  Fades to black otherwise.</param>
        /// <param name="duration">The amount of time to fade in frames.</param>
        /// <example>
        /// GAME:FadeOut(false, 60)
        /// </example>
        public LuaFunction FadeOut;

        public Coroutine _FadeOut(bool white, int duration)
        {
            return new Coroutine(GameManager.Instance.FadeOut(white, duration));
        }

        /// <summary>
        /// Fade into the screen. Waits to complete before continuing.
        /// </summary>
        /// <param name="duration">The amount of time to fade in frames.</param>
        /// <example>
        /// GAME:FadeIn(false, 60)
        /// </example>
        public LuaFunction FadeIn;
        public Coroutine _FadeIn(int duration)
        {
            return new Coroutine(GameManager.Instance.FadeIn(duration));
        }

        /// <summary>
        /// Centers the camera on a position.
        /// </summary>
        /// <param name="x">X coordinate of the camera center</param>
        /// <param name="y">Y coordinate of the camera center</param>
        /// <param name="duration">The amount of time it takes ot move to the destination</param>
        /// <param name="toPlayer">Destination is in absolute coordinates if false, and relative to the player character if set to true.</param>
        /// <example>
        /// GAME:MoveCamera(200, 240, 60, false)
        /// </example>
        public LuaFunction MoveCamera;

        public Coroutine _MoveCamera(int x, int y, int duration, bool toPlayer = false)
        {
            return new Coroutine(GroundScene.Instance.MoveCamera(new Loc(x, y), duration, toPlayer));
        }

        /// <summary>
        /// Centers the camera on a character.
        ///
        /// As we are simply moving the camera to a character, this will simply set ViewCenter and not ViewOffset.
        /// </summary>
        /// <param name="x">X coordinate of the camera center, as an offset for the chara</param>
        /// <param name="y">Y coordinate of the camera center, as an offset for the chara</param>
        /// <param name="duration">The amount of time it takes ot move to the destination</param>
        /// <param name="chara">The character to center on.</param>
        /// <example>
        /// GAME:MoveCameraToChara(200, 240, 60, false)
        /// </example>
        public LuaFunction MoveCameraToChara;
        
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

        //===================================
        // Mail
        //===================================

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public bool HasSOSMail()
        {
            string parentPath = PathMod.FromApp(DataManager.RESCUE_IN_PATH + DataManager.SOS_FOLDER);
            string[] files = System.IO.Directory.GetFiles(parentPath, "*" + DataManager.SOS_EXTENSION);
            return files.Length > 0;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public bool HasAOKMail()
        {
            string parentPath = PathMod.FromApp(DataManager.RESCUE_OUT_PATH + DataManager.AOK_FOLDER);
            string[] files = System.IO.Directory.GetFiles(parentPath, "*" + DataManager.AOK_EXTENSION);
            return files.Length > 0;
        }



        //===================================
        // Team Access
        //===================================

        /// <summary>
        /// Returns the index of the currently player controlled entity in the party.
        /// </summary>
        /// <returns>Index of the currently player controlled entity in the party.</returns>
        public int GetTeamLeaderIndex()
        {
            return DataManager.Instance.Save.ActiveTeam.LeaderIndex;
        }

        /// <summary>
        /// Sets the leader to the chosen index within the party.
        /// </summary>
        /// <param name="idx">The index of the team member within the team.</param>
        public void SetTeamLeaderIndex(int idx)
        {
            //make leader
            int oldIdx = DataManager.Instance.Save.ActiveTeam.LeaderIndex;
            DataManager.Instance.Save.ActiveTeam.LeaderIndex = idx;

            //update team
            if (GameManager.Instance.CurrentScene == GroundScene.Instance)
            {
                ZoneManager.Instance.CurrentGround.SetPlayerChar(new GroundChar(DataManager.Instance.Save.ActiveTeam.Leader,
                    ZoneManager.Instance.CurrentGround.ActiveChar.MapLoc,
                    ZoneManager.Instance.CurrentGround.ActiveChar.CharDir, "PLAYER"));
            }
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                ZoneManager.Instance.CurrentMap.CurrentTurnMap.AdjustLeaderSwap(Faction.Player, 0, false, oldIdx, idx);
                DungeonScene.Instance.ReloadFocusedPlayer();
            }
        }

        /// <summary>
        /// Prevents or allows the switching of leaders for the save file.
        /// </summary>
        /// <param name="canSwitch">Set to true to allow switching, set to false to prevent it.</param>
        public void SetCanSwitch(bool canSwitch)
        {
            DataManager.Instance.Save.NoSwitching = !canSwitch;
        }

        /// <summary>
        /// Prevents or allows the joining of recruits for the save file.
        /// </summary>
        /// <param name="canRecruit">Set to true to allow recruit joins, set to false to prevent it.</param>
        public void SetCanRecruit(bool canRecruit)
        {
            DataManager.Instance.Save.NoRecruiting = !canRecruit;
        }

        /// <summary>
        /// Returns the player party count.  Does not include guests.
        /// </summary>
        /// <returns>The count of players</returns>
        public int GetPlayerPartyCount()
        {
            return DataManager.Instance.Save.ActiveTeam.Players.Count;
        }

        /// <summary>
        /// Return the party as a LuaTable.  Does not include guests.
        /// </summary>
        /// <returns>A Lua Table of Characters</returns>
        public LuaTable GetPlayerPartyTable()
        {
            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, chara) table.insert(tbl, chara) end").First() as LuaFunction;
            foreach (Character ent in DataManager.Instance.Save.ActiveTeam.Players)
                addfn.Call(tbl, ent);
            return tbl;
        }

        /// <summary>
        /// Gets the character at the specified index within the player's team.
        /// </summary>
        /// <param name="index">The specified index</param>
        /// <returns>The team member retrieved.</returns>
        public Character GetPlayerPartyMember(int index)
        {
            return DataManager.Instance.Save.ActiveTeam.Players[index];
        }

        /// <summary>
        /// Gets the number of guests currently in the player's party.
        /// </summary>
        /// <returns>The number of guests</returns>
        public int GetPlayerGuestCount()
        {
            return DataManager.Instance.Save.ActiveTeam.Guests.Count;
        }

        /// <summary>
        /// Return the guests as a LuaTable
        /// </summary>
        /// <returns>A Lua Table of Characters</returns>
        public LuaTable GetPlayerGuestTable()
        {
            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, chara) table.insert(tbl, chara) end").First() as LuaFunction;
            foreach (var ent in DataManager.Instance.Save.ActiveTeam.Guests)
                addfn.Call(tbl, ent);
            return tbl;
        }

        /// <summary>
        /// Gets the character at the specified index within the player's guests.
        /// </summary>
        /// <param name="index">The specified index</param>
        /// <returns>The team member retrieved.</returns>
        public Character GetPlayerGuestMember(int index)
        {
            return DataManager.Instance.Save.ActiveTeam.Guests[index];
        }

        /// <summary>
        /// Gets the number of characters currently in the player's assembly.
        /// </summary>
        /// <returns>The number of characters</returns>
        public object GetPlayerAssemblyCount()
        {
            return DataManager.Instance.Save.ActiveTeam.Assembly.Count;
        }

        /// <summary>
        /// Return the assembly as a LuaTable
        /// </summary>
        /// <returns>A Lua Table of Characters</returns>
        public LuaTable GetPlayerAssemblyTable()
        {
            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, chara) table.insert(tbl, chara) end").First() as LuaFunction;
            foreach (var ent in DataManager.Instance.Save.ActiveTeam.Assembly)
                addfn.Call(tbl, ent);
            return tbl;
        }

        /// <summary>
        /// Gets the character at the specified index within the player's assembly.
        /// </summary>
        /// <param name="index">The specified index</param>
        /// <returns>The assembly member retrieved.</returns>
        public Character GetPlayerAssemblyMember(int index)
        {
            return DataManager.Instance.Save.ActiveTeam.Assembly[index];
        }


        /// <summary>
        /// Adds a character to the player's team.
        /// </summary>
        /// <param name="character">The character to add.</param>
        public void AddPlayerTeam(Character character)
        {
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                DungeonScene.Instance.AddCharToTeam(Faction.Player, 0, false, character);
                character.RefreshTraits();
                character.Tactic.Initialize(character);
            }
            else
            {
                DataManager.Instance.Save.ActiveTeam.Players.Add(character);
            }
        }

        /// <summary>
        /// Removes the character from the team, placing its item back in the inventory.
        /// </summary>
        /// <param name="slot">The slot of the player to remove.</param>
        public void RemovePlayerTeam(int slot)
        {
            Character player = DataManager.Instance.Save.ActiveTeam.Players[slot];

            if (GameManager.Instance.CurrentScene == GroundScene.Instance)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                {
                    InvItem heldItem = player.EquippedItem;
                    player.SilentDequipItem();
                    DataManager.Instance.Save.ActiveTeam.AddToInv(heldItem);
                }

                GroundScene.Instance.RemoveChar(slot);
            }
            else if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                {
                    InvItem heldItem = player.EquippedItem;
                    player.SilentDequipItem();
                    if (DataManager.Instance.Save.ActiveTeam.GetInvCount() + 1 < DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone))
                        DataManager.Instance.Save.ActiveTeam.AddToInv(heldItem);
                }

                DungeonScene.Instance.RemoveChar(new CharIndex(Faction.Player, 0, false, slot));
            }
            else
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                {
                    InvItem heldItem = player.EquippedItem;
                    player.SilentDequipItem();
                    DataManager.Instance.Save.ActiveTeam.AddToInv(heldItem);
                }

                ExplorerTeam team = DataManager.Instance.Save.ActiveTeam;

                team.Players.RemoveAt(slot);

                //update leader
                if (slot < team.LeaderIndex)
                    team.LeaderIndex--;
            }
        }

        /// <summary>
        /// Adds a character to the player's guests.
        /// </summary>
        /// <param name="character">The character to add.</param>
        public void AddPlayerGuest(Character character)
        {
            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                DungeonScene.Instance.AddCharToTeam(Faction.Player, 0, true, character);
                character.RefreshTraits();
                character.Tactic.Initialize(character);
            }
            else
            {
                DataManager.Instance.Save.ActiveTeam.Guests.Add(character);
            }
        }

        /// <summary>
        /// Removes the character from the team's guests, placing its item back in the inventory.
        /// </summary>
        /// <param name="slot">The slot of the player to remove.</param>
        public void RemovePlayerGuest(int slot)
        {
            Character player = DataManager.Instance.Save.ActiveTeam.Guests[slot];

            if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                player.SilentDequipItem();

            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                DungeonScene.Instance.RemoveChar(new CharIndex(Faction.Player, 0, true, slot));
            else
                DataManager.Instance.Save.ActiveTeam.Guests.RemoveAt(slot);
        }

        /// <summary>
        /// Adds a character to the player's assembly.
        /// </summary>
        /// <param name="character">The character to add.</param>
        public void AddPlayerAssembly(Character character)
        {
            DataManager.Instance.Save.ActiveTeam.Assembly.Insert(0, character);
        }

        /// <summary>
        /// Removes the character from the assembly, placing its item back in the inventory.
        /// </summary>
        /// <param name="slot">The slot of the player to remove.</param>
        public void RemovePlayerAssembly(int slot)
        {
            DataManager.Instance.Save.ActiveTeam.Assembly.RemoveAt(slot);
        }

        /// <summary>
        /// Sets a character's nickname
        /// </summary>
        /// <param name="character">The character to rename</param>
        /// <param name="nickname">The new name</param>
        public void SetCharacterNickname(Character character, string nickname)
        {
            character.Nickname = nickname;
        }

        /// <summary>
        /// Gets the character nickname
        /// </summary>
        /// <param name="character">The character to get the nickname from</param>
        /// <returns>The character's nickname</returns>
        public string GetCharacterNickname(Character character)
        {
            return character.Nickname;
        }

        /// <summary>
        /// Sets the name of the player's team
        /// </summary>
        /// <param name="teamname">The new team name</param>
        public void SetTeamName(string teamname)
        {
            DataManager.Instance.Save.ActiveTeam.Name = teamname;
            foreach (Character chara in DataManager.Instance.Save.ActiveTeam.Players)
                chara.OriginalTeam = DataManager.Instance.Save.ActiveTeam.Name;
            foreach(Character chara in DataManager.Instance.Save.ActiveTeam.Assembly)
                chara.OriginalTeam = DataManager.Instance.Save.ActiveTeam.Name;
        }

        /// <summary>
        /// Gets the name of the player's team
        /// </summary>
        /// <returns>The team's name</returns>
        public string GetTeamName()
        {
            return DataManager.Instance.Save.ActiveTeam.GetDisplayName();
        }

        /// <summary>
        /// Checks if the character can relearn any skills.
        /// </summary>
        /// <param name="character">The character to check</param>
        public bool CanRelearn(Character character)
        {
            return character.GetRelearnableSkills(true).Count > 0;
        }

        /// <summary>
        /// Checks if the character can forget any skills.
        /// </summary>
        /// <param name="character">The character to check</param>
        public bool CanForget(Character character)
        {
            int count = 0;
            foreach (SlotSkill skill in character.BaseSkills)
            {
                if (!String.IsNullOrEmpty(skill.SkillNum))
                    count++;
            }
            return count > 1;
        }

        /// <summary>
        /// Checks if the character can learn any skills.
        /// </summary>
        /// <param name="character">The character to check</param>
        public bool CanLearn(Character character)
        {
            foreach (SlotSkill skill in character.BaseSkills)
            {
                if (String.IsNullOrEmpty(skill.SkillNum))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks the levels gained by a character and prompts to learn all skills along the levels.
        /// Waits until all skills have been accepted or declined before continuing.
        /// </summary>
        /// <param name="chara">The character to prompt for learning.</param>
        /// <param name="oldLevel">The level that the character leveled up from.</param>
        /// <example>
        /// GAME:CheckLevelSkills(player, 5)
        /// </example>
        public LuaFunction CheckLevelSkills;
        public Coroutine _CheckLevelSkills(Character chara, int oldLevel)
        {
            return new Coroutine(__CheckLevelSkills(chara, oldLevel));
        }

        private IEnumerator<YieldInstruction> __CheckLevelSkills(Character chara, int oldLevel)
        {
            DungeonScene.GetLevelSkills(chara, oldLevel);

            foreach (string skill in DungeonScene.GetLevelSkills(chara, oldLevel))
            {
                int learn = -1;
                if (DataManager.Instance.CurrentReplay != null)
                    learn = DataManager.Instance.CurrentReplay.ReadUI();
                else
                {
                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.TryLearnSkill(chara, skill, (int slot) => { learn = slot; }, () => { }));
                    DataManager.Instance.LogUIPlay(learn);
                }
                if (learn > -1)
                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.LearnSkillWithFanfare(chara, skill, learn));
            }
        }

        /// <summary>
        /// Attempts to give a new skill to the specified character, prompting to replace an old one if they are full.
        /// Waits until all the skill has been accepted or declined before continuing.
        /// </summary>
        /// <param name="chara">The character to learn the skill</param>
        /// <param name="skill">The skill to learn</param>
        /// <example>
        /// GAME:TryLearnSkill(player, "thunder")
        /// </example>
        public LuaFunction TryLearnSkill;
        public Coroutine _TryLearnSkill(Character chara, string skill)
        {
            return new Coroutine(__TryLearnSkill(chara, skill));
        }

        private IEnumerator<YieldInstruction> __TryLearnSkill(Character chara, string skill)
        {
            int learn = -1;
            if (DataManager.Instance.CurrentReplay != null)
                learn = DataManager.Instance.CurrentReplay.ReadUI();
            else
            {
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.TryLearnSkill(chara, skill, (int slot) => { learn = slot; }, () => { }));
                DataManager.Instance.LogUIPlay(learn);
            }
            if (learn > -1)
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.LearnSkillWithFanfare(chara, skill, learn));
        }

        /// <summary>
        /// Gives a new skill to a specified character.
        /// Fails if the character's skills are full.
        /// </summary>
        /// <param name="chara">The character to learn the skill</param>
        /// <param name="skill">The skill to learn</param>
        public void LearnSkill(Character chara, string skill)
        {
            chara.LearnSkill(skill, true);
        }

        /// <summary>
        /// Removed a skill from the specified character.
        /// </summary>
        /// <param name="chara">The character to forget the skill</param>
        /// <param name="slot">The slot of the skill to forget</param>
        public void ForgetSkill(Character chara, int slot)
        {
            chara.DeleteSkill(slot);
        }

        /// <summary>
        /// Makes a skill impossible to forget or replace for the specified character.
        /// Note that this only affects normal gameplay. Scripts can still freely get rid of the skill.
        /// </summary>
        /// <param name="chara">The character to lock the skill</param>
        /// <param name="slot">The slot of the skill to lock</param>
        public void LockSkill(Character chara, int slot)
        {
            chara.SetSkillLocking(slot, true);
        }

        /// <summary>
        /// Unlocks a previously locked skill for the specified character, making it possible to be forgotten or replaced during normal gameplay.
        /// </summary>
        /// <param name="chara">The character to unlock the skill</param>
        /// <param name="slot">The slot of the skill to unlock</param>
        public void UnlockSkill(Character chara, int slot)
        {
            chara.SetSkillLocking(slot, false);
        }

        /// <summary>
        /// Gives a new skill to a specified character, replacing a specifically chosen slot.
        /// </summary>
        /// <param name="character">The character to learn the skill</param>
        /// <param name="skillId">The skill to learn</param>
        /// <param name="slot">The slot to replace</param>
        public void SetCharacterSkill(Character character, string skillId, int slot, bool enabled = true)
        {
            character.ReplaceSkill(skillId, slot, enabled);
        }

        /// <summary>
        /// Gets the skill from a specified character and specified slot.
        /// </summary>
        /// <param name="chara">The character to get the skill from.</param>
        /// <param name="slot">The slot to get the skill from.</param>
        /// <returns>The ID of the skill in the slot</returns>
        public string GetCharacterSkill(Character chara, int slot)
        {
            return chara.BaseSkills[slot].SkillNum;
        }

        /// <summary>
        /// Checks if the character can be promoted to a new class.
        /// </summary>
        /// <param name="character">The character to check</param>
        /// <returns>True if the character can be promoted, false otherwise.</returns>
        public bool CanPromote(Character character)
        {
            MonsterData entry = DataManager.Instance.GetMonster(character.BaseForm.Species);
            for (int ii = 0; ii < entry.Promotions.Count; ii++)
            {
                if (!DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(entry.Promotions[ii].Result.ToString()).Released)
                    continue;

                bool hardReq = false;
                foreach (PromoteDetail detail in entry.Promotions[ii].Details)
                {
                    if (detail.IsHardReq() && !detail.GetReq(character, false))
                    {
                        hardReq = true;
                        break;
                    }
                }
                if (!hardReq)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a list of possible classes that the character can prmote to.
        /// </summary>
        /// <param name="character">The character to check</param>
        /// <param name="bypassItem">An exception item that can bypass checks for promotion</param>
        /// <returns>A lua table of PromoteBranch objects</returns>
        public LuaTable GetAvailablePromotions(Character character, string bypassItem)
        {
            MonsterData entry = DataManager.Instance.GetMonster(character.BaseForm.Species);
            bool bypass = character.EquippedItem.ID == bypassItem;

            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, chara) table.insert(tbl, chara) end").First() as LuaFunction;

            for (int ii = 0; ii < entry.Promotions.Count; ii++)
            {
                if (!DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(entry.Promotions[ii].Result.ToString()).Released)
                    continue;
                if (entry.Promotions[ii].IsQualified(character, false))
                    addfn.Call(tbl, entry.Promotions[ii]);
                else if (bypass)
                {
                    bool hardReq = false;
                    foreach (PromoteDetail detail in entry.Promotions[ii].Details)
                    {
                        if (detail.IsHardReq() && !detail.GetReq(character, false))
                        {
                            hardReq = true;
                            break;
                        }
                    }
                    if (!hardReq)
                        addfn.Call(tbl, entry.Promotions[ii]);
                }
            }

            return tbl;
        }

        /// <summary>
        /// Promotes a character ot a new class.
        /// </summary>
        /// <param name="character">The character to promote</param>
        /// <param name="branch">The PromoteBranch to promote with</param>
        /// <param name="bypassItem">An exception item that can bypass checks for promotion</param>
        public void PromoteCharacter(Character character, PromoteBranch branch, string bypassItem)
        {
            MonsterData entry = DataManager.Instance.GetMonster(branch.Result);
            //exception item bypass
            bool bypass = character.EquippedItem.ID == bypassItem;
            MonsterID newData = character.BaseForm;
            newData.Species = branch.Result;
            branch.BeforePromote(character, false, ref newData);
            character.Promote(newData);
            character.FullRestore();
            branch.OnPromote(character, false, bypass);
            //remove exception item if there is one...
            if (bypass)
                character.SilentDequipItem();
            DataManager.Instance.Save.RegisterMonster(character.BaseForm.Species);
            DataManager.Instance.Save.RogueUnlockMonster(character.BaseForm.Species);
        }

        //===================================
        // Inventory
        //===================================

        /// <summary>
        /// Finds an item in the player's team and returns its slot within the inventory or among its team's equips.
        /// </summary>
        /// <param name="id">The item ID to search for.</param>
        /// <param name="held">Check equipped items.</param>
        /// <param name="inv">Check inventory items.</param>
        /// <returns>The InvSlot of the item. Invalid if the item could not be found.</returns>
        public InvSlot FindPlayerItem(string id, bool held, bool inv)
        {
            if (held)
            {
                for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
                {
                    Character activeChar = DataManager.Instance.Save.ActiveTeam.Players[ii];
                    if (activeChar.EquippedItem.ID == id)
                        return new InvSlot(true, ii);
                }
            }

            if (inv)
            {
                for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.GetInvCount(); ii++)
                {
                    if (DataManager.Instance.Save.ActiveTeam.GetInv(ii).ID == id)
                        return new InvSlot(false, ii);
                }
            }

            return InvSlot.Invalid;
        }

        /// <summary>
        /// Get the number of items equipped by players.  Does not include guests.
        /// </summary>
        /// <returns>The number of items.</returns>
        public int GetPlayerEquippedCount()
        {
            int nbitems = 0;
            foreach (Character player in DataManager.Instance.Save.ActiveTeam.Players)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                    nbitems++;
            }

            return nbitems;
        }

        /// <summary>
        /// Get the number of items in the bag.
        /// </summary>
        /// <returns>The number of items.</returns>
        public int GetPlayerBagCount()
        {
            return DataManager.Instance.Save.ActiveTeam.GetInvCount();
        }

        /// <summary>
        /// Gets the maximum amount of item the player's team can carry.
        /// </summary>
        /// <returns>The number of items.</returns>
        public int GetPlayerBagLimit()
        {
            return DataManager.Instance.Save.ActiveTeam.MaxInv;
        }

        /// <summary>
        /// Gets the equipped item for the character in the specified slot.
        /// </summary>
        /// <param name="slot">The team slot of the character to check</param>
        /// <returns>The character's equipped item</returns>
        public object GetPlayerEquippedItem(int slot)
        {
            return DataManager.Instance.Save.ActiveTeam.Players[slot].EquippedItem;
        }

        /// <summary>
        /// Gets the equipped item for the character in the specified guest slot.
        /// </summary>
        /// <param name="slot">The guest slot of the character to check</param>
        /// <returns>The character's equipped item</returns>
        public object GetGuestEquippedItem(int slot)
        {
            return DataManager.Instance.Save.ActiveTeam.Guests[slot].EquippedItem;
        }

        /// <summary>
        /// Gives an item and adds it to the player team's bag.
        /// </summary>
        /// <param name="item">The item to give</param>
        public void GivePlayerItem(InvItem item)
        {
            ItemData entry = DataManager.Instance.GetItem(item.ID);
            if (entry.MaxStack > 1)
            {
                int amount = item.Amount;
                foreach (InvItem inv in DataManager.Instance.Save.ActiveTeam.EnumerateInv())
                {
                    if (inv.ID == item.ID && inv.Cursed == item.Cursed && inv.Amount < entry.MaxStack)
                    {
                        int addValue = Math.Min(entry.MaxStack - inv.Amount, item.Amount);
                        inv.Amount += addValue;
                        amount -= addValue;
                        if (amount <= 0)
                            break;
                    }
                }
                if (amount > 0 && DataManager.Instance.Save.ActiveTeam.GetInvCount() < DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone))
                {
                    InvItem newInv = new InvItem(item);
                    newInv.Amount = amount;
                    DataManager.Instance.Save.ActiveTeam.AddToInv(newInv);
                }
            }
            else
                DataManager.Instance.Save.ActiveTeam.AddToInv(item);
        }

        /// <summary>
        /// Gives an item and adds it to the player team's bag.
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="count">The amount to give. Default 1</param>
        /// <param name="cursed">Whether the item is cursed. Default false.</param>
        /// <param name="hiddenval">The hidden value of the item. Default empty string.</param>
        public void GivePlayerItem(string id, int count = 1, bool cursed = false, string hiddenval = "")
        {
            count = Math.Max(1, count);
            for (int ii = 0; ii < count; ii++)
            {
                InvItem item = new InvItem(id, cursed, 1);
                item.HiddenValue = hiddenval;
                GivePlayerItem(item);
            }
        }

        /// <summary>
        /// Gets the item found at the specified slot of the player's bag.
        /// </summary>
        /// <param name="slot">The slot to check</param>
        /// <returns>The item found in the slot</returns>
        public object GetPlayerBagItem(int slot)
        {
            return DataManager.Instance.Save.ActiveTeam.GetInv(slot);
        }

        /// <summary>
        /// Remove an item from player inventory
        /// </summary>
        /// <param name="slot">The slot from which to remove the item</param>
        /// <param name="takeAll"></param>
        public void TakePlayerBagItem(int slot, bool takeAll = false)
        {
            if (!takeAll)
            {
                InvItem item = DataManager.Instance.Save.ActiveTeam.GetInv(slot);
                ItemData entry = (ItemData)item.GetData();
                if (entry.MaxStack > 1 && item.Amount > 1)
                {
                    item.Amount--;
                    return;
                }
            }
            DataManager.Instance.Save.ActiveTeam.RemoveFromInv(slot);
        }

        /// <summary>
        /// Remove the equipped item from a chosen member of the team
        /// </summary>
        /// <param name="slot">The slot of the character on the team from which to remove the item</param>
        /// <param name="takeAll"></param>
        public void TakePlayerEquippedItem(int slot, bool takeAll = false)
        {
            if (!takeAll)
            {
                InvItem item = DataManager.Instance.Save.ActiveTeam.Players[slot].EquippedItem;
                ItemData entry = (ItemData)item.GetData();

                if (entry.MaxStack > 1 && item.Amount > 1)
                {
                    item.Amount--;
                    return;
                }
            }
            DataManager.Instance.Save.ActiveTeam.Players[slot].SilentDequipItem();
        }

        /// <summary>
        /// Remove the equipped item from a chosen guest of the team
        /// </summary>
        /// <param name="slot">The slot of the character on the team's guest list from which to remove the item</param>
        /// <param name="takeAll"></param>
        public void TakeGuestEquippedItem(int slot, bool takeAll = false)
        {
            if (!takeAll)
            {
                InvItem item = DataManager.Instance.Save.ActiveTeam.Guests[slot].EquippedItem;
                ItemData entry = (ItemData)item.GetData();

                if (entry.MaxStack > 1 && item.Amount > 1)
                {
                    item.Amount--;
                    return;
                }
            }
            DataManager.Instance.Save.ActiveTeam.Guests[slot].SilentDequipItem();
        }

        /// <summary>
        /// Get the amount of items in the player's storage
        /// </summary>
        /// <returns></returns>
        public int GetPlayerStorageCount()
        {
            int count = DataManager.Instance.Save.ActiveTeam.BoxStorage.Count;
            foreach (string nb in DataManager.Instance.Save.ActiveTeam.Storage.Keys)
                count += DataManager.Instance.Save.ActiveTeam.Storage[nb];
            return count;
        }

        /// <summary>
        /// Get the amount of a specific item in the player's storage
        /// </summary>
        /// <param name="id">ID of the item ot check</param>
        /// <returns>The amount of copies currently in storage</returns>
        public int GetPlayerStorageItemCount(string id)
        {
            int val;
            if (DataManager.Instance.Save.ActiveTeam.Storage.TryGetValue(id, out val))
                return val;
            return 0;
        }

        /// <summary>
        /// Gives an item and adds it to the player team's storage.
        /// </summary>
        /// <param name="item">The item to give</param>
        public void GivePlayerStorageItem(InvItem item)
        {
            DataManager.Instance.Save.ActiveTeam.StoreItems(new List<InvItem> { item });
        }

        /// <summary>
        /// Gives an item and adds it to the player team's storage.
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="count">The amount to give. Default 1</param>
        /// <param name="cursed">Whether the item is cursed. Default false.</param>
        /// <param name="hiddenval">The hidden value of the item. Default empty string.</param>
        public void GivePlayerStorageItem(string id, int count = 1, bool cursed = false, string hiddenval = "")
        {
            for (int ii = 0; ii < count; ii++)
            {
                InvItem item = new InvItem(id, cursed, 1);
                item.HiddenValue = hiddenval;
                DataManager.Instance.Save.ActiveTeam.StoreItems(new List<InvItem> { item });
            }
        }

        /// <summary>
        /// Takes an item from the storage
        /// </summary>
        /// <param name="id">The ID of the item to take</param>
        public void TakePlayerStorageItem(string id)
        {
            DataManager.Instance.Save.ActiveTeam.TakeItems(new List<WithdrawSlot> { new WithdrawSlot(false, id, 0) });
        }

        /// <summary>
        /// Takes all items in the player team's bag and equipped items, and deposits them in storage.
        /// </summary>
        public void DepositAll()
        {
            List<InvItem> items = new List<InvItem>();
            int item_count = DataManager.Instance.Save.ActiveTeam.GetInvCount();

            // Get list from held items
            foreach (Character player in DataManager.Instance.Save.ActiveTeam.Players)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                    items.Add(player.EquippedItem);
            }

            for (int ii = 0; ii < item_count; ii++)
            {
                // Get a list of inventory items.
                InvItem item = DataManager.Instance.Save.ActiveTeam.GetInv(ii);
                items.Add(item);
            };

            // Store all items in the inventory.
            DataManager.Instance.Save.ActiveTeam.StoreItems(items);

            // Remove held items
            foreach (Character player in DataManager.Instance.Save.ActiveTeam.Players)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                    player.SilentDequipItem();
            }

            // Remove the items back to front to prevent removing them in the wrong order.
            for (int ii = DataManager.Instance.Save.ActiveTeam.GetInvCount() - 1; ii >= 0; ii--)
            {
                DataManager.Instance.Save.ActiveTeam.RemoveFromInv(ii);
            }
        }

        //===================================
        // Money
        //===================================

        /// <summary>
        /// Gets the amount of money the player currently has on hand.
        /// </summary>
        /// <returns>The amount of money.</returns>
        public int GetPlayerMoney()
        {
            return DataManager.Instance.Save.ActiveTeam.Money;
        }

        /// <summary>
        /// Adds money to the player's wallet.
        /// </summary>
        /// <param name="toadd">The amount of money to add.</param>
        public void AddToPlayerMoney(int toadd)
        {
            DataManager.Instance.Save.ActiveTeam.AddMoney(null, toadd);
        }

        /// <summary>
        /// Removes money from the player's wallet.
        /// </summary>
        /// <param name="toremove">The amount of money to remove.</param>
        public void RemoveFromPlayerMoney(int toremove)
        {
            DataManager.Instance.Save.ActiveTeam.LoseMoney(null, toremove);
        }

        /// <summary>
        /// Gets the amount of money in the player's bank
        /// </summary>
        /// <returns>The amount of money.</returns>
        public int GetPlayerMoneyBank()
        {
            return DataManager.Instance.Save.ActiveTeam.Bank;
        }

        /// <summary>
        /// Adds money to the player's bank.
        /// </summary>
        /// <param name="toadd">The amount of money to add.</param>
        public void AddToPlayerMoneyBank(int toadd)
        {
            DataManager.Instance.Save.ActiveTeam.Bank += toadd;
        }

        /// <summary>
        /// Removes money from the player's bank.
        /// </summary>
        /// <param name="toremove">The amount of money to remove.</param>
        public void RemoveFromPlayerMoneyBank(int toremove)
        {
            DataManager.Instance.Save.ActiveTeam.Bank -= toremove;
        }

        //===================================
        // Input
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



        //
        // GameProgress
        //

        /// <summary>
        /// Gets the random seed for the current adventure.
        /// </summary>
        /// <returns>The current adventure's seed.</returns>
        public ulong GetDailySeed()
        {
            return DataManager.Instance.Save.Rand.FirstSeed;
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
        /// Checks if the current game is in rogue mode.
        /// </summary>
        /// <returns>True if in rogue mode, false otherwise.</returns>
        public bool InRogueMode()
        {
            return DataManager.Instance.Save is RogueProgress;
        }

        /// <summary>
        /// TODO: WIP
        /// </summary>
        /// <returns></returns>
        public bool HasServerSet()
        {
            return DiagManager.Instance.CurSettings.ServerList.Count > 0;
        }

        /// <summary>
        /// Checks to see if rescue is allowed.
        /// </summary>
        /// <returns>True if rescues are allowed, false otherwise.</returns>
        public bool GetRescueAllowed()
        {
            return DataManager.Instance.Save.AllowRescue;
        }

        /// <summary>
        /// Sets the value in the player's save file to determine if they can be rescued or not.
        /// If rescue is possible on the Save File level, it can still be prevented by the map.
        /// </summary>
        /// <param name="allowed">Set to true to allow the player to be rescued.  False otherwise.</param>
        public void SetRescueAllowed(bool allowed)
        {
            DataManager.Instance.Save.AllowRescue = allowed;
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

        //===================================
        // Utils
        //===================================

        /// <summary>
        /// Waits for a specified number of frames before continuing.
        /// </summary>
        /// <param name="frames">The number of frames ot wait.  Each frame is 1/60th of a second.</param>
        /// <example>
        /// GAME:WaitFrames(60)
        /// </example>
        public LuaFunction WaitFrames;
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
                if( X < 0 )
                {
                    if (Y > 0)
                        return Dir8.UpRight;
                    else if (Y < 0)
                        return Dir8.DownRight;
                    else
                        return Dir8.Right;
                }
                else if( X > 0)
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

        /// <summary>
        /// Setups any extra functionalities for this object written on the lua side.
        /// </summary>
        public override void SetupLuaFunctions(LuaEngine state)
        {
            GroundSave = state.RunString("return function(_) return coroutine.yield(GAME:_GroundSave()) end").First() as LuaFunction;
            CheckLevelSkills = state.RunString("return function(_,chara, oldLevel) return coroutine.yield(GAME:_CheckLevelSkills(chara, oldLevel)) end").First() as LuaFunction;
            TryLearnSkill = state.RunString("return function(_,chara, skill) return coroutine.yield(GAME:_TryLearnSkill(chara, skill)) end").First() as LuaFunction;
            EnterRescue = state.RunString("return function(_, sosPath) return coroutine.yield(GAME:_EnterRescue(sosPath)) end").First() as LuaFunction;
            EnterDungeon = state.RunString("return function(_, dungeonid, structureid, mapid, entryid, stakes, recorded, silentRestrict) return coroutine.yield(GAME:_EnterDungeon(dungeonid, structureid, mapid, entryid, stakes, recorded, silentRestrict)) end").First() as LuaFunction;
            ContinueDungeon = state.RunString("return function(_, dungeonid, structureid, mapid, entryid) return coroutine.yield(GAME:_ContinueDungeon(dungeonid, structureid, mapid, entryid)) end").First() as LuaFunction;
            EndDungeonRun = state.RunString("return function(_, result, destzoneid, structureid, mapid, entryid, display, fanfare, completedZone) return coroutine.yield(GAME:_EndDungeonRun(result, destzoneid, structureid, mapid, entryid, display, fanfare, completedZone)) end").First() as LuaFunction;
            FadeOut = state.RunString("return function(_, bwhite, duration) return coroutine.yield(GAME:_FadeOut(bwhite, duration)) end").First() as LuaFunction;
            FadeIn = state.RunString("return function(_, duration) return coroutine.yield(GAME:_FadeIn(duration)) end").First() as LuaFunction;
            MoveCamera = state.RunString("return function(_, x, y, duration, toPlayer) return coroutine.yield(GAME:_MoveCamera(x, y, duration, toPlayer)) end").First() as LuaFunction;
            MoveCameraToChara = state.RunString("return function(_, x, y, duration, chara) return coroutine.yield(GAME:_MoveCameraToChara(x, y, duration, chara)) end").First() as LuaFunction;
            WaitFrames      = state.RunString("return function(_, frames) return coroutine.yield(GAME:_WaitFrames(frames)) end").First() as LuaFunction;
        }


    }
}
