using System;
using RogueElements;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MoneySpawnStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        [SubGroup]
        public MoneySpawnRange MoneyRange;

        public MoneySpawnStep() { }
        public MoneySpawnStep(RandRange moneyAmount) { MoneyRange = new MoneySpawnRange(moneyAmount); }

        public override void Apply(T map)
        {
            map.MoneyAmount = (MoneySpawnRange)MoneyRange.CopyState();
        }

        public override string ToString()
        {
            return String.Format("{0}: Amt:{1}", this.GetType().Name, MoneyRange.ToString());
        }
    }
}
