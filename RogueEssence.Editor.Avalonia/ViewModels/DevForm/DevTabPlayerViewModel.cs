using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Collections.ObjectModel;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Content;
using RogueEssence.Ground;

namespace RogueEssence.Dev.ViewModels
{
    public class DevTabPlayerViewModel : ViewModelBase
    {
        public DevTabPlayerViewModel()
        {
            Monsters = new ObservableCollection<string>();
            Forms = new ObservableCollection<string>();
            Skins = new ObservableCollection<string>();
            Genders = new ObservableCollection<string>();
            Anims = new ObservableCollection<string>();
        }

        private int level;
        public int Level
        {
            get => level;
            set
            {
                if (!this.SetIfChanged(ref level, value))
                    return;
                lock (GameBase.lockObj)
                {
                    if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                    {
                        if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
                            DungeonScene.Instance.FocusedCharacter.Level = level;
                    }
                    else if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                    {
                        if (DataManager.Instance.Save.ActiveTeam.Players.Count > 0)
                            DataManager.Instance.Save.ActiveTeam.Leader.Level = level;
                    }
                }
            }
        }
        public void UpdateLevel()
        {
            lock (GameBase.lockObj)
            {
                int upd = 0;
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
                        upd = DungeonScene.Instance.FocusedCharacter.Level;
                }
                else if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                {
                    if (DataManager.Instance.Save.ActiveTeam.Players.Count > 0)
                        upd = DataManager.Instance.Save.ActiveTeam.Leader.Level;
                }

                Level = upd;
            }
        }

        public ObservableCollection<string> Monsters { get; }

        private int chosenMonster;
        public int ChosenMonster
        {
            get { return chosenMonster; }
            set
            {
                this.SetIfChanged(ref chosenMonster, value);

                if (chosenMonster > -1)
                    SpeciesChanged();
            }
        }


        public ObservableCollection<string> Forms { get; }

        private int chosenForm;
        public int ChosenForm
        {
            get { return chosenForm; }
            set { this.SetIfChanged(ref chosenForm, value);
                if (chosenForm > -1)
                    UpdateSprite();
            }
        }

        public ObservableCollection<string> Skins { get; }

        private int chosenSkin;
        public int ChosenSkin
        {
            get { return chosenSkin; }
            set { this.SetIfChanged(ref chosenSkin, value);
                if (chosenSkin > -1)
                    UpdateSprite();
            }
        }

        public ObservableCollection<string> Genders { get; }

        private int chosenGender;
        public int ChosenGender
        {
            get { return chosenGender; }
            set { this.SetIfChanged(ref chosenGender, value);
                if (chosenGender > -1)
                    UpdateSprite();
            }
        }

        public ObservableCollection<string> Anims { get; }

        private int chosenAnim;
        public int ChosenAnim
        {
            get { return chosenAnim; }
            set
            {
                if (!this.SetIfChanged(ref chosenAnim, value))
                    return;
                lock (GameBase.lockObj)
                    GraphicsManager.GlobalIdle = chosenAnim;
            }
        }


        public void btnEditSprites_Click()
        {
            SpeciesEditViewModel mv = new SpeciesEditViewModel();
            Views.SpeciesEditForm editForm = new Views.SpeciesEditForm();
            mv.LoadFormDataEntries(true, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }

        public void btnEditPortraits_Click()
        {
            SpeciesEditViewModel mv = new SpeciesEditViewModel();
            Views.SpeciesEditForm editForm = new Views.SpeciesEditForm();
            mv.LoadFormDataEntries(false, editForm);
            editForm.DataContext = mv;
            editForm.Show();
        }


        public void btnRollSkill_Click()
        {
            lock (GameBase.lockObj)
            {
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
                    {
                        Character character = DungeonScene.Instance.FocusedCharacter;
                        BaseMonsterForm form = DataManager.Instance.GetMonster(character.BaseForm.Species).Forms[character.BaseForm.Form];

                        while (character.BaseSkills[0].SkillNum > -1)
                            character.DeleteSkill(0);
                        List<int> final_skills = form.RollLatestSkills(character.Level, new List<int>());
                        foreach (int skill in final_skills)
                            character.LearnSkill(skill, true);

                        DungeonScene.Instance.LogMsg(String.Format("Skills reloaded"), false, true);
                    }
                }
                else if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                {
                    if (DataManager.Instance.Save.ActiveTeam.Players.Count > 0)
                    {
                        Character character = DataManager.Instance.Save.ActiveTeam.Leader;
                        BaseMonsterForm form = DataManager.Instance.GetMonster(character.BaseForm.Species).Forms[character.BaseForm.Form];

                        while (character.BaseSkills[0].SkillNum > -1)
                            character.DeleteSkill(0);
                        List<int> final_skills = form.RollLatestSkills(character.Level, new List<int>());
                        foreach (int skill in final_skills)
                            character.LearnSkill(skill, true);
                        GameManager.Instance.SE("Menu/Sort");
                    }
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
        }

        bool updating;
        private void SpeciesChanged()
        {
            bool prevUpdate = updating;
            updating = true;

            lock (GameBase.lockObj)
            {
                int tempForm = chosenForm;
                Forms.Clear();
                MonsterData monster = DataManager.Instance.GetMonster(chosenMonster);
                for (int ii = 0; ii < monster.Forms.Count; ii++)
                    Forms.Add(ii.ToString("D2") + ": " + monster.Forms[ii].FormName.ToLocal());

                ChosenForm = Math.Clamp(tempForm, 0, Forms.Count - 1);
            }

            updating = prevUpdate;
            UpdateSprite();
        }

        public void UpdateSpecies(MonsterID id)
        {
            bool prevUpdate = updating;
            updating = true;

            ChosenMonster = id.Species;
            ChosenForm = id.Form;
            ChosenSkin = id.Skin;
            ChosenGender = (int)id.Gender;

            updating = prevUpdate;
        }

        private void UpdateSprite()
        {
            if (updating)
                return;

            lock (GameBase.lockObj)
            {
                if (GameManager.Instance.IsInGame())
                    DungeonScene.Instance.FocusedCharacter.Promote(new MonsterID(chosenMonster, chosenForm, chosenSkin, (Gender)chosenGender));


                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
                        DungeonScene.Instance.FocusedCharacter.Promote(new MonsterID(chosenMonster, chosenForm, chosenSkin, (Gender)chosenGender));
                }
                else if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                {
                    if (DataManager.Instance.Save.ActiveTeam.Players.Count > 0 && GroundScene.Instance.FocusedCharacter != null)
                    {
                        Character character = DataManager.Instance.Save.ActiveTeam.Leader;
                        character.Promote(new MonsterID(chosenMonster, chosenForm, chosenSkin, (Gender)chosenGender));
                        GroundChar leaderChar = GroundScene.Instance.FocusedCharacter;
                        ZoneManager.Instance.CurrentGround.SetPlayerChar(new GroundChar(DataManager.Instance.Save.ActiveTeam.Leader, leaderChar.MapLoc, leaderChar.CharDir, "PLAYER"));
                    }
                }
            }
        }

    }
}
