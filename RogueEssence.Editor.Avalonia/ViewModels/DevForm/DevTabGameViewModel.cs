using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Content;
using RogueEssence.Ground;

namespace RogueEssence.Dev.ViewModels
{
    public class DevTabGameViewModel : ViewModelBase
    {
        public DevTabGameViewModel()
        {
            Skills = new ObservableCollection<string>();
            Intrinsics = new ObservableCollection<string>();
            Statuses = new ObservableCollection<string>();
            Items = new ObservableCollection<string>();
        }

        public ObservableCollection<string> Skills { get; }

        private int chosenSkill;
        public int ChosenSkill
        {
            get { return chosenSkill; }
            set { this.SetIfChanged(ref chosenSkill, value); }
        }

        public ObservableCollection<string> Intrinsics { get; }

        private int chosenIntrinsic;
        public int ChosenIntrinsic
        {
            get { return chosenIntrinsic; }
            set { this.SetIfChanged(ref chosenIntrinsic, value); }
        }

        public ObservableCollection<string> Statuses { get; }

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
                MonsterTeam team = new MonsterTeam();
                Character focusedChar = DungeonScene.Instance.FocusedCharacter;
                CharData character = new CharData();
                character.BaseForm = focusedChar.CurrentForm;
                character.Nickname = "Clone";
                character.Level = focusedChar.Level;

                for (int ii = 0; ii < CharData.MAX_SKILL_SLOTS; ii++)
                    character.BaseSkills[ii] = new SlotSkill(focusedChar.BaseSkills[ii]);

                for (int ii = 0; ii < CharData.MAX_INTRINSIC_SLOTS; ii++)
                    character.BaseIntrinsics[ii] = focusedChar.BaseIntrinsics[ii];

                Character new_mob = new Character(character, team);
                team.Players.Add(new_mob);
                new_mob.CharLoc = focusedChar.CharLoc;
                new_mob.CharDir = focusedChar.CharDir;
                new_mob.Tactic = new AITactic(focusedChar.Tactic);
                new_mob.EquippedItem = new InvItem(focusedChar.EquippedItem);
                ZoneManager.Instance.CurrentMap.MapTeams.Add(new_mob.MemberTeam);
                new_mob.RefreshTraits();
                DungeonScene.Instance.PendingDevEvent = new_mob.OnMapStart();
            }
        }

        public void btnDespawn_Click()
        {
            lock (GameBase.lockObj)
                ZoneManager.Instance.CurrentMap.MapTeams.Clear();
        }

        public void btnSpawnItem_Click()
        {
            lock (GameBase.lockObj)
            {
                //Registry.SetValue(DiagManager.REG_PATH, "ItemChoice", cbSpawnItem.SelectedIndex);
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
                //Registry.SetValue(DiagManager.REG_PATH, "StatusChoice", cbStatus.SelectedIndex);
                StatusData entry = DataManager.Instance.GetStatus(chosenStatus);
                if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
                {
                    Character player = DungeonScene.Instance.FocusedCharacter;
                    if (entry.Targeted)
                        DungeonScene.Instance.LogMsg(String.Format("This is a targeted status."), false, true);
                    else
                    {
                        if (player.StatusEffects.ContainsKey(chosenStatus))
                            DungeonScene.Instance.PendingDevEvent = player.RemoveStatusEffect(chosenStatus);
                        else
                        {
                            StatusEffect status = new StatusEffect(chosenStatus);
                            status.LoadFromData();
                            DungeonScene.Instance.PendingDevEvent = player.AddStatusEffect(status);
                        }
                    }
                }
            }
        }

        public void btnLearnSkill_Click()
        {
            lock (GameBase.lockObj)
            {
                //Registry.SetValue(DiagManager.REG_PATH, "SkillChoice", cbSkills.SelectedIndex);
                if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
                {
                    Character player = DungeonScene.Instance.FocusedCharacter;
                    if (player.BaseSkills[CharData.MAX_SKILL_SLOTS - 1].SkillNum > -1)
                        player.DeleteSkill(0);
                    player.LearnSkill(chosenSkill, true);
                    DungeonScene.Instance.LogMsg(String.Format("Taught {1} to {0}.", player.Name, DataManager.Instance.GetSkill(chosenSkill).Name.ToLocal()), false, true);
                }
            }
        }

        public void btnGiveSkill_Click()
        {
            lock (GameBase.lockObj)
            {
                //Registry.SetValue(DiagManager.REG_PATH, "SkillChoice", cbSkills.SelectedIndex);
                Character player = DungeonScene.Instance.FocusedCharacter;
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                {
                    if (DungeonScene.Instance.GetMatchup(player, character) == Alignment.Foe)
                    {
                        if (character.BaseSkills[CharData.MAX_SKILL_SLOTS - 1].SkillNum > -1)
                            character.DeleteSkill(0);
                        character.LearnSkill(chosenSkill, true);
                    }

                }
                DungeonScene.Instance.LogMsg(String.Format("Taught {0} to all foes.", DataManager.Instance.GetSkill(chosenSkill).Name.ToLocal()), false, true);
            }
        }

        public void btnSetIntrinsic_Click()
        {
            lock (GameBase.lockObj)
            {
                //Registry.SetValue(DiagManager.REG_PATH, "IntrinsicChoice", cbIntrinsics.SelectedIndex);
                if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
                {
                    Character player = DungeonScene.Instance.FocusedCharacter;
                    player.LearnIntrinsic(chosenIntrinsic, 0);
                    DungeonScene.Instance.LogMsg(String.Format("Gave {1} to {0}.", player.Name, DataManager.Instance.GetIntrinsic(chosenIntrinsic).Name.ToLocal()), false, true);
                }
            }
        }

        public void btnGiveFoes_Click()
        {
            lock (GameBase.lockObj)
            {
                //Registry.SetValue(DiagManager.REG_PATH, "IntrinsicChoice", cbIntrinsics.SelectedIndex);
                Character player = DungeonScene.Instance.FocusedCharacter;
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                {
                    if (DungeonScene.Instance.GetMatchup(player, character) == Alignment.Foe)
                        character.LearnIntrinsic(chosenIntrinsic, 0);
                }
                DungeonScene.Instance.LogMsg(String.Format("Gave {0} to all foes.", DataManager.Instance.GetIntrinsic(chosenIntrinsic).Name.ToLocal()), false, true);
            }
        }
    }
}
