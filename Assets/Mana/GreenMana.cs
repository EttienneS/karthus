using System;
using UnityEngine;

public class GreenMana
{
    public static void BurnGreen(float amount)
    {
    }

    public static void CastGreen(float amount)
    {
    }

    public static void GainGreen(float amount)
    {
    }

    public static Mana GetBase(float startingTotal = 0)
    {
        return new Mana(ManaColor.Green, CastGreen, GainGreen, BurnGreen)
        {
            Total = startingTotal,
        };
    }
}
