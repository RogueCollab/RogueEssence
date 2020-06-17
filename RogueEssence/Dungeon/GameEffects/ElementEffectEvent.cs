using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class ElementEffectEvent : GameEvent
    {
        public abstract void Apply(GameEventOwner owner, Character ownerChar, int attacking, int defending, ref int effectiveness);
    }

}
