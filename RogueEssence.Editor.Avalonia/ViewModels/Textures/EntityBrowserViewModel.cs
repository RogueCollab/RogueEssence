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
using RogueEssence.Dev.Models;
using RogueEssence.Ground;
using RogueEssence.Data;
using System.IO;
using RogueEssence.Script;

namespace RogueEssence.Dev.ViewModels
{
    public class EntityBrowserViewModel : ViewModelBase
    {
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
            }
        }

        public int BoundsX
        {
            get => SelectedEntity.Bounds.Size.X;
            set
            {
                SelectedEntity.Bounds = new Rect(SelectedEntity.Bounds.Start, new Loc(value, SelectedEntity.Bounds.Size.Y));
                this.RaisePropertyChanged();
            }
        }

        public int BoundsY
        {
            get => SelectedEntity.Bounds.Size.Y;
            set
            {
                SelectedEntity.Bounds = new Rect(SelectedEntity.Bounds.Start, new Loc(SelectedEntity.Bounds.Size.X, value));
                this.RaisePropertyChanged();
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
                GroundEntity.EEntityTriggerTypes chosenTriggerType = SelectedEntity.GetTriggerType();
                this.RaiseAndSet(ref chosenTriggerType, value);
                if (!settingEnt)
                    SelectedEntity.SetTriggerType(chosenTriggerType);
                triggerTypeChanged();
            }
        }

        public ObservableCollection<EntScriptItem> ScriptItems { get; }

        public ObservableCollection<SpawnScriptItem> SpawnScriptItems { get; }








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
            }
        }






        public ObservableCollection<string> Monsters { get; }

        public int ChosenMonster
        {
            get
            {
                int feature = 0;
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    feature = charEnt.Data.BaseForm.Species;
                }
                return feature;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    charEnt.Data.BaseForm.Species = value;
                }
                this.RaisePropertyChanged();
                speciesChanged();
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
            }
        }

        public ObservableCollection<string> Skins { get; }

        public int ChosenSkin
        {
            get
            {
                int feature = 0;
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    feature = charEnt.Data.BaseForm.Skin;
                }
                return feature;
            }
            set
            {
                if (SelectedEntity.GetEntityType() == GroundEntity.EEntTypes.Character)
                {
                    GroundChar charEnt = SelectedEntity as GroundChar;
                    charEnt.Data.BaseForm.Skin = value;
                }
                this.RaisePropertyChanged();
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


            ObjectAnims = new ObservableCollection<string>();
            ObjectAnims.Add("---");
            string[] dirs = Directory.GetFiles(GraphicsManager.CONTENT_PATH + "Object/");
            for (int ii = 0; ii < dirs.Length; ii++)
            {
                string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
                ObjectAnims.Add(filename);
            }

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

            entTypeChanged();
            speciesChanged();

            //foreach (string s in TemplateManager.TemplateTypeNames)
            //    cmbTemplateType.Items.Add(s);

            BoundsX = 8;
            BoundsY = 8;

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
            SelectedEntity.DevOnEntityUnSelected();

            if (ent != null)
            {
                setEntity(ent);
                SelectedEntity.DevOnEntitySelected();
            }
            else
                setEntity(new GroundObject(new ObjAnimData(), new Rect(0, 0, 8, 8),
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
            ChosenObjectAnim = ChosenObjectAnim;
            OffsetX = OffsetX;
            OffsetY = OffsetY;
            StartFrame = StartFrame;
            EndFrame = EndFrame;
            FrameLength = FrameLength;

            ChosenMonster = ChosenMonster;
            ChosenForm = ChosenForm;
            ChosenSkin = ChosenSkin;
            ChosenGender = ChosenGender;

            Nickname = Nickname;

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
                        break;
                    }
                case GroundEntity.EEntTypes.Marker:
                    {

                        break;
                    }
                case GroundEntity.EEntTypes.Spawner:
                    {
                        retainTab |= TabIndex == 4;
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
                            chdata.BaseForm = new MonsterID();
                            chdata.Level = 1;
                            Character ch = new Character(chdata, null);
                            AITactic tactic = DataManager.Instance.GetAITactic(0);
                            ch.Tactic = new AITactic(tactic);

                            GroundChar gch = new GroundChar(ch, Loc.Zero, Dir8.Down, entName);
                            placeableEntity = gch;
                            break;
                        }
                    case GroundEntity.EEntTypes.Object:
                        {
                            placeableEntity = new GroundObject(new ObjAnimData(), new Rect(0, 0, 8, 8),
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
                MonsterData monster = DataManager.Instance.GetMonster(ChosenMonster);
                for (int ii = 0; ii < monster.Forms.Count; ii++)
                    Forms.Add(ii.ToString("D2") + ": " + monster.Forms[ii].FormName.ToLocal());

                ChosenForm = Math.Clamp(tempForm, 0, Forms.Count - 1);
            }

        }
    }
}
