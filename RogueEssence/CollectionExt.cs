using System;
using System.Collections.Generic;

namespace RogueEssence
{
    public static class CollectionExt
    {
        public delegate int CompareFunction<T>(T a, T b);

        public static void AddToSortedList<T>(List<T> list, T element, CompareFunction<T> compareFunc)
        {
            if (compareFunc == null)
                throw new ArgumentNullException(nameof(compareFunc));

            // stable
            int min = 0;
            int max = list.Count - 1;
            int point = max;
            int compare = -1;

            // binary search
            while (min <= max)
            {
                point = (min + max) / 2;

                compare = compareFunc(list[point], element);

                if (compare > 0)
                {
                    // go down
                    max = point - 1;
                }
                else if (compare < 0)
                {
                    // go up
                    min = point + 1;
                }
                else
                {
                    // go past the last index of equal comparison
                    point++;
                    while (point < list.Count && compareFunc(list[point], element) == 0)
                        point++;
                    list.Insert(point, element);
                    return;
                }
            }

            // no place found
            list.Insert(point + (compare > 0 ? 0 : 1), element);
        }

        public static void AssignExtendList<T>(List<T> list, int index, T element)
        {
            while (list.Count <= index)
                list.Add(default(T));
            list[index] = element;
        }

        public static T GetExtendList<T>(List<T> list, int index)
        {
            if (index < list.Count)
                return list[index];
            return default(T);
        }
    }
}
