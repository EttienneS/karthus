﻿using System;
using UnityEngine;

public class RedMana
{
    public static void BurnRed(float amount)
    {
    }

    public static void CastRed(float amount)
    {
    }

    public static void GainRed(float amount)
    {
    }

    public static Mana GetBase(float startingTotal = 0)
    {
        return new Mana(ManaColor.Red, CastRed, GainRed, BurnRed)
        {
            Total = startingTotal,
        };
    }
}
