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
}