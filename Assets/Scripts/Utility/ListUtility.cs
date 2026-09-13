using System;
using System.Collections.Generic;

namespace BabyBanjo.Core.Utility
{
    public class ListUtility
    {
        public static List<T> FisherYatesShuffle<T>(List<T> unshuffled)
        {
            Random r = new Random();
            List<T> shuffled = new List<T>(unshuffled);
            //Step 1: For each unshuffled item in the collection
            for (int n = shuffled.Count - 1; n > 0; --n)
            {
                //Step 2: Randomly pick an item which has not been shuffled
                int k = r.Next(n + 1);

                //Step 3: Swap the selected item with the last "unstruck" letter in the collection
                T temp = shuffled[n];
                shuffled[n] = shuffled[k];
                shuffled[k] = temp;
            }
            return shuffled;
        }
    }
}