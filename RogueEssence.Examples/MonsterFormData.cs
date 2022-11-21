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

        public override string RollSkin(IRandom rand)
        {
            return DataManager.DataType.Skin.ToString() + "_" + 0.ToString();
        }

        public override int GetPersonalityType(int discriminator)
        {
            return 0;
        }

        public override Gender RollGender(IRandom rand)
        {
            return Gender.Genderless;
        }

        public override string RollIntrinsic(IRandom rand, int bounds)
        {
            return DataManager.DataType.Intrinsic.ToString() + "_" + 0.ToString();
        }



        public override List<Gender> GetPossibleGenders()
        {
            List<Gender> genders = new List<Gender>();
            genders.Add(Gender.Genderless);
            return genders;
        }


        public override List<string> GetPossibleSkins()
        {
            List<string> colors = new List<string>();
            colors.Add(DataManager.DataType.Skin.ToString() + "_" + 0.ToString());
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

        public override bool CanLearnSkill(string skill)
        {
            return false;
        }
    }



}