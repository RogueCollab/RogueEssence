using System;
using System.Collections.Generic;
using RogueElements;
using System.Drawing;
using RogueEssence;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Examples
{

    [Serializable]
    public class MonsterFormData : BaseMonsterForm
    {
        public MonsterFormData()
        {

        }

        public override int GetStat(int level, Stat stat, int bonus)
        {
            return 1;
        }

        public override int RollSkin(IRandom rand)
        {
            return 0;
        }

        public override int GetPersonalityType(int discriminator)
        {
            return 0;
        }

        public override Gender RollGender(IRandom rand)
        {
            return Gender.Genderless;
        }

        public override int RollIntrinsic(IRandom rand, int bounds)
        {
            return 0;
        }



        public override List<Gender> GetPossibleGenders()
        {
            List<Gender> genders = new List<Gender>();
            genders.Add(Gender.Genderless);
            return genders;
        }


        public override List<int> GetPossibleSkins()
        {
            List<int> colors = new List<int>();
            colors.Add(0);
            return colors;
        }

        public override List<int> GetPossibleIntrinsicSlots()
        {
            List<int> intrinsics = new List<int>();
            intrinsics.Add(0);
            return intrinsics;
        }


        public override int GetMaxStat(Stat stat)
        {
            return 1;
        }

        public override int ReverseGetStat(Stat stat, int val, int level)
        {
            return 1;
        }

        public override int GetMaxStatBonus(Stat stat)
        {
            return 1;
        }

        public override bool CanLearnSkill(int skill)
        {
            return false;
        }
    }



}