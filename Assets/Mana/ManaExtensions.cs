using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ManaExtensions
{
    public static int ManaCount(this Dictionary<ManaColor, Mana> manaPool)
    {
        var count = 0;
        foreach (var kvp in manaPool)
        {
            count+= kvp.Value.Total;
        }

        return count;
    }

    public static ManaColor GetRandomManaColorFromPool(this Dictionary<ManaColor, Mana> manaPool)
    {
        var tmp = manaPool.Where(k => k.Value.Total > 0).Select(k => k.Key).Distinct().ToList();
        return tmp[Mathf.FloorToInt(Random.value * tmp.Count)];
    }

    public static Dictionary<ManaColor, Mana> ToManaPool(this Dictionary<ManaColor, int> manaCost)
    {
        var pool = new Dictionary<ManaColor, Mana>();

        foreach (var kvp in manaCost)
        {
            pool.GainMana(kvp.Key, kvp.Value);            
        }

        return pool;
    }

    public static List<ManaColor> ToFlatArray(this Dictionary<ManaColor, int> manaPool)
    {
        var flat = new List<ManaColor>();

        foreach (var kvp in manaPool)
        {
            for (var i = 0; i < kvp.Value;i++)
            {
                flat.Add(kvp.Key);
            }
        }

        return flat;
    }

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
            throw new Exception("Unable to burn mana you do not have!");
        }
        else
        {
            if (manaPool[color].Total < amount)
            {
                throw new Exception("Not enough mana!");
            }
            manaPool[color].Burn(amount);
        }
    }

}
