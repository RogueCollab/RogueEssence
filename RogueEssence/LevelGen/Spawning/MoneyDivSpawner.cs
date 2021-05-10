using System;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MoneyDivSpawner<T> : IStepSpawner<T, MoneySpawn> 
        where T : ISpawningGenContext<MoneySpawn>
    {
        //amounts cannot be over this % greater or less than the base value
        const int DIV_DIFF = 50;

        public RandRange DivAmount;

        public MoneyDivSpawner() { }

        public MoneyDivSpawner(RandRange divAmount)
        {
            DivAmount = divAmount;
        }

        public List<MoneySpawn> GetSpawns(T map)
        {
            MoneySpawn total = map.Spawner.Pick(map.Rand);
            int chosenDiv = Math.Max(1, DivAmount.Pick(map.Rand));
            int avgAmount = total.Amount / chosenDiv;
            int currentTotal = 0;
            List<MoneySpawn> results = new List<MoneySpawn>();
            for (int ii = 0; ii < chosenDiv; ii++)
            {
                int nextTotal = total.Amount;
                if (ii < chosenDiv)
                {
                    int expectedCurrentTotal = total.Amount * ii / chosenDiv;
                    int amount = avgAmount * (200 + map.Rand.Next(DIV_DIFF * 2) - DIV_DIFF) / 200;
                    nextTotal = expectedCurrentTotal + amount;
                }
                results.Add(new MoneySpawn(nextTotal - currentTotal));
                currentTotal = nextTotal;
            }
            return results;
        }

        public override string ToString()
        {
            return string.Format("{0}: Div:{1}", this.GetType().Name, this.DivAmount.ToString());
        }
    }
}
