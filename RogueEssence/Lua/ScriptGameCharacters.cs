using RogueEssence.Ground;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Linq;

using NLua;
using System.Collections.Generic;
using RogueElements;
using System;

namespace RogueEssence.Script
{
    public partial class ScriptGame : ILuaEngineComponent
    {


        //===================================
        // Characters
        //===================================


        /// <summary>
        /// Sets a character's nickname
        /// </summary>
        /// <param name="character">The character to rename</param>
        /// <param name="nickname">The new name</param>
        public void SetCharacterNickname(Character character, string nickname)
        {
            character.Nickname = nickname;
        }

        /// <summary>
        /// Gets the character nickname
        /// </summary>
        /// <param name="character">The character to get the nickname from</param>
        /// <returns>The character's nickname</returns>
        public string GetCharacterNickname(Character character)
        {
            return character.Nickname;
        }

        /// <summary>
        /// Sets the name of the player's team
        /// </summary>
        /// <param name="teamname">The new team name</param>
        public void SetTeamName(string teamname)
        {
            DataManager.Instance.Save.ActiveTeam.Name = teamname;
            foreach (Character chara in DataManager.Instance.Save.ActiveTeam.Players)
                chara.OriginalTeam = DataManager.Instance.Save.ActiveTeam.Name;
            foreach (Character chara in DataManager.Instance.Save.ActiveTeam.Assembly)
                chara.OriginalTeam = DataManager.Instance.Save.ActiveTeam.Name;
        }

        /// <summary>
        /// Gets the name of the player's team
        /// </summary>
        /// <returns>The team's name</returns>
        public string GetTeamName()
        {
            return DataManager.Instance.Save.ActiveTeam.GetDisplayName();
        }

        /// <summary>
        /// Checks if the character can relearn any skills.
        /// </summary>
        /// <param name="character">The character to check</param>
        public bool CanRelearn(Character character)
        {
            return character.GetRelearnableSkills(true).Count > 0;
        }

        /// <summary>
        /// Checks if the character can forget any skills.
        /// </summary>
        /// <param name="character">The character to check</param>
        public bool CanForget(Character character)
        {
            int count = 0;
            foreach (SlotSkill skill in character.BaseSkills)
            {
                if (!String.IsNullOrEmpty(skill.SkillNum))
                    count++;
            }
            return count > 1;
        }

        /// <summary>
        /// Checks if the character can learn any skills.
        /// </summary>
        /// <param name="character">The character to check</param>
        public bool CanLearn(Character character)
        {
            foreach (SlotSkill skill in character.BaseSkills)
            {
                if (String.IsNullOrEmpty(skill.SkillNum))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks the levels gained by a character and prompts to learn all skills along the levels.
        /// Waits until all skills have been accepted or declined before continuing.
        /// </summary>
        /// <example>
        /// GAME:CheckLevelSkills(player, 5)
        /// </example>
        public LuaFunction CheckLevelSkills;

        /// <summary>
        /// [LuaFunction] CheckLevelSkills
        /// </summary>
        /// <param name="chara">The character to prompt for learning.</param>
        /// <param name="oldLevel">The level that the character leveled up from.</param>
        /// <returns></returns>
        public Coroutine _CheckLevelSkills(Character chara, int oldLevel)
        {
            return new Coroutine(_checkLevelSkills(chara, oldLevel));
        }

        private IEnumerator<YieldInstruction> _checkLevelSkills(Character chara, int oldLevel)
        {
            DungeonScene.GetLevelSkills(chara, oldLevel);

            foreach (string skill in DungeonScene.GetLevelSkills(chara, oldLevel))
            {
                int learn = -1;
                if (DataManager.Instance.CurrentReplay != null)
                    learn = DataManager.Instance.CurrentReplay.ReadUI();
                else
                {
                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.TryLearnSkill(chara, skill, (int slot) => { learn = slot; }, () => { }));
                    DataManager.Instance.LogUIPlay(learn);
                }
                if (learn > -1)
                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.LearnSkillWithFanfare(chara, skill, learn));
            }
        }

        /// <summary>
        /// Attempts to give a new skill to the specified character, prompting to replace an old one if they are full.
        /// Waits until the skill has been accepted or declined before continuing.
        /// </summary>
        /// <example>
        /// GAME:TryLearnSkill(player, "thunder")
        /// </example>
        public LuaFunction TryLearnSkill;

        /// <summary>
        /// [LuaFunction] TryLearnSkill
        /// </summary>
        /// <param name="chara">The character to learn the skill</param>
        /// <param name="skill">The skill to learn</param>
        /// <returns></returns>
        public Coroutine _TryLearnSkill(Character chara, string skill)
        {
            return new Coroutine(_tryLearnSkill(chara, skill));
        }

