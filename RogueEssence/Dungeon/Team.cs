using System;
using System.Collections;
using System.Collections.Generic;
using RogueEssence.Data;
using System.Runtime.Serialization;
using RogueElements;
using RogueEssence.LevelGen;
using Newtonsoft.Json;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class Team
    {
        public EventedList<Character> Players { get; set; }
        public EventedList<Character> Guests { get; set; }

        public int LeaderIndex;

        /// <summary>
        /// If set to true, will attack/be attacked by Foe faction when in Ally faction.
        /// </summary>
        public bool FoeConflict;

        private List<InvItem> inventory;

        [NonSerialized]
        public Map ContainingMap;

        [NonSerialized]
        public Faction MapFaction;

        public Team()
        {
            Players = new EventedList<Character>();
            Guests = new EventedList<Character>();
            inventory = new List<InvItem>();
        }

        public Character Leader { get { return Players[LeaderIndex]; } }

        public int MemberGuestCount { get { return Players.Count + Guests.Count; } }

        public IEnumerable<Character> IterateByRank()
        {
            foreach(Character character in IterateMainByRank())
                yield return character;
            foreach (Character character in Guests)
                yield return character;
        }

        public IEnumerable<Character> IterateMainByRank()
        {
            yield return Leader;
            foreach (Character character in Players)
            {
                if (character != Leader)
                    yield return character;
            }
        }

        public IEnumerable<Character> EnumerateChars()
        {
            foreach (Character chara in Players)
                yield return chara;
            foreach (Character chara in Guests)
                yield return chara;
        }

        protected virtual void setMemberEvents()
        {
            Players.ItemAdding += addingMember;
            Guests.ItemAdding += addingMember;
            Players.ItemChanging += settingPlayer;
            Guests.ItemChanging += settingGuest;
            Players.ItemRemoving += removingMember;
            Guests.ItemRemoving += removingMember;
            Players.ItemsClearing += clearingPlayers;
            Guests.ItemsClearing += clearingGuests;
        }

        private void settingPlayer(int index, Character chara)
        {
            Players[index].MemberTeam = null;
            chara.MemberTeam = this;
            //update location caches
            ContainingMap?.RemoveCharLookup(Players[index]);
            ContainingMap?.AddCharLookup(chara);
        }
        private void settingGuest(int index, Character chara)
        {
            Guests[index].MemberTeam = null;
            chara.MemberTeam = this;
            //update location caches
            ContainingMap?.RemoveCharLookup(Guests[index]);
            ContainingMap?.AddCharLookup(chara);
        }
        private void addingMember(int index, Character chara)
        {
            chara.MemberTeam = this;
            //update location caches
            ContainingMap?.AddCharLookup(chara);
        }
        private void removingMember(int index, Character chara)
        {
            chara.MemberTeam = null;
            //update location caches
            ContainingMap?.RemoveCharLookup(chara);
        }
        private void clearingPlayers()
        {
            foreach (Character chara in Players)
            {
                chara.MemberTeam = null;
                //update location caches
                ContainingMap?.RemoveCharLookup(chara);
            }
        }
        private void clearingGuests()
        {
            foreach (Character chara in Guests)
            {
                chara.MemberTeam = null;
                //update location caches
                ContainingMap?.RemoveCharLookup(chara);
            }
        }

        public int GetInvCount()
        {
            return inventory.Count;
        }

        public InvItem GetInv(int slot)
        {
            return inventory[slot];
        }

        public IEnumerable<InvItem> EnumerateInv()
        {
            foreach (InvItem item in inventory)
                yield return item;
        }

        public void AddToInv(InvItem invItem, bool skipCheck = false)
        {
            inventory.Add(invItem);
            if (skipCheck)
                return;
            UpdateInv(null, invItem);
        }
        public void RemoveFromInv(int index, bool skipCheck = false)
        {
            InvItem invItem = inventory[index];
            inventory.RemoveAt(index);
            if (skipCheck)
                return;
            UpdateInv(invItem, null);
        }
        public void UpdateInv(InvItem oldItem, InvItem newItem)
        {
            bool update = false;
            if (oldItem != null)
            {
                ItemData itemEntry = DataManager.Instance.GetItem(oldItem.ID);
                if (itemEntry.BagEffect)
                    update = true;
            }
            if (newItem != null)
            {
                ItemData itemEntry = DataManager.Instance.GetItem(newItem.ID);
                if (itemEntry.BagEffect)
                    update = true;
            }
            if (oldItem == null && newItem == null)
                update = true;
            if (update)
            {
                foreach (Character chara in Players)
                    chara.RefreshTraits();
                foreach (Character chara in Guests)
                    chara.RefreshTraits();
            }
        }

        public void SortItems()
        {
            List<InvItem> newInv = new List<InvItem>();
            //for each inv item
            for (int kk = 0; kk < inventory.Count; kk++)
            {
                //find its new place
                for (int ii = newInv.Count; ii >= 0; ii--)
                {
                    if (ii == 0 || SucceedsInvItem(inventory[kk], newInv[ii - 1]))
                    {
                        newInv.Insert(ii, inventory[kk]);
                        break;
                    }
                }
            }
            inventory = newInv;
        }

        private bool SucceedsInvItem(InvItem inv1, InvItem inv2)
        {
            ItemData entry1 = DataManager.Instance.GetItem(inv1.ID);
            ItemData entry2 = DataManager.Instance.GetItem(inv2.ID);
            if (entry1.UsageType > entry2.UsageType)
                return true;
            else if (entry1.UsageType < entry2.UsageType)
                return false;
            return (inv1.ID >= inv2.ID);
        }

        public int GetInvValue()
        {
            int invValue = 0;
            foreach (InvItem item in inventory)
                invValue += item.GetSellValue();
            return invValue;
        }


        public CharIndex GetCharIndex(Character character)
        {
            for (int jj = 0; jj < Players.Count; jj++)
            {
                if (character == Players[jj])
                    return new CharIndex(Faction.None, -1, false, jj);
            }
            for (int jj = 0; jj < Guests.Count; jj++)
            {
                if (character == Guests[jj])
                    return new CharIndex(Faction.None, -1, true, jj);
            }
            return CharIndex.Invalid;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //No need to set member events since they'd already be set during the class construction phase of deserialization
            reconnectTeamReference();

            setMemberEvents();
        }

        protected virtual void reconnectTeamReference()
        {
            //reconnect Players' references
            foreach (Character player in Players)
                player.MemberTeam = this;
            foreach (Character player in Guests)
                player.MemberTeam = this;
        }

        public virtual void SaveLua()
        {
            foreach (Character player in Players)
                player.SaveLua();
            foreach (Character player in Guests)
                player.SaveLua();
        }

        public virtual void LoadLua()
        {
            foreach (Character player in Players)
                player.LoadLua();
            foreach (Character player in Guests)
                player.LoadLua();
        }
    }

    [Serializable]
    public class MonsterTeam : Team
    {
        public bool Unrecruitable;

        public MonsterTeam() : this(true)
        { }

        [JsonConstructor]
        public MonsterTeam(bool initEvents) : base()
        {
            if (initEvents)
                setMemberEvents();
        }
    }

    [Serializable]
    public class ExplorerTeam : Team
    {
        public const int MAX_TEAM_SLOTS = 4;

        public int MaxInv;

        public string Name;
        public EventedList<Character> Assembly;
        public int[] Storage;
        public List<InvItem> BoxStorage;
        public int Bank;
        public int Money;
        public int Rank { get; private set; }
        public int Fame;
        public int RankExtra;

        public ExplorerTeam() : this(true)
        { }
        [JsonConstructor]
        public ExplorerTeam(bool initEvents) : base()
        {
            Name = "";
            Assembly = new EventedList<Character>();
            BoxStorage = new List<InvItem>();
            Storage = new int[10000];//TODO: remove this magic number and make it an adjustable value

            if (initEvents)
                setMemberEvents();
        }

        public void SetRank(int rank)
        {
            Rank = rank;
            MaxInv = DataManager.Instance.GetRank(rank).BagSize;
        }

        public int GetMaxInvSlots(Zone zone)
        {
            int slots = MaxInv;
            if (zone != null && zone.BagSize > -1 && zone.BagSize < slots)
                slots = zone.BagSize;
            foreach (Character player in Players)
            {
                if (player.EquippedItem.ID > -1)
                    slots--;
            }
            return slots;
        }

        public int GetMaxTeam(Zone zone)
        {
            int slots = MAX_TEAM_SLOTS;
            if (zone != null && zone.TeamSize > -1 && zone.TeamSize < slots)
                slots = zone.TeamSize;
            return slots;
        }

        public string GetReferenceName()
        {
            if (Name != "")
                return Name;
            else
                return Players[0].BaseName;
        }

        public string GetDisplayName()
        {
            string name = Players[0].BaseName;
            if (Name != "")
                name = Name;
            return String.Format("[color=#FFA5FF]{0}[color]", name);
        }


        public List<InvItem> TakeItems(List<int> indices, bool remove = true)
        {
            List<int> removedBoxSlots = new List<int>();
            List<InvItem> invToTake = new List<InvItem>();
            for (int ii = 0; ii < indices.Count; ii++)
            {
                int index = indices[ii];
                if (index < DataManager.Instance.DataIndices[DataManager.DataType.Item].Count)
                {
                    ItemData entry = DataManager.Instance.GetItem(index);
                    if (entry.MaxStack > 1)
                    {
                        int existingStack = -1;
                        for (int jj = 0; jj < invToTake.Count; jj++)
                        {
                            if (invToTake[jj].ID == index && invToTake[jj].HiddenValue < entry.MaxStack)
                            {
                                existingStack = jj;
                                break;
                            }
                        }
                        if (existingStack > -1)
                            invToTake[existingStack].HiddenValue++;
                        else
                            invToTake.Add(new InvItem(index, false, 1));
                    }
                    else
                        invToTake.Add(new InvItem(index));
                    if (remove)
                        Storage[index]--;
                }
                else
                {
                    invToTake.Add(BoxStorage[index - DataManager.Instance.DataIndices[DataManager.DataType.Item].Count]);
                    removedBoxSlots.Add(index - DataManager.Instance.DataIndices[DataManager.DataType.Item].Count);
                }
            }
            removedBoxSlots.Sort();
            if (remove)
            {
                for (int ii = removedBoxSlots.Count - 1; ii >= 0; ii--)
                    BoxStorage.RemoveAt(removedBoxSlots[ii]);
            }

            return invToTake;
        }

        public void StoreItems(List<InvItem> invToStore)
        {
            foreach(InvItem item in invToStore)
            {
                ItemData entry = DataManager.Instance.GetItem(item.ID);
                if (entry.MaxStack > 1)
                    Storage[item.ID] += item.HiddenValue;
                else if (entry.UsageType == ItemData.UseType.Box)
                    BoxStorage.Add(item);
                else
                    Storage[item.ID]++;
            }
        }

        public void AddMoney(Character character, int amount)
        {
            Money += amount;

            //TODO: gained money events
        }

        public void LoseMoney(Character character, int amount)
        {
            Money -= amount;

            //TODO: lost money events
        }

        public int GetStorageValue()
        {
            int invValue = 0;
            foreach (InvItem item in BoxStorage)
                invValue += item.GetSellValue();
            for (int ii = 0; ii < Storage.Length; ii++)
            {
                if (Storage[ii] > 0)
                    invValue += DataManager.Instance.GetItem(ii).Price;
            }
            return invValue;
        }

        public int GetTotalScore()
        {
            return GetInvValue() + GetStorageValue() + Money + Bank;
        }

        public void AddToSortedAssembly(Character chara)
        {
            int idx = 0;
            while (idx < Assembly.Count)
            {
                if (!Assembly[idx].IsFavorite)
                    break;
                idx++;
            }
            Assembly.Insert(idx, chara);
        }

        public Character CreatePlayer(IRandom rand, MonsterID form, int level, int intrinsic, int personality)
        {
            MonsterID formData = form;
            MonsterData dex = DataManager.Instance.GetMonster(formData.Species);

            CharData character = new CharData();
            character.BaseForm = formData;
            character.Level = level;

            BaseMonsterForm formEntry = dex.Forms[formData.Form];

            List<int> final_skills = formEntry.RollLatestSkills(character.Level, new List<int>());
            for(int ii = 0; ii < final_skills.Count; ii++)
                character.BaseSkills[ii] = new SlotSkill(final_skills[ii]);

            if (form.Gender == Gender.Unknown)
                character.BaseForm.Gender = dex.Forms[formData.Form].RollGender(rand);
            
            if (intrinsic == -1)
                character.BaseIntrinsics[0] = formEntry.RollIntrinsic(rand, 2);
            else
                character.BaseIntrinsics[0] = intrinsic;

            if (personality == -1)
                character.Discriminator = rand.Next();
            else
                character.Discriminator = personality;


            character.OriginalUUID = DataManager.Instance.Save.UUID;
            character.OriginalTeam = DataManager.Instance.Save.ActiveTeam.Name;
            character.MetAt = Text.FormatKey("MET_AT_START");
            character.MetLoc = ZoneLoc.Invalid;

            return CreatePlayer(character);
        }



        public Character CreatePlayer(CharData character)
        {
            Character player = new Character(character);
            foreach (BackReference<Skill> skill in player.Skills)
            {
                if (skill.Element.SkillNum > -1)
                {
                    SkillData entry = DataManager.Instance.GetSkill(skill.Element.SkillNum);
                    skill.Element.Enabled = (entry.Data.Category == BattleData.SkillCategory.Physical || entry.Data.Category == BattleData.SkillCategory.Magical);
                }
            }
            AITactic tactic = DataManager.Instance.GetAITactic(0);
            player.Tactic = new AITactic(tactic);

            return player;
        }

        private void settingAssembly(int index, Character chara)
        {
            Assembly[index].MemberTeam = null;
            chara.MemberTeam = this;
        }
        private void addingAssembly(int index, Character chara)
        {
            chara.MemberTeam = this;
        }
        private void removingAssembly(int index, Character chara)
        {
            chara.MemberTeam = null;
        }
        private void clearingAssembly()
        {
            foreach (Character chara in Assembly)
                chara.MemberTeam = null;
        }

        protected override void setMemberEvents()
        {
            base.setMemberEvents();

            Assembly.ItemAdding += addingAssembly;
            Assembly.ItemChanging += settingAssembly;
            Assembly.ItemRemoving += removingAssembly;
            Assembly.ItemsClearing += clearingAssembly;
        }

        protected override void reconnectTeamReference()
        {
            base.reconnectTeamReference();
            foreach (Character player in Assembly)
                player.MemberTeam = this;
        }

        public override void SaveLua()
        {
            base.SaveLua();
            foreach (Character player in Assembly)
                player.SaveLua();
        }

        public override void LoadLua()
        {
            base.LoadLua();
            foreach (Character player in Assembly)
                player.LoadLua();
        }
    }

    [Serializable]
    public struct TempCharBackRef
    {
        public bool Assembly;
        public int Index;

        public TempCharBackRef(bool assembly, int index)
        {
            Assembly = assembly;
            Index = index;
        }
    }
}
