// <copyright file="CategorySpawnChooser.cs" company="Audino">
// Copyright (c) Audino
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueElements
{
    // TODO: probably make this more generic; this is made specifically for item categories in maps at present.
    [Serializable]
    public class CategorySpawnChooser<T> : IRandPicker<T>
    {
        public SpawnDict<string, SpawnList<T>> Spawns;

        public CategorySpawnChooser()
        {
            Spawns = new SpawnDict<string, SpawnList<T>>();
        }
        public CategorySpawnChooser(SpawnDict<string, SpawnList<T>> spawns)
        {
            Spawns = spawns;
        }
        public CategorySpawnChooser(CategorySpawnChooser<T> other)
        {
            Spawns = new SpawnDict<string, SpawnList<T>>();
            foreach (string key in other.Spawns.GetKeys())
            {
                SpawnList<T> list = new SpawnList<T>();
                SpawnList<T> otherList = other.Spawns.GetSpawn(key);
                for (int ii = 0; ii < otherList.Count; ii++)
                    list.Add(otherList.GetSpawn(ii), otherList.GetSpawnRate(ii));
                Spawns.Add(key, list, other.Spawns.GetSpawnRate(key));
            }
        }

        public IEnumerable<T> EnumerateOutcomes()
        {
            foreach (SpawnList<T> element in Spawns.EnumerateOutcomes())
            {
                foreach (T item in element.EnumerateOutcomes())
                    yield return item;
            }
        }

        public T Pick(IRandom rand)
        {
            SpawnDict<string, SpawnList<T>> tempSpawn = new SpawnDict<string, SpawnList<T>>();
            foreach (string key in Spawns.GetKeys())
            {
                SpawnList<T> otherList = Spawns.GetSpawn(key);
                if (!otherList.CanPick)
                    continue;
                tempSpawn.Add(key, otherList, Spawns.GetSpawnRate(key));
            }
            SpawnList<T> choice = tempSpawn.Pick(rand);
            return choice.Pick(rand);
        }

        public bool ChangesState => false;
        public bool CanPick
        {
            get
            {
                if (!Spawns.CanPick)
                    return false;
                foreach (SpawnList<T> spawn in Spawns.EnumerateOutcomes())
                {
                    if (spawn.CanPick)
                        return true;
                }
                return false;
            }
        }

        public IRandPicker<T> CopyState()
        {
            return new CategorySpawnChooser<T>(this);
        }
    }
}
