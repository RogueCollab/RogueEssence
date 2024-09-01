using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using System.IO;
using System.Runtime.Serialization;
using RogueEssence.Content;
using RogueEssence.Script;
using RogueEssence.Menu;
using Newtonsoft.Json;
using RogueEssence.Dev;

namespace RogueEssence.Data
{
    public enum TimeOfDay
    {
        Unknown = -1,
        Day,
        Dusk,
        Night,
        Dawn
    }
    [Serializable]
    public class ProfilePic
    {
        public MonsterID ID;
        public int Emote;

        public ProfilePic(MonsterID id, int emote)
        {
            ID = id;
            Emote = emote;
        }
    }
    [Serializable]
    public class RescueState
    {
        public SOSMail SOS;
        public bool Rescuing;

        public RescueState(SOSMail sos, bool rescuing)
        {
            SOS = sos;
            Rescuing = rescuing;
        }
    }

    [Serializable]
    public abstract class GameProgress
    {
        public const int MAJOR_TIME_DUR = 600;
        public const int MINOR_TIME_DUR = 300;

        public int TimeCycle;
        public TimeOfDay Time
        {
            get
            {
                if (TimeCycle < MAJOR_TIME_DUR)
                    return TimeOfDay.Day;
                else if (TimeCycle < MAJOR_TIME_DUR + MINOR_TIME_DUR)
                    return TimeOfDay.Dusk;
                else if (TimeCycle < MAJOR_TIME_DUR * 2 + MINOR_TIME_DUR)
                    return TimeOfDay.Night;
                else if (TimeCycle < (MAJOR_TIME_DUR + MINOR_TIME_DUR) * 2)
                    return TimeOfDay.Dawn;
                return TimeOfDay.Unknown;
            }
        }

        public enum ResultType
        {
            Unknown = -1,
            Downed,
            Failed,
            Cleared,
            Escaped,
            TimedOut,
            GaveUp,
            Rescue
        }
        public enum DungeonStakes
        {
            None,//progress in the dungeon is not recorded
            Progress,//progress in the dungeon is recorded
            Risk//progress in the dungeon is recorded, losing gives a penalty
        }
        public enum UnlockState
        {
            None,
            Discovered,
            Completed
        }

        public Version GameVersion;
        public ModHeader Quest;
        public List<ModHeader> Mods;

        public Settings.SkillDefault DefaultSkill;

        public ExplorerTeam ActiveTeam;
        public ReRandom Rand;

        [JsonConverter(typeof(MonsterUnlockConverter))]
        public Dictionary<string, UnlockState> Dex;
        [JsonConverter(typeof(MonsterBoolDictConverter))]
        public Dictionary<string, bool> RogueStarters;

        [JsonConverter(typeof(DungeonUnlockConverter))]
        public Dictionary<string, UnlockState> DungeonUnlocks;

        //TODO: set dungeon unlocks and event flags to save variables
        public string StartDate;
        public string UUID;
        public ProfilePic[] ProfilePics;

        public bool NoSwitching;
        public bool NoRecruiting;
        public RescueState Rescue;

        public ZoneLoc NextDest;

        public bool TeamMode;

        public int TotalTurns;
        public int TotalEXP;
        public int StartLevel;
        public int RescuesLeft;

        /// <summary>
        /// The total play time from past sessions.
        /// </summary>
        public TimeSpan SessionTime;

        /// <summary>
        /// The time when the current session was started.
        /// </summary>
        public DateTime SessionStartTime;


        //these values update and never clear
        public string EndDate;
        public string Location;
        public List<string> Trail;
        public List<ZoneLoc> LocTrail;
        public DungeonStakes Stakes;
        public bool MidAdventure;
        public ResultType Outcome;
        public string GeneratedAOK;
        public bool AllowRescue;

        public bool CutsceneMode;
        public string Song;//TODO: save the currently playing song


        //Added this for storing the serialized ScriptVar table
        [JsonConverter(typeof(Dev.ScriptVarsConverter))]
        public LuaTableContainer ScriptVars;

        public GameProgress()
        {
            GameVersion = new Version();
            Quest = ModHeader.Invalid;
            Mods = new List<ModHeader>();

            Dex = new Dictionary<string, UnlockState>();
            RogueStarters = new Dictionary<string, bool>();
            DungeonUnlocks = new Dictionary<string, UnlockState>();

            NextDest = ZoneLoc.Invalid;

            StartLevel = -1;
            StartDate = "";
            UUID = "";
            ProfilePics = new ProfilePic[0];
            EndDate = "";
            Location = "";
            Trail = new List<string>();
            LocTrail = new List<ZoneLoc>();
            Outcome = ResultType.Unknown;
        }

        public GameProgress(ulong seed, string uuid) : this()
        {
            Rand = new ReRandom(seed);
            UUID = uuid;

            ActiveTeam = new ExplorerTeam();
        }

        /// <summary>
        /// Updates the game-relevant options to the settings.
        /// Called whenever the settings change in a currently running game.
        /// Called whenever a new game is started, in order to inherit the existing settings.
        /// Called whenever a game is resumed, to update the settings to those that may have changed.
        /// DO NOT call when a replay is started.  Not that it would do anything...
        /// DO NOT call if a replay is in progress.  Not that it would do anything...
        /// </summary>
        /// <returns></returns>
        public void UpdateOptions()
        {
            if (DataManager.Instance.CurrentReplay == null)
            {
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    if (DefaultSkill != DiagManager.Instance.CurSettings.DefaultSkills)
                    {
                        GameAction options = new GameAction(GameAction.ActionType.Option, Dir8.None, (int)DiagManager.Instance.CurSettings.DefaultSkills);
                        DataManager.Instance.LogPlay(options);
                    }
                }
                DefaultSkill = DiagManager.Instance.CurSettings.DefaultSkills;
            }
        }

        public string GetDungeonTimeDisplay()
        {
            TimeSpan totalSessionTime = SessionTime;
            //add the time elapsed since current session start, if we have a session start
            if (SessionStartTime.Ticks > 0)
                totalSessionTime += (DateTime.Now - SessionStartTime);

            string display = "99:59:59";
            int totalHours = totalSessionTime.Hours;
            if (totalHours < 100)
                display = String.Format("{0:D2}", totalHours) + totalSessionTime.ToString(@"\:mm\:ss");

            return display;
        }
        public bool GetDefaultEnable(string moveIndex)
        {
            if (String.IsNullOrEmpty(moveIndex))
                return false;

            if (DefaultSkill == Settings.SkillDefault.All)
                return true;
            if (DefaultSkill == Settings.SkillDefault.None)
                return false;

            SkillData entry = DataManager.Instance.GetSkill(moveIndex);
            return (entry.Data.Category == BattleData.SkillCategory.Physical || entry.Data.Category == BattleData.SkillCategory.Magical);
        }

