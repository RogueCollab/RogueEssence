using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class RefreshEvent : GameEvent
    {
        public abstract void Apply(GameEventOwner owner, Character ownerChar, Character character);
    }

}
