using System;
using System.Collections.Generic;
using RogueEssence.Data;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using RogueEssence.Script;
using NLua;
using Newtonsoft.Json;
using QuadTrees.QTreePoint;
using RogueEssence.Dev;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class Character : CharData, ICharSprite, IEntityWithLuaData, IPointQuadStorable
    {

        public const int MAX_FULLNESS = 100;

        public const int MAX_SPEED = 3;
        public const int MIN_SPEED = -3;

        public string ProxyName;

        public string Name
        {
            get
            {
                if (ProxyName != "")
                    return ProxyName;
                if (MemberTeam is ExplorerTeam)
                {
                    if (!String.IsNullOrEmpty(Nickname))
                        return Nickname;
                    else
                        return DataManager.Instance.GetMonster(BaseForm.Species).Name.ToLocal();
                }
                else
                {
                    if (!String.IsNullOrEmpty(Nickname))
                        return Nickname;
                    else
                        return GetFullFormName(BaseForm);
                }
            }
        }

        /// <summary>
        /// Gets the name of the character, fully colored
        /// </summary>
        /// <param name="trueName">If set to true, uses Basename to bypass any alias or fake name.</param>
        /// <returns></returns>
        public string GetDisplayName(bool trueName)
        {
            string name = Name;
            if (trueName)
            {
                if (MemberTeam is ExplorerTeam)
                {
                    if (!String.IsNullOrEmpty(Nickname))
                        name = Nickname;
                    else
                        name = DataManager.Instance.GetMonster(BaseForm.Species).Name.ToLocal();
                }
                else
                {
                    if (!String.IsNullOrEmpty(Nickname))
                        name = Nickname;
                    else
                        name = GetFullFormName(BaseForm);
                }
            }

            Team team = MemberTeam;
            if (Unidentifiable && team != DataManager.Instance.Save.ActiveTeam)
                name = "???";

            if (team == DataManager.Instance.Save.ActiveTeam)
            {
                if (this == team.Leader)
                    return String.Format("[color=#009CFF]{0}[color]", name);
                return String.Format("[color=#FFFF00]{0}[color]", name);
            }
            return String.Format("[color=#00FFFF]{0}[color]", name);
        }

        public MonsterID ProxySprite;
        public MonsterID Appearance
        {
            get
            {
                if (ProxySprite.IsValid())
                    return ProxySprite;
                else
                    return CurrentForm;
            }
        }
        public MonsterID CurrentForm;

        public int MaxHP { get { return DataManager.Instance.GetMonster(BaseForm.Species).Forms[BaseForm.Form].GetStat(Level, Stat.HP, MaxHPBonus); } }
        public int ProxyAtk;
        public int Atk
        {
            get
            {
                if (ProxyAtk > -1)
                    return ProxyAtk;
                else
                    return BaseAtk;
            }
        }
        public int BaseAtk { get { return DataManager.Instance.GetMonster(CurrentForm.Species).Forms[CurrentForm.Form].GetStat(Level, Stat.Attack, AtkBonus); } }
        public int ProxyDef;
        public int Def
        {
            get
            {
                if (ProxyDef > -1)
                    return ProxyDef;
                else
                    return BaseDef;
            }
        }
        public int BaseDef { get { return DataManager.Instance.GetMonster(CurrentForm.Species).Forms[CurrentForm.Form].GetStat(Level, Stat.Defense, DefBonus); } }
        public int ProxyMAtk;
        public int MAtk
        {
            get
            {
                if (ProxyMAtk > -1)
                    return ProxyMAtk;
                else
                    return BaseMAtk;
            }
        }
        public int BaseMAtk { get { return DataManager.Instance.GetMonster(CurrentForm.Species).Forms[CurrentForm.Form].GetStat(Level, Stat.MAtk, MAtkBonus); } }
        public int ProxyMDef;
        public int MDef
        {
            get
            {
                if (ProxyMDef > -1)
                    return ProxyMDef;
                else
                    return BaseMDef;
            }
        }
        public int BaseMDef { get { return DataManager.Instance.GetMonster(CurrentForm.Species).Forms[CurrentForm.Form].GetStat(Level, Stat.MDef, MDefBonus); } }
        public int ProxySpeed;
        public int Speed
        {
            get
            {
                if (ProxySpeed > -1)
                    return ProxySpeed;
                else
                    return BaseSpeed;
            }
        }
        public int BaseSpeed { get { return DataManager.Instance.GetMonster(CurrentForm.Species).Forms[CurrentForm.Form].GetStat(Level, Stat.Speed, SpeedBonus); } }


        public List<BackReference<Skill>> Skills;

        [JsonConverter(typeof(ElementConverter))]
        public string Element1 { get; private set; }

        [JsonConverter(typeof(ElementConverter))]
        public string Element2 { get; private set; }

        public List<BackReference<Intrinsic>> Intrinsics;

        public InvItem EquippedItem;

        /// <summary>
        /// If turned on, this entity is considered NOT a part of the adventure for the duration of that adventure.
        /// </summary>
        public bool Absentee;

        public LuaTable LuaData
        {
            get { return LuaDataTable; }
            set { LuaDataTable = value; }
        }

        int movementSpeed;
        public int MovementSpeed
        {
            get { return movementSpeed; }
            set { movementSpeed = Math.Min(Math.Max(value, MIN_SPEED), MAX_SPEED); }
        }

        public int ChargeBoost;

        /// <summary>
        /// Position on the map grid in tiles.
        /// </summary>
        public Loc CharLoc
        {
            get { return currentCharAction.CharLoc; }
            set
            {
                Loc oldLoc = currentCharAction.CharLoc;
                currentCharAction.CharLoc = value;
                updateLoc(oldLoc);
            }
        }

        System.Drawing.Point IPointQuadStorable.Point { get { return new System.Drawing.Point(CharLoc.X, CharLoc.Y); } }

        /// <summary>
        /// Character's direction.
        /// </summary>
        public Dir8 CharDir
        {
            get { return currentCharAction.CharDir; }
            set { currentCharAction.CharDir = value; }
        }

        public int HP;
        public int HPRemainder;

        public int Fullness;
        public int FullnessRemainder;
        public int MaxFullness;

        private bool dead;
        public bool Dead
        {
            get { return dead; }
            set
            {
                dead = value;
                MemberTeam?.CharacterDeathChanged();
            }
        }

        [JsonConverter(typeof(StatusDictConverter))]
        public Dictionary<string, StatusEffect> StatusEffects;

        public bool MustHitNext;

        /// <summary>
        /// The number of turns this character must wait before being able to move again.
        /// </summary>
        public int TurnWait;

        /// <summary>
        /// The number of turn tiers that this character has moved OR acted on.
        /// </summary>
        public int TiersUsed;

        /// <summary>
        /// Whether the character has made an action during this map turn.  Only one action per map turn permitted.
        /// </summary>
        public bool TurnUsed;
        [NonSerialized]
        public List<StatusRef> StatusesTargetingThis;
        public bool EXPMarked;

        public AITactic Tactic;

        //calculable via save-loading
        public TerrainData.Mobility Mobility;
        public bool CantWalk;
        //will prevent the passive item effects, as well as the "stick" effect
        //does not affect active use
        public bool ItemDisabled;
        public bool IntrinsicDisabled;
        public bool CanRemoveStuck;
        public bool StopItemAtHit;
        public bool MovesScrambled;
        public bool ChargeSaver;

        /// <summary>
        /// The Wait command translates to attack.
        /// </summary>
        public bool WaitToAttack;

        /// <summary>
        /// Can't pick up/place/equip/take items, cant interact with ground/people.
        /// </summary>
        public bool CantInteract;
        /// <summary>
        /// Treated as a foe by friends in combat
        /// </summary>
        public bool EnemyOfFriend;
        /// <summary>
        /// Treats friends as foes in and out of combat
        /// </summary>
        public bool AttackFriend;
        //visibility and sight
        public Map.SightRange TileSight;
        public Map.SightRange CharSight;
        //sprite is not visible and information about the entity is unavailable
        public bool Unidentifiable;
        //position is not visible
        public bool Unlocatable;
        public bool SeeAllChars;
        public bool SeeItems;
        public bool SeeWallItems;
        public bool SeeTraps;
        public bool Unrecruitable;

        //miscellaneous traits
        public StateCollection<CharState> CharStates;

        //temporarily stores forced warp to prevent warp chains
        [NonSerialized]
        public List<Loc> WarpHistory;

        [NonSerialized]
        public Team MemberTeam;

        [NonSerialized]
        public int Proximity;

        public TempCharBackRef BackRef;

        public Character() : this(true)
        { }

        [JsonConstructor]
        public Character(bool populateSlots) : base(populateSlots)
        {
            Skills = new List<BackReference<Skill>>();
            if (populateSlots)
            {
                for (int ii = 0; ii < MAX_SKILL_SLOTS; ii++)
                    Skills.Add(new BackReference<Skill>(new Skill()));
            }

            Element1 = "";
            Element2 = "";

            Intrinsics = new List<BackReference<Intrinsic>>();
            if (populateSlots)
            {
                for (int ii = 0; ii < MAX_INTRINSIC_SLOTS; ii++)
                    Intrinsics.Add(new BackReference<Intrinsic>(new Intrinsic()));
            }

            EquippedItem = new InvItem();

            Proximity = -1;

            ProxyName = "";
            ProxySprite = MonsterID.Invalid;
            ProxyAtk = -1;
            ProxyDef = -1;
            ProxyMAtk = -1;
            ProxyMDef = -1;
            ProxySpeed = -1;
            CharStates = new StateCollection<CharState>();
            WarpHistory = new List<Loc>();
            StatusEffects = new Dictionary<string, StatusEffect>();
            IdleOverride = -1;
            currentCharAction = new EmptyCharAction(new CharAnimIdle(new Loc(), Dir8.Down));
            StatusesTargetingThis = new List<StatusRef>();
            TileSight = Map.SightRange.Any;
            CharSight = Map.SightRange.Any;

            BackRef = new TempCharBackRef(false, -1);
        }

        public Character(CharData baseChar)
            : this(baseChar, new Loc(), Dir8.Down) { }

        public Character(CharData baseChar, Loc newLoc, Dir8 charDir)
            : base(baseChar)
        {            
            CurrentForm = BaseForm;

            HP = MaxHP;

            MaxFullness = MAX_FULLNESS;
            Fullness = MaxFullness;

            Skills = new List<BackReference<Skill>>();
            for(int ii = 0; ii < BaseSkills.Count; ii++)
            {
                Skill newState = null;
                if (!String.IsNullOrEmpty(BaseSkills[ii].SkillNum))
                    newState = new Skill(BaseSkills[ii].SkillNum, DataManager.Instance.GetSkill(BaseSkills[ii].SkillNum).BaseCharges);
                else
                    newState = new Skill();
                Skills.Add(new BackReference<Skill>(newState, ii));
                BaseSkills[ii].Charges = newState.Charges;
            }

            Element1 = DataManager.Instance.GetMonster(CurrentForm.Species).Forms[CurrentForm.Form].Element1;
            Element2 = DataManager.Instance.GetMonster(CurrentForm.Species).Forms[CurrentForm.Form].Element2;

            Intrinsics = new List<BackReference<Intrinsic>>();
            for(int ii = 0; ii < BaseIntrinsics.Count; ii++)
                Intrinsics.Add(new BackReference<Intrinsic>(new Intrinsic(BaseIntrinsics[ii]), ii));

            EquippedItem = new InvItem();

            Proximity = -1;

            ProxyName = "";
            ProxySprite = MonsterID.Invalid;
            ProxyAtk = -1;
            ProxyDef = -1;
            ProxyMAtk = -1;
            ProxyMDef = -1;
            ProxySpeed = -1;
            CharStates = new StateCollection<CharState>();
            WarpHistory = new List<Loc>();
            StatusEffects = new Dictionary<string, StatusEffect>();
            IdleOverride = -1;
            currentCharAction = new EmptyCharAction(new CharAnimIdle(newLoc, charDir));
            StatusesTargetingThis = new List<StatusRef>();
            TileSight = Map.SightRange.Any;
            CharSight = Map.SightRange.Any;

            BackRef = new TempCharBackRef(false, -1);

            UpdateFrame();
        }

        public Character Clone(Team team)
        {
            CharData character = new CharData();
            character.BaseForm = this.BaseForm;
            character.Nickname = this.Nickname;
            character.Level = this.Level;
            character.MaxHPBonus = this.MaxHPBonus;
            character.AtkBonus = this.AtkBonus;
            character.DefBonus = this.DefBonus;
            character.MAtkBonus = this.MAtkBonus;
            character.MDefBonus = this.MDefBonus;
            character.SpeedBonus = this.SpeedBonus;

            for (int ii = 0; ii < CharData.MAX_SKILL_SLOTS; ii++)
                character.BaseSkills[ii] = new SlotSkill(this.BaseSkills[ii]);

            for (int ii = 0; ii < CharData.MAX_INTRINSIC_SLOTS; ii++)
                character.BaseIntrinsics[ii] = this.BaseIntrinsics[ii];

            Character new_mob = new Character(character);
            team.Players.Add(new_mob);

            new_mob.IdleOverride = IdleOverride;
            CharAnimIdle idleAction = new CharAnimIdle(this.CharLoc, this.CharDir);
            if (IdleOverride > -1)
                idleAction.Override = IdleOverride;
            new_mob.currentCharAction = new EmptyCharAction(idleAction);
            new_mob.Tactic = new AITactic(this.Tactic);
            new_mob.EquippedItem = new InvItem(this.EquippedItem);

            for (int ii = 0; ii < CharData.MAX_SKILL_SLOTS; ii++)
                new_mob.Skills[ii].Element.Enabled = this.Skills[ii].Element.Enabled;

            return new_mob;
        }

        public void OnRemove()
        {
            //remove all status from this character without saying anything
            List<string> keys = new List<string>();
            keys.AddRange(StatusEffects.Keys);
            for (int ii = 0; ii < keys.Count; ii++)
            {
                StatusEffect status = StatusEffects[keys[ii]];
                StatusData data = (StatusData)status.GetData();
                if (data.CarryOver)
                    continue;
                StatusEffects.Remove(keys[ii]);
                //need to remove the backreferences on their targets
                removeTargetStatusRef(status);
                //call all their OnTargetRemove one by one AFTER removal?
                //status.OnRemove(this, false);
            }

            //remove all backreferences to targeted status from this character
            while (StatusesTargetingThis.Count > 0)
            {
                StatusRef owner = StatusesTargetingThis[StatusesTargetingThis.Count - 1];
                //call all their OnTargetRemoved one by one?  Should we make an OnTargetRemoved event?
                StatusesTargetingThis.RemoveAt(StatusesTargetingThis.Count - 1);
                StatusEffect otherStatus = owner.TargetChar.GetStatusEffect(owner.ID);
                if (otherStatus != null)
                    otherStatus.TargetChar = null;
            }

            RestoreForm();
        }

        private List<int> baseRestore()
        {
            HP = MaxHP;
            HPRemainder = 0;

            Fullness = MaxFullness;
            FullnessRemainder = 0;

            //record changed skills
            List<int> skillIndices = new List<int>();
            bool[] turnOn = new bool[MAX_SKILL_SLOTS];
            for (int ii = 0; ii < MAX_SKILL_SLOTS; ii++)
            {
                skillIndices.Add(Skills[ii].BackRef);
                turnOn[ii] = Skills[ii].Element.Enabled;
            }
            //restore the old skills with old use count
            Skills.Clear();
            for (int ii = 0; ii < BaseSkills.Count; ii++)
            {
                Skill newState = null;
                if (!String.IsNullOrEmpty(BaseSkills[ii].SkillNum))
                {
                    int baseCharges = DataManager.Instance.GetSkill(BaseSkills[ii].SkillNum).BaseCharges + ChargeBoost;
                    BaseSkills[ii].Charges = baseCharges;
                    newState = new Skill(BaseSkills[ii].SkillNum, baseCharges, turnOn[ii]);
                }
                else
                    newState = new Skill();
                Skills.Add(new BackReference<Skill>(newState, ii));
            }

            Element1 = DataManager.Instance.GetMonster(CurrentForm.Species).Forms[CurrentForm.Form].Element1;
            Element2 = DataManager.Instance.GetMonster(CurrentForm.Species).Forms[CurrentForm.Form].Element2;

            Intrinsics.Clear();
            for (int ii = 0; ii < BaseIntrinsics.Count; ii++)
                Intrinsics.Add(new BackReference<Intrinsic>(new Intrinsic(BaseIntrinsics[ii]), ii));

            ProxyAtk = -1;
            ProxyDef = -1;
            ProxyMAtk = -1;
            ProxyMDef = -1;
            ProxySpeed = -1;

            StatusEffects = new Dictionary<string, StatusEffect>();

            return skillIndices;
        }

        public void FullRestore()
        {
            if (Dead)
            {
                Dead = false;
                DefeatAt = "";
            }

            List<int> skillIndices = baseRestore();

            OnSkillsChanged(skillIndices.ToArray());

            RefreshTraits();
        }

        public bool HasElement(string element)
        {
            return (element == Element1 || element == Element2);
        }

        public bool HasIntrinsic(string intrinsic)
        {
            foreach (BackReference<Intrinsic> checkIntrinsic in Intrinsics)
            {
                if (checkIntrinsic.Element.ID == intrinsic)
                    return true;
            }
            return false;
        }

        //for permanent change
        public override void Promote(MonsterID data)
        {
            base.Promote(data);
            RestoreForm();
        }

        //for transformations
        public void Transform(MonsterID formData)
        {
            //set form data to formdata
            CurrentForm = formData;

            //update typing
            Element1 = DataManager.Instance.GetMonster(CurrentForm.Species).Forms[CurrentForm.Form].Element1;
            Element2 = DataManager.Instance.GetMonster(CurrentForm.Species).Forms[CurrentForm.Form].Element2;

            //remap intrinsic to be the corresponding intrinsic of the new form
            MonsterData dex = DataManager.Instance.GetMonster(BaseForm.Species);
            BaseMonsterForm form = dex.Forms[BaseForm.Form];

            int prevIndex = 0;
            if (form.Intrinsic2 == BaseIntrinsics[0])
                prevIndex = 1;
            else if (form.Intrinsic3 == BaseIntrinsics[0])
                prevIndex = 2;

            MonsterData newDex = DataManager.Instance.GetMonster(CurrentForm.Species);
            BaseMonsterForm newForm = newDex.Forms[CurrentForm.Form];

            List<int> possibles = newForm.GetPossibleIntrinsicSlots();
            if (!possibles.Contains(prevIndex))
                prevIndex = 0;

            Intrinsics.Clear();
            if (prevIndex == 0)
                Intrinsics.Add(new BackReference<Intrinsic>(new Intrinsic(newForm.Intrinsic1)));
            else if (prevIndex == 1)
                Intrinsics.Add(new BackReference<Intrinsic>(new Intrinsic(newForm.Intrinsic2)));
            else if (prevIndex == 2)
                Intrinsics.Add(new BackReference<Intrinsic>(new Intrinsic(newForm.Intrinsic3)));

            //remove proxy stats
            ProxyAtk = -1;
            ProxyDef = -1;
            ProxyMAtk = -1;
            ProxyMDef = -1;
            ProxySpeed = -1;

            RefreshTraits();
        }

        public void RestoreForm()
        {
            CurrentForm = BaseForm;

            List<int> skillIndices = new List<int>();
            bool[] turnOn = new bool[MAX_SKILL_SLOTS];
            for (int ii = 0; ii < MAX_SKILL_SLOTS; ii++)
            {
                skillIndices.Add(Skills[ii].BackRef);
                turnOn[ii] = Skills[ii].Element.Enabled;
            }
            //restore the old skills with old use count
            Skills.Clear();
            for (int ii = 0; ii < BaseSkills.Count; ii++)
            {
                Skill newState = null;
                if (!String.IsNullOrEmpty(BaseSkills[ii].SkillNum))
                    newState = new Skill(BaseSkills[ii].SkillNum, BaseSkills[ii].Charges, turnOn[ii]);
                else
                    newState = new Skill();
                Skills.Add(new BackReference<Skill>(newState, ii));
            }

            Element1 = DataManager.Instance.GetMonster(CurrentForm.Species).Forms[CurrentForm.Form].Element1;
            Element2 = DataManager.Instance.GetMonster(CurrentForm.Species).Forms[CurrentForm.Form].Element2;

            Intrinsics.Clear();
            for (int ii = 0; ii < BaseIntrinsics.Count; ii++)
                Intrinsics.Add(new BackReference<Intrinsic>(new Intrinsic(BaseIntrinsics[ii]), ii));

            ProxyAtk = -1;
            ProxyDef = -1;
            ProxyMAtk = -1;
            ProxyMDef = -1;
            ProxySpeed = -1;

            OnSkillsChanged(skillIndices.ToArray());

            RefreshTraits();
        }

        public IEnumerator<YieldInstruction> UpdateFullness(bool combat)
        {
            int recovery = (combat ? 0 : 12);

            int residual = 0;
            if (MemberTeam == DungeonScene.Instance.ActiveTeam && MemberTeam.Leader == this)
            {
                residual = 80;
            }

            int prevFullness = Fullness;
            Fullness -= (residual + FullnessRemainder) / 1000;
            FullnessRemainder = (residual + FullnessRemainder) % 1000;

            if (MemberTeam == DungeonScene.Instance.ActiveTeam)
            {
                if (Fullness <= 0 && prevFullness > 0)
                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_HUNGER_EMPTY", GetDisplayName(true)));
                else if (Fullness <= 10 && prevFullness > 10)
                {
                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_HUNGER_CRITICAL", GetDisplayName(true)));
                    GameManager.Instance.SE(GraphicsManager.HungerSE);
                }
                else if (Fullness <= 20 && prevFullness > 20)
                {
                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_HUNGER_LOW", GetDisplayName(true)));
                    GameManager.Instance.SE(GraphicsManager.HungerSE);
                }
            }
            else
            {
                if (Fullness <= 0 && prevFullness > 0)
                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_HUNGER_EMPTY_FOE", GetDisplayName(false)));
            }

            if (Fullness <= 0)
            {
                Fullness = 0;
                FullnessRemainder = 0;

                if (MemberTeam == DungeonScene.Instance.ActiveTeam)
                    GameManager.Instance.SE(GraphicsManager.HungerSE);
                recovery = -60;
            }

            yield return CoroutineManager.Instance.StartCoroutine(ModifyHP(MaxHP * recovery));
        }

        public IEnumerator<YieldInstruction> ModifyHP(int residualHP)
        {
            OnModifyHP(ref residualHP);

            HP += (residualHP + HPRemainder) / 1000;
            HPRemainder = (residualHP + HPRemainder) % 1000;

            if (HP > MaxHP)
                HP = MaxHP;
            else if (HP < 0)
                HP = 0;

            if (HP == 0)
                yield return CoroutineManager.Instance.StartCoroutine(Die());
        }

        public IEnumerator<YieldInstruction> RestoreHP(int hp, bool anim = true)
        {
            if (Dead)
                yield break;

            OnRestoreHP(ref hp);

            if (hp != 0)
            {
                HP += hp;
                if (HP > MaxHP) HP = MaxHP;
                if (anim)
                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessBattleFX(this, this, DataManager.Instance.HealFX));

                if (!Unidentifiable)
                    DungeonScene.Instance.MeterChanged(CharLoc, hp, false);

                Loc? earshot = null;
                if (!anim)
                    earshot = CharLoc;
                DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_HP_RESTORE", GetDisplayName(false), hp), true, false, this, null);

                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(10, CharLoc));
            }
            yield break;
        }

        //find a way to check for death in this method, or check for death at certain specific points
        public IEnumerator<YieldInstruction> InflictDamage(int hp, bool anim = true, bool endure = false)
        {
            int takeHP = (hp < 0) ? HP : hp;
            Loc? earshot = null;
            if (!anim)
                earshot = CharLoc;
            if (takeHP == 0)
            {
                DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_DAMAGE_ZERO", GetDisplayName(false)), false, false, this, null);
                if (anim)
                    GameManager.Instance.SE(GraphicsManager.NullDmgSE);
                yield break;
            }

            HP -= takeHP;

            if (hp < 0)
                DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_DAMAGE_INFINITY", GetDisplayName(false)), false, false, this, null);
            else
            {
                if (!Unidentifiable)
                    DungeonScene.Instance.MeterChanged(CharLoc, -takeHP, false);
                DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_DAMAGE", GetDisplayName(false), takeHP), true, false, this, null);
            }

            int endureHP = endure ? 1 : 0;
            if (HP < endureHP)
            {
                if (endure)
                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_DAMAGE_ENDURE", GetDisplayName(false)), false, false, this, null);
                HP = endureHP;
            }


            //enqueue HP lost
            if (HP == 0)
                yield return CoroutineManager.Instance.StartCoroutine(Die());
            else if (anim)
            {
                CharAnimHurt hurtAnim = new CharAnimHurt();
                hurtAnim.CharLoc = CharLoc;
                hurtAnim.CharDir = CharDir;
                yield return CoroutineManager.Instance.StartCoroutine(this.StartAnim(hurtAnim));

                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(30, CharLoc));
            }
        }

        /// <summary>
        /// Makes the character disappear but also causes it to let go of its item.
        /// Technically should be handled by the scripts calling it.
        /// TODO: remove this method, after removing calls from rescue to script
        /// </summary>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> DieSilent()
        {
            HP = 0;
            Dead = true;


            //yield return CoroutineManager.Instance.StartCoroutine(OnDeath());

            //if (Dead)
            //{

            if (MemberTeam is ExplorerTeam)
            {

            }
            else
            {
                InvItem heldItem = EquippedItem;
                if (!String.IsNullOrEmpty(heldItem.ID))
                {
                    SilentDequipItem();
                    MapItem mapItem = new MapItem(heldItem);
                    yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.DropMapItem(mapItem, CharLoc, CharLoc, true));
                }
            }

            OnRemove();

            DefeatAt = ZoneManager.Instance.CurrentMap.GetColoredName();
            //}
        }

        public IEnumerator<YieldInstruction> Die()
        {
            SingleCharContext context = new SingleCharContext(this);
            yield return CoroutineManager.Instance.StartCoroutine(OnDeath(context));

            //TODO: refactor this code into OnDeath now that it can cancel properly   
            if (Dead)
            {
                if (MemberTeam is ExplorerTeam)
                {

                }
                else
                {
                    InvItem heldItem = EquippedItem;
                    if (!String.IsNullOrEmpty(heldItem.ID))
                    {
                        yield return CoroutineManager.Instance.StartCoroutine(DequipItem());
                        yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.DropItem(heldItem, CharLoc));
                    }
                }

                OnRemove();

                DefeatAt = ZoneManager.Instance.CurrentMap.GetColoredName();
                //DefeatDungeon = ZoneManager.Instance.CurrentZoneID;
                //DefeatFloor = ZoneManager.Instance.CurrentMapID;

            }
        }

        public IEnumerator<YieldInstruction> RestoreCharges(int charges)
        {
            yield return CoroutineManager.Instance.StartCoroutine(RestoreCharges(-1, charges, true, true));
        }

        public IEnumerator<YieldInstruction> RestoreCharges(int skillSlot, int charges, bool effect, bool declare)
        {
            for (int ii = 0; ii < Skills.Count; ii++)
            {
                if (!String.IsNullOrEmpty(Skills[ii].Element.SkillNum))
                {
                    if (skillSlot == ii || skillSlot == -1)
                    {
                        SkillData data = DataManager.Instance.GetSkill(Skills[ii].Element.SkillNum);
                        SetSkillCharges(ii, Math.Min(Skills[ii].Element.Charges + charges, data.BaseCharges + ChargeBoost));
                    }
                }
            }

            if (declare)
                DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_CHARGES_RESTORE", GetDisplayName(false)));

            if (effect)
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessBattleFX(this, this, DataManager.Instance.RestoreChargeFX));

        }

        public IEnumerator<YieldInstruction> DeductCharges(int skillSlot, int charges)
        {
            yield return CoroutineManager.Instance.StartCoroutine(DeductCharges(skillSlot, charges, true, true, true));
        }

        public IEnumerator<YieldInstruction> DeductCharges(int skillSlot, int charges, bool effect, bool declare, bool force)
        {
            if (ChargeSaver && force)
            {
                if (declare)
                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_CHARGES_LOST_ZERO", GetDisplayName(false)));
                yield break;
            }

            List<int> deductSlots = new List<int>();

            if (skillSlot > -1)
            {
                if (Skills[skillSlot].Element.Charges == 0)
                {
                    if (declare)
                        DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_CHARGES_LOST_NO_MORE", GetDisplayName(false), DataManager.Instance.GetSkill(Skills[skillSlot].Element.SkillNum).GetIconName()));
                    yield break;
                }
                SetSkillCharges(skillSlot, Math.Max(Skills[skillSlot].Element.Charges - charges, 0));
            }
            else
            {
                for (int ii = 0; ii < Skills.Count; ii++)
                {
                    if (!String.IsNullOrEmpty(Skills[ii].Element.SkillNum))
                    {
                        if (Skills[ii].Element.Charges > 0)
                        {
                            deductSlots.Add(ii);
                            SetSkillCharges(ii, Math.Max(Skills[ii].Element.Charges - charges, 0));
                        }
                    }
                }
                if (deductSlots.Count == 0)
                {
                    if (declare)
                        DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_CHARGES_LOST_ALL_NO_MORE", GetDisplayName(false)));
                    yield break;
                }
            }

            if (effect)
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessBattleFX(this, this, DataManager.Instance.LoseChargeFX));


            if (declare)
            {
                if (skillSlot == -1)
                {
                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_CHARGES_LOST_ALL", GetDisplayName(false), charges));
                    yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(30, CharLoc));

                    for (int ii = deductSlots.Count-1; ii >= 0; ii--)
                    {
                        if (Skills[deductSlots[ii]].Element.Charges > 0)
                            deductSlots.RemoveAt(ii);
                    }


                    if (deductSlots.Count > 0)
                    {
                        if (deductSlots.Count > 1)
                        {
                            string[] skillList = new string[deductSlots.Count];
                            for (int ii = 0; ii < deductSlots.Count; ii++)
                                skillList[ii] = DataManager.Instance.GetSkill(Skills[deductSlots[ii]].Element.SkillNum).GetIconName();
                            string skills = Text.BuildList(skillList);
                            DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_CHARGES_ZERO_ALL", GetDisplayName(false), skills));
                        }
                        else
                            DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_CHARGES_ZERO", GetDisplayName(false), DataManager.Instance.GetSkill(Skills[deductSlots[0]].Element.SkillNum).GetIconName()));

                        yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessEmoteFX(this, DataManager.Instance.NoChargeFX));
                    }
                }
                else
                {
                    if (Skills[skillSlot].Element.Charges == 0)
                    {
                        yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(30, CharLoc));
                        DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_CHARGES_ZERO", GetDisplayName(false), DataManager.Instance.GetSkill(Skills[skillSlot].Element.SkillNum).GetIconName()));

                        yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessEmoteFX(this, DataManager.Instance.NoChargeFX));
                    }
                    else
                    {
                        DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_CHARGES_LOST", GetDisplayName(false), DataManager.Instance.GetSkill(Skills[skillSlot].Element.SkillNum).GetIconName(), charges));
                        yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(30, CharLoc));
                    }
                }
            }
        }

        public void SetSkillCharges(int slot, int charges)
        {
            Skills[slot].Element.Charges = charges;
            if (Skills[slot].BackRef > -1)
                BaseSkills[Skills[slot].BackRef].Charges = Skills[slot].Element.Charges;
        }

        public StatusEffect GetStatusEffect(string id)
        {
            StatusEffect value;
            if (StatusEffects.TryGetValue(id, out value))
                return value;
            return null;
        }

        public IEnumerable<StatusEffect> IterateStatusEffects()
        {
            foreach (StatusEffect status in StatusEffects.Values)
                yield return status;
        }

        //TODO: method overloads that call other method overloads should just use "return" instead of "yield return"
        public IEnumerator<YieldInstruction> AddStatusEffect(StatusEffect status)
        {
            return AddStatusEffect(null, status, null, true);
        }
        public IEnumerator<YieldInstruction> AddStatusEffect(Character attacker, StatusEffect status, StateCollection<ContextState> parentStates, bool msg = true)
        {
            return AddStatusEffect(attacker, status, parentStates, msg, msg);
        }

        public IEnumerator<YieldInstruction> AddStatusEffect(Character attacker, StatusEffect status, StateCollection<ContextState> parentStates, bool checkmsg, bool msg)
        {
            StatusCheckContext context = new StatusCheckContext(attacker, this, status, checkmsg);
            if (parentStates != null)
            {
                foreach (ContextState state in parentStates)
                    context.ContextStates.Set(state.Clone<ContextState>());
            }

            yield return CoroutineManager.Instance.StartCoroutine(BeforeStatusCheck(context));
            if (context.CancelState.Cancel)
                yield break;
            context.msg = msg;

            yield return CoroutineManager.Instance.StartCoroutine(ExecuteAddStatus(context));

        }

        public IEnumerator<YieldInstruction> ExecuteAddStatus(StatusCheckContext context)
        {
            yield return CoroutineManager.Instance.StartCoroutine(RemoveStatusEffect(context.Status.ID, false));
            StatusEffects.Add(context.Status.ID, context.Status);

            RefreshTraits();

            if (context.Status.TargetChar != null)
            {
                context.Status.TargetChar.StatusesTargetingThis.Add(new StatusRef(context.Status.ID, this));
                context.Status.TargetChar.RefreshTraits();
            }
            //call all status's on add
            yield return CoroutineManager.Instance.StartCoroutine(OnAddStatus(context));

        }

        public void SilentRemoveStatus(string id)
        {
            StatusEffect statusToRemove;
            if (StatusEffects.TryGetValue(id, out statusToRemove))
            {
                StatusEffects.Remove(statusToRemove.ID);
                RefreshTraits();

                removeTargetStatusRef(statusToRemove);
            }
        }

        public IEnumerator<YieldInstruction> RemoveStatusEffect(string id, bool msg = true)
        {
            StatusEffect statusToRemove;
            if (StatusEffects.TryGetValue(id, out statusToRemove))
            {
                StatusCheckContext context = new StatusCheckContext(null, this, statusToRemove, msg);
                StatusEffects.Remove(statusToRemove.ID);
                RefreshTraits();

                removeTargetStatusRef(statusToRemove);
                //call all status's on remove
                yield return CoroutineManager.Instance.StartCoroutine(OnRemoveStatus(context));
            }
        }

        private void removeTargetStatusRef(StatusEffect status)
        {
            if (status.TargetChar == null)
                return;
            for (int ii = 0; ii < status.TargetChar.StatusesTargetingThis.Count; ii++)
            {
                StatusRef testRef = status.TargetChar.StatusesTargetingThis[ii];
                if (testRef.ID == status.ID && testRef.TargetChar == this)
                {
                    status.TargetChar.StatusesTargetingThis.RemoveAt(ii);
                    status.TargetChar.RefreshTraits();
                    return;
                }
            }
            throw new Exception("Could not find target status backreference.");
        }


        public void DeleteIntrinsic(int slot)
        {
            BaseIntrinsics.RemoveAt(slot);
            BaseIntrinsics.Add("");
            for (int ii = Intrinsics.Count; ii >= 0; ii--)
            {
                if (Intrinsics[ii].BackRef == slot)
                {
                    Intrinsics.RemoveAt(ii);
                    Intrinsics.Add(new BackReference<Intrinsic>(new Intrinsic(), slot));
                }
            }

            RefreshTraits();
        }

        public void LearnIntrinsic(string intrinsicNum, int slot = -1)
        {
            if (slot == -1)
            {
                for (int ii = 0; ii < MAX_INTRINSIC_SLOTS; ii++)
                {
                    if (String.IsNullOrEmpty(BaseIntrinsics[ii]))
                    {
                        slot = ii;
                        break;
                    }
                }
            }

            if (slot == -1)
                throw new Exception("No more room for intrinsics!");

            BaseIntrinsics[slot] = intrinsicNum;
            for (int ii = Intrinsics.Count - 1; ii >= 0; ii--)
            {
                if (Intrinsics[ii].BackRef == slot)
                    Intrinsics[ii].Element = new Intrinsic(BaseIntrinsics[slot]);
            }

            RefreshTraits();
        }


        public void ChangeSkill(int slot, string skillNum)
        {
            if (!String.IsNullOrEmpty(skillNum))
                Skills[slot] = new BackReference<Skill>(new Skill(skillNum, DataManager.Instance.GetSkill(skillNum).BaseCharges + ChargeBoost), -1);
            else
                Skills[slot] = new BackReference<Skill>(new Skill(), -1);

            RefreshTraits();
        }


        public void DeleteSkill(int slot)
        {

            BaseSkills.RemoveAt(slot);
            BaseSkills.Add(new SlotSkill());

            //update the backreferences to align with their original skills
            int owningSlot = -1;
            for (int ii = 0; ii < Skills.Count; ii++)
            {
                if (Skills[ii].BackRef > slot)
                    Skills[ii].BackRef--;
                else if (Skills[ii].BackRef == slot)
                    owningSlot = ii;
            }
            if (owningSlot > -1)
            {
                Skills.RemoveAt(owningSlot);
                Skills.Add(new BackReference<Skill>(new Skill(), MAX_SKILL_SLOTS - 1));

                List<int> skillIndices = new List<int>();
                for (int ii = 0; ii < MAX_SKILL_SLOTS; ii++)
                    skillIndices.Add(ii);

                skillIndices.RemoveAt(slot);
                skillIndices.Insert(slot, -1);

                OnSkillsChanged(skillIndices.ToArray());
            }
            RefreshTraits();
        }

        public void LearnSkill(string skillNum, bool enabled)
        {
            int newSlot = 0;
            foreach (SlotSkill skill in BaseSkills)
            {
                if (!String.IsNullOrEmpty(skill.SkillNum))
                    newSlot++;
            }
            ReplaceSkill(skillNum, newSlot, enabled);
        }
        public void ReplaceSkill(string skillNum, int newSlot, bool enabled)
        {
            List<int> skillIndices = baseReplaceSkill(skillNum, newSlot, enabled);

            OnSkillsChanged(skillIndices.ToArray());

            RefreshTraits();
        }

        private List<int> baseReplaceSkill(string skillNum, int newSlot, bool enabled)
        {
            List<int> skillIndices = new List<int>();
            for (int ii = 0; ii < MAX_SKILL_SLOTS; ii++)
                skillIndices.Add(ii);

            if (newSlot >= MAX_SKILL_SLOTS)
                return skillIndices;

            BaseSkills[newSlot] = new SlotSkill(skillNum);
            BaseSkills[newSlot].Charges = DataManager.Instance.GetSkill(skillNum).BaseCharges + ChargeBoost;
            Relearnables[skillNum] = true;

            int owningSlot = -1;
            for(int ii = 0; ii < Skills.Count; ii++)
            {
                if (Skills[ii].BackRef == newSlot)
                {
                    owningSlot = ii;
                    break;
                }
            }

            if (owningSlot != -1)
            {
                Skills[owningSlot] = new BackReference<Skill>(new Skill(skillNum, BaseSkills[newSlot].Charges, enabled), newSlot);
                skillIndices[owningSlot] = -1;
            }

            return skillIndices;
        }

        /// <summary>
        /// Editor-side skill changes.
        /// </summary>
        /// <param name="skillNum"></param>
        /// <param name="newSlot"></param>
        /// <param name="enabled"></param>
        public void EditSkill(string skillNum, int newSlot, bool enabled)
        {
            if (newSlot >= MAX_SKILL_SLOTS)
                return;

            BaseSkills[newSlot] = new SlotSkill(skillNum);
            if (!String.IsNullOrEmpty(skillNum))
                BaseSkills[newSlot].Charges = DataManager.Instance.GetSkill(skillNum).BaseCharges + ChargeBoost;

            int owningSlot = -1;
            for (int ii = 0; ii < Skills.Count; ii++)
            {
                if (Skills[ii].BackRef == newSlot)
                {
                    owningSlot = ii;
                    break;
                }
            }

            if (owningSlot != -1)
                Skills[owningSlot] = new BackReference<Skill>(new Skill(skillNum, BaseSkills[newSlot].Charges, enabled), newSlot);
        }

        public void SilentSwitchSkills(int slot)
        {
            BackReference<Skill> upState = Skills[slot];
            BackReference<Skill> downState = Skills[slot + 1];
            Skills[slot] = downState;
            Skills[slot + 1] = upState;

            if (upState.BackRef > -1 && downState.BackRef > -1)
            {
                int upRef = upState.BackRef;
                int downRef = downState.BackRef;
                SlotSkill upSkill = BaseSkills[upState.BackRef];
                SlotSkill downSkill = BaseSkills[downState.BackRef];
                BaseSkills[upState.BackRef] = downSkill;
                BaseSkills[downState.BackRef] = upSkill;
                upState.BackRef = downRef;
                downState.BackRef = upRef;
            }
        }

        public void SwitchSkills(int slot)
        {
            SilentSwitchSkills(slot);

            //need to switch relevant statuses around
            List<int> skillIndices = new List<int>();
            for (int ii = 0; ii < MAX_SKILL_SLOTS; ii++)
                skillIndices.Add(ii);

            skillIndices[slot] = slot + 1;
            skillIndices[slot + 1] = slot;

            OnSkillsChanged(skillIndices.ToArray());
            RefreshTraits();
        }

        public List<string> GetRelearnableSkills(bool includePreEvo)
        {
            List<BaseMonsterForm> entries = new List<BaseMonsterForm>();
            string evolutionStage = BaseForm.Species;
            MonsterData entryData;
            BaseMonsterForm entryForm;

            while (!string.IsNullOrEmpty(evolutionStage))
            {
                entryData = DataManager.Instance.GetMonster(evolutionStage);
                entryForm = entryData.Forms[BaseForm.Form];
                entries.Add(entryForm);
                evolutionStage = includePreEvo ? entryData.PromoteFrom : string.Empty;
            }

            List<string> forgottenSkills = new List<string>();
            foreach(BaseMonsterForm entry in entries)
            {
                foreach (string skill in entry.GetSkillsAtLevel(Level, true))
                {
                    bool hasSkill = false;
                    foreach (SlotSkill learnedSkill in BaseSkills)
                    {
                        if (learnedSkill.SkillNum == skill)
                        {
                            hasSkill = true;
                            break;
                        }
                    }
                    if (!hasSkill && !forgottenSkills.Contains(skill))
                        forgottenSkills.Add(skill);
                }
            }

            foreach (string key in Relearnables.Keys)
            {
                bool hasSkill = false;
                foreach (SlotSkill learnedSkill in BaseSkills)
                {
                    if (learnedSkill.SkillNum == key)
                    {
                        hasSkill = true;
                        break;
                    }
                }
                if (!hasSkill && !forgottenSkills.Contains(key))
                    forgottenSkills.Add(key);
            }
            return forgottenSkills;

            /*BaseMonsterForm entry = DataManager.Instance.GetMonster(BaseForm.Species).Forms[BaseForm.Form];
            List<string> forgottenSkills = new List<string>();
            foreach (string skill in entry.GetSkillsAtLevel(Level, true))
            {
                bool hasSkill = false;
                foreach (SlotSkill learnedSkill in BaseSkills)
                {
                    if (learnedSkill.SkillNum == skill)
                    {
                        hasSkill = true;
                        break;
                    }
                }
                if (!hasSkill && !forgottenSkills.Contains(skill))
                    forgottenSkills.Add(skill);
            }

            foreach(string key in Relearnables.Keys)
            {
                bool hasSkill = false;
                foreach (SlotSkill learnedSkill in BaseSkills)
                {
                    if (learnedSkill.SkillNum == key)
                    {
                        hasSkill = true;
                        break;
                    }
                }
                if (!hasSkill && !forgottenSkills.Contains(key))
                    forgottenSkills.Add(key);
            }
            return forgottenSkills;*/
        }

        public IEnumerator<YieldInstruction> ChangeElement(string element1, string element2, bool msg = true, bool vfx = true)
        {
            if (element1 == DataManager.Instance.DefaultElement && element2 != DataManager.Instance.DefaultElement)
            {
                element1 = element2;
                element2 = DataManager.Instance.DefaultElement;
            }

            bool equal1 = (Element1 == element1);
            bool equal2 = (Element2 == element2);
            Element1 = element1;
            Element2 = element2;

            if (msg)
            {
                ElementData type1Data = DataManager.Instance.GetElement(element1);
                ElementData type2Data = DataManager.Instance.GetElement(element2);
                if (element1 != DataManager.Instance.DefaultElement && element2 != DataManager.Instance.DefaultElement)
                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_ELEMENT_CHANGE_DUAL", GetDisplayName(false), type1Data.GetIconName(), type2Data.GetIconName()));
                else if (element1 != DataManager.Instance.DefaultElement)
                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_ELEMENT_CHANGE", GetDisplayName(false), type1Data.GetIconName()));
            }
            if (vfx)
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessBattleFX(this, this, DataManager.Instance.ElementFX));

            RefreshTraits();
        }

        public IEnumerator<YieldInstruction> ReplaceIntrinsic(int slot, string intrinsic, bool msg = true, bool vfx = true)
        {
            if (intrinsic == Intrinsics[slot].Element.ID)
            {
                if (msg)
                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_INTRINSIC_CHANGE_NONE", GetDisplayName(false)));
                yield break;
            }

            if (msg)
            {
                if (intrinsic != DataManager.Instance.DefaultIntrinsic)
                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_INTRINSIC_CHANGE_GAIN", GetDisplayName(false), DataManager.Instance.GetIntrinsic(intrinsic).GetColoredName()));
                else
                    DungeonScene.Instance.LogMsg(Text.FormatKey("MSG_INTRINSIC_CHANGE_LOST", GetDisplayName(false), DataManager.Instance.GetIntrinsic(Intrinsics[slot].Element.ID).GetColoredName()));
            }
            if (vfx)
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ProcessBattleFX(this, this, DataManager.Instance.ElementFX));

            Intrinsics[slot] = new BackReference<Intrinsic>(new Intrinsic(intrinsic));

            RefreshTraits();
        }
        
        public IEnumerator<YieldInstruction> EquipItem(InvItem item)
        {
            ItemCheckContext context = new ItemCheckContext(this, new MapItem(item), new MapItem(EquippedItem));

            SilentEquipItem(item);

            yield return CoroutineManager.Instance.StartCoroutine(OnEquip(context));
        }

        public void SilentEquipItem(InvItem item)
        {
            EquippedItem = item;
            RefreshTraits();
        }

        /// <summary>
        /// Removes a character's held item, effectively deleting it.
        /// </summary>
        public IEnumerator<YieldInstruction> DequipItem()
        {
            ItemCheckContext context = new ItemCheckContext(this, new MapItem(), new MapItem(EquippedItem));

            SilentDequipItem();

            yield return CoroutineManager.Instance.StartCoroutine(OnEquip(context));
        }

        public void SilentDequipItem()
        {
            EquippedItem = new InvItem();
            RefreshTraits();
        }

        //find a way to prevent repeated calls to this method in various other methods
        private void baseRefresh()
        {
            ProxyName = "";
            ProxySprite = MonsterID.Invalid;

            CantWalk = false;

            ItemDisabled = false;
            IntrinsicDisabled = false;
            CanRemoveStuck = false;
            StopItemAtHit = false;

            MovesScrambled = false;

            ChargeSaver = false;
            WaitToAttack = false;
            CantInteract = false;
            EnemyOfFriend = false;

            Unidentifiable = false;
            Unlocatable = false;
            SeeAllChars = false;
            SeeItems = false;
            SeeWallItems = false;
            SeeTraps = false;

            TileSight = Map.SightRange.Any;
            CharSight = Map.SightRange.Any;

            //Mobility
            Mobility = TerrainData.Mobility.Passable;

            CharStates.Clear();

            MovementSpeed = 0;

            ChargeBoost = 0;
            foreach (BackReference<Skill> skillState in Skills)
            {
                skillState.Element.Sealed = false;
            }
        }

        private void refreshProximity()
        {
            int oldProximity = Proximity;
            Proximity = -1;

            if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
            {
                foreach ((PassiveActive owner, ProximityPassive passive) proximityTuple in findProximityPassives())
                    Proximity = Math.Max(Proximity, proximityTuple.passive.ProximityEvent.Radius);
            }

            if (Proximity != oldProximity)
                MemberTeam?.ContainingMap?.ModifyCharProximity(this, oldProximity);
        }

        //should work in dungeon and ground modes (ground modes will have certain passives disabled, such as map effects/positional effects
        public void RefreshTraits()
        {
            TerrainData.Mobility oldMobility = Mobility;

            baseRefresh();

            refreshProximity();

            OnRefresh();

            maintainMaximums();

            if (Mobility != oldMobility)
                MemberTeam?.ContainingMap?.DisplacedChars.Add(this);
        }

        private void maintainMaximums()
        {
            for (int ii = 0; ii < Skills.Count; ii++)
            {
                if (!String.IsNullOrEmpty(Skills[ii].Element.SkillNum))
                {
                    int maxCharges = DataManager.Instance.GetSkill(Skills[ii].Element.SkillNum).BaseCharges + ChargeBoost;
                    //bring charges up to maximum if maximum is enforced
                    if (DataManager.Instance.Save != null && !DataManager.Instance.Save.MidAdventure)
                        SetSkillCharges(ii, maxCharges);

                    //cap off over-maximum values
                    if (Skills[ii].Element.Charges > maxCharges)
                        SetSkillCharges(ii, maxCharges);
                }
            }
            for (int ii = 0; ii < BaseSkills.Count; ii++)
            {
                if (!String.IsNullOrEmpty(BaseSkills[ii].SkillNum))
                {
                    int maxCharges = DataManager.Instance.GetSkill(BaseSkills[ii].SkillNum).BaseCharges + ChargeBoost;

                    //cap off over-maximum values
                    if (BaseSkills[ii].Charges > maxCharges)
                        BaseSkills[ii].Charges = maxCharges;
                }
            }
        }

        public IEnumerable<PassiveContext> IteratePassives(Priority defaultPortPriority)
        {
            bool dungeonMode = GameManager.Instance.CurrentScene == DungeonScene.Instance;
            if (dungeonMode)
            {
                //check map conditions
                foreach (MapStatus status in ZoneManager.Instance.CurrentMap.Status.Values)
                    yield return new PassiveContext(status, status.GetData(), defaultPortPriority, this);
            }

            //check statuses
            foreach (StatusEffect status in StatusEffects.Values)
                yield return new PassiveContext(status, status.GetData(), defaultPortPriority, this);
            foreach (StatusRef statusRef in StatusesTargetingThis)
                yield return new PassiveContext(statusRef.TargetChar.GetStatusEffect(statusRef.ID), statusRef.GetStatusEntry().TargetPassive, defaultPortPriority, statusRef.TargetChar);

            //check equipped item
            Dictionary<string, int> activeItems = new Dictionary<string, int>();
            if (!String.IsNullOrEmpty(EquippedItem.ID) && !ItemDisabled)
            {
                yield return new PassiveContext(EquippedItem, EquippedItem.GetData(), defaultPortPriority, this);
                activeItems.Add(EquippedItem.ID, BattleContext.EQUIP_ITEM_SLOT);
            }
            //check bag items
            if (!ItemDisabled && MemberTeam != null)
            {
                for (int ii = 0; ii < MemberTeam.GetInvCount(); ii++)
                {
                    ItemData itemData = DataManager.Instance.GetItem(MemberTeam.GetInv(ii).ID);
                    if (itemData.BagEffect)
                    {
                        if (!activeItems.ContainsKey(MemberTeam.GetInv(ii).ID))
                            activeItems.Add(MemberTeam.GetInv(ii).ID, ii);
                    }
                }

                foreach (string key in activeItems.Keys)
                {
                    if (activeItems[key] > BattleContext.EQUIP_ITEM_SLOT)
                        yield return new PassiveContext(MemberTeam.GetInv(activeItems[key]), MemberTeam.GetInv(activeItems[key]).GetData(), defaultPortPriority, this);
                }
            }
            //check intrinsic
            if (!IntrinsicDisabled)
            {
                foreach (BackReference<Intrinsic> intrinsic in Intrinsics)
				{
                    if (!String.IsNullOrEmpty(intrinsic.Element.ID))
                        yield return new PassiveContext(intrinsic.Element, intrinsic.Element.GetData(), defaultPortPriority, this);
				}
            }

            if (dungeonMode)
            {
                //iterate through all other characters' proximity effects which touch this character
                StablePriorityQueue<int, Character> charQueue = new StablePriorityQueue<int, Character>();
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateProximityCharacters(CharLoc))
                {
                    if (!character.Dead)
                        charQueue.Enqueue(-character.Speed, character);
                }
                int totalPriority = 0;

                while (charQueue.Count > 0)
                {
                    Character character = charQueue.Dequeue();
                    foreach (PassiveContext effect in character.IterateProximityPassives(this, CharLoc, new Priority(defaultPortPriority, totalPriority)))
                        yield return effect;
                    totalPriority++;
                }
            }
        }

        public IEnumerable<PassiveContext> IterateProximityPassives(Character character, Loc targetLoc, Priority portPriority)
        {
            foreach ((PassiveActive owner, ProximityPassive passive) proximityTuple in findProximityPassives())
            {
                if (ZoneManager.Instance.CurrentMap.InRange(this.CharLoc, targetLoc, proximityTuple.passive.ProximityEvent.Radius) &&
                    (DungeonScene.Instance.GetMatchup(character, this) & proximityTuple.passive.ProximityEvent.TargetAlignments) != Alignment.None)
                    yield return new PassiveContext(proximityTuple.owner, proximityTuple.passive.ProximityEvent, portPriority, this);
            }
        }
        private IEnumerable<(PassiveActive, ProximityPassive)> findProximityPassives()
        {
            //check all of their entries for proximity ranges; return them only if their ranges are above 0
            //whatever proximity passive class is used, it needs to somehow refer back to the character owning it
            //can the passive active be made up on the spot?

            //check map conditions
            foreach (MapStatus status in ZoneManager.Instance.CurrentMap.Status.Values)
            {
                ProximityPassive proximity = (ProximityPassive)status.GetData();
                if (proximity.ProximityEvent.Radius > -1)
                    yield return (status, proximity);
            }

            //check statuses
            foreach (StatusEffect status in StatusEffects.Values)
            {
                ProximityPassive proximity = (ProximityPassive)status.GetData();
                if (proximity.ProximityEvent.Radius > -1)
                    yield return (status, proximity);
            }

            //check eqipped item
            Dictionary<string, int> activeItems = new Dictionary<string, int>();
            if (!String.IsNullOrEmpty(EquippedItem.ID) && !ItemDisabled)
            {
                ProximityPassive proximity = (ProximityPassive)EquippedItem.GetData();
                if (proximity.ProximityEvent.Radius > -1)
                {
                    yield return (EquippedItem, proximity);
                    activeItems.Add(EquippedItem.ID, BattleContext.EQUIP_ITEM_SLOT);
                }
            }

            // check bag items
            if (!ItemDisabled && MemberTeam != null)
            {
                for (int ii = 0; ii < MemberTeam.GetInvCount(); ii++)
                {
                    ItemData itemData = DataManager.Instance.GetItem(MemberTeam.GetInv(ii).ID);
                    if (itemData.BagEffect && itemData.ProximityEvent.Radius > -1)
                    {
                        if (!activeItems.ContainsKey(MemberTeam.GetInv(ii).ID))
                            activeItems.Add(MemberTeam.GetInv(ii).ID, ii);
                    }
                }

                foreach (string key in activeItems.Keys)
                {
                    if (activeItems[key] > BattleContext.EQUIP_ITEM_SLOT)
                    {
                        InvItem invItem = MemberTeam.GetInv(activeItems[key]);
                        ItemData itemData = DataManager.Instance.GetItem(invItem.ID);
                        yield return (invItem, itemData);
                    }
                }
            }


            // check intrinsic
            if (!IntrinsicDisabled)
            {
                foreach (BackReference<Intrinsic> intrinsic in Intrinsics)
                {
                    if (!String.IsNullOrEmpty(intrinsic.Element.ID))
                    {
						ProximityPassive proximity = (ProximityPassive)intrinsic.Element.GetData();
						if (proximity.ProximityEvent.Radius > -1)
                            yield return (intrinsic.Element, proximity);
                    }
                }
            }

        }

        public IEnumerator<YieldInstruction> BeforeTryAction(BattleContext context)
        {
            DungeonScene.EventEnqueueFunction<BattleEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<BattleEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.BeforeTryActions, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.BeforeTryActions, this);
                //check the action's effects
                context.Data.AddEventsToQueue(queue, maxPriority, ref nextPriority, context.Data.BeforeTryActions, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, effectContext.EventData.BeforeTryActions, this);
            };
            foreach (EventQueueElement<BattleEvent> effect in DungeonScene.IterateEvents(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                if (context.CancelState.Cancel) yield break;
            }
        }

        public IEnumerator<YieldInstruction> BeforeAction(BattleContext context)
        {
            DungeonScene.EventEnqueueFunction<BattleEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<BattleEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.BeforeActions, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.BeforeActions, this);
                //check the action's effects
                context.Data.AddEventsToQueue(queue, maxPriority, ref nextPriority, context.Data.BeforeActions, this);

                    foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                        effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, effectContext.EventData.BeforeActions, this);
                };
            foreach (EventQueueElement<BattleEvent> effect in DungeonScene.IterateEvents(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                if (context.CancelState.Cancel) yield break;
            }
        }

        public IEnumerator<YieldInstruction> OnAction(BattleContext context)
        {
            DungeonScene.EventEnqueueFunction<BattleEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<BattleEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnActions, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnActions, this);
                //check the action's effects
                context.Data.AddEventsToQueue<BattleEvent>(queue, maxPriority, ref nextPriority, context.Data.OnActions, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue<BattleEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.OnActions, this);
            };
            foreach (EventQueueElement<BattleEvent> effect in DungeonScene.IterateEvents<BattleEvent>(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                if (context.CancelState.Cancel) yield break;
            }
        }

        public IEnumerator<YieldInstruction> AfterActionTaken(BattleContext context)
        {
            DungeonScene.EventEnqueueFunction<BattleEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<BattleEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.AfterActions, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.AfterActions, this);
                //check the action's effects
                context.Data.AddEventsToQueue(queue, maxPriority, ref nextPriority, context.Data.AfterActions, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, effectContext.EventData.AfterActions, this);
            };
            foreach (EventQueueElement<BattleEvent> effect in DungeonScene.IterateEvents(function))
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
        }

        public IEnumerator<YieldInstruction> HitTile(BattleContext context)
        {
            DungeonScene.EventEnqueueFunction<BattleEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<BattleEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnHitTiles, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnHitTiles, this);
                context.Data.AddEventsToQueue(queue, maxPriority, ref nextPriority, context.Data.OnHitTiles, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, effectContext.EventData.OnHitTiles, this);
            };
            foreach (EventQueueElement<BattleEvent> effect in DungeonScene.IterateEvents(function))
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
        }

        public void OnModifyHP(ref int hpChange)
        {
            try
            {
                DungeonScene.EventEnqueueFunction<HPChangeEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<HPChangeEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
                {
                    DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.ModifyHPs, this);
                    if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                        ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.ModifyHPs, this);

                    foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                        effectContext.AddEventsToQueue<HPChangeEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.ModifyHPs, this);
                };
                foreach (EventQueueElement<HPChangeEvent> effect in DungeonScene.IterateEvents<HPChangeEvent>(function))
                    effect.Event.Apply(effect.Owner, effect.OwnerChar, ref hpChange);

            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public void OnRestoreHP(ref int hpChange)
        {
            try
            {
                DungeonScene.EventEnqueueFunction<HPChangeEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<HPChangeEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
                {
                    DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.RestoreHPs, this);
                    if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                        ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.RestoreHPs, this);

                    foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                        effectContext.AddEventsToQueue<HPChangeEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.RestoreHPs, this);
                };
                foreach (EventQueueElement<HPChangeEvent> effect in DungeonScene.IterateEvents<HPChangeEvent>(function))
                    effect.Event.Apply(effect.Owner, effect.OwnerChar, ref hpChange);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        private void OnSkillsChanged(int[] skillIndices)
        {
            try
            {
                DungeonScene.EventEnqueueFunction<SkillChangeEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<SkillChangeEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
                {
                    //check statuses
                    foreach (StatusEffect status in StatusEffects.Values)
                        status.AddEventsToQueue<SkillChangeEvent>(queue, maxPriority, ref nextPriority, ((StatusData)status.GetData()).OnSkillChanges, this);
                };
                foreach (EventQueueElement<SkillChangeEvent> effect in DungeonScene.IterateEvents<SkillChangeEvent>(function))
                    effect.Event.Apply(effect.Owner, this, skillIndices);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        private void OnRefresh()
        {
            try
            {
                DungeonScene.EventEnqueueFunction<RefreshEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<RefreshEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
                {
                    DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnRefresh, this);
                    if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                        ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnRefresh, this);

                    foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                        effectContext.AddEventsToQueue<RefreshEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.OnRefresh, this);
                };
                foreach (EventQueueElement<RefreshEvent> effect in DungeonScene.IterateEvents<RefreshEvent>(function))
                    effect.Event.Apply(effect.Owner, effect.OwnerChar, this);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public IEnumerator<YieldInstruction> OnEquip(ItemCheckContext context)
        {
            DungeonScene.EventEnqueueFunction<ItemGivenEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<ItemGivenEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnEquips, this);
                if (GameManager.Instance.CurrentScene == DungeonScene.Instance)
                    ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnEquips, this);
                
                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, effectContext.EventData.OnEquips, this);
            };
            foreach (EventQueueElement<ItemGivenEvent> effect in DungeonScene.IterateEvents(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                if (context.CancelState.Cancel)
                    yield break;
            }
        }

        public IEnumerator<YieldInstruction> OnPickup(ItemCheckContext context)
        {
            DungeonScene.EventEnqueueFunction<ItemGivenEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<ItemGivenEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnPickups, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnPickups, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, effectContext.EventData.OnPickups, this);
            };
            foreach (EventQueueElement<ItemGivenEvent> effect in DungeonScene.IterateEvents(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                if (context.CancelState.Cancel)
                    yield break;
            }
        }



        /// <summary>
        /// Was once called independently as a part of DungeonScene.BeginFloor.
        /// Was refactored out to allow modifiable order between the map/game-wide events and character events.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> OnMapStart()
        {
            // should this be extended to the callers?
            SingleCharContext context = new SingleCharContext(this);
            DungeonScene.EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<SingleCharEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnMapStarts, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnMapStarts, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.OnMapStarts, this);
            };
            foreach (EventQueueElement<SingleCharEvent> effect in DungeonScene.IterateEvents<SingleCharEvent>(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                if (context.CancelState.Cancel)
                    yield break;
            }
        }

        public IEnumerator<YieldInstruction> OnTurnStart(SingleCharContext context)
        {
            DungeonScene.EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<SingleCharEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnTurnStarts, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnTurnStarts, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.OnTurnStarts, this);
            };
            foreach (EventQueueElement<SingleCharEvent> effect in DungeonScene.IterateEvents<SingleCharEvent>(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                if (context.CancelState.Cancel)
                    yield break;
            }
        }

        public IEnumerator<YieldInstruction> OnTurnEnd(SingleCharContext context)
        {
            DungeonScene.EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<SingleCharEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnTurnEnds, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnTurnEnds, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.OnTurnEnds, this);
            };
            foreach (EventQueueElement<SingleCharEvent> effect in DungeonScene.IterateEvents<SingleCharEvent>(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                if (context.CancelState.Cancel)
                    yield break;
            }
        }

        /// <summary>
        /// Currently not used.  Was once called independently as a part of DungeonScene.ProcessMapTurnEnd
        /// Was refactored out to allow modifiable order between the map/game-wide events and character events.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerator<YieldInstruction> OnMapTurnEnd(SingleCharContext context)
        {
            DungeonScene.EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<SingleCharEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnMapTurnEnds, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnMapTurnEnds, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.OnMapTurnEnds, this);
            };
            foreach (EventQueueElement<SingleCharEvent> effect in DungeonScene.IterateEvents<SingleCharEvent>(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                if (context.CancelState.Cancel)
                    yield break;
            }
        }

        public IEnumerator<YieldInstruction> OnWalk(SingleCharContext context)
        {
            DungeonScene.EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<SingleCharEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnWalks, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnWalks, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.OnWalks, this);
            };
            foreach (EventQueueElement<SingleCharEvent> effect in DungeonScene.IterateEvents<SingleCharEvent>(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                if (context.CancelState.Cancel)
                    yield break;
            }
        }

        public IEnumerator<YieldInstruction> OnDeath(SingleCharContext context)
        {
            DungeonScene.EventEnqueueFunction<SingleCharEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<SingleCharEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnDeaths, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnDeaths, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue<SingleCharEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.OnDeaths, this);
            };
            foreach (EventQueueElement<SingleCharEvent> effect in DungeonScene.IterateEvents<SingleCharEvent>(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                if (context.CancelState.Cancel)
                    yield break;
            }
        }

        public IEnumerator<YieldInstruction> BeforeStatusCheck(StatusCheckContext context)
        {
            DungeonScene.EventEnqueueFunction<StatusGivenEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<StatusGivenEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.BeforeStatusAdds, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.BeforeStatusAdds, this);
                //check pending status
                context.Status.AddEventsToQueue(queue, maxPriority, ref nextPriority, context.Status.GetData().BeforeStatusAdds, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue(queue, maxPriority, ref nextPriority, effectContext.EventData.BeforeStatusAdds, this);
            };
            foreach (EventQueueElement<StatusGivenEvent> effect in DungeonScene.IterateEvents(function))
            {
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
                if (context.CancelState.Cancel)
                    yield break;
            }
        }

        private IEnumerator<YieldInstruction> OnAddStatus(StatusCheckContext context)
        {
            DungeonScene.EventEnqueueFunction<StatusGivenEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<StatusGivenEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnStatusAdds, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnStatusAdds, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue<StatusGivenEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.OnStatusAdds, this);
            };
            foreach (EventQueueElement<StatusGivenEvent> effect in DungeonScene.IterateEvents<StatusGivenEvent>(function))
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
        }

        private IEnumerator<YieldInstruction> OnRemoveStatus(StatusCheckContext context)
        {
            DungeonScene.EventEnqueueFunction<StatusGivenEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<StatusGivenEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnStatusRemoves, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnStatusRemoves, this);

                //check removed status
                context.Status.AddEventsToQueue<StatusGivenEvent>(queue, maxPriority, ref nextPriority, context.Status.GetData().OnStatusRemoves, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue<StatusGivenEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.OnStatusRemoves, this);
            };
            foreach (EventQueueElement<StatusGivenEvent> effect in DungeonScene.IterateEvents<StatusGivenEvent>(function))
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
        }
        
        public IEnumerator<YieldInstruction> OnAddMapStatus(MapStatus status, bool msg)
        {
            DungeonScene.EventEnqueueFunction<MapStatusGivenEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<MapStatusGivenEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnMapStatusAdds, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnMapStatusAdds, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue<MapStatusGivenEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.OnMapStatusAdds, this);
            };
            foreach (EventQueueElement<MapStatusGivenEvent> effect in DungeonScene.IterateEvents<MapStatusGivenEvent>(function))
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, this, status, msg));
        }

        public IEnumerator<YieldInstruction> OnRemoveMapStatus(MapStatus status, bool msg)
        {
            DungeonScene.EventEnqueueFunction<MapStatusGivenEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<MapStatusGivenEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnMapStatusRemoves, this);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnMapStatusRemoves, this);

                //check removed status
                status.AddEventsToQueue<MapStatusGivenEvent>(queue, maxPriority, ref nextPriority, status.GetData().OnMapStatusRemoves, this);

                foreach (PassiveContext effectContext in IteratePassives(GameEventPriority.USER_PORT_PRIORITY))
                    effectContext.AddEventsToQueue<MapStatusGivenEvent>(queue, maxPriority, ref nextPriority, effectContext.EventData.OnMapStatusRemoves, this);
            };
            foreach (EventQueueElement<MapStatusGivenEvent> effect in DungeonScene.IterateEvents<MapStatusGivenEvent>(function))
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, this, status, msg));
        }


        //SIGHT LOGIC

        //either border of sight range, or border of the screen
        public static Loc GetSightDims()
        {
            int width = MathUtils.DivUp(GraphicsManager.ScreenWidth - GraphicsManager.TileSize, 2 * GraphicsManager.TileSize);
            int height = MathUtils.DivUp(GraphicsManager.ScreenHeight - GraphicsManager.TileSize, 2 * GraphicsManager.TileSize);
            return new Loc(width, height);
        }


        public Map.SightRange GetTileSight()
        {
            Map.SightRange sight = TileSight;
            if (sight == Map.SightRange.Any)
                sight = ZoneManager.Instance.CurrentMap.TileSight;
            return sight;
        }

        public Map.SightRange GetCharSight()
        {
            Map.SightRange sight = CharSight;
            if (sight == Map.SightRange.Any)
                sight = ZoneManager.Instance.CurrentMap.CharSight;
            return sight;
        }

        public void UpdateTileSight(Fov.LightOperation lightOp)
        {
            switch (GetTileSight())
            {
                case Map.SightRange.Blind:
                    {
                        lightOp(CharLoc.X, CharLoc.Y, 1f);
                        break;
                    }
                case Map.SightRange.Murky:
                    {
                        List<Loc> tiles = new List<Loc>();
                        for (int x = -1; x <= 1; x++)
                        {
                            for (int y = -1; y <= 1; y++)
                                lightOp(CharLoc.X + x, CharLoc.Y + y, 1f);
                        }
                        break;
                    }
                case Map.SightRange.Dark:
                    {
                        Loc seen = GetSightDims();
                        Rect sightBounds = Rect.FromPoints(CharLoc - seen, CharLoc + seen + Loc.One);
                        Fov.CalculateAnalogFOV(sightBounds.Start, sightBounds.Size, CharLoc, DungeonScene.Instance.VisionBlocked, lightOp);
                        break;
                    }
                default:
                    {
                        Loc seen = GetSightDims();
                        Rect sightBounds = Rect.FromPoints(CharLoc - seen, CharLoc + seen + Loc.One);
                        sightBounds = MemberTeam.ContainingMap.GetClampedSight(sightBounds);
                        for (int x = sightBounds.X; x < sightBounds.End.X; x++)
                        {
                            for (int y = sightBounds.Y; y < sightBounds.End.Y; y++)
                                lightOp(x, y, 1f);
                        }
                        break;
                    }
            }
        }


        /// <summary>
        /// Does not guarantee any order
        /// </summary>
        /// <param name="targetAlignment"></param>
        /// <returns></returns>
        public List<Character> GetSeenCharacters(Alignment targetAlignment)
        {
            List<Character> seenChars = new List<Character>();
            if (SeeAllChars)
            {
                foreach (Character target in ZoneManager.Instance.CurrentMap.IterateCharacters())
                {
                    if (DungeonScene.Instance.IsTargeted(this, target, targetAlignment, false))
                        seenChars.Add(target);
                }
            }
            else
            {
                //add members of the same team - can be seen no matter what
                foreach (Character target in MemberTeam.EnumerateChars())
                {
                    if (DungeonScene.Instance.IsTargeted(this, target, targetAlignment, false))
                        seenChars.Add(target);
                }

                Loc radius = GetSightDims();
                Rect sightBounds = Rect.FromPoints(CharLoc - radius, CharLoc + radius + Loc.One);
                sightBounds = MemberTeam.ContainingMap.GetClampedSight(sightBounds);

                //iterate through everyone in max sight range EXCEPT members of the same team
                foreach (Character target in ZoneManager.Instance.CurrentMap.GetCharsInRect(sightBounds))
                {
                    if (target.MemberTeam == MemberTeam)
                        continue;
                    if (DungeonScene.Instance.IsTargeted(this, target, targetAlignment, false) && CanSeeCharacter(target))
                        seenChars.Add(target);
                }
            }
            return seenChars;
        }

        public bool CanSeeCharacter(Character character)
        {
            return CanSeeCharacter(character, GetCharSight());
        }
        public bool CanSeeCharacter(Character character, Map.SightRange sight)
        {
            if (character == null)
                return false;

            if (SeeAllChars)
                return true;

            if (character.MemberTeam == this.MemberTeam)
                return true;

            if (character.Unlocatable)
                return false;

            if (CanSeeLoc(character.CharLoc, sight))
                return true;
            return false;
        }
        
        public IEnumerable<Loc> GetLocsVisible() { return currentCharAction.GetLocsVisible(); }
        public IEnumerable<VisionLoc> GetVisionLocs() { return currentCharAction.GetVisionLocs(); }

        public bool CanSeeLoc(Loc loc, Map.SightRange sight)
        {
            return CanSeeLocFromLoc(CharLoc, loc, sight);
        }
        public bool CanSeeLocFromLoc(Loc fromLoc, Loc toLoc, Map.SightRange sight)
        {
            //needs to be edited according to FOV
            switch (sight)
            {
                case Map.SightRange.Blind:
                    return false;
                case Map.SightRange.Murky:
                    {
                        Rect sightBounds = new Rect(fromLoc - Loc.One, Loc.One * 3);
                        return MemberTeam.ContainingMap.InBounds(sightBounds, toLoc);
                    }
                case Map.SightRange.Dark:
                    {
                        Loc seen = GetSightDims();
                        Rect sightBounds = new Rect(fromLoc - seen, seen * 2 + Loc.One);

                        foreach (Loc testLoc in MemberTeam.ContainingMap.IterateLocInBounds(sightBounds, toLoc))
                        {
                            if (Fov.IsInFOV(fromLoc, testLoc, DungeonScene.Instance.VisionBlocked))
                                return true;
                        }
                        return false;
                    }
                default:
                        return IsInSightBoundsFrom(fromLoc, toLoc);
            }
        }

        public bool IsInSightBounds(Loc loc)
        {
            return IsInSightBoundsFrom(CharLoc, loc);
        }

        public bool IsInSightBoundsFrom(Loc fromLoc, Loc loc)
        {
            Loc seen = GetSightDims();
            Rect sightBounds = new Rect(fromLoc - seen, seen * 2 + Loc.One);
            sightBounds = MemberTeam.ContainingMap.GetClampedSight(sightBounds);

            return MemberTeam.ContainingMap.InBounds(sightBounds, loc);
        }




        //DRAWING LOGIC

        const int STATUS_FRAME_LENGTH = 5;

        [NonSerialized]
        private CharAction currentCharAction;

        [NonSerialized]
        private Emote currentEmote;

        [NonSerialized]
        public int IdleOverride;

        public Loc CharLocFrom { get { return currentCharAction.CharLocFrom; } }
        public bool ActionDone { get { return currentCharAction.ActionDone; } }
        private Loc drawOffset { get { return currentCharAction.DrawOffset; } }
        /// <summary>
        /// Position on the map in pixels.
        /// </summary>
        public Loc MapLoc { get { return currentCharAction.MapLoc; } }
        public int LocHeight { get { return currentCharAction.LocHeight; } }

        private void updateLoc(Loc oldLoc)
        {
            if (oldLoc == CharLoc)
                return;
            //update location caches
            MemberTeam?.ContainingMap?.ModifyCharLookup(this, oldLoc);
            MemberTeam?.ContainingMap?.DisplacedChars.Add(this);
        }

        public void StartEmote(Emote emote)
        {
            currentEmote = emote;
        }
        
        public bool OccupiedwithAction()
        {
            return (!currentCharAction.ActionDone && currentCharAction.MajorAction && !Dead);
        }

        public IEnumerator<YieldInstruction> StartAnim(CharAnimation charAnim)
        {
            if (IdleOverride > -1)
            {
                CharAnimIdle idleAction = charAnim as CharAnimIdle;
                if (idleAction != null)
                    idleAction.Override = IdleOverride;
            }

            charAnim.SetLocWithoutVisual(MemberTeam.ContainingMap.WrapLoc(charAnim.CharLoc));
            if (OccupiedwithAction())
            {
                Loc preInterruptLoc = CharLoc;
                //if it's a major anim, it must wait.  put this on the top of the stack to be executed the moment it is available
                //ProcessInterruptingAnim can have 3 outcomes:
                //-It tells the new animation to wait until it is currently done
                //-It prevents the new animation from being executed but changes the current animation in some way (likely CharLoc)
                bool wait = currentCharAction.ProcessInterruptingAnim(charAnim);
                if (wait)
                    yield return new WaitWhile(OccupiedwithAction);
                else
                {
                    updateLoc(preInterruptLoc);
                    yield break;
                }
            }

            CharAction prevAction = currentCharAction;
            CharAction newCharAction = new EmptyCharAction(charAnim);
            newCharAction.PickUpFrom(Appearance.ToCharID(), currentCharAction);
            Loc oldLoc = CharLoc;
            currentCharAction = newCharAction;
            updateLoc(oldLoc);

            UpdateFrame();
        }

        public IEnumerator<YieldInstruction> PerformBattleAction(CombatAction action, BattleContext context)
        {
            yield return new WaitUntil(DungeonScene.Instance.AnimationsOver);
            yield return new WaitWhile(OccupiedwithAction);

            yield return CoroutineManager.Instance.StartCoroutine(action.OnIntro(this));
            action.BeginAction(this, currentCharAction);
            Loc oldLoc = CharLoc;
            currentCharAction = action;
            updateLoc(oldLoc);
            UpdateFrame();

            ZoneManager.Instance.CurrentMap.UpdateExploration(this);
            //wait until action is passed
            yield return new WaitUntil(() => { return (action.ActionPassed || Dead); });
            //then, since it's now on OnPass logic, release the hitboxes and calculate the hit times
            yield return CoroutineManager.Instance.StartCoroutine(action.ReleaseHitboxes(context, context.TargetTileWithExplosion, context.ProcessHitTile));
            //wait until the right amount of time has passed, then "hit" each target
            //the removal of the currentCharAction can be handled in this method itself.  should it?
            //perhaps all anims should be character actions?  or charactions contain anims,
            //and only charactions should be assigned to activechars
            yield return new WaitUntil(DungeonScene.Instance.AnimationsOver);
        }

        public IEnumerator<YieldInstruction> MockCharAction(SkillData skill)
        {
            MockActionContext context = new MockActionContext(this, skill.HitboxAction.Clone(), skill.Explosion, skill.Data);
            yield return CoroutineManager.Instance.StartCoroutine(MockCharAction(context.HitboxAction, context.TargetTileWithExplosion, context.ProcessHitTile));
        }

        public IEnumerator<YieldInstruction> MockCharAction(CombatAction action, DungeonScene.HitboxEffect effect, DungeonScene.HitboxEffect tileEffect)
        {
            GameManager.Instance.ForceReady();

            yield return new WaitUntil(DungeonScene.Instance.AnimationsOver);
            yield return new WaitWhile(OccupiedwithAction);

            MockActionContext context = new MockActionContext(this, action);

            yield return CoroutineManager.Instance.StartCoroutine(action.OnIntro(this));
            action.BeginAction(this, currentCharAction);
            Loc oldLoc = CharLoc;
            currentCharAction = action;
            updateLoc(oldLoc);
            UpdateFrame();

            ZoneManager.Instance.CurrentMap.UpdateExploration(this);
            //wait until action is passed
            yield return new WaitUntil(() => { return (action.ActionPassed || Dead); });
            //then, since it's now on OnPass logic, release the hitboxes and calculate the hit times
            yield return CoroutineManager.Instance.StartCoroutine(action.ReleaseHitboxes(context, effect, tileEffect));
            //wait until the right amount of time has passed, then "hit" each target
            //the removal of the currentCharAction can be handled in this method itself.  should it?
            //perhaps all anims should be character actions?  or charactions contain anims,
            //and only charactions should be assigned to activechars
            yield return new WaitUntil(DungeonScene.Instance.AnimationsOver);
        }

        //need to rethink the way actions are handled; maybe make them into their own classes
        //combine them such that char actions contain anims?
        public void Update(FrameTick elapsedTime)
        {
            if (Dead)
                return;

            currentCharAction.OnUpdate(elapsedTime, Appearance, MovementSpeed);

            if (currentCharAction.WantsToEnd())
            {
                CharAnimIdle idleAction = new CharAnimIdle(CharLoc, CharDir);
                if (IdleOverride > -1)
                    idleAction.Override = IdleOverride;
                EmptyCharAction action = new EmptyCharAction(idleAction);
                action.PickUpFrom(Appearance.ToCharID(), currentCharAction);
                currentCharAction = action;
            }

            UpdateFrame();

            if (currentEmote != null)
            {
                currentEmote.Update(DungeonScene.Instance, elapsedTime);
                if (currentEmote.Finished)
                    currentEmote = null;
            }

            currentCharAction.OnUpdateHitboxes(elapsedTime);
        }

        public void UpdateFrame()
        {
            currentCharAction.UpdateFrame();

            HashSet<DrawEffect> drawEffects = new HashSet<DrawEffect>();

            foreach (StatusEffect status in StatusEffects.Values)
                drawEffects.Add(((StatusData)status.GetData()).DrawEffect);

            if (Unidentifiable)
                drawEffects.Add(DrawEffect.Transparent);

            currentCharAction.UpdateDrawEffects(drawEffects);
        }

        public void DrawShadow(SpriteBatch spriteBatch, Loc offset, int terrainShadow)
        {
            CharSheet sheet = GraphicsManager.GetChara(Appearance.ToCharID());
            int teamStatus = 2;
            //if (DataManager.Instance.Save != null || !DataManager.Instance.Save.CutsceneMode)
            //{
                if (ZoneManager.Instance.CurrentMap.AllyTeams.Contains(MemberTeam))
                    teamStatus = 1;
                else if (MemberTeam == DungeonScene.Instance.ActiveTeam)
                {
                    if (DataManager.Instance.Save.TeamMode && DungeonScene.Instance.FocusedCharacter == this)
                    {
                        if (GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(15) % 2 == 0)
                            teamStatus = 0;
                    }
                    else
                        teamStatus = 0;
                }
            //}
            if (terrainShadow == 0)
                terrainShadow = sheet.ShadowSize;
            int animFrame = (int)(GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(5) % 3);
            Loc shadowType = new Loc(animFrame, teamStatus + terrainShadow * 3);
            Loc shadowPoint = currentCharAction.GetActionPoint(sheet, ActionPointType.Shadow);

            GraphicsManager.Shadows.DrawTile(spriteBatch,
                (shadowPoint - offset).ToVector2() - new Vector2(GraphicsManager.Shadows.TileWidth / 2, GraphicsManager.Shadows.TileHeight / 2),
                shadowType.X, shadowType.Y);
        }

        private void drawCross(SpriteBatch spriteBatch, Loc loc, Color color)
        {
            GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(loc.X - 2, loc.Y, 5, 1), null, color);
            GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(loc.X, loc.Y - 2, 1, 5), null, color);
        }

        public void DrawDebug(SpriteBatch spriteBatch, Loc offset)
        {
            CharSheet sheet = GraphicsManager.GetChara(Appearance.ToCharID());
            Loc center = currentCharAction.GetActionPoint(sheet, ActionPointType.Center);
            Loc head = currentCharAction.GetActionPoint(sheet, ActionPointType.Head);
            Loc leftHand = currentCharAction.GetActionPoint(sheet, ActionPointType.LeftHand);
            Loc rightHand = currentCharAction.GetActionPoint(sheet, ActionPointType.RightHand);

            drawCross(spriteBatch, head - offset, Color.Black);
            Color centerColor = new Color(0, 255, 0, 255);
            if (leftHand == center)
                centerColor = new Color(255, centerColor.G, centerColor.B, centerColor.A);
            else
                drawCross(spriteBatch, leftHand - offset, Color.Red);
            if (rightHand == center)
                centerColor = new Color(centerColor.R, centerColor.G, 255, centerColor.A);
            else
                drawCross(spriteBatch, rightHand - offset, Color.Blue);

            drawCross(spriteBatch, center - offset, centerColor);
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            CharSheet sheet = GraphicsManager.GetChara(Appearance.ToCharID());
            currentCharAction.Draw(spriteBatch, offset, sheet);

            if (currentEmote != null)
            {
                Loc head = currentCharAction.GetActionPoint(sheet, ActionPointType.Head);
                currentEmote.Draw(spriteBatch, offset - head - drawOffset);
            }
            else //if (DataManager.Instance.Save != null || !DataManager.Instance.Save.CutsceneMode)
            {
                List<string> icons = new List<string>();

                foreach (StatusEffect status in StatusEffects.Values)
                {
                    StatusData entry = (StatusData)status.GetData();
                    StackState stack = status.StatusStates.GetWithDefault<StackState>();
                    string emote = "";
                    if (stack != null && stack.Stack < 0)
                        emote = entry.DropEmoticon;
                    else
                        emote = entry.Emoticon;

                    if (emote != "")
                        icons.Add(emote);
                    if (entry.FreeEmote != "")
                    {
                        DirSheet iconSheet = GraphicsManager.GetIcon(entry.FreeEmote);
                        Loc animPos = new Loc(MapLoc.X + GraphicsManager.TileSize / 2 - iconSheet.TileWidth / 2,
                            MapLoc.Y + GraphicsManager.TileSize / 2 - iconSheet.TileHeight / 2 - LocHeight) - offset;
                        int currentFrame = (int)(GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(STATUS_FRAME_LENGTH) % (ulong)(iconSheet.TotalFrames));
                        iconSheet.DrawDir(spriteBatch, animPos.ToVector2(), currentFrame, CharDir);
                    }
                }

                if (icons.Count > 0)
                {
                    int framesPerIcon = (icons.Count == 1) ? GraphicsManager.GetIcon(icons[0]).TotalFrames : 24;
                    int frameTotal = (int)(GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(STATUS_FRAME_LENGTH) % (ulong)(icons.Count * framesPerIcon));

                    int currentIcon = frameTotal / framesPerIcon;

                    DirSheet iconSheet = GraphicsManager.GetIcon(icons[currentIcon]);

                    int currentFrame = frameTotal % iconSheet.TotalFrames;

                    Loc head = currentCharAction.GetActionPoint(sheet, ActionPointType.Head);
                    Loc frontDraw = head - offset - new Loc(0, GraphicsManager.TEX_SIZE * 2) - new Loc(iconSheet.TileWidth / 2, iconSheet.TileHeight / 2);

                    iconSheet.DrawDir(spriteBatch, frontDraw.ToVector2(), currentFrame);
                }
            }
        }


        public void GetCurrentSprite(out CharID currentForm, out Loc currentOffset, out int currentHeight, out int currentAnim, out int currentTime, out int currentFrame)
        {
            currentForm = Appearance.ToCharID();
            currentOffset = drawOffset;
            currentHeight = LocHeight;
            CharSheet sheet = GraphicsManager.GetChara(Appearance.ToCharID());
            currentCharAction.GetCurrentSprite(sheet, out currentAnim, out currentTime, out currentFrame);
        }

        public Loc GetDrawLoc(Loc offset)
        {
            return currentCharAction.GetDrawLoc(offset, GraphicsManager.GetChara(Appearance.ToCharID()));
        }

        public Loc GetDrawSize()
        {
            return new Loc(GraphicsManager.GetChara(Appearance.ToCharID()).TileWidth, GraphicsManager.GetChara(Appearance.ToCharID()).TileHeight);
        }

        //
        //Script Stuff
        //

        /// <summary>
        /// Cleanup references and etc from the script engine if needed
        /// </summary>
        public void DoCleanup()
        {
            //!TODO: Implement this when we add character specific events in dungeon mode
        }

        private Loc serializationLoc;
        private Dir8 serializationDir;

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            serializationLoc = CharLoc;
            serializationDir = CharDir;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //reinitialize empty lists
            StatusesTargetingThis = new List<StatusRef>();

            //restore idle position and direction
            currentCharAction = new EmptyCharAction(new CharAnimIdle(serializationLoc, serializationDir));
        }
    }
}