        public ContactInfo CreateContactInfo()
        {
            ContactInfo info = new ContactInfo(UUID);
            info.Data.TeamName = ActiveTeam.Name;
            info.Data.Rank = ActiveTeam.Rank;
            info.Data.RankStars = ActiveTeam.RankExtra;

            info.Data.TeamProfile = ProfilePics;

            return info;
        }

        public void UpdateTeamProfile(bool standard)
        {
            ulong curSeed = Rand.FirstSeed;
            List<int> totalEmotes = new List<int>();
            for (int ii = 0; ii < GraphicsManager.Emotions.Count; ii++)
            {
                EmotionType emoType = GraphicsManager.Emotions[ii];
                if (emoType.AllowRandom)
                    totalEmotes.Add(ii);
            }
            List<ProfilePic> teamProfile = new List<ProfilePic>();
            foreach (Character chara in ActiveTeam.Players)
            {
                int piece = totalEmotes[(int)((curSeed ^ (ulong)chara.Discriminator) % (ulong)totalEmotes.Count)];
                if (standard)
                    piece = 0;
                teamProfile.Add(new ProfilePic(chara.BaseForm, piece));
            }
            ProfilePics = teamProfile.ToArray();
        }

        public UnlockState GetMonsterUnlock(string index)
        {
            UnlockState state = UnlockState.None;
            Dex.TryGetValue(index, out state);
            return state;
        }

        public int GetTotalMonsterUnlock(UnlockState state)
        {
            int total = 0;
            foreach(string key in Dex.Keys)
            {
                if (Dex[key] == state)
                    total++;
            }
            return total;
        }

        public virtual void RegisterMonster(string index)
        {
            Dex[index] = UnlockState.Completed;
        }

        public virtual void SeenMonster(string index)
        {
            if (GetMonsterUnlock(index) == UnlockState.None)
                Dex[index] = UnlockState.Discovered;
        }

        public virtual void RogueUnlockMonster(string index)
        {
            RogueStarters[index] = true;
        }
        public bool GetRogueUnlock(string index)
        {
            bool state = false;
            RogueStarters.TryGetValue(index, out state);
            return state;
        }
        public UnlockState GetDungeonUnlock(string index)
        {
            UnlockState state = UnlockState.None;
            DungeonUnlocks.TryGetValue(index, out state);
            return state;
        }
        public void UnlockDungeon(string index)
        {
            if (GetDungeonUnlock(index) == UnlockState.None)
                DungeonUnlocks[index] = UnlockState.Discovered;
        }
        public void CompleteDungeon(string index)
        {
            DungeonUnlocks[index] = UnlockState.Completed;
        }

        public abstract IEnumerator<YieldInstruction> BeginGame(string zoneID, ulong seed, DungeonStakes stakes, bool recorded, bool noRestrict);
        public abstract IEnumerator<YieldInstruction> EndGame(ResultType result, ZoneLoc nextArea, bool display, bool fanfare, string completedZoneId);

        /// <summary>
        /// Begin counting play time for session
        /// </summary>
        public void BeginSession()
        {
            SessionTime = TimeSpan.Zero;
            SessionStartTime = DateTime.Now;
        }

        /// <summary>
        /// Stops counting play time for the end of an adventure or suspending via quicksave
        /// </summary>
        public void EndSession()
        {
            SessionTime = SessionTime + (DateTime.Now - SessionStartTime);
            SessionStartTime = new DateTime(0);
        }

        /// <summary>
        /// Loads play time from replay data
        /// resume counting play time by setting the sesion start
        /// </summary>
        public void ResumeSession(ReplayData replay)
        {
            SessionTime = new TimeSpan(replay.SessionTime);
            if (replay.SessionStartTime > 0L)
            {
                //special case: use dirty value from forcequitted session
                SessionStartTime = new DateTime(replay.SessionStartTime);
            }
            else
                SessionStartTime = DateTime.Now;
        }

        public abstract int GetTotalScore();

        public static bool ShouldRemoveItem(ZoneEntrySummary zone, string itemID)
        {
            if (zone.KeepTreasure)
            {
                //check if the item's a treasure
                ItemEntrySummary summary = (ItemEntrySummary)DataManager.Instance.DataIndices[DataManager.DataType.Item].Get(itemID);
                return (summary.UsageType != ItemData.UseType.Treasure);
            }
            return true;
        }

        public int GetTotalRemovableItems(ZoneEntrySummary zone)
        {
            int removableSlots = 0;
            foreach (Character player in ActiveTeam.Players)
            {
                if (!String.IsNullOrEmpty(player.EquippedItem.ID) && ShouldRemoveItem(zone, player.EquippedItem.ID))
                    removableSlots++;
            }
            for (int ii = 0; ii < ActiveTeam.GetInvCount(); ii++)
            {
                if (ShouldRemoveItem(zone, ActiveTeam.GetInv(ii).ID))
                    removableSlots++;
            }
            return removableSlots;
        }

