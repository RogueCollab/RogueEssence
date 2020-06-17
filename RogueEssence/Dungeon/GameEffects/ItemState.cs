using System;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class ItemState : GameplayState
    {

    }


    [Serializable]
    public class ItemIndexState : ItemState
    {

        public int Index;
        public ItemIndexState() { }
        public ItemIndexState(int idx) { Index = idx; }
        protected ItemIndexState(ItemIndexState other) { Index = other.Index; }
        public override GameplayState Clone() { return new ItemIndexState(this); }
    }


    [Serializable]
    public class MaterialState : ItemState
    {
        public override GameplayState Clone() { return new MaterialState(); }
    }

}
