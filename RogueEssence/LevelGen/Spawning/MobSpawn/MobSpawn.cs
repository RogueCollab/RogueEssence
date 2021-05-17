using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueElements;

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
        [Dev.MonsterID(0, false, false, true, true)]
        public MonsterID BaseForm;

        [Dev.SubGroup]
        [Dev.RangeBorder(0, false, true)]
        public RandRange Level;

        [Dev.DataType(1, DataManager.DataType.Skill, false)]
        public List<int> SpecifiedSkills;

        [Dev.DataType(0, DataManager.DataType.Intrinsic, true)]
        public int Intrinsic;

        [Dev.DataType(0, DataManager.DataType.AI, false)]
        public int Tactic;

        public List<MobSpawnExtra> SpawnFeatures;
        //extra spawn event
        //items
        //status problems
        //special statuses
        //stat changes

        public MobSpawn()
        {
            BaseForm = new MonsterID(0, 0, -1, Gender.Unknown);
            SpecifiedSkills = new List<int>();
            Intrinsic = -1;
            SpawnFeatures = new List<MobSpawnExtra>();
        }
        protected MobSpawn(MobSpawn other) : this()
        {
            BaseForm = other.BaseForm;
            Tactic = other.Tactic;
            Level = other.Level;
            SpecifiedSkills.AddRange(other.SpecifiedSkills);
            Intrinsic = other.Intrinsic;
            foreach(MobSpawnExtra extra in other.SpawnFeatures)
                SpawnFeatures.Add(extra.Copy());
        }
        public MobSpawn Copy() { return new MobSpawn(this); }
        ISpawnable ISpawnable.Copy() { return Copy(); }

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

            if (Intrinsic == -1)
                character.BaseIntrinsics[0] = formEntry.RollIntrinsic(map.Rand, 2);
            else
                character.BaseIntrinsics[0] = Intrinsic;

            character.Discriminator = map.Rand.Next();

            Character new_mob = new Character(character, team);
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
