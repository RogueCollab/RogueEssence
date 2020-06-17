using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class SkillChangeEvent : GameEvent
    {
        public abstract void Apply(GameEventOwner owner, Character character, int[] skillIndices);
    }

}
