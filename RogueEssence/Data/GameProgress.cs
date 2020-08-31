using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Ground;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using RogueEssence.Content;
using RogueEssence.Script;
using RogueEssence.Menu;

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

        public ExplorerTeam ActiveTeam;
        public ReRandom Rand;

        public UnlockState[] Dex;
        public UnlockState[] DungeonUnlocks;

        //TODO: set dungeon unlocks and event flags to save variables

        public string StartDate;
        public string UUID;
        public ProfilePic[] ProfilePics;

        public RescueState Rescue;

        public ZoneLoc NextDest;

        public bool TeamMode;

        public int TotalTurns;
        public int TotalEXP;
        public int StartLevel;
        public int RescuesLeft;

        //these values update and never clear
        public string EndDate;
        public string Location;
        public List<string> Trail;
        public DungeonStakes Stakes;
        public bool MidAdventure;
        public ResultType Outcome;
        public string GeneratedAOK;
        public bool AllowRescue;

        public bool CutsceneMode;
        public string Song;//TODO: save the currently playing song


        //Added this for storing the serialized ScriptVar table
        public string ScriptVars;

        public GameProgress()
        {
            ActiveTeam = new ExplorerTeam();

            Dex = new UnlockState[DataManager.Instance.DataIndices[DataManager.DataType.Monster].Count];
            DungeonUnlocks = new UnlockState[DataManager.Instance.DataIndices[DataManager.DataType.Zone].Count];

            NextDest = ZoneLoc.Invalid;

            StartLevel = -1;
            StartDate = "";
            UUID = "";
            ProfilePics = new ProfilePic[0];
            EndDate = "";
            Location = "";
            Trail = new List<string>();
            Outcome = ResultType.Unknown;
        }

        public GameProgress(ulong seed, string uuid) : this()
        {
            Rand = new ReRandom(seed);
            UUID = uuid;
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
            ulong totalEmotes = (ulong)GraphicsManager.Emotions.Count;
            List<ProfilePic> teamProfile = new List<ProfilePic>();
            foreach (Character chara in ActiveTeam.Players)
            {
                int piece = (int)((curSeed ^ (ulong)chara.Discriminator) % totalEmotes);
                if (standard)
                    piece = 0;
                teamProfile.Add(new ProfilePic(chara.BaseForm, piece));
            }
            ProfilePics = teamProfile.ToArray();
        }

        public virtual void SeenMonster(int index)
        {
            if (Dex[index] == UnlockState.None)
                Dex[index] = UnlockState.Discovered;
        }
        public virtual void RegisterMonster(int index)
        {
            Dex[index] = UnlockState.Completed;
        }
        public abstract IEnumerator<YieldInstruction> BeginGame(int zoneID, ulong seed, DungeonStakes stakes, bool recorded, bool noRestrict);
        public abstract IEnumerator<YieldInstruction> EndGame(ResultType result, ZoneLoc nextArea, bool display, bool fanfare);


        public IEnumerator<YieldInstruction> RestrictTeam(ZoneData zone, bool silent)
        {
            int teamSize = ExplorerTeam.MAX_TEAM_SLOTS;
            if (zone.TeamSize > -1)
                teamSize = zone.TeamSize;
            if (zone.TeamRestrict)
                teamSize = 1;

            List<string> teamRestrictions = new List<string>();
            while (ActiveTeam.Players.Count > teamSize)
            {
                int sendHomeIndex = ActiveTeam.Players.Count - 1;
                if (sendHomeIndex == ActiveTeam.LeaderIndex)
                    sendHomeIndex--;
                teamRestrictions.Add(ActiveTeam.Players[sendHomeIndex].BaseName);
                if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                    GroundScene.Instance.SilentSendHome(sendHomeIndex);
                else if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                    DungeonScene.Instance.SilentSendHome(sendHomeIndex);
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
            }
            
            List<string> bagRestrictions = new List<string>();
            if (zone.MoneyRestrict && ActiveTeam.Money > 0)
            {
                ActiveTeam.Bank += ActiveTeam.Money;
                ActiveTeam.Money = 0;
                bagRestrictions.Add(Text.FormatKey("DLG_RESTRICT_MONEY"));
            }

            if (zone.BagRestrict > -1)
            {
                bool removedItems = false;
                int heldSlots = 0;
                foreach (Character player in ActiveTeam.Players)
                {
                    if (player.EquippedItem.ID > -1)
                        heldSlots++;
                }
                List<InvItem> itemsToStore = new List<InvItem>();
                while (ActiveTeam.GetInvCount() + heldSlots > zone.BagRestrict && ActiveTeam.GetInvCount() > 0)
                {
                    removedItems = true;
                    itemsToStore.Add(ActiveTeam.GetInv(ActiveTeam.GetInvCount() - 1));
                    
                    ActiveTeam.RemoveFromInv(ActiveTeam.GetInvCount() - 1);
                }
                while (ActiveTeam.GetInvCount() + heldSlots > zone.BagRestrict)
                {
                    foreach (Character player in ActiveTeam.Players)
                    {
                        if (player.EquippedItem.ID > -1)
                        {
                            removedItems = true;
                            itemsToStore.Add(player.EquippedItem);
                            player.DequipItem();
                            heldSlots--;
                            break;
                        }
                    }
                }
                ActiveTeam.StoreItems(itemsToStore);
                if (!silent && removedItems)
                {
                    if (zone.BagRestrict > 0)
                        bagRestrictions.Add(Text.FormatKey("DLG_RESTRICT_ITEM_SLOT", zone.BagRestrict));
                    else
                        bagRestrictions.Add(Text.FormatKey("DLG_RESTRICT_ITEM_ALL"));
                }
            }
            if (bagRestrictions.Count > 0 && !silent)
            {
                string compositeList = Text.BuildList(bagRestrictions.ToArray());
                string finalMsg = Text.FormatKey("DLG_RESTRICT_BAG", (compositeList[0].ToString()).ToUpper() + compositeList.Substring(1));
                yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(finalMsg));
            }
        }


        public IEnumerator<YieldInstruction> RestrictLevel(ZoneData zone, bool silent)
        {
            if (zone.Level > -1)
            {
                StartLevel = zone.Level;
                for (int ii = 0; ii < ActiveTeam.Players.Count; ii++)
                {
                    RestrictCharLevel(ActiveTeam.Players[ii], zone.Level, true);
                    ActiveTeam.Players[ii].BackRef = new TempCharBackRef(false, ii);
                }
                for (int ii = 0; ii < ActiveTeam.Assembly.Count; ii++)
                {
                    RestrictCharLevel(ActiveTeam.Assembly[ii], zone.Level, true);
                    ActiveTeam.Assembly[ii].BackRef = new TempCharBackRef(true, ii);
                }
                if (!silent)
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_RESTRICT_LEVEL", StartLevel)));
            }
        }

        public static void RestrictCharLevel(Character character, int level, bool capOnly)
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

            character.HP = character.MaxHP;
            
            //reroll skills
            BaseMonsterForm form = DataManager.Instance.GetMonster(character.BaseForm.Species).Forms[character.BaseForm.Form];

            while (character.BaseSkills[0].SkillNum > -1)
                character.DeleteSkill(0);
            List<int> final_skills = form.RollLatestSkills(character.Level, new List<int>());
            foreach (int skill in final_skills)
                character.LearnSkill(skill, true);
        }

        public IEnumerator<YieldInstruction> RestoreLevel()
        {
            if (StartLevel > -1)
            {
                GameState state = DataManager.Instance.LoadMainGameState();
                if (state == null)
                {
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_ERR_READ_SAVE")));
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_ERR_READ_SAVE_FALLBACK")));
                    yield break;
                }
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
        }

        private void restoreCharLevel(Character character, Character charFrom, int level)
        {
            //compute the amount of experience removed from the original character
            int removedEXP = 0;
            MonsterData monsterData = DataManager.Instance.GetMonster(charFrom.BaseForm.Species);
            BaseMonsterForm monsterForm = monsterData.Forms[charFrom.BaseForm.Form];
            int growth = monsterData.EXPTable;
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
            if (character.Level < DataManager.Instance.MaxLevel)
            {
                character.EXP += removedEXP;

                while (character.EXP >= growthData.GetExpToNext(character.Level))
                {
                    if (character.Level >= DataManager.Instance.MaxLevel)
                    {
                        character.EXP = 0;
                        break;
                    }
                    character.EXP -= growthData.GetExpToNext(character.Level);
                    character.Level++;
                }
            }
            //add stat boosts
            character.MaxHPBonus = Math.Min(character.MaxHPBonus + charFrom.MaxHPBonus, monsterForm.GetMaxStatBonus(Stat.HP));
            character.AtkBonus = Math.Min(character.AtkBonus + charFrom.AtkBonus, monsterForm.GetMaxStatBonus(Stat.Attack));
            character.DefBonus = Math.Min(character.DefBonus + charFrom.DefBonus, monsterForm.GetMaxStatBonus(Stat.Defense));
            character.MAtkBonus = Math.Min(character.MAtkBonus + charFrom.MAtkBonus, monsterForm.GetMaxStatBonus(Stat.MAtk));
            character.MDefBonus = Math.Min(character.MDefBonus + charFrom.MDefBonus, monsterForm.GetMaxStatBonus(Stat.MDef));
            character.SpeedBonus = Math.Min(character.SpeedBonus + charFrom.SpeedBonus, monsterForm.GetMaxStatBonus(Stat.Speed));
            character.HP = character.MaxHP;

            //restore skills
            while (character.BaseSkills[0].SkillNum > -1)
                character.DeleteSkill(0);
            for(int ii = 0; ii < charFrom.BaseSkills.Count; ii++)
            {
                if (charFrom.BaseSkills[ii].SkillNum > -1)
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

            //restoreCharLevel the backref
            character.BackRef = new TempCharBackRef(false, -1);
        }

        /// <summary>
        /// Removes cursed and hidden values from items after leaving dungeon mode.
        /// </summary>
        protected void ClearDungeonItems()
        {
            foreach (Character character in ActiveTeam.Players)
            {
                if (character.EquippedItem.ID > -1)
                {
                    character.EquippedItem.Cursed = false;
                    ItemData entry = DataManager.Instance.GetItem(character.EquippedItem.ID);
                    if (entry.MaxStack <= 1 && entry.UsageType != ItemData.UseType.Box)
                        character.EquippedItem.HiddenValue = 0;
                }
            }
            foreach (InvItem item in ActiveTeam.EnumerateInv())
            {
                item.Cursed = false;
                ItemData entry = DataManager.Instance.GetItem(item.ID);
                if (entry.MaxStack <= 1 && entry.UsageType != ItemData.UseType.Box)
                    item.HiddenValue = 0;
            }
        }

        public static void SaveMainData(BinaryWriter writer, GameProgress save)
        {
            //notify script engine
            LuaEngine.Instance.SaveData(save);

            using (MemoryStream classStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(classStream, save);
                writer.Write(classStream.Position);
                classStream.WriteTo(writer.BaseStream);
            }
        }

        public static GameProgress LoadMainData(BinaryReader reader)
        {
            long length = reader.ReadInt64();
            using (MemoryStream classStream = new MemoryStream(reader.ReadBytes((int)length)))
            {
                IFormatter formatter = new BinaryFormatter();
                GameProgress outsave = (GameProgress)formatter.Deserialize(classStream); //Had to tweak this around a bit, so I could notify the script engine to load the script save data
                return outsave;
            }

        }

        public void RestartLogs(ulong seed)
        {
            StartDate = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            TotalTurns = 0;
            EndDate = "";
            Location = "";
            Trail = new List<string>();

            Rand = new ReRandom(seed);//reseed own random
            DataManager.Instance.MsgLog.Clear();
        }


        public List<int> MergeDexTo(GameProgress destProgress)
        {
            //monster encounters
            List<int> newRecruits = new List<int>();
            for (int ii = 0; ii < DataManager.Instance.DataIndices[DataManager.DataType.Monster].Count; ii++)
            {
                if (Dex[ii] == UnlockState.Completed && destProgress.Dex[ii] != UnlockState.Completed)
                {
                    MonsterData entry = DataManager.Instance.GetMonster(ii);
                    if (entry.PromoteFrom == -1)
                    {
                        bool isOriginal = false;
                        for (int jj = 0; jj < DataManager.Instance.StartChars.Count; jj++)
                        {
                            if (ii == DataManager.Instance.StartChars[jj])
                                isOriginal = true;
                        }
                        if (!isOriginal)
                            newRecruits.Add(ii);
                    }
                    destProgress.Dex[ii] = UnlockState.Completed;
                }
                if (Dex[ii] == UnlockState.Discovered && destProgress.Dex[ii] == UnlockState.None)
                    destProgress.Dex[ii] = UnlockState.Discovered;
            }
            return newRecruits;
        }
    }

    [Serializable]
    public class MainProgress : GameProgress
    {
        public ulong FirstSeed;
        public int TotalAdventures;

        public List<CharData> CharsToStore;
        public List<InvItem> ItemsToStore;
        public int[] StorageToStore;
        public int MoneyToStore;

        public MainProgress()
        {
            CharsToStore = new List<CharData>();
            ItemsToStore = new List<InvItem>();
            StorageToStore = new int[DataManager.Instance.DataIndices[DataManager.DataType.Item].Count];
        }

        public MainProgress(ulong seed, string uuid)
            : base(seed, uuid)
        {
            FirstSeed = seed;

            CharsToStore = new List<CharData>();
            ItemsToStore = new List<InvItem>();
            StorageToStore = new int[DataManager.Instance.DataIndices[DataManager.DataType.Item].Count];
        }


        public static void LossPenalty(GameProgress save)
        {
            //remove money
            save.ActiveTeam.Money = save.ActiveTeam.Money / 2;
            //remove bag items
            for (int ii = save.ActiveTeam.GetInvCount() - 1; ii >= 0; ii--)
            {
                ItemData entry = DataManager.Instance.GetItem(save.ActiveTeam.GetInv(ii).ID);
                if (!entry.CannotDrop)
                    save.ActiveTeam.RemoveFromInv(ii);
            }

            //remove equips
            foreach (Character player in save.ActiveTeam.Players)
            {
                if (player.EquippedItem.ID > -1)
                {
                    ItemData entry = DataManager.Instance.GetItem(player.EquippedItem.ID);
                    if (!entry.CannotDrop)
                        player.DequipItem();
                }
            }
        }

        public override IEnumerator<YieldInstruction> BeginGame(int zoneID, ulong seed, DungeonStakes stakes, bool recorded, bool noRestrict)
        {
            ZoneData zone = DataManager.Instance.GetZone(zoneID);
            //restrict team size/bag size/etc
            if (!noRestrict)
                yield return CoroutineManager.Instance.StartCoroutine(RestrictTeam(zone, false));

            MidAdventure = true;
            Stakes = stakes;
            //create a copy (from save and load) of the current state and mark it with loss
            DataManager.Instance.SaveMainGameState();

            GameState state = DataManager.Instance.LoadMainGameState();
            if (state != null)
            {
                if (Stakes == DungeonStakes.Risk)
                    LossPenalty(state.Save);

                DataManager.Instance.SaveMainGameState(state);
            }

            //set everyone's levels and mark them for backreferral
            if (!noRestrict)
                yield return CoroutineManager.Instance.StartCoroutine(RestrictLevel(zone, false));

            RestartLogs(seed);
            RescuesLeft = zone.Rescues;

            if (recorded)
                DataManager.Instance.BeginPlay(DataManager.QUICKSAVE_FILE_PATH);
        }

        public override IEnumerator<YieldInstruction> EndGame(ResultType result, ZoneLoc nextArea, bool display, bool fanfare)
        {
            Outcome = result;
            bool recorded = DataManager.Instance.RecordingReplay;
            string recordFile = null;
            if (result == ResultType.Rescue)
            {
                Location = ZoneManager.Instance.CurrentZone.Name.ToLocal();

                DataManager.Instance.MsgLog.Clear();
                //end the game with a recorded ending
                recordFile = DataManager.Instance.EndPlay(this, StartDate);

                SOSMail sos = Rescue.SOS;
                string dateRescued = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
                ReplayData replay = DataManager.Instance.LoadReplay(DataManager.REPLAY_PATH + recordFile, false);
                AOKMail aok = new AOKMail(sos, DataManager.Instance.Save, dateRescued, replay);
                GeneratedAOK = DataManager.SaveRescueMail(DataManager.RESCUE_OUT_PATH + DataManager.AOK_PATH, aok, false);
                string deletePath = DataManager.FindRescueMail(DataManager.RESCUE_IN_PATH + DataManager.SOS_PATH, sos, sos.Extension);
                if (deletePath != null)
                    File.Delete(deletePath);

                if (nextArea.IsValid()) //  if an exit is specified, go to the exit.
                    NextDest = nextArea;
                else
                    NextDest = new ZoneLoc(1, new SegLoc(-1, 1));
            }
            else if (result != ResultType.Cleared)
            {
                if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                    Location = ZoneManager.Instance.CurrentGround.GetSingleLineName();
                else if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                    Location = ZoneManager.Instance.CurrentMap.GetSingleLineName();

                DataManager.Instance.MsgLog.Clear();
                //end the game with a recorded ending
                recordFile = DataManager.Instance.EndPlay(this, StartDate);

                if (Outcome != ResultType.Escaped && Stakes == DungeonStakes.Risk) //remove all items
                    LossPenalty(this);

                if (nextArea.IsValid()) //  if an exit is specified, go to the exit.
                    NextDest = nextArea;
                else
                    NextDest = new ZoneLoc(1, new SegLoc(-1, 1));

            }
            else
            {
                int completedZone = ZoneManager.Instance.CurrentZoneID;
                DungeonUnlocks[completedZone] = UnlockState.Completed;

                Location = ZoneManager.Instance.CurrentZone.Name.ToLocal();

                DataManager.Instance.MsgLog.Clear();
                //end the game with a recorded ending
                recordFile = DataManager.Instance.EndPlay(this, StartDate);

                if (nextArea.IsValid()) //  if an exit is specified, go to the exit.
                    NextDest = nextArea;
                else
                    NextDest = new ZoneLoc(1, new SegLoc(-1, 1));

            }

            TotalAdventures++;

            foreach (Character character in ActiveTeam.Players)
            {
                character.Dead = false;
                character.FullRestore();
            }
            foreach (Character character in ActiveTeam.Assembly)
            {
                character.Dead = false;
                character.FullRestore();
            }
            MidAdventure = false;
            ClearDungeonItems();
            //clear rescue status
            Rescue = null;

            //merge back the team if the dungeon was level-limited
            yield return CoroutineManager.Instance.StartCoroutine(RestoreLevel());

            //save the result to the main file
            if (Stakes != DungeonStakes.None)
            {
                GameState state = DataManager.Instance.LoadMainGameState();
                MainProgress mainSave = state.Save as MainProgress;
                mainSave.MergeDataTo(this);
                DataManager.Instance.SaveMainGameState();
            }
            else
            {
                GameState state = DataManager.Instance.LoadMainGameState();
                MainProgress mainSave = state.Save as MainProgress;
                mainSave.MergeDataTo(mainSave);
                DataManager.Instance.SetProgress(state.Save);
                DataManager.Instance.Save.NextDest = NextDest;
            }

            MenuBase.Transparent = false;

            if (recorded && display)
            {
                GameProgress ending = DataManager.Instance.GetRecord(DataManager.REPLAY_PATH + recordFile);

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
                MergeDexTo(destProgress);

            foreach (CharData charData in CharsToStore)
                destProgress.ActiveTeam.Assembly.Add(new Character(charData, ActiveTeam, new Loc(), Dir8.Down));
            CharsToStore.Clear();

            destProgress.ActiveTeam.StoreItems(ItemsToStore);
            ItemsToStore.Clear();

            //just add storage values
            for (int ii = 0; ii < StorageToStore.Length; ii++)
            {
                destProgress.ActiveTeam.Storage[ii] += StorageToStore[ii];
                StorageToStore[ii] = 0;
            }

            //put the money in the bank
            destProgress.ActiveTeam.Bank += MoneyToStore;
            MoneyToStore = 0;
        }
    }


    [Serializable]
    public class RogueProgress : GameProgress
    {

        public RogueProgress()
        { }
        public RogueProgress(ulong seed, string uuid) : base(seed, uuid)
        { }

        public override void SeenMonster(int index)
        {
            base.SeenMonster(index);
        }
        public override void RegisterMonster(int index)
        {
            base.RegisterMonster(index);
        }

        public override IEnumerator<YieldInstruction> BeginGame(int zoneID, ulong seed, DungeonStakes stakes, bool recorded, bool noRestrict)
        {
            MidAdventure = true;
            Stakes = stakes;

            if (recorded)
                DataManager.Instance.BeginPlay(DataManager.ROGUE_PATH + DataManager.Instance.Save.StartDate + DataManager.QUICKSAVE_EXTENSION);

            yield break;
        }

        public override IEnumerator<YieldInstruction> EndGame(ResultType result, ZoneLoc nextArea, bool display, bool fanfare)
        {
            bool recorded = DataManager.Instance.RecordingReplay;
            //if lose, end the play, display plaque, and go to title
            if (result != ResultType.Cleared)
            {
                if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                    Location = ZoneManager.Instance.CurrentGround.GetSingleLineName();
                else if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                    Location = ZoneManager.Instance.CurrentMap.GetSingleLineName();

                Outcome = result;

                DataManager.Instance.MsgLog.Clear();
                //end the game with a recorded ending
                string recordFile = DataManager.Instance.EndPlay(this, null);

                MenuBase.Transparent = false;
                //save to the main file
                GameState state = DataManager.Instance.LoadMainGameState();
                List<int> newRecruits = new List<int>();
                if (state != null)
                {
                    newRecruits = MergeDexTo(state.Save);
                    DataManager.Instance.SaveMainGameState(state);
                }


                if (recorded && display)
                {
                    GameProgress ending = DataManager.Instance.GetRecord(Data.DataManager.REPLAY_PATH + recordFile);

                    if (fanfare)
                    {
                        if (result != ResultType.Cleared)
                            GameManager.Instance.Fanfare("Fanfare/MissionFail");
                        else
                            GameManager.Instance.Fanfare("Fanfare/MissionClear");
                    }
                    else
                        GameManager.Instance.SE("Menu/Skip");

                    FinalResultsMenu menu = new FinalResultsMenu(ending);
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(menu));

                    List<RecordHeaderData> scores = RecordHeaderData.LoadHighScores();
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new ScoreMenu(scores, ending.StartDate)));

                }

                if (newRecruits.Count > 0)
                {
                    yield return new WaitForFrames(10);
                    GameManager.Instance.Fanfare("Fanfare/NewArea");
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_NEW_CHARS")));
                }

                yield return new WaitForFrames(20);

                GameManager.Instance.SceneOutcome = GameManager.Instance.RestartToTitle();
            }
            else
            {
                int completedZone = ZoneManager.Instance.CurrentZoneID;

                MidAdventure = true;
                ClearDungeonItems();

                //  if there isn't a next area, end the play, display the plaque, return to title screen
                //GameManager.Instance.Fanfare("Fanfare/MissionClear");
                Location = ZoneManager.Instance.CurrentZone.Name.ToLocal();

                Outcome = result;

                DataManager.Instance.MsgLog.Clear();
                //end the game with a recorded ending
                string recordFile = DataManager.Instance.EndPlay(this, null);

                MenuBase.Transparent = false;

                //save to the main file
                GameState state = DataManager.Instance.LoadMainGameState();
                if (state != null)
                {
                    MergeDexTo(state.Save);
                    state.Save.DungeonUnlocks[completedZone] = UnlockState.Completed;
                    DataManager.Instance.SaveMainGameState(state);
                }



                if (recorded)
                {
                    GameProgress ending = DataManager.Instance.GetRecord(Data.DataManager.REPLAY_PATH + recordFile);

                    if (fanfare)
                    {
                        if (result != ResultType.Cleared)
                            GameManager.Instance.Fanfare("Fanfare/MissionFail");
                        else
                            GameManager.Instance.Fanfare("Fanfare/MissionClear");
                    }
                    else
                        GameManager.Instance.SE("Menu/Skip");

                    FinalResultsMenu menu = new FinalResultsMenu(ending);
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(menu));

                    List<RecordHeaderData> scores = RecordHeaderData.LoadHighScores();
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(new ScoreMenu(scores, ending.StartDate)));
                }


                if (state != null && Stakes != DungeonStakes.None)
                {
                    bool allowTransfer = false;
                    QuestionDialog question = MenuManager.Instance.CreateQuestion(Text.FormatKey("DLG_TRANSFER_ASK"),
                        () => { allowTransfer = true; }, () => { });
                    yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.ProcessMenuCoroutine(question));

                    if (allowTransfer)
                    {
                        MainProgress mainSave = state.Save as MainProgress;
                        //put the new recruits into assembly
                        foreach (Character character in ActiveTeam.Players)
                        {
                            if (!(character.Dead && DataManager.Instance.GetSkin(character.BaseForm.Skin).Challenge))
                                mainSave.CharsToStore.Add(new CharData(character));
                        }
                        foreach (Character character in ActiveTeam.Assembly)
                        {
                            if (!(character.Dead && DataManager.Instance.GetSkin(character.BaseForm.Skin).Challenge))
                                mainSave.CharsToStore.Add(new CharData(character));
                        }

                        //put the new items into the storage
                        foreach (InvItem item in ActiveTeam.EnumerateInv())
                            mainSave.ItemsToStore.Add(item);
                        foreach (InvItem item in ActiveTeam.BoxStorage)
                            mainSave.ItemsToStore.Add(item);

                        mainSave.StorageToStore = ActiveTeam.Storage;

                        mainSave.MoneyToStore = state.Save.ActiveTeam.Money + state.Save.ActiveTeam.Bank;
                    }

                    DataManager.Instance.SaveMainGameState(state);

                    if (allowTransfer)
                        yield return CoroutineManager.Instance.StartCoroutine(MenuManager.Instance.SetDialogue(Text.FormatKey("DLG_TRANSFER_COMPLETE")));
                }



                yield return new WaitForFrames(20);
            }
        }


    }

}
