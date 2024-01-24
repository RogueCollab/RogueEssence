using System;
using RogueEssence.Dungeon;

namespace RogueEssence.Data
{
    [Serializable]
    public class SkillData : IDescribedData
    {
        public override string ToString()
        {
            return Name.ToLocal();
        }

        /// <summary>
        /// The name of the data
        /// </summary>
        public LocalText Name { get; set; }

        /// <summary>
        /// The description of the data
        /// </summary>
        [Dev.Multiline(0)]
        public LocalText Desc { get; set; }

        /// <summary>
        /// Is it released and allowed to show up in the game?
        /// </summary>
        public bool Released { get; set; }

        /// <summary>
        /// Comments visible to only developers
        /// </summary>
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        /// <summary>
        /// Index number of the skill for sorting.  Must be unique
        /// </summary>
        public int IndexNum;

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment, IndexNum); }

        /// <summary>
        /// The number of times the skill can be used.
        /// </summary>
        public int BaseCharges;

        /// <summary>
        /// How many times the skill attacks.
        /// </summary>
        [Dev.NumberRange(0, 1, Int32.MaxValue)]
        public int Strikes;

        /// <summary>
        /// Data on the hitbox of the attack.  Controls range and targeting.
        /// </summary>
        public CombatAction HitboxAction;

        /// <summary>
        /// Optional data to specify a splash effect on the tiles hit.
        /// </summary>
        public ExplosionData Explosion;

        /// <summary>
        /// Events that occur with this skill.
        /// Before it's used, when it hits, after it's used, etc.
        /// </summary>
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


        /// <summary>
        /// Gets the colored text string of the skill
        /// </summary>
        /// <returns></returns>
        public string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }

        /// <summary>
        /// Gets the colored text string of the skill, with icon included
        /// </summary>
        /// <returns></returns>
        public string GetIconName()
        {
            ElementData element = DataManager.Instance.GetElement(Data.Element);
            return String.Format("{0}\u2060{1}", element.Symbol, GetColoredName());
        }
    }
}
