using System;
using RogueEssence.Dev;
using RogueEssence.Dungeon;
using System.Collections.Generic;
using RogueEssence.Ground;

namespace RogueEssence.Data
{
    [Serializable]
    public class ItemData : ProximityPassive, IDescribedData
    {
        public override string ToString()
        {
            return Name.ToLocal();
        }
        public enum UseType
        {
            None,
            Use,
            UseOther,
            Throw,
            Eat,
            Drink,
            Learn,
            Box,
            Treasure
        }

        /// <summary>
        /// The name of the data
        /// </summary>
        public LocalText Name { get; set; }

        /// <summary>
        /// How the item looks in the game.
        /// </summary>
        [Anim(0, "Item/")]
        public string Sprite;

        /// <summary>
        /// The icon displayed next to the item's name.
        /// </summary>
        [Alias(0, "Item_Icon")]
        public int Icon;


        /// <summary>
        /// The description of the item
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

        public EntrySummary GenerateEntrySummary()
        {
            ItemEntrySummary summary = new ItemEntrySummary(Name, Released, Comment, SortCategory, Icon, UsageType);
            foreach (ItemState state in ItemStates)
                summary.States.Add(new FlagType(state.GetType()));
            return summary;
        }

        /// <summary>
        /// The number order of the item for sorting
        /// </summary>
        public int SortCategory;

        /// <summary>
        /// How much the item sells for.
        /// </summary>
        [Dev.NumberRange(0, -1, Int32.MaxValue)]
        public int Price;

        /// <summary>
        /// The rarity rating of the item.
        /// </summary>
        public int Rarity;

        /// <summary>
        /// The maximum amount a single slot of this item can be stacked.
        /// 0 is unstackable.
        /// </summary>
        public int MaxStack;

        /// <summary>
        /// Cannot be manually dropped, cannot be lost, cannot be stolen.
        /// </summary>
        public bool CannotDrop;

        /// <summary>
        /// Determines whether the item activates in bag or on equip.
        /// </summary>
        public bool BagEffect;

        /// <summary>
        /// Special variables that this item contains.
        /// They are potentially checked against in a select number of battle events.
        /// </summary>
        [ListCollapse]
        public StateCollection<ItemState> ItemStates;
        
        /// <summary>
        /// List of ground actions that can be used with that item.
        /// </summary>
        public List<GroundItemEvent> GroundUseActions;
        
        /// <summary>
        /// The hitbox of the attack that comes out when the item is used.
        /// </summary>
        public CombatAction UseAction;

        /// <summary>
        /// The splash effect that is triggered for each target of the UseAction hitbox.
        /// </summary>
        public ExplosionData Explosion;

        /// <summary>
        /// The effects of using the item.
        /// </summary>
        public BattleData UseEvent;

        /// <summary>
        /// Define whether this is a food, drink, etc for the proper sound/animation on use
        /// "None" and "ammo" will prevent use, but UseEffect can still be triggered by throwing it.
        /// This means that throw effect is the same as use effect.
        /// </summary>
        public UseType UsageType;

        /// <summary>
        /// Defines whether this item flies in an arc or in a straight line.
        /// </summary>
        public bool ArcThrow;

        /// <summary>
        /// Defines whether this item will disappear if thrown, even if it doesnt hit a target.
        /// </summary>
        public bool BreakOnThrow;

        /// <summary>
        /// Defines the custom graphics for the item when it is thrown.
        /// Set to an empty anim to use the item's own sprite.
        /// </summary>
        public Content.AnimData ThrowAnim;
        
        public ItemData()
        {
            Name = new LocalText();
            Desc = new LocalText();
            Sprite = "";
            Icon = -1;
            Comment = "";

            ItemStates = new StateCollection<ItemState>();
            GroundUseActions = new List<GroundItemEvent>();

            UseAction = new AttackAction();
            Explosion = new ExplosionData();
            UseEvent = new BattleData();
            ThrowAnim = new Content.AnimData();
        }


        /// <summary>
        /// Gets the colored text string of the item
        /// </summary>
        /// <returns></returns>
        public string GetColoredName()
        {
            if (UsageType == UseType.Treasure)
                return String.Format("[color=#6384E6]{0}[color]", Name.ToLocal());
            else
                return String.Format("[color=#FFCEFF]{0}[color]", Name.ToLocal());
        }

        /// <summary>
        /// Gets the colored text string of the item, with icon included
        /// </summary>
        /// <returns></returns>
        public string GetIconName()
        {
            string prefix = "";
            if (Icon > -1)
                prefix += ((char)(Icon + 0xE0A0)).ToString();

            return String.Format("{0}{1}", prefix, GetColoredName());
        }
    }


    [Serializable]
    public class ItemEntrySummary : EntrySummary
    {
        public int Icon;
        public ItemData.UseType UsageType;
        public List<FlagType> States;

        public ItemEntrySummary() : base()
        {
            States = new List<FlagType>();
        }

        public ItemEntrySummary(LocalText name, bool released, string comment, int sort, int icon, ItemData.UseType useType) : base(name, released, comment, sort)
        {
            Icon = icon;
            UsageType = useType;
            States = new List<FlagType>();
        }

        public override string GetColoredName()
        {
            if (UsageType == ItemData.UseType.Treasure)
                return String.Format("[color=#6384E6]{0}[color]", Name.ToLocal());
            else
                return String.Format("[color=#FFCEFF]{0}[color]", Name.ToLocal());
        }

        /// <summary>
        /// Gets the colored text string of the item, with icon included
        /// </summary>
        /// <returns></returns>
        public string GetIconName()
        {
            string prefix = "";
            if (Icon > -1)
                prefix += ((char)(Icon + 0xE0A0)).ToString();

            return String.Format("{0}{1}", prefix, GetColoredName());
        }

        public bool ContainsState<T>() where T : ItemState
        {
            return ContainsState(typeof(T));
        }

        public bool ContainsState(Type type)
        {
            foreach (FlagType testType in States)
            {
                if (testType.FullType == type)
                    return true;
            }
            return false;
        }
    }
}
