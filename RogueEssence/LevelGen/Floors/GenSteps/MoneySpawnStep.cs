using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MoneySpawnStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        public MoneySpawnRange MoneyRange;

        public MoneySpawnStep() { }
        public MoneySpawnStep(RandRange moneyAmount) { MoneyRange = new MoneySpawnRange(moneyAmount); }

        public override void Apply(T map)
        {
            map.MoneyAmount = (MoneySpawnRange)MoneyRange.CopyState();
        }
    }
}
