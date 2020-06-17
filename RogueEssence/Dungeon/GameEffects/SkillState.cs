using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class SkillState : GameplayState
    {

    }
    [Serializable]
    public class BasePowerState : SkillState
    {
        public int Power;
        public BasePowerState() { }
        public BasePowerState(int power) { Power = power; }
        protected BasePowerState(BasePowerState other) { Power = other.Power; }
        public override GameplayState Clone() { return new BasePowerState(this); }
    }
}
