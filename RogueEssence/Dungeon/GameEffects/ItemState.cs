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
    public class ItemIDState : ItemState
    {
        public string ID;
        public ItemIDState() { ID = ""; }
        public ItemIDState(string idx) { ID = idx; }
        protected ItemIDState(ItemIDState other) { ID = other.ID; }
        public override GameplayState Clone() { return new ItemIDState(this); }
    }


    [Serializable]
    public class MaterialState : ItemState
    {
        public override GameplayState Clone() { return new MaterialState(); }
    }

}
