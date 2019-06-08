using System;
using UnityEngine;

public class BlackMana
{
    public static void BurnBlack(float amount)
    {
    }

    public static void CastBlack(float amount)
    {
    }

    public static void GainBlack(float amount)
    {
    }

    public static Mana GetBase(float startingTotal = 0)
    {
        return new Mana(ManaColor.Black, CastBlack, GainBlack, BurnBlack)
        {
            Total = startingTotal,
        };
    }
}
