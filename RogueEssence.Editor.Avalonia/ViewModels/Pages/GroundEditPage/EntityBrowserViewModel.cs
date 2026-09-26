using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using RogueElements;
using RogueEssence.Dungeon;
using ReactiveUI;
using RogueEssence.Content;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using RogueEssence.Ground;
using RogueEssence.Data;
using System.IO;
using RogueEssence.Script;

namespace RogueEssence.Dev.ViewModels
{
    public class EntityBrowserViewModel : ViewModelBase
    {
        public event Action PreviewChanged;

        public GroundEntity SelectedEntity { get; private set; }


        private bool allowEntTypes;
        public bool AllowEntTypes
        {
            get => allowEntTypes;
            set => this.SetIfChanged(ref allowEntTypes, value);
        }

        public ObservableCollection<string> EntityTypes { get; }

        private int chosenEntityType;
        public int ChosenEntityType
        {
            get => chosenEntityType;
            set
            {
                this.SetIfChanged(ref chosenEntityType, value);
                entTypeChanged();
            }
        }

        public string EntName
        {
            get => SelectedEntity.EntName;
            set
            {
                SelectedEntity.EntName = value;
                SelectedEntity.ReloadEvents();
                this.RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Directions { get; }

        public int ChosenDirection
        {
            get => (int)SelectedEntity.Direction;
            set
            {
                SelectedEntity.Direction = (Dir8)value;
                this.RaisePropertyChanged();
                PreviewChanged?.Invoke();
            }
        }

        public int BoundsX
        {
            get => SelectedEntity.Bounds.Size.X;
            set
            {
                SelectedEntity.Bounds = new Rect(SelectedEntity.Bounds.Start, new Loc(value, SelectedEntity.Bounds.Size.Y));
                this.RaisePropertyChanged();
                PreviewChanged?.Invoke();
            }
        }

        public int BoundsY
        {
            get => SelectedEntity.Bounds.Size.Y;
            set
            {
                SelectedEntity.Bounds = new Rect(SelectedEntity.Bounds.Start, new Loc(SelectedEntity.Bounds.Size.X, value));
                this.RaisePropertyChanged();
                PreviewChanged?.Invoke();
            }
        }

        public bool EntEnabled
        {
            get => SelectedEntity.EntEnabled;
            set
            {
                SelectedEntity.EntEnabled = value;
                this.RaisePropertyChanged();
            }
        }



        private int tabIndex;
        public int TabIndex
        {
            get => tabIndex;
            set
            {
                this.SetIfChanged(ref tabIndex, value);
            }
        }





        public ObservableCollection<GroundEntity.EEntityTriggerTypes> TriggerTypes { get; }

        public GroundEntity.EEntityTriggerTypes ChosenTriggerType
        {
            get => SelectedEntity.GetTriggerType();
            set
            {
                if (!settingEnt)
                    SelectedEntity.SetTriggerType(value);
                this.RaisePropertyChanged();
                triggerTypeChanged();
            }
        }

        public ObservableCollection<EntScriptItem> ScriptItems { get; }

        public ObservableCollection<SpawnScriptItem> SpawnScriptItems { get; }








        private Type[] assignables;
        public ObservableCollection<string> AnimTypes { get; }

        public int ChosenAnimType
        {
            get
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    return Array.IndexOf(assignables, groundEnt.ObjectAnim.GetType());
                }
                return 0;
            }
            set
            {
                if (value < 0)
                    return;
                Type type = assignables[value];
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    IPlaceableAnimData newData = (IPlaceableAnimData)ReflectionExt.CreateMinimalInstance(type);
                    newData.LoadFrom(groundEnt.ObjectAnim);
                    groundEnt.ObjectAnim = newData;
                }
                this.RaisePropertyChanged();
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                    animTypeChanged();
                PreviewChanged?.Invoke();
            }
        }

        public ObservableCollection<string> ObjectAnims { get; }

