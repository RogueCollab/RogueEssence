using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class ElementEffectEvent : GameEvent
    {
        public abstract void Apply(GameEventOwner owner, Character ownerChar, string attacking, string defending, ref int effectiveness);
    }

}
