using System;
#if EDITORS
using System.Windows.Forms;
#endif
using System.Drawing;
using RogueEssence.Dungeon;

namespace RogueEssence.Data
{
    [Serializable]
    public class ItemData : ProximityPassive, IDescribedData
    {
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

        public override string ToString()
        {
            return Name.DefaultText;
        }

        public LocalText Name { get; set; }
        public int Sprite;
        public int Icon;

        [Dev.Multiline(0)]
        public LocalText Desc { get; set; }

        public bool Released { get; set; }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        [Dev.NumberRange(0, -1, Int32.MaxValue)]
        public int Price;

        //whether or not the item autosticks
        public bool Cursed;

        public int Rarity;
        public int MaxStack;

        public bool CannotDrop;

        //a simple bool to determine whether the item activates in bag or on hold
        //NOTE: There is no event that fires when items are given to or taken from the bag, thus RefreshTraits for bag items WILL NOT WORK right now
        public bool BagEffect;

        [Dev.SubGroup]
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
            Icon = -1;
            Comment = "";

            ItemStates = new StateCollection<ItemState>();

            UseAction = new AttackAction();
            Explosion = new ExplosionData();
            UseEvent = new BattleData();
            ThrowAnim = new Content.AnimData();
        }
        
#if EDITORS
        protected override void LoadMemberControl(TableLayoutPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            if (name == "Sprite")
            {
                loadLabelControl(control, name);
                //for strings, use an edit textbox
                Dev.SpriteBrowser browser = new Dev.SpriteBrowser();
                browser.Size = new Size(210, 256);
                browser.ChosenPic = (int)member;
                control.Controls.Add(browser);
            }
            else
            {
                base.LoadMemberControl(control, name, type, attributes, member, isWindow);
            }
        }

        protected override void SaveMemberControl(TableLayoutPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            if (name == "Sprite")
            {
                int controlIndex = 0;
                controlIndex++;
                Dev.SpriteBrowser browser = (Dev.SpriteBrowser)control.Controls[controlIndex];
                member = browser.ChosenPic;
                controlIndex++;
            }
            else
            {
                base.SaveMemberControl(control, name, type, attributes, ref member, isWindow);
            }
        }
#endif
    }


    [Serializable]
    public class ItemEntrySummary : EntrySummary
    {
        //TODO: implement this so that swap menus don't have to load an item to see if it's a treasure item
        public ItemData.UseType UsageType;

        public ItemEntrySummary() : base()
        {

        }

        public ItemEntrySummary(LocalText name, bool released, string comment, ItemData.UseType useType) : base(name, released, comment)
        {
            UsageType = useType;
        }
    }

}

