using Avalonia.Controls;
using ReactiveUI;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev.Views;
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
            //Teams = new TeamBoxViewModel(DiagManager.Instance.DevEditor.MapEditor.Edits);

            MonsterTeam team = new MonsterTeam();
            SelectedEntity = new Character(new CharData());
            SelectedEntity.Level = 1;
            SelectedEntity.Tactic = new AITactic(DataManager.Instance.GetAITactic(DataManager.Instance.DefaultAI));
            team.Players.Add(SelectedEntity);

            Directions = new ObservableCollection<string>();
            foreach (Dir8 dir in DirExt.VALID_DIR8)
                Directions.Add(dir.ToLocal());

            Tactics = new ObservableCollection<string>();
            Dictionary<string, string> tactic_names = DataManager.Instance.DataIndices[DataManager.DataType.AI].GetLocalStringArray(true);

            tacticKeys = new List<string>();

            foreach (string key in tactic_names.Keys)
            {
                tacticKeys.Add(key);
                Tactics.Add(key + ": " + tactic_names[key]);
            }

            Monsters = new ObservableCollection<string>();
            monsterKeys = new List<string>();
            Dictionary<string, string> monster_names = DataManager.Instance.DataIndices[DataManager.DataType.Monster].GetLocalStringArray(true);
            foreach (string key in monster_names.Keys)
            {
                Monsters.Add(key + ": " + monster_names[key]);
                monsterKeys.Add(key);
            }

            Forms = new ObservableCollection<string>();

            Skins = new ObservableCollection<string>();
            skinKeys = new List<string>();
            Dictionary<string, string> skin_names = DataManager.Instance.DataIndices[DataManager.DataType.Skin].GetLocalStringArray(true);
            foreach (string key in skin_names.Keys)
            {
                Skins.Add(key + ": " + skin_names[key]);
                skinKeys.Add(key);
            }

            Genders = new ObservableCollection<string>();
            for (int ii = 0; ii <= (int)Gender.Female; ii++)
                Genders.Add(((Gender)ii).ToLocal());

            Intrinsics = new ObservableCollection<string>();
            intrinsicKeys = new List<string>();
            Intrinsics.Add("---: None");
            intrinsicKeys.Add("");
            Dictionary<string, string> intrinsic_names = DataManager.Instance.DataIndices[DataManager.DataType.Intrinsic].GetLocalStringArray(true);
            foreach (string key in intrinsic_names.Keys)
            {
                Intrinsics.Add(key + ": " + intrinsic_names[key]);
                intrinsicKeys.Add(key);
            }

            Equips = new ObservableCollection<string>();
            itemKeys = new List<string>();
            Equips.Add("---: None");
            itemKeys.Add("");
            Dictionary<string, string> item_names = DataManager.Instance.DataIndices[DataManager.DataType.Item].GetLocalStringArray(true);
            foreach (string key in item_names.Keys)
            {
                Equips.Add(key + ": " + item_names[key]);
                itemKeys.Add(key);
            }

            Skills = new ObservableCollection<string>();
            skillKeys = new List<string>();
            Skills.Add("**Empty**");
            skillKeys.Add("");
            Dictionary<string, string> skill_names = DataManager.Instance.DataIndices[DataManager.DataType.Skill].GetLocalStringArray(true);
            foreach (string key in skill_names.Keys)
            {
                Skills.Add(key + ": " + skill_names[key]);
                skillKeys.Add(key);
            }

            speciesChanged();

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            Statuses = new CollectionBoxViewModel(form.MapEditForm, new StringConv(typeof(StatusEffect), new object[0]));
            Statuses.OnMemberChanged += Statuses_Changed;
            Statuses.OnEditItem += Statuses_EditItem;
        }

        private EntEditMode entMode;
        public EntEditMode EntMode
        {
            get { return entMode; }
            set
            {
                this.SetIfChanged(ref entMode, value);
                EntModeChanged();
            }
        }

        public TeamBoxViewModel Teams { get; set; }

        public ObservableCollection<string> Directions { get; }

        public int ChosenDir
        {
            get => (int)SelectedEntity.CharDir;
            set
            {
                SelectedEntity.CharDir = (Dir8)value;
                this.RaisePropertyChanged();
                animChanged();
            }
        }

        private List<string> tacticKeys;

        public ObservableCollection<string> Tactics { get; }

        public int ChosenTactic
        {
            get { return tacticKeys.IndexOf(SelectedEntity.Tactic.ID); }
            set
            {
                SelectedEntity.Tactic = new AITactic(DataManager.Instance.GetAITactic(tacticKeys[value]));
                this.RaisePropertyChanged();
            }
        }

        public string Nickname
        {
            get { return SelectedEntity.Nickname; }
            set { this.RaiseAndSet(ref SelectedEntity.Nickname, value); }
        }


        private List<string> monsterKeys;

        public ObservableCollection<string> Monsters { get; }

        public int ChosenMonster
        {
            get
            {
                return monsterKeys.IndexOf(SelectedEntity.BaseForm.Species);
            }
            set
            {
                SelectedEntity.BaseForm.Species = monsterKeys[value];
                SelectedEntity.RestoreForm();
                this.RaisePropertyChanged();
                speciesChanged();
                updateStats();
                animChanged();
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
                if (value > -1)
                {
                    SelectedEntity.BaseForm.Form = value;
                    SelectedEntity.RestoreForm();
                    SelectedEntity.HP = SelectedEntity.MaxHP;
                    updateStats();
                }
                this.RaisePropertyChanged();
                animChanged();
            }
        }

        private List<string> skinKeys;

        public ObservableCollection<string> Skins { get; }

        public int ChosenSkin
        {
            get
            {
                return skinKeys.IndexOf(SelectedEntity.BaseForm.Skin);
            }
            set
            {
                SelectedEntity.BaseForm.Skin = skinKeys[value];
                SelectedEntity.RestoreForm();
                this.RaisePropertyChanged();
                animChanged();
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
                SelectedEntity.RestoreForm();
                this.RaisePropertyChanged();
                animChanged();
            }
        }

        public int Level
        {
            get { return SelectedEntity.Level; }
            set
            {
                this.RaiseAndSet(ref SelectedEntity.Level, value);
                SelectedEntity.HP = SelectedEntity.MaxHP;
                updateStats();
            }
        }

        public ObservableCollection<string> Intrinsics { get; }

        List<string> intrinsicKeys;

        public int ChosenIntrinsic
        {
            get { return intrinsicKeys.IndexOf(SelectedEntity.Intrinsics[0].Element.ID); }
            set
            {
                SelectedEntity.LearnIntrinsic(intrinsicKeys[value], 0);
                this.RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Equips { get; }

        List<string> itemKeys;

        public int ChosenEquip
        {
            get { return itemKeys.IndexOf(SelectedEntity.EquippedItem.ID); }
            set
            {
                lock (GameBase.lockObj)
                {
                    if (value > 0)
                    {
                        InvItem item = new InvItem(itemKeys[value]);
                        ItemData entry = (ItemData)item.GetData();
                        if (entry.MaxStack > 1)
                            item.Amount = entry.MaxStack;
                        SelectedEntity.EquippedItem = item;
                    }
                    else
                        SelectedEntity.EquippedItem = new InvItem();
                }
                this.RaisePropertyChanged();
            }
        }

        private List<string> skillKeys;
        public ObservableCollection<string> Skills { get; }

        //TODO: replace with observable collection- it MUST sync with the original model
        public int ChosenSkill0
        {
            get { return skillKeys.IndexOf(SelectedEntity.BaseSkills[0].SkillNum); }
            set
            {
                SelectedEntity.EditSkill(skillKeys[value], 0, SelectedEntity.Skills[0].Element.Enabled);
                this.RaisePropertyChanged();
            }
        }

        public int ChosenSkill1
        {
            get { return skillKeys.IndexOf(SelectedEntity.BaseSkills[1].SkillNum); }
            set
            {
                SelectedEntity.EditSkill(skillKeys[value], 1, SelectedEntity.Skills[1].Element.Enabled);
                this.RaisePropertyChanged();
            }
        }

        public int ChosenSkill2
        {
            get { return skillKeys.IndexOf(SelectedEntity.BaseSkills[2].SkillNum); }
            set
            {
                SelectedEntity.EditSkill(skillKeys[value], 2, SelectedEntity.Skills[2].Element.Enabled);
                this.RaisePropertyChanged();
            }
        }

        public int ChosenSkill3
        {
            get { return skillKeys.IndexOf(SelectedEntity.BaseSkills[3].SkillNum); }
            set
            {
                SelectedEntity.EditSkill(skillKeys[value], 3, SelectedEntity.Skills[3].Element.Enabled);
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
                this.RaiseAndSet(ref SelectedEntity.MaxHPBonus, value);
                SelectedEntity.HP = SelectedEntity.MaxHP;
                HPTotal = HPTotal;
            }
        }

        public int AtkBonus
        {
            get { return SelectedEntity.AtkBonus; }
            set { this.RaiseAndSet(ref SelectedEntity.AtkBonus, value); AtkTotal = AtkTotal; }
        }

        public int DefBonus
        {
            get { return SelectedEntity.DefBonus; }
            set { this.RaiseAndSet(ref SelectedEntity.DefBonus, value); DefTotal = DefTotal; }
        }

        public int MAtkBonus
        {
            get { return SelectedEntity.MAtkBonus; }
            set { this.RaiseAndSet(ref SelectedEntity.MAtkBonus, value); MAtkTotal = MAtkTotal; }
        }

        public int MDefBonus
        {
            get { return SelectedEntity.MDefBonus; }
            set { this.RaiseAndSet(ref SelectedEntity.MDefBonus, value); MDefTotal = MDefTotal; }
        }

        public int SpeedBonus
        {
            get { return SelectedEntity.SpeedBonus; }
            set { this.RaiseAndSet(ref SelectedEntity.SpeedBonus, value); SpeedTotal = SpeedTotal; }
        }


        public string HPTotal
        {
            get { return "= " + SelectedEntity.MaxHP; }
            set { this.RaisePropertyChanged(); }
        }

        public string AtkTotal
        {
            get { return "= " + SelectedEntity.Atk; }
            set { this.RaisePropertyChanged(); }
        }

        public string DefTotal
        {
            get { return "= " + SelectedEntity.Def; }
            set { this.RaisePropertyChanged(); }
        }

        public string MAtkTotal
        {
            get { return "= " + SelectedEntity.MAtk; }
            set { this.RaisePropertyChanged(); }
        }

        public string MDefTotal
        {
            get { return "= " + SelectedEntity.MDef; }
            set { this.RaisePropertyChanged(); }
        }

        public string SpeedTotal
        {
            get { return "= " + SelectedEntity.Speed; }
            set { this.RaisePropertyChanged(); }
        }

        public bool Unrecruitable
        {
            get { return SelectedEntity.Unrecruitable; }
            set { this.RaiseAndSet(ref SelectedEntity.Unrecruitable, value); }
        }

        public bool Ally
        {
            get;
            set;
        }
        
        public CollectionBoxViewModel Statuses { get; set; }

        public Character SelectedEntity;

        private void EntModeChanged()
        {
            if (entMode == EntEditMode.SelectEntity)
            {
                DungeonEditScene.Instance.CharacterInProgress = null;
            }
            else
            {
                //copy the selection
                MonsterTeam team = new MonsterTeam();
                Character clone = SelectedEntity.Clone(team);
                team.Players.Add(clone);
                setEntity(clone);
                animChanged();
            }
        }

        public void TabbedIn()
        {
            animChanged();
        }

        public void TabbedOut()
        {
            DungeonEditScene.Instance.CharacterInProgress = null;
        }


        public void ProcessUndo()
        {
            if (EntMode == EntEditMode.SelectEntity)
                SelectEntity(null);
        }

        public void ProcessInput(InputManager input)
        {
            bool inWindow = Collision.InBounds(GraphicsManager.WindowWidth, GraphicsManager.WindowHeight, input.MouseLoc);

            Loc mapCoords = DungeonEditScene.Instance.ScreenCoordsToMapCoords(input.MouseLoc);

            bool inBounds = Collision.InBounds(ZoneManager.Instance.CurrentMap.Width, ZoneManager.Instance.CurrentMap.Height, mapCoords);


            switch (EntMode)
            {
                case EntEditMode.PlaceEntity:
                    {
                        DungeonEditScene.Instance.CharacterInProgress.CharLoc = mapCoords;
                        if (!inBounds)
                            return;
                        if (inWindow && input.JustPressed(FrameInput.InputType.LeftMouse))
                            PlaceEntity(mapCoords);
                        else if (input.JustPressed(FrameInput.InputType.RightMouse))
                            RemoveEntityAt(mapCoords);
                        break;
                    }
                case EntEditMode.SelectEntity:
                    {
                        if (!inBounds)
                            return;
                        if (inWindow && input.JustPressed(FrameInput.InputType.LeftMouse))
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
                MonsterData monster = DataManager.Instance.GetMonster(monsterKeys[ChosenMonster]);
                for (int ii = 0; ii < monster.Forms.Count; ii++)
                    Forms.Add(ii.ToString("D2") + ": " + monster.Forms[ii].FormName.ToLocal());

                ChosenForm = Math.Clamp(tempForm, 0, Forms.Count - 1);
            }
        }


        public void Statuses_Changed()
        {
            Dictionary<string, StatusEffect> statuses = new Dictionary<string, StatusEffect>();
            List<StatusEffect> states = Statuses.GetList<List<StatusEffect>>();
            for (int ii = 0; ii < states.Count; ii++)
                statuses[states[ii].ID] = states[ii];
            SelectedEntity.StatusEffects = statuses;
        }

        public void Statuses_EditItem(int index, object element, CollectionBoxViewModel.EditElementOp op)
        {
            string elementName = "Statuses[" + index + "]";
            DataEditForm frmData = new DataEditRootForm();
            frmData.Title = DataEditor.GetWindowTitle(SelectedEntity.Name, elementName, element, typeof(StatusEffect), new object[0]);

            DataEditor.LoadClassControls(frmData.ControlPanel, ZoneManager.Instance.CurrentMap.AssetName, null, elementName, typeof(StatusEffect), new object[0], element, true, new Type[0]);
            DataEditor.TrackTypeSize(frmData, typeof(StatusEffect));

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            frmData.SelectedOKEvent += async () =>
            {
                element = DataEditor.SaveClassControls(frmData.ControlPanel, elementName, typeof(StatusEffect), new object[0], true, new Type[0]);

                bool itemExists = false;

                List<StatusEffect> states = (List<StatusEffect>)Statuses.GetList(typeof(List<StatusEffect>));
                for (int ii = 0; ii < states.Count; ii++)
                {
                    if (ii != index)
                    {
                        if (states[ii].ID == ((StatusEffect)element).ID)
                            itemExists = true;
                    }
                }

                if (itemExists)
                {
                    await MessageBox.Show(frmData, "Cannot add duplicate IDs.", "Entry already exists.", MessageBox.MessageBoxButtons.Ok);
                    return false;
                }
                else
                {
                    op(index, element);
                    return true;
                }
            };

            form.MapEditForm.RegisterChild(frmData);
            frmData.Show();
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

            DiagManager.Instance.DevEditor.MapEditor.Edits.Apply(new MapEntityStateUndo());

            for (int ii = 0; ii < ZoneManager.Instance.CurrentMap.AllyTeams.Count; ii++)
            {
                Team team = ZoneManager.Instance.CurrentMap.AllyTeams[ii];
                if (team.Players.Contains(ent))
                    team.Players.Remove(ent);
                if (team.Guests.Contains(ent))
                    team.Guests.Remove(ent);

                if (team.MemberGuestCount == 0)
                    ZoneManager.Instance.CurrentMap.AllyTeams.RemoveAt(ii);
            }

            for (int ii = 0; ii < ZoneManager.Instance.CurrentMap.MapTeams.Count; ii++)
            {
                Team team = ZoneManager.Instance.CurrentMap.MapTeams[ii];
                if (team.Players.Contains(ent))
                    team.Players.Remove(ent);
                if (team.Guests.Contains(ent))
                    team.Guests.Remove(ent);

                if (team.MemberGuestCount == 0)
                    ZoneManager.Instance.CurrentMap.MapTeams.RemoveAt(ii);
            }
        }

        public void PreviewEntity(Loc position)
        {
            MonsterTeam team = new MonsterTeam();
            Character placeableEntity = SelectedEntity.Clone(team);
            placeableEntity.CharLoc = position;
            DungeonEditScene.Instance.CharacterInProgress = placeableEntity;
        }

        public void PlaceEntity(Loc position)
        {
            RemoveEntityAt(position);

            MonsterTeam team = new MonsterTeam();
            Character placeableEntity = SelectedEntity.Clone(team);

            placeableEntity.CharLoc = position;

            DiagManager.Instance.DevEditor.MapEditor.Edits.Apply(new MapEntityStateUndo());

            if (Ally)
            {
                ZoneManager.Instance.CurrentMap.AllyTeams.Add(team);
            }
            else
            {
                ZoneManager.Instance.CurrentMap.MapTeams.Add(team);
            }
            placeableEntity.UpdateFrame();
        }



        public void SelectEntity(Character ent)
        {
            if (ent != null)
            {
                DiagManager.Instance.DevEditor.MapEditor.Edits.Apply(new MapEntityStateUndo());
                setEntity(ent);
            }
            else
            {
                MonsterTeam team = new MonsterTeam();
                SelectedEntity = new Character(new CharData());
                SelectedEntity.Level = 1;
                SelectedEntity.Tactic = new AITactic(DataManager.Instance.GetAITactic(DataManager.Instance.DefaultAI));
                team.Players.Add(SelectedEntity);
                setEntity(SelectedEntity);
            }
        }

        private void setEntity(Character ent)
        {
            SelectedEntity = ent;

            Nickname = Nickname;
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

            Unrecruitable = Unrecruitable;

            List<StatusEffect> states = new List<StatusEffect>();
            foreach (StatusEffect state in SelectedEntity.StatusEffects.Values)
                states.Add(state);
            Statuses.LoadFromList(states);
        }

        private void animChanged()
        {
            if (entMode == EntEditMode.PlaceEntity)
                PreviewEntity(Loc.Zero);
        }

        private void updateStats()
        {
            HPTotal = HPTotal;
            AtkTotal = AtkTotal;
            DefTotal = DefTotal;
            MAtkTotal = MAtkTotal;
            MDefTotal = MDefTotal;
            SpeedTotal = SpeedTotal;
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
            {
                SelectedEntity.CharLoc = loc;
                SelectedEntity.UpdateFrame();
            }
        }
    }

    public class MapEntityState
    {
        public EventedList<Team> AllyTeam
        {
            get;
            set;
        }
        public EventedList<Team> EnemyTeam
        {
            get;
            set;
        }

        public MapEntityState(EventedList<Team> allyTeam, EventedList<Team> enemyTeam)
        {
            AllyTeam = allyTeam;
            EnemyTeam = enemyTeam;
        }
    }

    public class MapEntityStateUndo : StateUndo<MapEntityState>
    {
        public MapEntityStateUndo()
        {
        }

        public override MapEntityState GetState()
        {
            return new MapEntityState(ZoneManager.Instance.CurrentMap.AllyTeams, ZoneManager.Instance.CurrentMap.MapTeams);
        }

        public override void SetState(MapEntityState state)
        {
            ZoneManager.Instance.CurrentMap.AllyTeams = state.AllyTeam;
            ZoneManager.Instance.CurrentMap.MapTeams = state.EnemyTeam;
        }
    }
}
