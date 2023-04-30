using System;
using RogueElements;
using RogueEssence.Dev;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Determines the total amount of money that will be spawned on the whole floor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MoneySpawnStep<T> : GenStep<T>, IMoneySpawnStep
        where T : BaseMapGenContext
    {
        [SubGroup]
        public MoneySpawnRange MoneyRange { get; set; }

        public MoneySpawnStep() { }
        public MoneySpawnStep(RandRange moneyAmount) { MoneyRange = new MoneySpawnRange(moneyAmount); }

        public override void Apply(T map)
        {
            map.MoneyAmount = (MoneySpawnRange)MoneyRange.CopyState();
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", this.GetType().GetFormattedTypeName(), MoneyRange.ToString());
        }
    }

    public interface IMoneySpawnStep
    {
        MoneySpawnRange MoneyRange { get; set; }
    }
}
