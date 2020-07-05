using System;
using Random = UnityEngine.Random;

public static class RandomHelper
{
    internal static int Roll(int max)
    {
        return Random.Range(0, max) + 1;
    }

    public static T RandomEnumValue<T>()
    {
        var v = Enum.GetValues(typeof(T));
        return (T)v.GetValue(new System.Random().Next(0, v.Length - 1));
    }
}