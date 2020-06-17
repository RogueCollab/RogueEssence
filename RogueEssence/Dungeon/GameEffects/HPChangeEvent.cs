using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class HPChangeEvent : GameEvent
    {
        public abstract void Apply(GameEventOwner owner, Character ownerChar, bool gradual, ref int hpChange);
    }

}
