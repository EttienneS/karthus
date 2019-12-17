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

    public static Dictionary<ManaColor, float> AddPools(params Dictionary<ManaColor, float>[] pools)
    {
        var total = new Dictionary<ManaColor, float>();

        foreach (var pool in pools)
        {
            foreach (var kvp in pool)
            {
                if (!total.ContainsKey(kvp.Key))
                {
                    total.Add(kvp.Key, 0);
                }
                total[kvp.Key] += kvp.Value;
            }
        }

        return total;
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

    public static ManaPool ToManaPool(this Dictionary<ManaColor, float> manaCost, IEntity entity)
    {
        var pool = new ManaPool(entity);

        foreach (var kvp in manaCost)
        {
            pool.GainMana(kvp.Key, kvp.Value);
        }

        return pool;
    }

    public static Dictionary<ManaColor, float> ToManaValue(this ManaPool manaPool)
    {
        var dict = new Dictionary<ManaColor, float>();
        foreach (var kvp in manaPool)
        {
            dict.Add(kvp.Key, kvp.Value.Total);
        }

        return dict;
    }

    public static float GetTotal(this Dictionary<ManaColor, float> manaCost, ManaColor color)
    {
        if (manaCost.ContainsKey(color))
        {
            return manaCost[color];
        }
        return 0;
    }

    public static void GainMana(this Dictionary<ManaColor, float> manaCost, ManaColor color, float amount)
    {
        if (!manaCost.ContainsKey(color))
        {
            manaCost.Add(color, 0);
        }
        manaCost[color] += amount;
    }

    public static void BurnMana(this Dictionary<ManaColor, float> manaCost, ManaColor color, float amount)
    {
        if (!manaCost.ContainsKey(color) || manaCost[color] < amount)
        {
            throw new System.Exception("Cannot burn what you do not have");
        }

        manaCost[color] -= amount;
    }

    public static string GetString(this Dictionary<ManaColor, float> manaCost, int count = 1)
    {
        var str = "";
        foreach (var kvp in manaCost)
        {
            switch (kvp.Key)
            {
                case ManaColor.Black:
                    str += "B:";
                    break;

                case ManaColor.Blue:
                    str += "U:";
                    break;

                case ManaColor.Red:
                    str += "R:";
                    break;

                case ManaColor.White:
                    str += "W:";
                    break;

                case ManaColor.Green:
                    str += "G:";
                    break;
            }
            str += kvp.Value * count + ", ";
        }

        return str.Trim(',');
    }

    internal static ManaColor GetManaWithMost(this Dictionary<ManaColor, float> manaCost)
    {
        var most = ManaColor.Blue;
        var max = float.MinValue;

        foreach (var kvp in manaCost)
        {
            if (kvp.Value > max)
            {
                most = kvp.Key;
                max = kvp.Value;
            }
        }
        return most;
    }

    internal static void Transfer(this Dictionary<ManaColor, float> source, Dictionary<ManaColor, float> target, ManaColor manaColor, float amount)
    {
        if (amount > 0)
        {
            source.BurnMana(manaColor, amount);
            target.GainMana(manaColor, amount);
        }
        else
        {
            target.BurnMana(manaColor, amount);
            source.GainMana(manaColor, amount);
        }
    }

    internal static bool HasMana(this Dictionary<ManaColor, float> source, Dictionary<ManaColor, float> manaCost)
    {
        foreach (var kvp in manaCost)
        {
            if (!source.HasMana(kvp.Key, kvp.Value))
            {
                return false;
            }
        }

        return true;
    }

    internal static bool HasMana(this Dictionary<ManaColor, float> source, ManaColor color, float amount)
    {
        if (!source.ContainsKey(color))
        {
            return false;
        }

        return source[color] >= amount;
    }
}