using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using NLua;
using RogueEssence.Data;
using RogueEssence.Dev;

namespace RogueEssence.Dungeon
{

    [Serializable]
    public class CharData
    {

        public static int MAX_SKILL_SLOTS = 4;
        public static int MAX_INTRINSIC_SLOTS = 1;

        public string Nickname;
        public string BaseName
        {
            get
            {
                string name = Nickname;
                if (String.IsNullOrEmpty(Nickname))
                    name = DataManager.Instance.GetMonster(BaseForm.Species).Name.ToLocal();

                return name;
            }
        }

        public string OriginalUUID;
        public string OriginalTeam;

        public static string GetFullFormName(MonsterID id)
        {
            string name = DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(id.Species).Name.ToLocal();
            SkinData data = DataManager.Instance.GetSkin(id.Skin);
            if (data.Symbol != '\0')
                name = data.Symbol + name;
            if (id.Gender != Gender.Genderless)
            {
                char genderChar = (id.Gender == Gender.Male) ? '\u2642' : '\u2640';
                if (name[name.Length - 1] != genderChar)
                    name += genderChar;
            }
            return name;
        }


        public MonsterID BaseForm;

        public int Level;
        public int EXP;

        //permanent stat boosts
        public int MaxHPBonus;
        public int AtkBonus;
        public int DefBonus;
        public int MAtkBonus;
        public int MDefBonus;
        public int SpeedBonus;

        public List<SlotSkill> BaseSkills;

        [JsonConverter(typeof(IntrinsicListConverter))]
        public List<string> BaseIntrinsics;
        private int _intrinsicSlot = -1;
        public int FormIntrinsicSlot {
            get
            {
                if (_intrinsicSlot<0)
                {
                    if (BaseForm.Species == null)
                        return 0;
                    _intrinsicSlot = GetFormIntrinsicSlot(BaseIntrinsics[0]);
                }
                return _intrinsicSlot;
            }
            set => _intrinsicSlot = Math.Max(0, Math.Min(value, 3));
        }

        [JsonConverter(typeof(RelearnableConverter))]
        public Dictionary<string, bool> Relearnables;

        public int Discriminator;

        public string MetAt;
        public ZoneLoc MetLoc;
        public string DefeatAt;
        public ZoneLoc DefeatLoc;

        /// <summary>
        /// Cannot be removed from assembly.
        /// </summary>
        public bool IsFounder;

        /// <summary>
        /// Cannot be removed from team.
        /// </summary>
        public bool IsPartner;

        public bool NameLocked;

        public bool IsFavorite;

        public bool Unrecruitable;

        public List<BattleEvent> ActionEvents;

        [JsonConverter(typeof(Dev.ScriptVarsConverter))]
        public Script.LuaTableContainer ScriptVars;
        [NonSerialized]
        public LuaTable LuaDataTable;


        public CharData() : this(true)
        {
            BaseForm = new MonsterID(DataManager.Instance.DefaultMonster, 0, DataManager.Instance.DefaultSkin, Gender.Genderless);
        }

        [JsonConstructor]
        public CharData(bool populateSlots)
        {
            Nickname = "";
            OriginalUUID = "";
            OriginalTeam = "";
            BaseSkills = new List<SlotSkill>();
            if (populateSlots)
            {
                for (int ii = 0; ii < MAX_SKILL_SLOTS; ii++)
                    BaseSkills.Add(new SlotSkill());
            }
            BaseIntrinsics = new List<string>();
            if (populateSlots)
            {
                for (int ii = 0; ii < MAX_INTRINSIC_SLOTS; ii++)
                    BaseIntrinsics.Add("");
            }
            Relearnables = new Dictionary<string, bool>();

            MetAt = "";
            MetLoc = ZoneLoc.Invalid;
            DefeatAt = "";
            DefeatLoc = ZoneLoc.Invalid;
            ActionEvents = new List<BattleEvent>();
            LuaDataTable = Script.LuaEngine.Instance.RunString("return {}").First() as LuaTable;
        }

