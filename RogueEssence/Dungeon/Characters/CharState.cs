using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class CharState : GameplayState
    {

    }


    [Serializable]
    public abstract class ModGenState : CharState
    {
        public int Mod;

        public ModGenState() { }
        public ModGenState(int mod) { Mod = mod; }

        protected ModGenState(ModGenState other) { Mod = other.Mod; }
    }


}
