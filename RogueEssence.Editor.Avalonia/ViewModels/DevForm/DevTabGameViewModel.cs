using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Content;
using RogueEssence.Ground;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{
    public class DevTabGameViewModel : ViewModelBase
    {
        public DevTabGameViewModel()
        {
            Skills = new ObservableCollection<string>();
            SkillKeys = new List<string>();
            Intrinsics = new ObservableCollection<string>();
            IntrinsicKeys = new List<string>();

            Statuses = new ObservableCollection<string>();
            StatusKeys = new List<string>();
            Items = new ObservableCollection<string>();
        }

        public ObservableCollection<string> Skills { get; }

        public List<string> SkillKeys;

        private int chosenSkill;
        public int ChosenSkill
        {
            get { return chosenSkill; }
            set { this.SetIfChanged(ref chosenSkill, value); }
        }

        public ObservableCollection<string> Intrinsics { get; }

        public List<string> IntrinsicKeys;

        private int chosenIntrinsic;
        public int ChosenIntrinsic
        {
            get { return chosenIntrinsic; }
            set { this.SetIfChanged(ref chosenIntrinsic, value); }
        }

        public ObservableCollection<string> Statuses { get; }

        public List<string> StatusKeys;

        private int chosenStatus;
        public int ChosenStatus
        {
            get { return chosenStatus; }
            set { this.SetIfChanged(ref chosenStatus, value); }
        }

        public ObservableCollection<string> Items { get; }

        private int chosenItem;
        public int ChosenItem
        {
            get { return chosenItem; }
            set { this.SetIfChanged(ref chosenItem, value); }
        }

        private bool hideSprites;
        public bool HideSprites
        {
            get { return hideSprites; }
            set
            {
                this.SetIfChanged(ref hideSprites, value);
                DataManager.Instance.HideChars = value;
            }
        }

        private bool hideObjects;
        public bool HideObjects
        {
            get { return hideObjects; }
            set { this.SetIfChanged(ref hideObjects, value);
                DataManager.Instance.HideObjects = value;
            }
        }


        public void btnSpawn_Click()
        {
            lock (GameBase.lockObj)
            {
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    MonsterTeam team = new MonsterTeam();
                    Character new_mob = DungeonScene.Instance.FocusedCharacter.Clone(team);
                    ZoneManager.Instance.CurrentMap.MapTeams.Add(new_mob.MemberTeam);
                    new_mob.RefreshTraits();
                    DungeonScene.Instance.PendingDevEvent = new_mob.OnMapStart();
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
        }

        public void btnDespawn_Click()
        {
            lock (GameBase.lockObj)
            {
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    ZoneManager.Instance.CurrentMap.AllyTeams.Clear();
                    ZoneManager.Instance.CurrentMap.MapTeams.Clear();
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
        }

        public void btnSpawnItem_Click()
        {
            lock (GameBase.lockObj)
            {
                DevForm.SetConfig("ItemChoice", chosenItem);
                InvItem item = new InvItem(chosenItem);
                ItemData entry = (ItemData)item.GetData();
                if (entry.MaxStack > 1)
                    item.HiddenValue = entry.MaxStack;
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                    DungeonScene.Instance.PendingDevEvent = DungeonScene.Instance.DropItem(item, DungeonScene.Instance.FocusedCharacter.CharLoc);
                else if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                {
                    if (DataManager.Instance.Save.ActiveTeam.GetInvCount() < DataManager.Instance.Save.ActiveTeam.GetMaxInvSlots(ZoneManager.Instance.CurrentZone))
                    {
                        GameManager.Instance.SE("Menu/Sort");
                        DataManager.Instance.Save.ActiveTeam.AddToInv(item);
                    }
                    else
                        GameManager.Instance.SE("Menu/Cancel");
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
        }

        public void btnToggleStatus_Click()
        {
            lock (GameBase.lockObj)
            {
                DevForm.SetConfig("StatusChoice", chosenStatus);
                StatusData entry = DataManager.Instance.GetStatus(StatusKeys[chosenStatus]);
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
                    {
                        Character player = DungeonScene.Instance.FocusedCharacter;
                        if (entry.Targeted)
                            DungeonScene.Instance.LogMsg(String.Format("This is a targeted status."), false, true);
                        else
                        {
                            if (player.StatusEffects.ContainsKey(StatusKeys[chosenStatus]))
                                DungeonScene.Instance.PendingDevEvent = player.RemoveStatusEffect(StatusKeys[chosenStatus]);
                            else
                            {
                                StatusEffect status = new StatusEffect(StatusKeys[chosenStatus]);
                                status.LoadFromData();
                                DungeonScene.Instance.PendingDevEvent = player.AddStatusEffect(status);
                            }
                        }
                    }
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
        }

        public void btnLearnSkill_Click()
        {
            lock (GameBase.lockObj)
            {
                DevForm.SetConfig("SkillChoice", chosenSkill);

                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
                    {
                        Character player = DungeonScene.Instance.FocusedCharacter;
                        if (!String.IsNullOrEmpty(player.BaseSkills[CharData.MAX_SKILL_SLOTS - 1].SkillNum))
                            player.DeleteSkill(0);
                        player.LearnSkill(SkillKeys[chosenSkill], true);
                        DungeonScene.Instance.LogMsg(String.Format("Taught {1} to {0}.", player.Name, DataManager.Instance.GetSkill(SkillKeys[chosenSkill]).Name.ToLocal()), false, true);
                    }
                }
                else if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                {
                    if (DataManager.Instance.Save.ActiveTeam.Players.Count > 0)
                    {
                        Character player = DataManager.Instance.Save.ActiveTeam.Leader;
                        if (!String.IsNullOrEmpty(player.BaseSkills[CharData.MAX_SKILL_SLOTS - 1].SkillNum))
                            player.DeleteSkill(0);
                        player.LearnSkill(SkillKeys[chosenSkill], true);
                        GameManager.Instance.SE("Menu/Sort");
                    }
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
        }

        public void btnGiveSkill_Click()
        {
            lock (GameBase.lockObj)
            {
                DevForm.SetConfig("SkillChoice", chosenSkill);
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    Character player = DungeonScene.Instance.FocusedCharacter;
                    foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                    {
                        if (DungeonScene.Instance.GetMatchup(player, character) == Alignment.Foe)
                        {
                            if (!String.IsNullOrEmpty(character.BaseSkills[CharData.MAX_SKILL_SLOTS - 1].SkillNum))
                                character.DeleteSkill(0);
                            character.LearnSkill(SkillKeys[chosenSkill], true);
                        }
                    }
                    DungeonScene.Instance.LogMsg(String.Format("Taught {0} to all foes.", DataManager.Instance.GetSkill(SkillKeys[chosenSkill]).Name.ToLocal()), false, true);
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
        }

        public void btnSetIntrinsic_Click()
        {
            lock (GameBase.lockObj)
            {
                DevForm.SetConfig("IntrinsicChoice", chosenIntrinsic);

                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
                    {
                        Character player = DungeonScene.Instance.FocusedCharacter;
                        player.LearnIntrinsic(IntrinsicKeys[chosenIntrinsic], 0);
                        DungeonScene.Instance.LogMsg(String.Format("Gave {1} to {0}.", player.Name, DataManager.Instance.GetIntrinsic(IntrinsicKeys[chosenIntrinsic]).Name.ToLocal()), false, true);
                    }
                }
                else if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                {
                    if (DataManager.Instance.Save.ActiveTeam.Players.Count > 0)
                    {
                        Character player = DataManager.Instance.Save.ActiveTeam.Leader;
                        player.LearnIntrinsic(IntrinsicKeys[chosenIntrinsic], 0);
                        GameManager.Instance.SE("Menu/Sort");
                    }
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
        }

        public void btnGiveFoes_Click()
        {
            lock (GameBase.lockObj)
            {
                DevForm.SetConfig("IntrinsicChoice", chosenIntrinsic);

                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    Character player = DungeonScene.Instance.FocusedCharacter;
                    foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                    {
                        if (DungeonScene.Instance.GetMatchup(player, character) == Alignment.Foe)
                            character.LearnIntrinsic(IntrinsicKeys[chosenIntrinsic], 0);
                    }
                    DungeonScene.Instance.LogMsg(String.Format("Gave {0} to all foes.", DataManager.Instance.GetIntrinsic(IntrinsicKeys[chosenIntrinsic]).Name.ToLocal()), false, true);
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
        }
    }
}
