using ReactiveUI;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RogueEssence.Dev.ViewModels
{
    public class MapTabEntitiesViewModel : ViewModelBase
    {
        public delegate void EntityOp(Character ent);

        public MapTabEntitiesViewModel()
        {
            MonsterTeam team = new MonsterTeam();
            SelectedEntity = new Character(new CharData(), team);
            SelectedEntity.Tactic = new AITactic(DataManager.Instance.GetAITactic(0));
            team.Players.Add(SelectedEntity);

            Directions = new ObservableCollection<string>();
            foreach (Dir8 dir in DirExt.VALID_DIR8)
                Directions.Add(dir.ToLocal());

            Tactics = new ObservableCollection<string>();
            string[] tactic_names = DataManager.Instance.DataIndices[DataManager.DataType.AI].GetLocalStringArray();
            for (int ii = 0; ii < tactic_names.Length; ii++)
                Tactics.Add(ii.ToString("D2") + ": " + tactic_names[ii]);

            Monsters = new ObservableCollection<string>();
            string[] monster_names = DataManager.Instance.DataIndices[DataManager.DataType.Monster].GetLocalStringArray();
            for (int ii = 0; ii < monster_names.Length; ii++)
                Monsters.Add(ii.ToString("D3") + ": " + monster_names[ii]);

            Forms = new ObservableCollection<string>();

            Skins = new ObservableCollection<string>();
            string[] skin_names = DataManager.Instance.DataIndices[DataManager.DataType.Skin].GetLocalStringArray();
            for (int ii = 0; ii < DataManager.Instance.DataIndices[DataManager.DataType.Skin].Count; ii++)
                Skins.Add(skin_names[ii]);

            Genders = new ObservableCollection<string>();
            for (int ii = 0; ii <= (int)Gender.Female; ii++)
                Genders.Add(((Gender)ii).ToLocal());

            Intrinsics = new ObservableCollection<string>();
            string[] intrinsic_names = DataManager.Instance.DataIndices[DataManager.DataType.Intrinsic].GetLocalStringArray();
            for (int ii = 0; ii < intrinsic_names.Length; ii++)
                Intrinsics.Add(ii.ToString("D3") + ": " + intrinsic_names[ii]);

            Equips = new ObservableCollection<string>();
            Equips.Add("---: None");
            string[] item_names = DataManager.Instance.DataIndices[DataManager.DataType.Item].GetLocalStringArray();
            for (int ii = 0; ii < item_names.Length; ii++)
                Equips.Add(ii.ToString("D3") + ": " + item_names[ii]);

            Skills = new ObservableCollection<string>();
            Skills.Add("---: None");
            string[] skill_names = DataManager.Instance.DataIndices[DataManager.DataType.Skill].GetLocalStringArray();
            for (int ii = 0; ii < skill_names.Length; ii++)
                Skills.Add(ii.ToString("D3") + ": " + skill_names[ii]);

            speciesChanged();

        }

        private EntEditMode entMode;
        public EntEditMode EntMode
        {
            get { return entMode; }
            set
            {
                this.SetIfChanged(ref entMode, value);
            }
        }


        public ObservableCollection<string> Directions { get; }

        public int ChosenDir
        {
            get => (int)SelectedEntity.CharDir;
            set
            {
                SelectedEntity.CharDir = (Dir8)value;
                this.RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Tactics { get; }

        public int ChosenTactic
        {
            get { return SelectedEntity.Tactic.ID; }
            set
            {
                SelectedEntity.Tactic = new AITactic(DataManager.Instance.GetAITactic(value));
                this.RaisePropertyChanged();
            }
        }

        public string Nickname
        {
            get { return SelectedEntity.Nickname; }
            set { this.RaiseAndSetIfChanged(ref SelectedEntity.Nickname, value); }
        }


        public ObservableCollection<string> Monsters { get; }

        public int ChosenMonster
        {
            get
            {
                return SelectedEntity.BaseForm.Species;
            }
            set
            {
                SelectedEntity.BaseForm.Species = value;
                this.RaisePropertyChanged();
                speciesChanged();
            }
        }


        public ObservableCollection<string> Forms { get; }

        public int ChosenForm
        {
            get
            {
                return SelectedEntity.BaseForm.Form;
            }
            set
            {
                SelectedEntity.BaseForm.Form = value;
                SelectedEntity.HP = SelectedEntity.MaxHP;
                this.RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Skins { get; }

        public int ChosenSkin
        {
            get
            {
                return SelectedEntity.BaseForm.Skin;
            }
            set
            {
                SelectedEntity.BaseForm.Skin = value;
                this.RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Genders { get; }

        public int ChosenGender
        {
            get
            {
                return (int)SelectedEntity.BaseForm.Gender;
            }
            set
            {
                SelectedEntity.BaseForm.Gender = (Gender)value;
                this.RaisePropertyChanged();
            }
        }

        public int Level
        {
            get { return SelectedEntity.Level; }
            set
            {
                this.SetIfChanged(ref SelectedEntity.Level, value);
                SelectedEntity.HP = SelectedEntity.MaxHP;
            }
        }

        public ObservableCollection<string> Intrinsics { get; }

        public int ChosenIntrinsic
        {
            get { return SelectedEntity.Intrinsics[0].Element.ID; }
            set
            {
                SelectedEntity.ReplaceIntrinsic(0, value, false, false);
                this.RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Equips { get; }

        public int ChosenEquip
        {
            get { return SelectedEntity.EquippedItem.ID + 1; }
            set
            {
                lock (GameBase.lockObj)
                {
                    InvItem item = new InvItem(value - 1);
                    ItemData entry = (ItemData)item.GetData();
                    if (entry.MaxStack > 1)
                        item.HiddenValue = entry.MaxStack;
                    SelectedEntity.EquipItem(item);
                }
                this.RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Skills { get; }

        //TODO: replace with observable collection- it MUST sync with the original model
        public int ChosenSkill0
        {
            get { return SelectedEntity.BaseSkills[0].SkillNum + 1; }
            set
            {
                SelectedEntity.ReplaceSkill(value - 1, 0, SelectedEntity.Skills[0].Element.Enabled);
                this.RaisePropertyChanged();
            }
        }

        public int ChosenSkill1
        {
            get { return SelectedEntity.BaseSkills[1].SkillNum + 1; }
            set
            {
                SelectedEntity.ReplaceSkill(value - 1, 1, SelectedEntity.Skills[1].Element.Enabled);
                this.RaisePropertyChanged();
            }
        }

        public int ChosenSkill2
        {
            get { return SelectedEntity.BaseSkills[2].SkillNum + 1; }
            set
            {
                SelectedEntity.ReplaceSkill(value - 1, 2, SelectedEntity.Skills[2].Element.Enabled);
                this.RaisePropertyChanged();
            }
        }

        public int ChosenSkill3
        {
            get { return SelectedEntity.BaseSkills[3].SkillNum + 1; }
            set
            {
                SelectedEntity.ReplaceSkill(value - 1, 3, SelectedEntity.Skills[3].Element.Enabled);
                this.RaisePropertyChanged();
            }
        }

        public bool SkillsOn0
        {
            get { return SelectedEntity.Skills[0].Element.Enabled; }
            set
            {
                SelectedEntity.Skills[0].Element.Enabled = value;
                this.RaisePropertyChanged();
            }
        }

        public bool SkillsOn1
        {
            get { return SelectedEntity.Skills[1].Element.Enabled; }
            set
            {
                SelectedEntity.Skills[1].Element.Enabled = value;
                this.RaisePropertyChanged();
            }
        }

        public bool SkillsOn2
        {
            get { return SelectedEntity.Skills[2].Element.Enabled; }
            set
            {
                SelectedEntity.Skills[2].Element.Enabled = value;
                this.RaisePropertyChanged();
            }
        }

        public bool SkillsOn3
        {
            get { return SelectedEntity.Skills[3].Element.Enabled; }
            set
            {
                SelectedEntity.Skills[3].Element.Enabled = value;
                this.RaisePropertyChanged();
            }
        }

        public int HPBonus
        {
            get { return SelectedEntity.MaxHPBonus; }
            set
            {
                this.RaiseAndSetIfChanged(ref SelectedEntity.MaxHPBonus, value);
                SelectedEntity.HP = SelectedEntity.MaxHP;
            }
        }

        public int AtkBonus
        {
            get { return SelectedEntity.AtkBonus; }
            set { this.RaiseAndSetIfChanged(ref SelectedEntity.AtkBonus, value); }
        }

        public int DefBonus
        {
            get { return SelectedEntity.DefBonus; }
            set { this.RaiseAndSetIfChanged(ref SelectedEntity.DefBonus, value); }
        }

        public int MAtkBonus
        {
            get { return SelectedEntity.MAtkBonus; }
            set { this.RaiseAndSetIfChanged(ref SelectedEntity.MAtkBonus, value); }
        }

        public int MDefBonus
        {
            get { return SelectedEntity.MDefBonus; }
            set { this.RaiseAndSetIfChanged(ref SelectedEntity.MDefBonus, value); }
        }

        public int SpeedBonus
        {
            get { return SelectedEntity.SpeedBonus; }
            set { this.RaiseAndSetIfChanged(ref SelectedEntity.SpeedBonus, value); }
        }

        public Character SelectedEntity;


        public void ProcessInput(InputManager input)
        {
            Loc mapCoords = DungeonEditScene.Instance.ScreenCoordsToMapCoords(input.MouseLoc);

            switch (EntMode)
            {
                case EntEditMode.PlaceEntity:
                    {
                        if (input.JustPressed(FrameInput.InputType.LeftMouse))
                            PlaceEntity(mapCoords);
                        else if (input.JustPressed(FrameInput.InputType.RightMouse))
                            RemoveEntityAt(mapCoords);
                        break;
                    }
                case EntEditMode.SelectEntity:
                    {
                        if (input.JustPressed(FrameInput.InputType.LeftMouse))
                            SelectEntityAt(mapCoords);
                        else if (input[FrameInput.InputType.LeftMouse])
                            MoveEntity(mapCoords);
                        else if (input.Direction != input.PrevDirection)
                            MoveEntity(SelectedEntity.CharLoc + input.Direction.GetLoc());
                        break;
                    }
            }
        }

        private void speciesChanged()
        {
            lock (GameBase.lockObj)
            {
                int tempForm = ChosenForm;
                Forms.Clear();
                MonsterData monster = DataManager.Instance.GetMonster(ChosenMonster);
                for (int ii = 0; ii < monster.Forms.Count; ii++)
                    Forms.Add(ii.ToString("D2") + ": " + monster.Forms[ii].FormName.ToLocal());

                ChosenForm = Math.Clamp(tempForm, 0, Forms.Count - 1);
            }
        }


        /// <summary>
        /// Select the entity at that position and displays its data for editing
        /// </summary>
        /// <param name="position"></param>
        public void RemoveEntityAt(Loc position)
        {
            OperateOnEntityAt(position, RemoveEntity);
        }

        public void RemoveEntity(Character ent)
        {
            if (ent == null)
                return;

            for (int ii = 0; ii < ZoneManager.Instance.CurrentMap.MapTeams.Count; ii++)
            {
                if (ZoneManager.Instance.CurrentMap.MapTeams[ii].Players.Contains(ent))
                {
                    ZoneManager.Instance.CurrentMap.MapTeams[ii].Players.Remove(ent);
                    if (ZoneManager.Instance.CurrentMap.MapTeams[ii].Players.Count == 0)
                        ZoneManager.Instance.CurrentMap.MapTeams.RemoveAt(ii);
                }
            }
        }

        public void PlaceEntity(Loc position)
        {
            MonsterTeam team = new MonsterTeam();
            Character placeableEntity = SelectedEntity.Clone(team);
            team.Players.Add(placeableEntity);

            placeableEntity.CharLoc = position;
        }



        public void SelectEntity(Character ent)
        {
            if (ent != null)
                setEntity(ent);
            else
            {
                MonsterTeam team = new MonsterTeam();
                SelectedEntity = new Character(new CharData(), team);
                SelectedEntity.Tactic = new AITactic(DataManager.Instance.GetAITactic(0));
                team.Players.Add(SelectedEntity);
                setEntity(SelectedEntity);
            }
        }

        private void setEntity(Character ent)
        {
            SelectedEntity = ent;
            ChosenDir = ChosenDir;
            ChosenTactic = ChosenTactic;
            ChosenMonster = ChosenMonster;
            ChosenForm = ChosenForm;
            ChosenSkin = ChosenSkin;
            ChosenGender = ChosenGender;
            Level = Level;
            ChosenIntrinsic = ChosenIntrinsic;
            ChosenEquip = ChosenEquip;

            ChosenSkill0 = ChosenSkill0;
            ChosenSkill1 = ChosenSkill1;
            ChosenSkill2 = ChosenSkill2;
            ChosenSkill3 = ChosenSkill3;

            SkillsOn0 = SkillsOn0;
            SkillsOn1 = SkillsOn1;
            SkillsOn2 = SkillsOn2;
            SkillsOn3 = SkillsOn3;

            HPBonus = HPBonus;
            AtkBonus = AtkBonus;
            DefBonus = DefBonus;
            MAtkBonus = MAtkBonus;
            MDefBonus = MDefBonus;
            SpeedBonus = SpeedBonus;
        }

        /// <summary>
        /// Select the entity at that position and displays its data for editing
        /// </summary>
        /// <param name="position"></param>
        public void SelectEntityAt(Loc position)
        {
            OperateOnEntityAt(position, SelectEntity);
        }

        public void OperateOnEntityAt(Loc position, EntityOp op)
        {
            Character chara = ZoneManager.Instance.CurrentMap.GetCharAtLoc(position);
            op(chara);
        }

        private void MoveEntity(Loc loc)
        {
            if (SelectedEntity != null)
                SelectedEntity.CharLoc = loc;
        }
    }
}
