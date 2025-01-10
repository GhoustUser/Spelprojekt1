using System;
using System.Collections.Generic;
using System.Linq;

namespace Misc
{
    public static class ListUtils
    {
        /* ---- this function partially uses code from ChatGPT-4o ---- */
        public static List<T> GetHighestValues<T>(List<T> items, Func<T, T, float> comparison)
        {
            //if list is empty, return nothing
            if (items == null || items.Count == 0)
            {
                return new List<T>();
            }

            //find the highest value
            T highest = items[0];
            foreach (var item in items)
            {
                if (comparison(item, highest) > 0)
                {
                    highest = item;
                }
            }

            //return all items that are equal to the highest value
            return items.Where(item => comparison(item, highest) == 0).ToList();
        }
        /* ---- this function partially uses code from ChatGPT-4o ---- */
        public static List<int> GetHighestValuesIndices<T>(List<T> items, Func<T, T, float> comparison)
        {
            if (items == null || items.Count == 0)
            {
                return new List<int>();
            }

            //find the highest value
            T highest = items.Aggregate((current, next) => comparison(current, next) > 0 ? current : next);

            //find the indices of all items equal to the highest value
            return items
                .Select((item, index) => new { item, index }) // Pair items with their indices
                .Where(x => comparison(x.item, highest) == 0) // Keep only those equal to the highest
                .Select(x => x.index) // Extract indices
                .ToList();
        }

    }
}
