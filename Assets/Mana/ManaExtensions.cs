using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ManaExtensions
{
    public static Color GetActualColor(this ManaColor manaColor)
    {
        switch (manaColor)
        {
            case ManaColor.Green:
                return new Color(0, 0.6f, 0);

            case ManaColor.Blue:
                return Color.blue;

            case ManaColor.Black:
                return Color.black;

            case ManaColor.White:
                return Color.white;

            case ManaColor.Red:
                return Color.red;

            default:
                throw new NotImplementedException();
        }
    }

    public static Mana GetBaseMana(ManaColor color, int startAmount)
    {
        switch (color)
        {
            case ManaColor.Red:
                return RedMana.GetBase(startAmount);

            case ManaColor.Green:
                return GreenMana.GetBase(startAmount);

            case ManaColor.Blue:
                return BlueMana.GetBase(startAmount);

            case ManaColor.Black:
                return BlackMana.GetBase(startAmount);

            case ManaColor.White:
                return WhiteMana.GetBase(startAmount);

            default:
                throw new KeyNotFoundException();
        }
    }

    public static void GainMana(this Dictionary<ManaColor, Mana> manaPool, ManaColor color, int amount)
    {
        if (!manaPool.ContainsKey(color))
        {
            manaPool.Add(color, GetBaseMana(color, amount));
        }
        else
        {
            manaPool[color].Total += amount;
        }
    }

    public static void BurnMana(this Dictionary<ManaColor, Mana> manaPool, ManaColor color, int amount)
    {
        if (!manaPool.ContainsKey(color))
        {
            throw new System.Exception("Unable to burn mana you do not have!");
        }
        else
        {
            if (manaPool[color].Total < amount)
            {
                throw new System.Exception("Not enough mana!");
            }
            manaPool[color].Burn(amount);
        }
    }

}
