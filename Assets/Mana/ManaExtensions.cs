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

    internal static Dictionary<ManaColor, int> GetCostPool(params (ManaColor, int)[] requiredPool)
    {
        var pool = new Dictionary<ManaColor, int>();

        foreach (var v in requiredPool)
        {
            pool.Add(v.Item1, v.Item2);
        }

        return pool;
    }

    public static Color GetActualColor(this ManaColor manaColor, float alpha = 1f)
    {
        var color = new Color(0, 0, 0, alpha);
        switch (manaColor)
        {
            case ManaColor.Green:
                color = new Color(0, 0.6f, 0);
                break;

            case ManaColor.Blue:
                color = Color.blue;
                break;

            case ManaColor.Black:
                color = new Color(0.3f, 0.3f, 0.3f);
                break;

            case ManaColor.White:
                color = Color.white;
                break;

            case ManaColor.Red:
                color = Color.red;
                break;
        }

        color.a = Mathf.Max(alpha, 0.1f);

        return color;
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