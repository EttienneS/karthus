using System;
using UnityEngine;

public class BlueMana
{
    public static void BurnBlue(float amount)
    {
    }

    public static void CastBlue(float amount)
    {
    }

    public static void GainBlue(float amount)
    {
    }

    public static Mana GetBase(float startingTotal = 0)
    {
        return new Mana(ManaColor.Blue, CastBlue, GainBlue, BurnBlue)
        {
            Total = startingTotal,
        };
    }
}
