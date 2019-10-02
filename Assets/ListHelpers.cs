using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public static class ListHelpers
{
    public static T GetRandomItem<T>(this IEnumerable<T> list)
    {
        return list.ElementAt(Random.Range(0, list.Count() - 1));
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