        public CharData(CharData other)
        {
            //clean variables
            Nickname = other.Nickname;
            BaseForm = other.BaseForm;
            Level = other.Level;

            MaxHPBonus = other.MaxHPBonus;
            AtkBonus = other.AtkBonus;
            DefBonus = other.DefBonus;
            MAtkBonus = other.MAtkBonus;
            MDefBonus = other.MDefBonus;
            SpeedBonus = other.SpeedBonus;
            Unrecruitable = other.Unrecruitable;

            BaseSkills = new List<SlotSkill>();
            foreach (SlotSkill skill in other.BaseSkills)
                BaseSkills.Add(new SlotSkill(skill));
            BaseIntrinsics = new List<string>();
            BaseIntrinsics.AddRange(other.BaseIntrinsics);
            Relearnables = new Dictionary<string, bool>();
            foreach (string key in other.Relearnables.Keys)
                Relearnables[key] = other.Relearnables[key];

            OriginalUUID = other.OriginalUUID;
            OriginalTeam = other.OriginalTeam;
            MetAt = other.MetAt;
            MetLoc = other.MetLoc;
            DefeatAt = other.DefeatAt;
            DefeatLoc = other.DefeatLoc;
            IsFounder = other.IsFounder;
            IsFavorite = other.IsFavorite;
            Discriminator = other.Discriminator;

            ActionEvents = new List<BattleEvent>();
            foreach (BattleEvent effect in other.ActionEvents)
                ActionEvents.Add((BattleEvent)effect.Clone());
            //TODO: deep copy?
            LuaDataTable = other.LuaDataTable;
        }


        public bool HasBaseIntrinsic(string intrinsic)
        {
            foreach (string checkIntrinsic in BaseIntrinsics)
            {
                if (checkIntrinsic == intrinsic)
                    return true;
            }
            return false;
        }

        public bool HasBaseSkill(string skill)
        {
            foreach (SlotSkill checkSkill in BaseSkills)
            {
                if (checkSkill.SkillNum == skill)
                    return true;
            }
            return false;
        }

        public int GetFormIntrinsicSlot(string intrinsic)
        {
            if (BaseForm.Species == null)
                return 0;

            MonsterData dex = DataManager.Instance.GetMonster(BaseForm.Species);
            BaseMonsterForm form = dex.Forms[BaseForm.Form];

            int index = _intrinsicSlot;
            if (form.Intrinsic1 == intrinsic)
                index = 0;
            else if (form.Intrinsic2 == intrinsic)
                index = 1;
            else if (form.Intrinsic3 == intrinsic)
                index = 2;
            else if (index<0)
                index = DataManager.Instance.Save.Rand.Next(0, 3);
            return index;
        }

        public virtual void Promote(MonsterID data)
        {
            int newIndex = FormIntrinsicSlot;

            BaseForm = data;
            BaseIntrinsics.Clear();
            
            MonsterData newDex = DataManager.Instance.GetMonster(BaseForm.Species);
            BaseMonsterForm newForm = newDex.Forms[BaseForm.Form];

            List<int> possibles = newForm.GetPossibleIntrinsicSlots();
            if (!possibles.Contains(newIndex))
                newIndex = 0;

            if (newIndex == 0)
                BaseIntrinsics.Add(newForm.Intrinsic1);
            else if (newIndex == 1)
                BaseIntrinsics.Add(newForm.Intrinsic2);
            else if (newIndex == 2)
                BaseIntrinsics.Add(newForm.Intrinsic3);
        }

        public void SaveLua()
        {
            ScriptVars = Script.LuaEngine.Instance.SaveLuaTable(LuaDataTable);
        }

        public void LoadLua()
        {
            if (ScriptVars == null)
            {
                LuaDataTable = Script.LuaEngine.Instance.RunString("return {}").First() as LuaTable;
                return;
            }

            LuaDataTable = Script.LuaEngine.Instance.LoadLuaTable(ScriptVars);
            if (LuaDataTable == null)
            {
                //Make sure thers is at least a table in the data table when done deserializing.
                DiagManager.Instance.LogInfo(String.Format("CharData.LoadLua(): Couldn't deserialize LuaDataTable string '{0}'!", ScriptVars));
                LuaDataTable = Script.LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            }
        }


        public override string ToString()
        {
            return BaseForm.Species;
        }
    }
}
