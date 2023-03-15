using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Divides a given amount of money into a specified number of pickups.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MoneyDivSpawner<T> : IStepSpawner<T, MoneySpawn> 
        where T : ISpawningGenContext<MoneySpawn>
    {
        //amounts cannot be over this % greater or less than the base value
        const int DIV_DIFF = 50;

        /// <summary>
        /// The number of pickups to split the total sum of money into.
        /// </summary>
        public RandRange DivAmount;

        public MoneyDivSpawner() { }

        public MoneyDivSpawner(RandRange divAmount)
        {
            DivAmount = divAmount;
        }

        public List<MoneySpawn> GetSpawns(T map)
        {
            MoneySpawn total = map.Spawner.Pick(map.Rand);
            int chosenDiv = Math.Min(total.Amount, Math.Max(1, DivAmount.Pick(map.Rand)));
            int avgAmount = total.Amount / chosenDiv;
            int currentTotal = 0;
            List<MoneySpawn> results = new List<MoneySpawn>();
            for (int ii = 0; ii < chosenDiv; ii++)
            {
                int nextTotal = total.Amount;
                if (ii + 1 < chosenDiv)
                {
                    int expectedCurrentTotal = total.Amount * (ii+1) / chosenDiv;
                    int amount = avgAmount * (/*map.Rand.Next(DIV_DIFF * 2)*/((ii % 2 == 0) ? 0 : 99) - DIV_DIFF) / 200;
                    nextTotal = expectedCurrentTotal + amount;
                }
                if (nextTotal > currentTotal)
                    results.Add(new MoneySpawn(nextTotal - currentTotal));
                currentTotal = nextTotal;
            }
            return results;
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", this.GetType().GetFormattedTypeName(), this.DivAmount.ToString());
        }
    }
}
