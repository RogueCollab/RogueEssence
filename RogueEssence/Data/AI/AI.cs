using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.Data
{


    [Serializable]
    public class AITactic : IEntryData
    {
        public override string ToString()
        {
            return Name.ToLocal();
        }

        public LocalText Name { get; set; }
        public bool Released { get; set; }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new AIEntrySummary(Name, Released, Comment, Assignable); }


        public int ID;

        /// <summary>
        /// Can be assigned via tactics menu
        /// </summary>
        public bool Assignable;
        public List<BasePlan> Plans;

        [NonSerialized]
        protected BasePlan currentPlan;
        
        public GameAction GetNextMove(Character controlledChar, bool preThink, ReRandom rand)
        {
            foreach (BasePlan plan in Plans)
            {
                GameAction result = AttemptPlan(controlledChar, plan, preThink, rand);
                if (result != null)
                    return result;
            }

            currentPlan = null;
            return new GameAction(GameAction.ActionType.Wait, Dir8.None);
        }

        protected GameAction AttemptPlan(Character controlledChar, BasePlan plan, bool preThink, ReRandom rand)
        {
            if ((currentPlan != null) && (currentPlan.GetType() == plan.GetType()))
                return currentPlan.Think(controlledChar, preThink, rand);
            else
            {
                plan.SwitchedIn();
                GameAction result = plan.Think(controlledChar, preThink, rand);
                if (result != null)
                    currentPlan = plan;
                return result;
            }
        }

        public AITactic()
        {
            Name = new LocalText();
            Comment = "";
            Plans = new List<BasePlan>();
        }

        public AITactic(AITactic other) : this()
        {
            ID = other.ID;
            Assignable = other.Assignable;
            Name = new LocalText(other.Name);
            Comment = other.Comment;
            foreach (BasePlan plan in other.Plans)
                Plans.Add(plan.CreateNew());
        }

        public void Initialize(Character controlledChar)
        {
            foreach (BasePlan plan in Plans)
                plan.Initialize(controlledChar);
        }

        public GameAction GetAction(Character controlledChar, ReRandom rand, bool preThink)
        {
            try
            {
                return GetNextMove(controlledChar, preThink, rand);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new Exception("AI Error\n", ex));
            }
            return new GameAction(GameAction.ActionType.Wait, Dir8.None);
        }

        public string GetColoredName()
        {
            return String.Format("{0}", Name.ToLocal());
        }
    }



    [Serializable]
    public class AIEntrySummary : EntrySummary
    {
        public bool Assignable;

        public AIEntrySummary() : base()
        {

        }

        public AIEntrySummary(LocalText name, bool released, string comment, bool assignable)
            : base(name, released, comment)
        {
            Assignable = assignable;
        }


        public override string GetColoredName()
        {
            return String.Format("{0}", Name.ToLocal());
        }
    }
}