        public IEnumerator<YieldInstruction> RestrictTeam(ZoneEntrySummary zone, bool silent)
        {
            List<string> teamRestrictions = new List<string>();
            List<string> bagRestrictions = new List<string>();

            try
            {
                int teamSize = ExplorerTeam.MAX_TEAM_SLOTS;
                if (zone.TeamSize > -1)
                    teamSize = zone.TeamSize;
                if (zone.TeamRestrict)
                    teamSize = 1;

                while (ActiveTeam.Players.Count > teamSize)
                {
                    int sendHomeIndex = ActiveTeam.Players.Count - 1;
                    if (sendHomeIndex == ActiveTeam.LeaderIndex)
                        sendHomeIndex--;
                    teamRestrictions.Add(ActiveTeam.Players[sendHomeIndex].GetDisplayName(true));
                    if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                        GroundScene.Instance.SilentSendHome(sendHomeIndex);
                    else if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                        DungeonScene.Instance.SilentSendHome(sendHomeIndex);
                }

                if (zone.MoneyRestrict && ActiveTeam.Money > 0)
                {
                    ActiveTeam.Bank += ActiveTeam.Money;
                    ActiveTeam.Money = 0;
                    bagRestrictions.Add(Text.FormatKey("DLG_RESTRICT_MONEY"));
                }

                if (zone.BagRestrict > -1)
                {
                    int totalSlots = 0;
                    foreach (Character player in ActiveTeam.Players)
                    {
                        if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                            totalSlots++;
                    }
                    totalSlots += ActiveTeam.GetInvCount();

                    bool removedItems = false;
                    int removableSlots = GetTotalRemovableItems(zone);
                    bool hasTreasure = removableSlots < totalSlots;

                    List<InvItem> itemsToStore = new List<InvItem>();
                    for (int ii = ActiveTeam.GetInvCount() - 1; ii >= 0 && removableSlots > zone.BagRestrict; ii--)
                    {
                        InvItem item = ActiveTeam.GetInv(ii);
                        if (ShouldRemoveItem(zone, item.ID))
                        {
                            removedItems = true;
                            itemsToStore.Add(item);
                            ActiveTeam.RemoveFromInv(ii);
                            removableSlots--;
                        }
                    }
                    for (int ii = ActiveTeam.Players.Count - 1; ii >= 0 && removableSlots > zone.BagRestrict; ii--)
                    {
                        InvItem item = ActiveTeam.Players[ii].EquippedItem;
                        if (!String.IsNullOrEmpty(item.ID) && ShouldRemoveItem(zone, item.ID))
                        {
                            removedItems = true;
                            itemsToStore.Add(item);
                            ActiveTeam.Players[ii].SilentDequipItem();
                            removableSlots--;
                        }
                    }
                    ActiveTeam.StoreItems(itemsToStore);
                    if (!silent && removedItems)
                    {
                        if (zone.BagRestrict > 0)
                        {
                            if (hasTreasure)
                                bagRestrictions.Add(Text.FormatKey("DLG_RESTRICT_ITEM_SLOT_NON_TREASURE", zone.BagRestrict));
                            else
                                bagRestrictions.Add(Text.FormatKey("DLG_RESTRICT_ITEM_SLOT", zone.BagRestrict));
                        }
                        else
                        {
                            if (hasTreasure)
                                bagRestrictions.Add(Text.FormatKey("DLG_RESTRICT_ITEM_ALL_NON_TREASURE"));
                            else
                                bagRestrictions.Add(Text.FormatKey("DLG_RESTRICT_ITEM_ALL"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }

            if (!silent)
            {
                if (teamRestrictions.Count > 1)
                {
                    string compositeList = Text.BuildList(teamRestrictions.ToArray());
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("MSG_TEAM_SENT_HOME_PLURAL", compositeList)));
                }
                else if (teamRestrictions.Count == 1)
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("MSG_TEAM_SENT_HOME", teamRestrictions[0])));

                if (bagRestrictions.Count > 0)
                {
                    string compositeList = Text.BuildList(bagRestrictions.ToArray());
                    string finalMsg = Text.FormatKey("DLG_RESTRICT_BAG", (compositeList[0].ToString()).ToUpper() + compositeList.Substring(1));
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(finalMsg));
                }
            }
        }

        public void PrepAdventureStates()
        {
            MidAdventure = true;

            AITactic defaultTactic = DataManager.Instance.GetAITactic(DataManager.Instance.DefaultAI);

            //clear defeat destinations, set absentee status, reload tactics
            for (int ii = 0; ii < ActiveTeam.Players.Count; ii++)
            {
                ActiveTeam.Players[ii].Absentee = false;
                ActiveTeam.Players[ii].DefeatAt = "";
                ActiveTeam.Players[ii].DefeatLoc = ZoneLoc.Invalid;
                if (ActiveTeam.Players[ii].Tactic != null)
                    ActiveTeam.Players[ii].Tactic = new AITactic(ActiveTeam.Players[ii].Tactic);
                else
                    ActiveTeam.Players[ii].Tactic = new AITactic(defaultTactic);
            }
            for (int ii = 0; ii < ActiveTeam.Guests.Count; ii++)
            {
                ActiveTeam.Guests[ii].Absentee = false;
                ActiveTeam.Guests[ii].DefeatAt = "";
                ActiveTeam.Guests[ii].DefeatLoc = ZoneLoc.Invalid;
                if (ActiveTeam.Guests[ii].Tactic != null)
                    ActiveTeam.Guests[ii].Tactic = new AITactic(ActiveTeam.Guests[ii].Tactic);
                else
                    ActiveTeam.Guests[ii].Tactic = new AITactic(defaultTactic);
            }
            for (int ii = 0; ii < ActiveTeam.Assembly.Count; ii++)
            {
                ActiveTeam.Assembly[ii].Absentee = true;
                ActiveTeam.Assembly[ii].DefeatAt = "";
                ActiveTeam.Assembly[ii].DefeatLoc = ZoneLoc.Invalid;
                if (ActiveTeam.Assembly[ii].Tactic != null)
                    ActiveTeam.Assembly[ii].Tactic = new AITactic(ActiveTeam.Assembly[ii].Tactic);
                else
                    ActiveTeam.Assembly[ii].Tactic = new AITactic(defaultTactic);
            }
        }

