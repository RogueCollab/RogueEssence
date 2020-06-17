using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public struct MoneySpawn : ISpawnable
    {
        public int Amount;

        public MoneySpawn(int amount)
        {
            Amount = amount;
        }

        public MoneySpawn(MoneySpawn other)
        {
            Amount = other.Amount;
        }

        public ISpawnable Copy() { return new MoneySpawn(this); }
    }
}