        public int ChosenObjectAnim
        {
            get
            {
                int chosenObjectAnim = 0;
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    chosenObjectAnim = ObjectAnims.IndexOf(groundEnt.ObjectAnim.AnimIndex);
                    if (chosenObjectAnim == -1)
                        chosenObjectAnim = 0;
                }
                return chosenObjectAnim;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    groundEnt.ObjectAnim.AnimIndex = value > 0 ? ObjectAnims[value] : "";
                }
                this.RaisePropertyChanged();
                PreviewChanged?.Invoke();
            }
        }


        public int OffsetX
        {
            get
            {
                int frame = -1;
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    frame = groundEnt.DrawOffset.X;
                }
                return frame;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    groundEnt.DrawOffset.X = value;
                }
                this.RaisePropertyChanged();
                PreviewChanged?.Invoke();
            }
        }

        public int OffsetY
        {
            get
            {
                int frame = -1;
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    frame = groundEnt.DrawOffset.Y;
                }
                return frame;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    groundEnt.DrawOffset.Y = value;
                }
                this.RaisePropertyChanged();
                PreviewChanged?.Invoke();
            }
        }


        public int StartFrame
        {
            get
            {
                int frame = -1;
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    frame = groundEnt.ObjectAnim.StartFrame;
                }
                return frame;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    groundEnt.ObjectAnim.StartFrame = value;
                }
                this.RaisePropertyChanged();
                PreviewChanged?.Invoke();
            }
        }

        public int EndFrame
        {
            get
            {
                int frame = -1;
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    frame = groundEnt.ObjectAnim.EndFrame;
                }
                return frame;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    groundEnt.ObjectAnim.EndFrame = value;
                }
                this.RaisePropertyChanged();
                PreviewChanged?.Invoke();
            }
        }

        public int FrameLength
        {
            get
            {
                int frame = 0;
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    frame = groundEnt.ObjectAnim.FrameTime;
                }
                return frame;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    groundEnt.ObjectAnim.FrameTime = value;
                }
                this.RaisePropertyChanged();
                PreviewChanged?.Invoke();
            }
        }

        public bool Passable
        {
            get
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    return groundEnt.Passable;
                }
                return false;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Object)
                {
                    GroundObject groundEnt = SelectedEntity as GroundObject;
                    groundEnt.Passable = value;
                }
                this.RaisePropertyChanged();
            }
        }






        private List<string> monsterKeys;
        public ObservableCollection<string> Monsters { get; }

        public int ChosenMonster
        {
            get
            {
                int feature = 0;
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    feature = monsterKeys.IndexOf(charEnt.Data.BaseForm.Species);
                }
                return feature;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    charEnt.Data.BaseForm.Species = monsterKeys[value];
                }
                this.RaisePropertyChanged();
                speciesChanged();
                PreviewChanged?.Invoke();
            }
        }


        public ObservableCollection<string> Forms { get; }

        public int ChosenForm
        {
            get
            {
                int feature = 0;
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    feature = charEnt.Data.BaseForm.Form;
                }
                return feature;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    charEnt.Data.BaseForm.Form = value;
                }
                this.RaisePropertyChanged();
                PreviewChanged?.Invoke();
            }
        }

        private List<string> skinKeys;

        public ObservableCollection<string> Skins { get; }

        public int ChosenSkin
        {
            get
            {
                int feature = 0;
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    feature = skinKeys.IndexOf(charEnt.Data.BaseForm.Skin);
                }
                return feature;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    charEnt.Data.BaseForm.Skin = skinKeys[value];
                }
                this.RaisePropertyChanged();
                PreviewChanged?.Invoke();
            }
        }

        public ObservableCollection<string> Genders { get; }

        public int ChosenGender
        {
            get
            {
                int feature = 0;
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    feature = (int)charEnt.Data.BaseForm.Gender;
                }
                return feature;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    charEnt.Data.BaseForm.Gender = (Gender)value;
                }
                this.RaisePropertyChanged();
                PreviewChanged?.Invoke();
            }
        }

        public string Nickname
        {
            get
            {
                string feature = "";
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    feature = charEnt.Data.Nickname;
                }
                return feature;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    charEnt.Data.Nickname = value;
                }
                this.RaisePropertyChanged();
            }
        }
        public bool AIEnabled
        {
            get
            {
                bool feature = false;
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    feature = charEnt.AIEnabled;
                }
                return feature;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    charEnt.AIEnabled = value;
                }
                this.RaisePropertyChanged();
            }
        }




        public string SpawnName
        {
            get
            {
                string feature = "";
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Spawner)
                {
                    GroundSpawner spawnEnt = SelectedEntity as GroundSpawner;
                    feature = spawnEnt.NPCName;
                }
                return feature;
            }
            set
            {
                string feature = "";
                this.RaiseAndSet(ref feature, value);
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Spawner)
                {
                    GroundSpawner spawnEnt = SelectedEntity as GroundSpawner;
                    spawnEnt.NPCName = feature;
                }
            }
        }


        public EntityBrowserViewModel()
        {

            EntityTypes = new ObservableCollection<string>();
            for (int ii = 0; ii < (int)GroundEntity.EEntTypes.Count; ii++)
                EntityTypes.Add(((GroundEntity.EEntTypes)ii).ToLocal());

            Directions = new ObservableCollection<string>();
            foreach (Dir8 dir in DirExt.VALID_DIR8)
                Directions.Add(dir.ToLocal());

            TriggerTypes = new ObservableCollection<GroundEntity.EEntityTriggerTypes>();

            ScriptItems = new ObservableCollection<EntScriptItem>();

            SpawnScriptItems = new ObservableCollection<SpawnScriptItem>();


            AnimTypes = new ObservableCollection<string>();
            assignables = typeof(IPlaceableAnimData).GetAssignableTypes();
            foreach (Type type in assignables)
            {
                IPlaceableAnimData newData = (IPlaceableAnimData)ReflectionExt.CreateMinimalInstance(type);
                AnimTypes.Add(newData.AssetType.ToString());
            }
            ObjectAnims = new ObservableCollection<string>();

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


            entTypeChanged();
            speciesChanged();

            //foreach (string s in TemplateManager.TemplateTypeNames)
            //    cmbTemplateType.Items.Add(s);

            BoundsX = GroundAction.HITBOX_WIDTH;
            BoundsY = GroundAction.HITBOX_HEIGHT;

            FrameLength = 1;

            //selectedEntity = null;
        }

        public GroundEntity CreateEntity()
        {
            //clone the selected entity
            return SelectedEntity.Clone();
        }

        public void SelectEntity(GroundEntity ent)
        {
            if (ent != null)
                setEntity(ent);
            else
                setEntity(new GroundObject(new ObjAnimData(), new Rect(0, 0, GroundAction.HITBOX_WIDTH, GroundAction.HITBOX_HEIGHT),
                                GroundEntity.EEntityTriggerTypes.None, "NewObject"));
        }

        private void setEntity(GroundEntity ent)
        {
            settingEnt = true;
            SelectedEntity = ent;

            //set the entity dropdown
            ChosenEntityType = (int)SelectedEntity.GetEntityType();

            GroundEntity.EEntityTriggerTypes triggerType = SelectedEntity.GetTriggerType();
            TriggerTypes.Clear();
            TriggerTypes.Add(GroundEntity.EEntityTriggerTypes.None);
            switch ((GroundEntity.EEntTypes)chosenEntityType)
            {
                case GroundEntity.EEntTypes.Character:
                    {
                        TriggerTypes.Add(GroundEntity.EEntityTriggerTypes.Action);
                        break;
                    }
                case GroundEntity.EEntTypes.Object:
                    {
                        TriggerTypes.Add(GroundEntity.EEntityTriggerTypes.Action);
                        TriggerTypes.Add(GroundEntity.EEntityTriggerTypes.Touch);
                        TriggerTypes.Add(GroundEntity.EEntityTriggerTypes.TouchOnce);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            //update the collections; they must load the appropriate lists
            //will a new array need to be created from the script settings?
            ChosenTriggerType = triggerType;

            //update ALL fields to the new data by self-assignment
            EntName = EntName;
            ChosenDirection = ChosenDirection;
            //bounds will be double assigned but don't worry about that
            BoundsX = BoundsX;
            BoundsY = BoundsY;

            EntEnabled = EntEnabled;
            ChosenAnimType = ChosenAnimType;
            ChosenObjectAnim = ChosenObjectAnim;
            OffsetX = OffsetX;
            OffsetY = OffsetY;
            StartFrame = StartFrame;
            EndFrame = EndFrame;
            FrameLength = FrameLength;
            Passable = Passable;

            ChosenMonster = ChosenMonster;
            ChosenForm = ChosenForm;
            ChosenSkin = ChosenSkin;
            ChosenGender = ChosenGender;

            Nickname = Nickname;
            AIEnabled = AIEnabled;

            SpawnName = SpawnName;

            settingEnt = false;
        }

        private bool settingEnt;
        private void entTypeChanged()
        {
            bool retainTab = false;
            switch ((GroundEntity.EEntTypes)chosenEntityType)
            {
                case GroundEntity.EEntTypes.Character:
                    {
                        retainTab |= TabIndex == 1;
                        retainTab |= TabIndex == 2;
                        break;
                    }
                case GroundEntity.EEntTypes.Object:
                    {
                        retainTab |= TabIndex == 3;
                        retainTab |= TabIndex == 4;
                        break;
                    }
                case GroundEntity.EEntTypes.Marker:
                    {

                        break;
                    }
                case GroundEntity.EEntTypes.Spawner:
                    {
                        retainTab |= TabIndex == 5;
                        break;
                    }
            }
            if (!retainTab)
                TabIndex = 0;


            if (!settingEnt)
            {
                //create a new entity
                GroundEntity placeableEntity = null;

                string entName = String.Format("New{0}", ((GroundEntity.EEntTypes)chosenEntityType).ToString());
                if (SelectedEntity != null)
                    entName = SelectedEntity.EntName;

                switch ((GroundEntity.EEntTypes)chosenEntityType)
                {
                    case GroundEntity.EEntTypes.Character:
                        {

                            CharData chdata = new CharData();
                            chdata.Nickname = "";
                            chdata.BaseForm = new MonsterID(DataManager.Instance.DefaultMonster, 0, DataManager.Instance.DefaultSkin, Gender.Genderless);
                            chdata.Level = 1;
                            Character ch = new Character(chdata);
                            AITactic tactic = DataManager.Instance.GetAITactic(DataManager.Instance.DefaultAI);
                            ch.Tactic = new AITactic(tactic);

                            GroundChar gch = new GroundChar(ch, Loc.Zero, Dir8.Down, entName);
                            placeableEntity = gch;
                            break;
                        }
                    case GroundEntity.EEntTypes.Object:
                        {
                            placeableEntity = new GroundObject(new ObjAnimData(), new Rect(0, 0, GroundAction.HITBOX_WIDTH, GroundAction.HITBOX_HEIGHT),
                                GroundEntity.EEntityTriggerTypes.None, entName);
                            break;
                        }
                    case GroundEntity.EEntTypes.Marker:
                        {
                            placeableEntity = new GroundMarker(entName, Loc.Zero, Dir8.Down);
                            break;
                        }
                    case GroundEntity.EEntTypes.Spawner:
                        {
                            placeableEntity = new GroundSpawner(entName, "", new CharData());
                            break;
                        }
                }
                placeableEntity.EntEnabled = true;

                setEntity(placeableEntity);

                ChosenTriggerType = 0;
            }

        }

        private void triggerTypeChanged()
        {
            //First add the ones based on the Activation type
            ScriptItems.Clear();
            SpawnScriptItems.Clear();

            switch (ChosenTriggerType)
            {
                case GroundEntity.EEntityTriggerTypes.Action:
                    {
                        ScriptItems.Add(new EntScriptItem(LuaEngine.EEntLuaEventTypes.Action, SelectedEntity));
                        break;
                    }
                case GroundEntity.EEntityTriggerTypes.Touch:
                case GroundEntity.EEntityTriggerTypes.TouchOnce:
                    {
                        ScriptItems.Add(new EntScriptItem(LuaEngine.EEntLuaEventTypes.Touch, SelectedEntity));
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            //Then add any extra ones that comes with the entity type
            switch ((GroundEntity.EEntTypes)chosenEntityType)
            {
                case GroundEntity.EEntTypes.Character:
                    {
                        ScriptItems.Add(new EntScriptItem(LuaEngine.EEntLuaEventTypes.Think, SelectedEntity));
                        break;
                    }
                case GroundEntity.EEntTypes.Object:
                    {
                        ScriptItems.Add(new EntScriptItem(LuaEngine.EEntLuaEventTypes.Update, SelectedEntity));
                        break;
                    }
                case GroundEntity.EEntTypes.Spawner:
                    {
                        ScriptItems.Add(new EntScriptItem(LuaEngine.EEntLuaEventTypes.EntSpawned, SelectedEntity));
                        GroundSpawner spwn = SelectedEntity as GroundSpawner;
                        SpawnScriptItems.Add(new SpawnScriptItem(LuaEngine.EEntLuaEventTypes.Action, spwn));
                        SpawnScriptItems.Add(new SpawnScriptItem(LuaEngine.EEntLuaEventTypes.Think, spwn));
                        break;
                    }
                default:
                    {
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



        private void animTypeChanged()
        {
            GroundObject groundEnt = SelectedEntity as GroundObject;
            string oldIndex = groundEnt.ObjectAnim.AnimIndex;
            ObjectAnims.Clear();
            ObjectAnims.Add("**EMPTY**");
            string[] dirs = PathMod.GetModFiles(GraphicsManager.CONTENT_PATH + groundEnt.ObjectAnim.AssetType.ToString() + "/");
            int newAnim = 0;
            for (int ii = 0; ii < dirs.Length; ii++)
            {
                string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                ObjectAnims.Add(filename);

                if (filename == oldIndex)
                    newAnim = ii + 1;
            }
            ChosenObjectAnim = newAnim;
        }
    }
}
