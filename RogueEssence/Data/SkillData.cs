using System;
using RogueEssence.Dungeon;

namespace RogueEssence.Data
{
    [Serializable]
    public class SkillData : IDescribedData
    {
        /// <summary>
        /// Returns the name of the skill in the current language.
        /// </summary>
        /// <returns>The skill name.</returns>
        public override string ToString()
        {
            return Name.ToLocal();
        }

        /// <summary>
        /// The name of the skill, containing all translations.
        /// Use the name's ToLocal() function to get the name in current language.
        /// For proper battle log formatting, use GetColoredName() or GetIconName() instead.
        /// </summary>
        public LocalText Name { get; set; }

        /// <summary>
        /// The description of the skill, containing all translations.
        /// Use the name's ToLocal() function to get the name in current language.
        /// </summary>
        [Dev.Multiline(0)]
        public LocalText Desc { get; set; }

        /// <summary>
        /// Internal flag to show whether a skill is completed and allowed to appear in the game.
        /// Skills that are not released appear with an asterisk next to their names when viewed in the Dev Mode editors. 
        /// </summary>
        public bool Released { get; set; }

        /// <summary>
        /// An internal piece of text only visible using the Dev Mode editors, or by calling this property.
        /// Usually used to take notes on the skill if necessary. 
        /// </summary>
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        /// <summary>
        /// Index number of the skill for sorting.  Must be unique.
        /// </summary>
        public int IndexNum;

        /// <summary>
        /// Returns an EntrySummary of the skill.
        /// </summary>
        /// <returns></returns>
        public EntrySummary GenerateEntrySummary()
        {
            BasePowerState powerState = Data.SkillStates.GetWithDefault<BasePowerState>();
            SkillDataSummary summary = new SkillDataSummary(Name, Released, Comment, IndexNum);
            summary.RangeDescription = HitboxAction.GetDescription();
            summary.BaseCharges = BaseCharges;
            summary.BasePower = powerState != null ? powerState.Power : -1;
            summary.HitRate = Data.HitRate;
            summary.Category = Data.Category;
            summary.Element = Data.Element;
            summary.Description = Desc;
            return summary;
        }

        /// <summary>
        /// The default number of times the skill can be used.
        /// </summary>
        public int BaseCharges;

        /// <summary>
        /// How many times the skill attacks.
        /// Set to greater than 1 for multi-strike attacks.
        /// </summary>
        [Dev.NumberRange(0, 1, Int32.MaxValue)]
        public int Strikes;

        /// <summary>
        /// Data on the hitbox of the attack.  Controls range and targeting.
        /// Range - Where it hits, how far away, etc.
        /// Targeting - Whether it hits allies, foes, self, or any combination thereof.
        /// </summary>
        public CombatAction HitboxAction;

        /// <summary>
        /// Optional data to specify a splash effect on the tiles hit.
        /// Contains its own targeting data, separate from HitboxAction.
        /// This allows for skills that explode on allies, but only hit foes, for example.
        /// </summary>
        public ExplosionData Explosion;

        /// <summary>
        /// Events that occur with this skill.
        /// Before it's used, when it hits, after it's used, etc.
        /// Most combat logic relating to the skill belongs here.
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
        /// Gets the name of the skill with appropriate color text code, ideal for use in menus.
        /// </summary>
        /// <returns></returns>
        public string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }

        /// <summary>
        /// Gets the name of the skill with appropriate color text code, and with icon included.
        /// This is ideal for use in the message log.
        /// </summary>
        /// <returns></returns>
        public string GetIconName()
        {
            ElementData element = DataManager.Instance.GetElement(Data.Element);
            return String.Format("{0}\u2060{1}", element.Symbol, GetColoredName());
        }
    }
    
    [Serializable]
    public class SkillDataSummary : EntrySummary
    {
        public string Element;
        public BattleData.SkillCategory Category;
        public int BasePower;
        public int BaseCharges;
        public int HitRate;
        public string RangeDescription;
        public LocalText Description;
        
        public SkillDataSummary() : base() { }

        public SkillDataSummary(LocalText name, bool released, string comment, int sort)
            : base(name, released, comment, sort)
        { }
        
        public override string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }

        /// <summary>
        /// Gets the colored text string of the skill, with icon included
        /// </summary>
        /// <returns></returns>
        public string GetIconName()
        {
            ElementData element = DataManager.Instance.GetElement(Element);
            return String.Format("{0}\u2060{1}", element.Symbol, GetColoredName());
        }
    }
}
