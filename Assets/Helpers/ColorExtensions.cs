using System;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ColorExtensions
{
    public static Color FromFloatArrayString(this string color)
    {
        var parts = color.Split(',');
        return new Color(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
    }

    public static Color GetRandomColor(float alpha = 1.0f)
    {
        return new Color(Random.value, Random.value, Random.value, alpha);
    }

    public static Color ToColor(this float[] arr)
    {
        return new Color(arr[0], arr[1], arr[2], arr[3]);
    }

    public static float[] ToFloatArray(this Color color)
    {
        return new[] { color.r, color.g, color.b, color.a };
    }

    public static string ToFloatArrayString(this Color color)
    {
        return $"{color.r},{color.g},{color.b},{color.a}";
    }

    internal static Color GetColorFromHex(this string hexString)
    {
        Color col;
        hexString = "#" + hexString.Trim('#');
        if (ColorUtility.TryParseHtmlString(hexString, out col))
        {
            return col;
        }

        throw new Exception("Unable to parse color");
    }

}