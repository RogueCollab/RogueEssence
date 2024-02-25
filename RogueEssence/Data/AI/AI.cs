using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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

        [Dev.Multiline(0)]
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new AIEntrySummary(Name, Released, Comment, Assignable); }

        [JsonConverter(typeof(Dev.AIConverter))]
        public string ID;

        /// <summary>
        /// Can be assigned via tactics menu
        /// </summary>
        public bool Assignable;
        public List<BasePlan> Plans;

        [NonSerialized]
        protected BasePlan currentPlan;
        
        public GameAction GetNextMove(Character controlledChar, bool preThink, List<Character> waitingChars, IRandom rand)
        {
            foreach (BasePlan plan in Plans)
            {
                GameAction result = AttemptPlan(controlledChar, plan, preThink, waitingChars, rand);
                if (result != null)
                    return result;
            }

            currentPlan = null;
            return new GameAction(GameAction.ActionType.Wait, Dir8.None);
        }

        protected GameAction AttemptPlan(Character controlledChar, BasePlan plan, bool preThink, List<Character> waitingChars, IRandom rand)
        {
            if ((currentPlan != null) && (currentPlan.GetType() == plan.GetType()))
                return currentPlan.Think(controlledChar, preThink, rand, waitingChars);
            else
            {
                plan.SwitchedIn(currentPlan);
                GameAction result = plan.Think(controlledChar, preThink, rand, waitingChars);
                if (result != null)
                    currentPlan = plan;
                return result;
            }
        }

        public AITactic()
        {
            ID = "";
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

        /// <summary>
        /// Gets the next move of an AI controlled character
        /// </summary>
        /// <param name="controlledChar">The character this AI is controlling</param>
        /// <param name="rand">Random object</param>
        /// <param name="preThink">Determines whether this is a preliminary decision or a final decision. All AI first make a preliminary decision to determine if there are conflicts (such as wanting to move to a square already occupied). Those conflicts are resolved my re-ordering the characters' actions, and then asking them each to decide on a move again (preThink = false).  Pre-think often ignores ally NPCs in the way as choosing to move into them signals a desire for them to move.</param>
        /// <param name="waitingChars">During pre-think, represents the entire chain of allies that are waiting for this character to move.</param>
        /// <returns></returns>
        public GameAction GetAction(Character controlledChar, IRandom rand, bool preThink, List<Character> waitingChars)
        {
            try
            {
                return GetNextMove(controlledChar, preThink, waitingChars, rand);
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