        /// <summary>
        /// Executed when the save file is loaded without a quicksave.  Check and clear all quicksave-related variables.
        /// </summary>
        public IEnumerator<YieldInstruction> LoadedWithoutQuicksave()
        {
            //Remove any dangling rescues
            if (Rescue != null)
                Rescue = null;
            //should never be mid-adventure; we backed out of a dungeon attempt
            if (MidAdventure)
            {
                //give loss penalty if we entered at Risk
                if (Outcome != ResultType.Unknown)
                {
                    if (Stakes == DungeonStakes.Risk)
                        yield return CoroutineManager.Instance.StartCoroutine(LuaEngine.Instance.OnLossPenalty(this));
                    //retain the outcome as the last adventure's outcome
                }
                MidAdventure = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="capOnly">Will force lower level to specified level if false.</param>
        /// <param name="permanent"></param>
        /// <param name="silent"></param>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> RestrictLevel(int level, bool capOnly, bool permanent, bool silent, bool keepSkills)
        {
            StartLevel = level;
            try
            {
                for (int ii = 0; ii < ActiveTeam.Players.Count; ii++)
                {
                    RestrictCharLevel(ActiveTeam.Players[ii], level, capOnly, keepSkills);
                    if (!permanent)
                        ActiveTeam.Players[ii].BackRef = new TempCharBackRef(false, ii);
                }
                for (int ii = 0; ii < ActiveTeam.Guests.Count; ii++)
                {
                    RestrictCharLevel(ActiveTeam.Guests[ii], level, capOnly, keepSkills);
                    //no backref for guests
                }
                for (int ii = 0; ii < ActiveTeam.Assembly.Count; ii++)
                {
                    RestrictCharLevel(ActiveTeam.Assembly[ii], level, capOnly, keepSkills);
                    if (!permanent)
                        ActiveTeam.Assembly[ii].BackRef = new TempCharBackRef(true, ii);
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }

            if (!silent)
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_RESTRICT_LEVEL", StartLevel)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="character"></param>
        /// <param name="level"></param>
        /// <param name="capOnly">Will force lower level to specified level if false.</param>
        public void RestrictCharLevel(Character character, int level, bool capOnly, bool keepSkills)
        {
            //set level
            if (capOnly)
                character.Level = Math.Min(level, character.Level);
            else
                character.Level = level;

            if (character.Level == level)
                character.EXP = 0;
            //clear boosts
            character.MaxHPBonus = 0;
            character.AtkBonus = 0;
            character.DefBonus = 0;
            character.MAtkBonus = 0;
            character.MDefBonus = 0;
            character.SpeedBonus = 0;

            if (!character.Dead)
                character.HP = character.MaxHP;

            //reroll skills
            BaseMonsterForm form = DataManager.Instance.GetMonster(character.BaseForm.Species)
                .Forms[character.BaseForm.Form];

            if (!keepSkills) {
                while (!String.IsNullOrEmpty(character.BaseSkills[0].SkillNum))
                    character.DeleteSkill(0);
                List<string> final_skills = form.RollLatestSkills(character.Level, new List<string>());
                foreach (string skill in final_skills)
                    character.LearnSkill(skill, GetDefaultEnable(skill));
            }

            character.Relearnables = new Dictionary<string, bool>();
        }

        public void RestoreLevel()
        {
            if (StartLevel > -1)
            {
                GameState state = DataManager.Instance.LoadMainGameState(false);
                //the current save file has the level-restricted characters
                //the other save file has the original characters
                for (int ii = 0; ii < ActiveTeam.Players.Count; ii++)
                {
                    TempCharBackRef backRef = ActiveTeam.Players[ii].BackRef;
                    if (backRef.Index > -1)
                        restoreCharLevel(ActiveTeam.Players[ii], backRef.Assembly ? state.Save.ActiveTeam.Assembly[backRef.Index] : state.Save.ActiveTeam.Players[backRef.Index], StartLevel);
                }
                for (int ii = 0; ii < ActiveTeam.Assembly.Count; ii++)
                {
                    TempCharBackRef backRef = ActiveTeam.Assembly[ii].BackRef;
                    if (ActiveTeam.Assembly[ii].BackRef.Index > -1)
                        restoreCharLevel(ActiveTeam.Assembly[ii], backRef.Assembly ? state.Save.ActiveTeam.Assembly[backRef.Index] : state.Save.ActiveTeam.Players[backRef.Index], StartLevel);
                }
                StartLevel = -1;
            }
            //also, restore default hunger values
            {
                for (int ii = 0; ii < ActiveTeam.Players.Count; ii++)
                {
                    ActiveTeam.Players[ii].MaxFullness = Character.MAX_FULLNESS;
                    ActiveTeam.Players[ii].Fullness = Character.MAX_FULLNESS;
                }
                for (int ii = 0; ii < ActiveTeam.Assembly.Count; ii++)
                {
                    ActiveTeam.Assembly[ii].MaxFullness = Character.MAX_FULLNESS;
                    ActiveTeam.Assembly[ii].Fullness = Character.MAX_FULLNESS;
                }
            }
        }

        private void restoreCharLevel(Character character, Character charFrom, int level)
        {
            //compute the amount of experience removed from the original character
            MonsterData monsterData = DataManager.Instance.GetMonster(charFrom.BaseForm.Species);
            BaseMonsterForm monsterForm = monsterData.Forms[charFrom.BaseForm.Form];
            try
            {
                int removedEXP = 0;
                string growth = monsterData.EXPTable;
                GrowthData growthData = DataManager.Instance.GetGrowth(growth);
                if (level <= charFrom.Level)
                {
                    removedEXP += charFrom.EXP;
                    int origLevel = charFrom.Level;
                    while (origLevel > level)
                    {
                        removedEXP += growthData.GetExpToNext(origLevel - 1);
                        origLevel--;
                    }
                }
                //add the EXP to the character
                if (character.Level < DataManager.Instance.Start.MaxLevel)
                {
                    character.EXP += removedEXP;

                    while (character.EXP >= growthData.GetExpToNext(character.Level))
                    {
                        character.EXP -= growthData.GetExpToNext(character.Level);
                        character.Level++;

                        if (character.Level >= DataManager.Instance.Start.MaxLevel)
                        {
                            character.EXP = 0;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }

            try
            {
                //add stat boosts
                character.MaxHPBonus = Math.Min(character.MaxHPBonus + charFrom.MaxHPBonus, monsterForm.GetMaxStatBonus(Stat.HP));
                character.AtkBonus = Math.Min(character.AtkBonus + charFrom.AtkBonus, monsterForm.GetMaxStatBonus(Stat.Attack));
                character.DefBonus = Math.Min(character.DefBonus + charFrom.DefBonus, monsterForm.GetMaxStatBonus(Stat.Defense));
                character.MAtkBonus = Math.Min(character.MAtkBonus + charFrom.MAtkBonus, monsterForm.GetMaxStatBonus(Stat.MAtk));
                character.MDefBonus = Math.Min(character.MDefBonus + charFrom.MDefBonus, monsterForm.GetMaxStatBonus(Stat.MDef));
                character.SpeedBonus = Math.Min(character.SpeedBonus + charFrom.SpeedBonus, monsterForm.GetMaxStatBonus(Stat.Speed));
                character.HP = character.MaxHP;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }

            try
            {
                //restore skills
                while (!String.IsNullOrEmpty(character.BaseSkills[0].SkillNum))
                    character.DeleteSkill(0);
                for (int ii = 0; ii < charFrom.BaseSkills.Count; ii++)
                {
                    if (!String.IsNullOrEmpty(charFrom.BaseSkills[ii].SkillNum))
                    {
                        bool enabled = false;
                        foreach (BackReference<Skill> skill in charFrom.Skills)
                        {
                            if (skill.BackRef == ii)
                            {
                                enabled = skill.Element.Enabled;
                                break;
                            }
                        }
                        character.LearnSkill(charFrom.BaseSkills[ii].SkillNum, enabled);
                    }
                }

                //restore remembered skills
                foreach (string key in charFrom.Relearnables.Keys)
                    character.Relearnables[key] = charFrom.Relearnables[key];
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }

            //restoreCharLevel the backref
            character.BackRef = new TempCharBackRef(false, -1);
        }

        /// <summary>
        /// Removes cursed and hidden values from items after leaving dungeon mode.
        /// </summary>
        protected void ClearDungeonItems()
        {
            try
            {
                foreach (Character character in ActiveTeam.EnumerateChars())
                {
                    if (!String.IsNullOrEmpty(character.EquippedItem.ID))
                    {
                        character.EquippedItem.Cursed = false;
                        ItemData entry = DataManager.Instance.GetItem(character.EquippedItem.ID);
                        if (entry.UsageType != ItemData.UseType.Box)
                            character.EquippedItem.HiddenValue = "";
                    }
                }
                foreach (InvItem item in ActiveTeam.EnumerateInv())
                {
                    item.Cursed = false;
                    ItemData entry = DataManager.Instance.GetItem(item.ID);
                    if (entry.UsageType != ItemData.UseType.Box)
                        item.HiddenValue = "";
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public static void SaveMainData(BinaryWriter writer, GameProgress save)
        {
            try
            {
                using (MemoryStream classStream = new MemoryStream())
                {
                    Serializer.SerializeData(classStream, save, true);
                    writer.Write(classStream.Position);

                    classStream.WriteTo(writer.BaseStream);
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public static GameProgress LoadMainData(BinaryReader reader)
        {
            long oldOffset = reader.BaseStream.Position;
            long length = reader.ReadInt64();
            using (MemoryStream classStream = new MemoryStream(reader.ReadBytes((int)length)))
                return (GameProgress)Serializer.DeserializeData(classStream);
        }

        public void RestartLogs(ulong seed)
        {
            StartDate = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            TotalTurns = 0;
            EndDate = "";
            Location = "";
            Trail = new List<string>();
            LocTrail = new List<ZoneLoc>();

            Rand = new ReRandom(seed);//reseed own random
            DataManager.Instance.MsgLog.Clear();
        }


        public void FullStateRestore()
        {
            foreach (Character character in ActiveTeam.EnumerateChars())
                character.FullRestore();
            foreach (Character character in ActiveTeam.Assembly)
                character.FullRestore();
            MidAdventure = false;
            ClearDungeonItems();
            //clear rescue status
            Rescue = null;
        }

        public List<string> MergeDexTo(GameProgress destProgress, bool completion)
        {
            //monster encounters
            List<string> newRecruits = new List<string>();
            foreach(string key in DataManager.Instance.DataIndices[DataManager.DataType.Monster].GetOrderedKeys(true))
            {
                if (GetMonsterUnlock(key) == UnlockState.Completed && destProgress.GetMonsterUnlock(key) != UnlockState.Completed)
                {
                    if (completion)
                        destProgress.RegisterMonster(key);
                }
                if (GetMonsterUnlock(key) == UnlockState.Discovered && destProgress.GetMonsterUnlock(key) == UnlockState.None)
                {
                    if (completion)
                        destProgress.SeenMonster(key);
                }
                if (GetRogueUnlock(key) && !destProgress.GetRogueUnlock(key))
                {
                    destProgress.RogueUnlockMonster(key);
                    newRecruits.Add(key);
                }
            }
            return newRecruits;
        }

        public List<ModVersion> GetModVersion()
        {
            List<ModVersion> result = new List<ModVersion>();
            result.Add(new ModVersion("[Game]", Guid.Empty, GameVersion));
            if (Quest.IsValid())
                result.Add(new ModVersion(Quest.Name, Quest.UUID, Quest.Version));

            foreach (ModHeader mod in Mods)
                result.Add(new ModVersion(mod.Name, mod.UUID, mod.Version));

            return result;
        }

        public List<ModDiff> GetModDiffs()
        {
            List<ModVersion> oldVersion = GetModVersion();
            List<ModVersion> newVersion = PathMod.GetModVersion();

            return PathMod.DiffModVersions(oldVersion, newVersion);
        }

        /// <summary>
        /// Returns true if
        /// Game version is higher than save's game version
        /// Quest is different
        /// Quest version is higher than save's quest version
        /// Mods are added or removed
        /// Mod version is higher than the save's mod version
        /// </summary>
        /// <returns></returns>
        public bool IsOldVersion()
        {
            if (GameVersion < Versioning.GetVersion())
                return true;

            if (Quest.UUID != PathMod.Quest.UUID || Quest.Version < PathMod.Quest.Version)
                return true;

            bool[] foundNewMod = new bool[PathMod.Mods.Length];
            foreach (ModHeader oldMod in Mods)
            {
                bool found = false;
                for (int ii = 0; ii < PathMod.Mods.Length; ii++)
                {
                    ModHeader newMod = PathMod.Mods[ii];
                    if (oldMod.UUID == newMod.UUID)
                    {
                        found = true;
                        foundNewMod[ii] = true;
                        if (oldMod.Version != newMod.Version)
                            return true;
                        break;
                    }
                }
                if (!found)
                    return true;
            }
            for (int ii = 0; ii < foundNewMod.Length; ii++)
            {
                if (!foundNewMod[ii])
                    return true;
            }
            return false;
        }


        public void UpdateVersion()
        {
            GameVersion = Versioning.GetVersion();
            Quest = PathMod.Quest;
            Mods.Clear();
            foreach (ModHeader curMod in PathMod.Mods)
                Mods.Add(curMod);
        }


        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //TODO: v1.1 delete this
            if (GameVersion == null)
                GameVersion = new Version();
            //TODO: v1.1 delete this
            if (!Quest.IsValid())
                Quest = ModHeader.Invalid;
            //TODO: v1.1 delete this
            if (Mods == null)
                Mods = new List<ModHeader>();
        }
    }

    [Serializable]
    public class MainProgress : GameProgress
    {
        public ulong FirstSeed;
        public int TotalAdventures;

        public List<CharData> CharsToStore;
        public List<InvItem> ItemsToStore;

        [JsonConverter(typeof(ItemStorageConverter))]
        public Dictionary<string, int> StorageToStore;
        public int MoneyToStore;

        [JsonConstructor]
        public MainProgress()
        {
            CharsToStore = new List<CharData>();
            ItemsToStore = new List<InvItem>();
            StorageToStore = new Dictionary<string, int>();
        }

        public MainProgress(ulong seed, string uuid)
            : base(seed, uuid)
        {
            FirstSeed = seed;

            CharsToStore = new List<CharData>();
            ItemsToStore = new List<InvItem>();
            StorageToStore = new Dictionary<string, int>();
        }
        
        public override int GetTotalScore() { return 0; }

        public override IEnumerator<YieldInstruction> BeginGame(string zoneID, ulong seed, DungeonStakes stakes, bool recorded, bool noRestrict)
        {
            ZoneEntrySummary zone = (ZoneEntrySummary)DataManager.Instance.DataIndices[DataManager.DataType.Zone].Get(zoneID);

            //restrict team size/bag size/etc
            if (!noRestrict)
                yield return CoroutineManager.Instance.StartCoroutine(RestrictTeam(zone, false));
            
            Stakes = stakes;

            PrepAdventureStates();

            //pre-save with a GiveUp outcome
            Outcome = ResultType.GaveUp;
            DataManager.Instance.SaveMainGameState();
            Outcome = ResultType.Unknown;

            //set everyone's levels and mark them for backreferral
            //need to mention the instance on save directly since it has been backed up and changed
            if (!noRestrict && zone.LevelCap)
                yield return CoroutineManager.Instance.StartCoroutine(RestrictLevel(zone.Level, true, false, false, zone.KeepSkills));

            RestartLogs(seed);
            RescuesLeft = zone.Rescues;
            BeginSession();

            if (recorded)
                DataManager.Instance.BeginPlay(PathMod.ModSavePath(DataManager.SAVE_PATH, DataManager.QUICKSAVE_FILE_PATH), zoneID, false, false, SessionStartTime);
        }

        public override IEnumerator<YieldInstruction> EndGame(ResultType result, ZoneLoc nextArea, bool display, bool fanfare, string completedZoneId)
        {
            EndSession();

            Outcome = result;
            bool recorded = DataManager.Instance.RecordingReplay;
            string recordFile = null;
            if (result == ResultType.Rescue)
            {
                try
                {
                    Location = ZoneManager.Instance.CurrentZone.GetDisplayName();

                    DataManager.Instance.MsgLog.Clear();
                    //end the game with a recorded ending
                    recordFile = DataManager.Instance.EndPlay(this, StartDate);

                    SOSMail sos = Rescue.SOS;
                    string dateRescued = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
                    ReplayData replay = DataManager.Instance.LoadReplay(PathMod.ModSavePath(DataManager.REPLAY_PATH, recordFile), false);
                    AOKMail aok = new AOKMail(sos, DataManager.Instance.Save, dateRescued, replay);
                    GeneratedAOK = DataManager.SaveRescueMail(PathMod.FromApp(DataManager.RESCUE_OUT_PATH + DataManager.AOK_FOLDER), aok, false);
                    string deletePath = DataManager.FindRescueMail(PathMod.FromApp(DataManager.RESCUE_IN_PATH + DataManager.SOS_FOLDER), sos, sos.Extension);
                    if (deletePath != null)
                        File.Delete(deletePath);

                    if (nextArea.IsValid()) //  if an exit is specified, go to the exit.
                        NextDest = nextArea;
                    else
                        NextDest = DataManager.Instance.Start.Map;
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }
            else if (result != ResultType.Cleared)
            {
                try
                {
                    if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                        Location = ZoneManager.Instance.CurrentGround.GetColoredName();
                    else if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                        Location = ZoneManager.Instance.CurrentMap.GetColoredName();

                    DataManager.Instance.MsgLog.Clear();
                    //end the game with a recorded ending
                    recordFile = DataManager.Instance.EndPlay(this, StartDate);
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }

                if (Outcome != ResultType.Escaped && Stakes == DungeonStakes.Risk) //remove all items
                    yield return CoroutineManager.Instance.StartCoroutine(LuaEngine.Instance.OnLossPenalty(this));

                if (nextArea.IsValid()) //  if an exit is specified, go to the exit.
                    NextDest = nextArea;
                else
                    NextDest = DataManager.Instance.Start.Map;
            }
            else
            {
                try
                {
                    CompleteDungeon(completedZoneId);

                    Location = ZoneManager.Instance.CurrentZone.GetDisplayName();

                    DataManager.Instance.MsgLog.Clear();
                    //end the game with a recorded ending
                    recordFile = DataManager.Instance.EndPlay(this, StartDate);

                    if (nextArea.IsValid()) //  if an exit is specified, go to the exit.
                        NextDest = nextArea;
                    else
                        NextDest = DataManager.Instance.Start.Map;
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }

            TotalAdventures++;

            FullStateRestore();

            bool mergeDataBack = (Stakes != DungeonStakes.None);
            try
            {
                //merge back the team if the dungeon was level-limited
                RestoreLevel();
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                mergeDataBack = false;
            }

            GameState state = DataManager.Instance.LoadMainGameState(false);
            MainProgress mainSave = state?.Save as MainProgress;

            //save the result to the main file
            if (mergeDataBack)
            {
                if (mainSave != null)
                    mainSave.MergeDataTo(this);
                DataManager.Instance.SaveMainGameState();
            }
            else
            {
                if (mainSave != null)
                    DataManager.Instance.SetProgress(mainSave);
                DataManager.Instance.Save.CompleteDungeon(completedZoneId);
                DataManager.Instance.Save.NextDest = NextDest;
            }

            MenuBase.Transparent = false;

            if (recorded && display)
            {
                GameProgress ending = DataManager.Instance.GetRecord(PathMod.ModSavePath(DataManager.REPLAY_PATH, recordFile));

                if (fanfare)
                {
                    if (result != ResultType.Cleared && result != ResultType.Escaped && result != ResultType.Rescue)
                        GameManager.Instance.Fanfare("Fanfare/MissionFail");
                    else
                        GameManager.Instance.Fanfare("Fanfare/MissionClear");
                }
                else
                    GameManager.Instance.SE("Menu/Skip");

                FinalResultsMenu menu = new FinalResultsMenu(ending);
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(menu));
            }

            yield return new WaitForFrames(20);
        }

        public void MergeDataTo(MainProgress destProgress)
        {
            if (this != destProgress)
                MergeDexTo(destProgress, true);

            //for merging data imported from roguelocke
            foreach (CharData charData in CharsToStore)
            {
                Character chara = new Character(charData);
                AITactic tactic = DataManager.Instance.GetAITactic(DataManager.Instance.DefaultAI);
                chara.Tactic = new AITactic(tactic);
                destProgress.ActiveTeam.Assembly.Add(chara);
            }
            CharsToStore.Clear();

            destProgress.ActiveTeam.StoreItems(ItemsToStore);
            ItemsToStore.Clear();

            //just add storage values
            foreach(string key in StorageToStore.Keys)
            {
                destProgress.ActiveTeam.Storage[key] = StorageToStore[key] + destProgress.ActiveTeam.Storage.GetValueOrDefault(key, 0);
                StorageToStore[key] = 0;
            }

            //put the money in the bank
            destProgress.ActiveTeam.Bank += MoneyToStore;
            MoneyToStore = 0;
        }

        public void DeleteOutdatedAssets(DataManager.DataType assetType)
        {
            //illegal items:
            if ((assetType & DataManager.DataType.Item) != DataManager.DataType.None)
            {
                //delete from inv
                for (int ii = ActiveTeam.GetInvCount() - 1; ii >= 0; ii--)
                {
                    ItemData entry = DataManager.Instance.GetItem(ActiveTeam.GetInv(ii).ID);
                    if (entry == null)
                        ActiveTeam.RemoveFromInv(ii, true);
                }

                //remove equips
                foreach (Character player in ActiveTeam.EnumerateChars())
                {
                    if (!String.IsNullOrEmpty(player.EquippedItem.ID))
                    {
                        ItemData entry = DataManager.Instance.GetItem(player.EquippedItem.ID);
                        if (entry == null)
                            player.EquippedItem = new InvItem();
                    }
                }

                //delete from storage
                List<string> removeKeys = new List<string>();
                foreach (string key in ActiveTeam.Storage.Keys)
                {
                    ItemData entry = DataManager.Instance.GetItem(key);
                    if (entry == null)
                        removeKeys.Add(key);

                }
                foreach (string key in removeKeys)
                    ActiveTeam.Storage.Remove(key);

                //delete from boxstorage
                for (int ii = ActiveTeam.BoxStorage.Count - 1; ii >= 0; ii--)
                {
                    ItemData entry = DataManager.Instance.GetItem(ActiveTeam.BoxStorage[ii].ID);
                    if (entry == null)
                        ActiveTeam.BoxStorage.RemoveAt(ii);
                }
            }

            //TODO: delete skills, intrinsics, and monsters
        }
    }


    [Serializable]
    public class RogueProgress : GameProgress
    {
        public bool Seeded { get; set; }

        public RogueConfig Config;
        
        [JsonConstructor]
        public RogueProgress()
        { }
        public RogueProgress(string uuid,  RogueConfig config) : base(config.Seed, uuid)
        {
            Seeded = !config.SeedRandomized;
            Config = config;
        }
        public override int GetTotalScore()
        {
            return ActiveTeam.GetInvValue() + ActiveTeam.GetStorageValue() + ActiveTeam.Money + ActiveTeam.Bank;
        }

        public override IEnumerator<YieldInstruction> BeginGame(string zoneID, ulong seed, DungeonStakes stakes, bool recorded, bool noRestrict)
        {
            ZoneEntrySummary zone = (ZoneEntrySummary)DataManager.Instance.DataIndices[DataManager.DataType.Zone].Get(zoneID);
            
            MidAdventure = true;
            Stakes = stakes;
            Outcome = ResultType.Unknown;

            yield return CoroutineManager.Instance.StartCoroutine(RestrictLevel(zone.Level, false, true, true, false));

            BeginSession();

            if (recorded)
            {
                if (!Directory.Exists(PathMod.ModSavePath(DataManager.ROGUE_PATH)))
                    Directory.CreateDirectory(PathMod.ModSavePath(DataManager.ROGUE_PATH));
                DataManager.Instance.BeginPlay(PathMod.ModSavePath(DataManager.ROGUE_PATH, DataManager.Instance.Save.StartDate + DataManager.QUICKSAVE_EXTENSION), zoneID, true, Seeded, SessionStartTime);
            }

            yield break;
        }

        public override IEnumerator<YieldInstruction> EndGame(ResultType result, ZoneLoc nextArea, bool display, bool fanfare, string completedZoneId)
        {
            EndSession();

            bool recorded = DataManager.Instance.RecordingReplay;
            //if lose, end the play, display plaque, and go to title
            if (result != ResultType.Cleared)
            {
                if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                    Location = ZoneManager.Instance.CurrentGround.GetColoredName();
                else if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                    Location = ZoneManager.Instance.CurrentMap.GetColoredName();

                Outcome = result;

                DataManager.Instance.MsgLog.Clear();
                //end the game with a recorded ending
                string recordFile = DataManager.Instance.EndPlay(this, null);

                MenuBase.Transparent = false;
                //save to the main file
                GameState state = DataManager.Instance.LoadMainGameState(false);
                List<string> newRecruits = new List<string>();
                if (state != null)
                {
                    newRecruits = MergeDexTo(state.Save, false);
                    DataManager.Instance.SaveGameState(state);
                }

                if (recorded && display)
                {
                    GameProgress ending = DataManager.Instance.GetRecord(PathMod.ModSavePath(DataManager.REPLAY_PATH, recordFile));

                    if (fanfare)
                    {
                        if (result != ResultType.Escaped)
                            GameManager.Instance.Fanfare("Fanfare/MissionFail");
                        else
                            GameManager.Instance.Fanfare("Fanfare/MissionClear");
                    }
                    else
                        GameManager.Instance.SE("Menu/Skip");

                    FinalResultsMenu menu = new FinalResultsMenu(ending);
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(menu));
                    
                    Dictionary<string, List<RecordHeaderData>> scores = RecordHeaderData.LoadHighScores();

                    // handle the case for seeded runs and there's currently no scores for the current zone
                    if (scores.ContainsKey(ZoneManager.Instance.CurrentZoneID))
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new ScoreMenu(scores, ZoneManager.Instance.CurrentZoneID, PathMod.ModSavePath(DataManager.REPLAY_PATH, recordFile))));   
                    }
                }

                if (newRecruits.Count > 0)
                {
                    yield return new WaitForFrames(10);
                    GameManager.Instance.Fanfare("Fanfare/NewArea");
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_NEW_CHARS")));
                }
            }
            else
            {
                MidAdventure = false;
                ClearDungeonItems();

                //  if there isn't a next area, end the play, display the plaque, return to title screen
                //GameManager.Instance.Fanfare("Fanfare/MissionClear");
                Location = ZoneManager.Instance.CurrentZone.GetDisplayName();

                Outcome = result;

                DataManager.Instance.MsgLog.Clear();
                //end the game with a recorded ending
                string recordFile = DataManager.Instance.EndPlay(this, null);

                MenuBase.Transparent = false;

                //save to the main file
                GameState state = DataManager.Instance.LoadMainGameState(false);
                if (state != null)
                {
                    MergeDexTo(state.Save, true);
                    state.Save.CompleteDungeon(completedZoneId);
                    DataManager.Instance.SaveGameState(state);
                }
                
                if (recorded)
                {
                    GameProgress ending = DataManager.Instance.GetRecord(PathMod.ModSavePath(DataManager.REPLAY_PATH, recordFile));

                    if (fanfare)
                        GameManager.Instance.Fanfare("Fanfare/MissionClear");
                    else
                        GameManager.Instance.SE("Menu/Skip");

                    FinalResultsMenu menu = new FinalResultsMenu(ending);
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(menu));

                    Dictionary<string, List<RecordHeaderData>> scores = RecordHeaderData.LoadHighScores();
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new ScoreMenu(scores, ZoneManager.Instance.CurrentZoneID, PathMod.ModSavePath(DataManager.REPLAY_PATH, recordFile))));
                }

                yield return CoroutineManager.Instance.StartCoroutine(askTransfer(state, completedZoneId));
            }
            yield return new WaitForFrames(20);
        }

        private IEnumerator<YieldInstruction> askTransfer(GameState state, string completedZone)
        {
            //ask to transfer if the dungeon records progress, and it is NOT a seeded run.
            if (state != null && Stakes != DungeonStakes.None && !Seeded)
            {
                bool allowTransfer = false;
                ZoneData zoneData = DataManager.Instance.GetZone(completedZone);
                DialogueBox question = MenuManager.Instance.CreateQuestion(zoneData.Rogue == RogueStatus.AllTransfer ? Text.FormatKey("DLG_TRANSFER_ALL_ASK") : Text.FormatKey("DLG_TRANSFER_ITEM_ASK"),
                    () => { allowTransfer = true; }, () => { });
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(question));

                if (allowTransfer)
                {
                    MainProgress mainSave = state.Save as MainProgress;

                    if (zoneData.Rogue == RogueStatus.AllTransfer)
                    {
                        //put the new recruits into assembly
                        foreach (Character character in ActiveTeam.Players)
                        {
                            if (!(character.Dead && DataManager.Instance.GetSkin(character.BaseForm.Skin).Challenge))
                            {
                                if (!String.IsNullOrEmpty(character.EquippedItem.ID))
                                    mainSave.ItemsToStore.Add(character.EquippedItem);
                                mainSave.CharsToStore.Add(new CharData(character));
                            }
                        }
                        foreach (Character character in ActiveTeam.Assembly)
                        {
                            if (!(character.Dead && DataManager.Instance.GetSkin(character.BaseForm.Skin).Challenge))
                                mainSave.CharsToStore.Add(new CharData(character));
                        }
                    }

                    //put the new items into the storage
                    foreach (InvItem item in ActiveTeam.EnumerateInv())
                        mainSave.ItemsToStore.Add(item);
                    foreach (InvItem item in ActiveTeam.BoxStorage)
                        mainSave.ItemsToStore.Add(item);

                    mainSave.StorageToStore = ActiveTeam.Storage;

                    mainSave.MoneyToStore = ActiveTeam.Money + ActiveTeam.Bank;
                }

                DataManager.Instance.SaveGameState(state);

                if (allowTransfer)
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_TRANSFER_COMPLETE")));
            }
        }

        public static IEnumerator<YieldInstruction> StartRogue(RogueConfig config)
        {
            DataManager.Instance.PreLoadZone(config.Destination);
            GameManager.Instance.BGM("", true);
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.FadeOut(false));
            GameProgress save = new RogueProgress(Guid.NewGuid().ToString().ToUpper(), config);
            save.UnlockDungeon(config.Destination);
            DataManager.Instance.SetProgress(save);
            DataManager.Instance.Save.UpdateVersion();
            DataManager.Instance.Save.UpdateOptions();
            DataManager.Instance.Save.StartDate = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            DataManager.Instance.Save.ActiveTeam = new ExplorerTeam();
            DataManager.Instance.Save.ActiveTeam.Name = config.TeamName;

            MonsterData monsterData = DataManager.Instance.GetMonster(config.Starter);

            int formSlot = config.FormSetting;
            List<int> forms = CharaDetailMenu.GetPossibleForms(monsterData);
            if (formSlot >= forms.Count)
                formSlot = forms.Count - 1;
            if (formSlot == -1)
                formSlot = MathUtils.Rand.Next(forms.Count);

            int formIndex = forms[formSlot];

            Gender gender = CharaDetailMenu.LimitGender(monsterData, formIndex, config.GenderSetting);
            if (gender == Gender.Unknown)
                gender = monsterData.Forms[formIndex].RollGender(MathUtils.Rand);
            
            int intrinsicSlot = CharaDetailMenu.LimitIntrinsic(monsterData, formIndex, config.IntrinsicSetting);
            string intrinsic;
            if (intrinsicSlot == -1)
                intrinsic = monsterData.Forms[formIndex].RollIntrinsic(MathUtils.Rand, 3);
            else if (intrinsicSlot == 0)
                intrinsic = monsterData.Forms[formIndex].Intrinsic1;
            else if (intrinsicSlot == 1)
                intrinsic = monsterData.Forms[formIndex].Intrinsic2;
            else
                intrinsic = monsterData.Forms[formIndex].Intrinsic3;

            Character newChar = DataManager.Instance.Save.ActiveTeam.CreatePlayer(MathUtils.Rand, new MonsterID(config.Starter, formIndex, config.SkinSetting, gender), DataManager.Instance.Start.Level, intrinsic, DataManager.Instance.Start.Personality);
            newChar.Nickname = config.Nickname;
            DataManager.Instance.Save.ActiveTeam.Players.Add(newChar);

            try
            {
                LuaEngine.Instance.OnNewGame();
                if (DataManager.Instance.Save.ActiveTeam.Players.Count == 0)
                    throw new Exception("Script generated an invalid team!");
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            
            yield return CoroutineManager.Instance.StartCoroutine(GameManager.Instance.BeginGameInSegment(new ZoneLoc(config.Destination, new SegLoc()), GameProgress.DungeonStakes.Risk, true, false));
        }
    }
    
    public class RogueConfig
    {
        public string Destination;
        public bool DestinationRandomized;
        public string TeamName;
        public bool TeamRandomized;
        public string Starter;
        public bool StarterRandomized;
        public int IntrinsicSetting;
        public int FormSetting;
        public Gender GenderSetting;
        public ulong Seed;
        public bool SeedRandomized;
        public string SkinSetting;
        public string Nickname;

        public RogueConfig()
        {
        }

        public RogueConfig(RogueConfig other)
        {
            Destination = other.Destination;
            DestinationRandomized = other.DestinationRandomized;
            TeamName = other.TeamName;
            TeamRandomized = other.TeamRandomized;
            Starter = other.Starter;
            StarterRandomized = other.StarterRandomized;
            IntrinsicSetting = other.IntrinsicSetting;
            FormSetting = other.FormSetting;
            GenderSetting = other.GenderSetting;
            Seed = other.Seed;
            SeedRandomized = other.SeedRandomized;
            SkinSetting = other.SkinSetting;
            Nickname = other.Nickname;
        }

        public static RogueConfig RerollFromOther(RogueConfig oldConfig)
        {
            RogueConfig config = new RogueConfig(oldConfig);
            if (config.TeamRandomized)
                config.TeamName = DataManager.Instance.Start.Teams[MathUtils.Rand.Next(DataManager.Instance.Start.Teams.Count)];

            if (config.SeedRandomized)
                config.Seed = MathUtils.Rand.NextUInt64();

            if (config.StarterRandomized)
            {
                List<string> starters = CharaChoiceMenu.GetStartersList();
                string starter = starters[MathUtils.Rand.Next(starters.Count)];
                config.Starter = starter;
                config.IntrinsicSetting = -1;
                config.FormSetting = -1;
                config.GenderSetting = Gender.Unknown;
            }
            if (config.DestinationRandomized)
            {
                List<string> destinations = RogueDestMenu.GetDestinationsList();
                config.Destination = destinations[MathUtils.Rand.Next(destinations.Count)];
            }
            return config;
        }
    }
}
