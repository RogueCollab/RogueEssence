using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class GameEvent
    {
        public abstract GameEvent Clone();
    }
}
