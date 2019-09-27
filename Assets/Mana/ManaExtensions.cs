using System;
using System.Collections.Generic;
using UnityEngine;

public static class ManaExtensions
{
    public static Color GetActualColorFromString(string colorName, float alpha)
    {
        var color = new Color(1f, 0.9f, 0.15f);
        switch (colorName)
        {
            case "Red":
                color = GetActualColor(ManaColor.Red);
                break;
            case "Green":
                color = GetActualColor(ManaColor.Green);
                break;
            case "Blue":
                color = GetActualColor(ManaColor.Blue);
                break;
            case "Black":
                color = GetActualColor(ManaColor.Black);
                break;
            case "White":
                color = GetActualColor(ManaColor.White);
                break;
        }
        color.a = alpha + 0.1f;
        return color;
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
                return new Color(0.3f, 0.3f, 0.3f);

            case ManaColor.White:
                return Color.white;

            case ManaColor.Red:
                return Color.red;

            default:
                throw new NotImplementedException();
        }
    }

    public static List<ManaColor> ToFlatArray(this Dictionary<ManaColor, int> manaPool)
    {
        var flat = new List<ManaColor>();

        foreach (var kvp in manaPool)
        {
            for (var i = 0; i < kvp.Value; i++)
            {
                flat.Add(kvp.Key);
            }
        }

        return flat;
    }

    public static ManaPool ToManaPool(this Dictionary<ManaColor, int> manaCost, IEntity entity)
    {
        var pool = new ManaPool(entity);

        foreach (var kvp in manaCost)
        {
            pool.GainMana(kvp.Key, kvp.Value);
        }

        return pool;
    }
}