using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueElements;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace RogueEssence.LevelGen
{
    //TODO: port this interface down to RogueElements
    //to address concerns that map gen context may need to be passed into the logic responsible for creating a spawnable entity
    public interface ISpawnGenerator<T>
    {
        Character Spawn(Team team, T map);
    }

    [Serializable]
    public class MobSpawn : ISpawnable, ISpawnGenerator<IMobSpawnMap>
    {
        /// <summary>
        /// The species, form, etc. of the mob spawned.
        /// </summary>
        [Dev.MonsterID(0, false, false, true, true)]
        public MonsterID BaseForm;

        /// <summary>
        /// The level of the monster spawned.
        /// </summary>
        [Dev.SubGroup]
        [Dev.RangeBorder(0, false, true)]
        public RandRange Level;

        /// <summary>
        /// The skills for the mob.  Empty spaces will be filled based on its current level.
        /// </summary>
        [Dev.DataType(1, DataManager.DataType.Skill, false)]
        public List<int> SpecifiedSkills;

        /// <summary>
        /// The passive skill for the mob.
        /// </summary>
        [JsonConverter(typeof(Dev.IntrinsicConverter))]
        [Dev.DataType(0, DataManager.DataType.Intrinsic, true)]
        public string Intrinsic;

        /// <summary>
        /// The mob's AI.
        /// </summary>
        [JsonConverter(typeof(Dev.AIConverter))]
        [Dev.DataType(0, DataManager.DataType.AI, false)]
        public string Tactic;

        /// <summary>
        /// Conditions that must be met in order for the mob to spawn.
        /// </summary>
        public List<MobSpawnCheck> SpawnConditions;

        /// <summary>
        /// Additional alterations made to the mob after it is created but before it is spawned.
        /// </summary>
        public List<MobSpawnExtra> SpawnFeatures;
        
        public MobSpawn()
        {
            BaseForm = new MonsterID(0, 0, -1, Gender.Unknown);
            SpecifiedSkills = new List<int>();
            Intrinsic = "";
            Tactic = "";
            SpawnConditions = new List<MobSpawnCheck>();
            SpawnFeatures = new List<MobSpawnExtra>();
        }
        protected MobSpawn(MobSpawn other) : this()
        {
            BaseForm = other.BaseForm;
            Tactic = other.Tactic;
            Level = other.Level;
            SpecifiedSkills.AddRange(other.SpecifiedSkills);
            Intrinsic = other.Intrinsic;
            foreach (MobSpawnCheck extra in other.SpawnConditions)
                SpawnConditions.Add(extra.Copy());
            foreach (MobSpawnExtra extra in other.SpawnFeatures)
                SpawnFeatures.Add(extra.Copy());
        }
        public MobSpawn Copy() { return new MobSpawn(this); }
        ISpawnable ISpawnable.Copy() { return Copy(); }

        public bool CanSpawn()
        {
            foreach (MobSpawnCheck extra in SpawnConditions)
            {
                if (!extra.CanSpawn())
                    return false;
            }
            return true;
        }

        protected Character SpawnBase(Team team, IMobSpawnMap map)
        {
            MonsterID formData = BaseForm;
            MonsterData dex = DataManager.Instance.GetMonster(formData.Species);
            if (formData.Form == -1)
            {
                int form = map.Rand.Next(dex.Forms.Count);
                formData.Form = form;
            }

            BaseMonsterForm formEntry = dex.Forms[formData.Form];

            if (formData.Gender == Gender.Unknown)
                formData.Gender = formEntry.RollGender(map.Rand);

            if (formData.Skin == -1)
                formData.Skin = formEntry.RollSkin(map.Rand);

            CharData character = new CharData();
            character.BaseForm = formData;
            character.Level = Level.Pick(map.Rand);
            
            List<int> final_skills = formEntry.RollLatestSkills(character.Level, SpecifiedSkills);
            for (int ii = 0; ii < final_skills.Count; ii++)
                character.BaseSkills[ii] = new SlotSkill(final_skills[ii]);

            if (String.IsNullOrEmpty(Intrinsic))
                character.BaseIntrinsics[0] = formEntry.RollIntrinsic(map.Rand, 2);
            else
                character.BaseIntrinsics[0] = Intrinsic;

            character.Discriminator = map.Rand.Next();

            Character new_mob = new Character(character);
            team.Players.Add(new_mob);

            return new_mob;
        }

        public virtual Character Spawn(Team team, IMobSpawnMap map)
        {
            Character newChar = SpawnBase(team, map);

            AITactic tactic = DataManager.Instance.GetAITactic(Tactic);
            newChar.Tactic = new AITactic(tactic);

            foreach (MobSpawnExtra spawnExtra in SpawnFeatures)
                spawnExtra.ApplyFeature(map, newChar);

            return newChar;
        }

        public override string ToString()
        {
            MonsterData entry = DataManager.Instance.GetMonster(BaseForm.Species);
            return String.Format("{0} Lv.{1}", entry.Name.ToLocal(), Level);
        }
    }


}
