using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class GameEvent : Dev.EditorData
    {
        public abstract GameEvent Clone();
    }

    
}

