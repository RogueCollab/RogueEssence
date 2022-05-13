using System;
using RogueEssence.Dev;
using RogueEssence.Dungeon;
using System.Collections.Generic;

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
            Box
        }

        public LocalText Name { get; set; }


        [Anim(0, "Item/")]
        public string Sprite;
        [Alias(0, "Item_Icon")]
        public int Icon;

        [Dev.Multiline(0)]
        public LocalText Desc { get; set; }

        public bool Released { get; set; }
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary()
        {
            ItemEntrySummary summary = new ItemEntrySummary(Name, Released, Comment, UsageType);
            foreach (ItemState state in ItemStates)
                summary.States.Add(new FlagType(state.GetType()));
            return summary;
        }

        [Dev.NumberRange(0, -1, Int32.MaxValue)]
        public int Price;

        public int Rarity;
        public int MaxStack;

        public bool CannotDrop;

        //a simple bool to determine whether the item activates in bag or on hold
        //NOTE: There is no event that fires when items are given to or taken from the bag, thus RefreshTraits for bag items WILL NOT WORK right now
        public bool BagEffect;

        public StateCollection<ItemState> ItemStates;

        //add equip effects
        //inherited from PassiveEffect; check there.

        public CombatAction UseAction;
        public ExplosionData Explosion;

        //the effect of using it
        public BattleData UseEvent;

        //define whether this is a food, drink, etc for the proper sound/animation on use
        //"none" and "ammo" will prevent use, but UseEffect can still be triggered by throwing it
        //(this means that throw effect is the same as use effect)
        public UseType UsageType;

        //define whether this item flies in an arc or in a straight line
        public bool ArcThrow;
        //define an AnimData for the custom graphic when flying (-1 for using the item graphic itself)
        public Content.AnimData ThrowAnim;
        
        public ItemData()
        {
            Name = new LocalText();
            Desc = new LocalText();
            Sprite = "";
            Icon = -1;
            Comment = "";

            ItemStates = new StateCollection<ItemState>();

            UseAction = new AttackAction();
            Explosion = new ExplosionData();
            UseEvent = new BattleData();
            ThrowAnim = new Content.AnimData();
        }


        public string GetColoredName()
        {
            return String.Format("[color=#FFCEFF]{0}[color]", Name.ToLocal());
        }

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
        public ItemData.UseType UsageType;
        public List<FlagType> States;

        public ItemEntrySummary() : base()
        {
            States = new List<FlagType>();
        }

        public ItemEntrySummary(LocalText name, bool released, string comment, ItemData.UseType useType) : base(name, released, comment)
        {
            UsageType = useType;
            States = new List<FlagType>();
        }

        public override string GetColoredName()
        {
            return String.Format("[color=#FFCEFF]{0}[color]", Name.ToLocal());
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
