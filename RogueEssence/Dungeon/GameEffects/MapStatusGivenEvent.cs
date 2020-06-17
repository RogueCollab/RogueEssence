using System;
using System.Collections.Generic;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class MapStatusGivenEvent : GameEvent
    {
        public abstract IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, Character character, MapStatus status, bool msg);
    }

}