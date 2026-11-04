using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace IntellectualGame.Core
{
    public static class ListExtensions
    {
        /// <summary>Fisher-Yates shuffle in place.</summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