        private IEnumerator<YieldInstruction> _tryLearnSkill(Character chara, string skill)
        {
            int learn = -1;
            if (DataManager.Instance.CurrentReplay != null)
                learn = DataManager.Instance.CurrentReplay.ReadUI();
            else
            {
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.TryLearnSkill(chara, skill, (int slot) => { learn = slot; }, () => { }));
                DataManager.Instance.LogUIPlay(learn);
            }
            if (learn > -1)
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.LearnSkillWithFanfare(chara, skill, learn));
        }

        /// <summary>
        /// Gives a new skill to a specified character.
        /// Fails if the character's skills are full.
        /// </summary>
        /// <param name="chara">The character to learn the skill</param>
        /// <param name="skill">The skill to learn</param>
        public void LearnSkill(Character chara, string skill)
        {
            chara.LearnSkill(skill, true);
        }

        /// <summary>
        /// Removed a skill from the specified character.
        /// </summary>
        /// <param name="chara">The character to forget the skill</param>
        /// <param name="slot">The slot of the skill to forget</param>
        public void ForgetSkill(Character chara, int slot)
        {
            chara.DeleteSkill(slot);
        }

        /// <summary>
        /// Makes a skill impossible to forget or replace for the specified character.
        /// Note that this only affects normal gameplay. Scripts can still freely get rid of the skill.
        /// </summary>
        /// <param name="chara">The character to lock the skill</param>
        /// <param name="slot">The slot of the skill to lock</param>
        public void LockSkill(Character chara, int slot)
        {
            chara.SetSkillLocking(slot, true);
        }

        /// <summary>
        /// Unlocks a previously locked skill for the specified character, making it possible to be forgotten or replaced during normal gameplay.
        /// </summary>
        /// <param name="chara">The character to unlock the skill</param>
        /// <param name="slot">The slot of the skill to unlock</param>
        public void UnlockSkill(Character chara, int slot)
        {
            chara.SetSkillLocking(slot, false);
        }

        /// <summary>
        /// Gives a new skill to a specified character, replacing a specifically chosen slot.
        /// </summary>
        /// <param name="character">The character to learn the skill</param>
        /// <param name="skillId">The skill to learn</param>
        /// <param name="slot">The slot to replace</param>
        public void SetCharacterSkill(Character character, string skillId, int slot, bool enabled = true)
        {
            character.ReplaceSkill(skillId, slot, enabled);
        }

        /// <summary>
        /// Gets the skill from a specified character and specified slot.
        /// </summary>
        /// <param name="chara">The character to get the skill from.</param>
        /// <param name="slot">The slot to get the skill from.</param>
        /// <returns>The ID of the skill in the slot</returns>
        public string GetCharacterSkill(Character chara, int slot)
        {
            return chara.BaseSkills[slot].SkillNum;
        }

        /// <summary>
        /// Checks if the character can be promoted to a new class.
        /// </summary>
        /// <param name="character">The character to check</param>
        /// <returns>True if the character can be promoted, false otherwise.</returns>
        public bool CanPromote(Character character)
        {
            MonsterData entry = DataManager.Instance.GetMonster(character.BaseForm.Species);
            for (int ii = 0; ii < entry.Promotions.Count; ii++)
            {
                if (!DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(entry.Promotions[ii].Result.ToString()).Released)
                    continue;

                bool hardReq = false;
                foreach (PromoteDetail detail in entry.Promotions[ii].Details)
                {
                    if (detail.IsHardReq() && !detail.GetReq(character, false))
                    {
                        hardReq = true;
                        break;
                    }
                }
                if (!hardReq)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a list of possible classes that the character can prmote to.
        /// </summary>
        /// <param name="character">The character to check</param>
        /// <param name="bypassItem">An exception item that can bypass checks for promotion</param>
        /// <returns>A lua table of PromoteBranch objects</returns>
        public LuaTable GetAvailablePromotions(Character character, string bypassItem)
        {
            MonsterData entry = DataManager.Instance.GetMonster(character.BaseForm.Species);
            bool bypass = character.EquippedItem.ID == bypassItem;

            LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, chara) table.insert(tbl, chara) end").First() as LuaFunction;

            for (int ii = 0; ii < entry.Promotions.Count; ii++)
            {
                if (!DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(entry.Promotions[ii].Result.ToString()).Released)
                    continue;
                if (entry.Promotions[ii].IsQualified(character, false))
                    addfn.Call(tbl, entry.Promotions[ii]);
                else if (bypass)
                {
                    bool hardReq = false;
                    foreach (PromoteDetail detail in entry.Promotions[ii].Details)
                    {
                        if (detail.IsHardReq() && !detail.GetReq(character, false))
                        {
                            hardReq = true;
                            break;
                        }
                    }
                    if (!hardReq)
                        addfn.Call(tbl, entry.Promotions[ii]);
                }
            }

            return tbl;
        }

        /// <summary>
        /// Promotes a character ot a new class.
        /// </summary>
        /// <param name="character">The character to promote</param>
        /// <param name="branch">The PromoteBranch to promote with</param>
        /// <param name="bypassItem">An exception item that can bypass checks for promotion</param>
        public void PromoteCharacter(Character character, PromoteBranch branch, string bypassItem)
        {
            MonsterData entry = DataManager.Instance.GetMonster(branch.Result);
            //exception item bypass
            bool bypass = character.EquippedItem.ID == bypassItem;
            MonsterID newData = character.BaseForm;
            newData.Species = branch.Result;
            branch.BeforePromote(character, false, ref newData);
            character.Promote(newData);
            character.FullRestore();
            branch.OnPromote(character, false, bypass);
            //remove exception item if there is one...
            if (bypass)
                character.SilentDequipItem();
            DataManager.Instance.Save.RegisterMonster(character.BaseForm);
            DataManager.Instance.Save.RogueUnlockMonster(character.BaseForm.Species);
        }


    }
}
