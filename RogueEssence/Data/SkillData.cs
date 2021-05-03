using System;
using RogueEssence.Dungeon;

namespace RogueEssence.Data
{
    [Serializable]
    public class SkillData : IDescribedData
    {
        public override string ToString()
        {
            return Name.DefaultText;
        }

        public LocalText Name { get; set; }

        [Dev.Multiline(0)]
        public LocalText Desc { get; set; }

        public bool Released { get; set; }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }


        public int BaseCharges;

        [Dev.NumberRange(0, 1, Int32.MaxValue)]
        public int Strikes;
        public CombatAction HitboxAction;
        public ExplosionData Explosion;

        public BattleData Data;


        public SkillData()
        {
            Name = new LocalText();
            Desc = new LocalText();
            Comment = "";

            Data = new BattleData();
            Explosion = new ExplosionData();

            Strikes = 1;
            HitboxAction = new AttackAction();
        }


        public string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }

        public string GetIconName()
        {
            ElementData element = DataManager.Instance.GetElement(Data.Element);
            return String.Format("{0}\u2060{1}", element.Symbol, GetColoredName());
        }
    }
}
