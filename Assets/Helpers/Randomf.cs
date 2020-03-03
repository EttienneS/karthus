using System;

public static class Randomf
{
    private static Random _rand = new Random();

    public static float Range(int min, int max)
    {
        return _rand.Next(min, max);
    }

    internal static int Roll(int max)
    {
        return _rand.Next(0, max) + 1;
    }
}