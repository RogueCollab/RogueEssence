using System;
using System.Collections;
using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Selects a MoneySpawn with an amount in a predefined range.
    /// </summary>
    [Serializable]
    public struct MoneySpawnRange : IRandPicker<MoneySpawn>
    {
        public int Min;
        public int Max;
        public bool ChangesState { get { return false; } }
        public bool CanPick { get { return Min <= Max; } }

        public MoneySpawnRange(int num) { Min = num; Max = num; }
        public MoneySpawnRange(int min, int max) { Min = min; Max = max; }
        public MoneySpawnRange(RandRange other)
        {
            Min = other.Min;
            Max = other.Max;
        }
        public MoneySpawnRange(MoneySpawnRange other)
        {
            Min = other.Min;
            Max = other.Max;
        }
        public IRandPicker<MoneySpawn> CopyState() { return new MoneySpawnRange(this); }

        public IEnumerator<MoneySpawn> GetEnumerator()
        {
            yield return new MoneySpawn(Min);
            for (int ii = Min + 1; ii < Max; ii++)
                yield return new MoneySpawn(ii);
        }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public MoneySpawn Pick(IRandom rand) { return new MoneySpawn(rand.Next(Min, Max)); }
    }
}
