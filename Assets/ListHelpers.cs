using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public static class ListHelpers
{
    public static T GetRandomItem<T>(this IEnumerable<T> list)
    {
        return list.ElementAt(Random.Range(0, list.Count() - 1));
    }
}
