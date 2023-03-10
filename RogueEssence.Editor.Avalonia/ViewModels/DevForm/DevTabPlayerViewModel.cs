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
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels
{
    public class DevTabPlayerViewModel : ViewModelBase
    {
        public DevTabPlayerViewModel()
        {
            Monsters = new ObservableCollection<string>();
            MonsterKeys = new List<string>();
            Forms = new ObservableCollection<string>();
            Skins = new ObservableCollection<string>();
            SkinKeys = new List<string>();
            Genders = new ObservableCollection<string>();
            Anims = new ObservableCollection<string>();
        }

        public void LoadMonstersNumeric()
        {
            monstersNumeric = DevForm.GetConfig("MonsterNumeric", 0) != 0;
        }

        private void loadMonsterKeys()
        {
            string oldId = "";
            if (chosenMonster > -1 && chosenMonster < MonsterKeys.Count)
                oldId = MonsterKeys[chosenMonster];

            Monsters.Clear();
            MonsterKeys.Clear();
            List<string> keys = DataManager.Instance.DataIndices[DataManager.DataType.Monster].GetOrderedKeys(monstersNumeric);
            foreach (string key in keys)
            {
                string localString = DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(key).GetLocalString(true);
                if (monstersNumeric)
                    Monsters.Add(String.Format("{0}: {1}", DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(key).SortOrder.ToString("D4"), localString));
                else
                    Monsters.Add(String.Format("{0}: {1}", key, localString));
                MonsterKeys.Add(key);
            }
            int idx = MonsterKeys.IndexOf(oldId);
            if (idx > -1)
                ChosenMonster = idx;
            else
                ChosenMonster = 0;
        }

        public void ReloadMonsters()
        {
            bool prevUpdate = updating;
            updating = true;
            loadMonsterKeys();

            ChosenForm = -1;
            ChosenForm = 0;

            Dictionary<string, string> skin_names = DataManager.Instance.DataIndices[DataManager.DataType.Skin].GetLocalStringArray(true);
            Skins.Clear();
            SkinKeys.Clear();
            foreach (string key in skin_names.Keys)
            {
                Skins.Add(key + ": " + skin_names[key]);
                SkinKeys.Add(key);
            }
            ChosenSkin = -1;
            ChosenSkin = 0;

            Genders.Clear();
            for (int ii = 0; ii < 3; ii++)
                Genders.Add(((Gender)ii).ToString());
            ChosenGender = -1;
            ChosenGender = 0;
            updating = prevUpdate;
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

        private bool monstersNumeric;
        public List<string> MonsterKeys;

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


        public List<string> SkinKeys;
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

                RestartAnim(false);
            }
        }

        private bool justMe;
        public bool JustMe
        {
            get => justMe;
            set
            {
                this.RaiseAndSet(ref justMe, value);
                RestartAnim(false);
            }
        }

        private bool justOnce;
        public bool JustOnce
        {
            get => justOnce;
            set
            {
                this.RaiseAndSet(ref justOnce, value);
                RestartAnim(false);
            }
        }

        public void RestartAnim(bool refreshOthers)
        {
            lock (GameBase.lockObj)
            {
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    Character chara = DungeonScene.Instance.FocusedCharacter;

                    if (justOnce) //make it an action anim
                    {
                        if (justMe)
                        {
                            if (refreshOthers)
                                DungeonScene.Instance.PendingDevEvent = AnimateOneRefreshOthers(chara, chosenAnim);
                            else
                                DungeonScene.Instance.PendingDevEvent = chara.StartAnim(new CharAnimAction(chara.CharLoc, chara.CharDir, chosenAnim, true));
                        }
                        else
                            DungeonScene.Instance.PendingDevEvent = AnimateAll(chosenAnim, true, false, -1);
                    }
                    else
                    {
                        if (justMe)
                        {
                            chara.IdleOverride = chosenAnim;
                            if (refreshOthers)
                                DungeonScene.Instance.PendingDevEvent = AnimateAll(chosenAnim, false, false, -1);
                            else
                                DungeonScene.Instance.PendingDevEvent = chara.StartAnim(new CharAnimIdle(chara.CharLoc, chara.CharDir));
                        }
                        else
                        {
                            GraphicsManager.GlobalIdle = chosenAnim;
                            DungeonScene.Instance.PendingDevEvent = AnimateAll(chosenAnim, false, true, -1);
                        }
                    }

                }
                else if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                {
                    GroundChar chara = GroundScene.Instance.FocusedCharacter;

                    if (justOnce)
                    {
                        if (justMe)
                        {
                            chara.StartAction(new IdleAnimGroundAction(chara.Position, chara.Direction, chosenAnim, false));
                            if (refreshOthers)
                            {
                                foreach (GroundChar groundChar in ZoneManager.Instance.CurrentGround.IterateCharacters())
                                {
                                    if (groundChar != chara)
                                        groundChar.StartAction(new IdleGroundAction(groundChar.Position, groundChar.Direction));
                                }
                            }
                        }
                        else
                        {
                            foreach (GroundChar groundChar in ZoneManager.Instance.CurrentGround.IterateCharacters())
                                groundChar.StartAction(new IdleAnimGroundAction(groundChar.Position, groundChar.Direction, chosenAnim, false));
                        }
                    }
                    else
                    {
                        if (justMe)
                        {
                            chara.IdleOverride = chosenAnim;
                            chara.StartAction(new IdleGroundAction(chara.Position, chara.Direction));
                            if (refreshOthers)
                            {
                                foreach (GroundChar groundChar in ZoneManager.Instance.CurrentGround.IterateCharacters())
                                {
                                    if (groundChar != chara)
                                        groundChar.StartAction(new IdleGroundAction(groundChar.Position, groundChar.Direction));
                                }
                            }
                        }
                        else
                        {
                            GraphicsManager.GlobalIdle = chosenAnim;

                            foreach (GroundChar groundChar in ZoneManager.Instance.CurrentGround.IterateCharacters())
                            {
                                groundChar.IdleOverride = -1;
                                groundChar.StartAction(new IdleGroundAction(groundChar.Position, groundChar.Direction));
                            }
                        }
                    }
                }

            }
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

                        while (!String.IsNullOrEmpty(character.BaseSkills[0].SkillNum))
                            character.DeleteSkill(0);
                        List<string> final_skills = form.RollLatestSkills(character.Level, new List<string>());
                        foreach (string skill in final_skills)
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

                        while (!String.IsNullOrEmpty(character.BaseSkills[0].SkillNum))
                            character.DeleteSkill(0);
                        List<string> final_skills = form.RollLatestSkills(character.Level, new List<string>());
                        foreach (string skill in final_skills)
                            character.LearnSkill(skill, true);
                        GameManager.Instance.SE("Menu/Sort");
                    }
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
        }

        public void btnResetAnim_Click()
        {
            RestartAnim(true);
        }

        public IEnumerator<YieldInstruction> AnimateAll(int charAnim, bool action, bool setOverride, int idleOverride)
        {
            foreach (Character chara in ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                if (setOverride)
                    chara.IdleOverride = idleOverride;

                CharAnimation newAnim;
                if (action)
                    newAnim = new CharAnimAction(chara.CharLoc, chara.CharDir, charAnim, true);
                else
                    newAnim = new CharAnimIdle(chara.CharLoc, chara.CharDir);
                yield return CoroutineManager.Instance.StartCoroutine(chara.StartAnim(newAnim));
            }
        }

        public IEnumerator<YieldInstruction> AnimateOneRefreshOthers(Character chara, int charAnim)
        {
            foreach (Character groundChar in ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                CharAnimation newAnim;
                if (chara == groundChar)
                    newAnim = new CharAnimAction(groundChar.CharLoc, groundChar.CharDir, charAnim, true);
                else
                    newAnim = new CharAnimIdle(groundChar.CharLoc, groundChar.CharDir);
                yield return CoroutineManager.Instance.StartCoroutine(groundChar.StartAnim(newAnim));
            }
        }


        public void btnChangeOrder_Click()
        {
            bool prevUpdate = updating;
            updating = true;

            monstersNumeric = !monstersNumeric;
            DevForm.SetConfig("MonsterNumeric", monstersNumeric ? 1 : 0);
            loadMonsterKeys();

            updating = prevUpdate;
        }

        /// <summary>
        /// Denotes the dev UI is updating from the sprite
        /// </summary>
        bool updating;
        private void SpeciesChanged()
        {
            bool prevUpdate = updating;
            updating = true;

            lock (GameBase.lockObj)
            {
                int tempForm = chosenForm;
                Forms.Clear();
                MonsterData monster = DataManager.Instance.GetMonster(MonsterKeys[chosenMonster]);
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

            if (chosenMonster != MonsterKeys.IndexOf(id.Species))
                ChosenMonster = MonsterKeys.IndexOf(id.Species);
            if (ChosenForm != id.Form)
                ChosenForm = id.Form;
            if (ChosenSkin != SkinKeys.IndexOf(id.Skin))
                ChosenSkin = SkinKeys.IndexOf(id.Skin);
            if (ChosenGender != (int)id.Gender)
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
                    DungeonScene.Instance.FocusedCharacter.Promote(new MonsterID(MonsterKeys[chosenMonster], chosenForm, SkinKeys[chosenSkin], (Gender)chosenGender));


                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                {
                    if (DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null)
                        DungeonScene.Instance.FocusedCharacter.Promote(new MonsterID(MonsterKeys[chosenMonster], chosenForm, SkinKeys[chosenSkin], (Gender)chosenGender));
                }
                else if (GameManager.Instance.CurrentScene == GroundScene.Instance)
                {
                    if (DataManager.Instance.Save.ActiveTeam.Players.Count > 0 && GroundScene.Instance.FocusedCharacter != null)
                    {
                        Character character = DataManager.Instance.Save.ActiveTeam.Leader;
                        character.Promote(new MonsterID(MonsterKeys[chosenMonster], chosenForm, SkinKeys[chosenSkin], (Gender)chosenGender));
                        GroundChar leaderChar = GroundScene.Instance.FocusedCharacter;
                        ZoneManager.Instance.CurrentGround.SetPlayerChar(new GroundChar(DataManager.Instance.Save.ActiveTeam.Leader, leaderChar.MapLoc, leaderChar.CharDir, "PLAYER"));
                    }
                }
            }
        }

    }
}
