using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Helpers
{
    public static float GetValueFromFloatRange(string input)
    {
        var scaleStringParts = GlobalizeFloatString(input).Split('~');

        var scale = 1f;
        if (scaleStringParts.Length > 1)
        {
            var min = float.Parse(scaleStringParts[0]);
            var max = float.Parse(scaleStringParts[1]);

            scale = Random.Range(min, max);
        }

        return scale;
    }

    public static string WildcardToRegex(string pattern)
    {
        return Regex.Escape(pattern).Replace("*", ".*").Replace("?", ".");
    }

    public static string GlobalizeFloatString(string input)
    {
        return input.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
    }

    public static T RandomEnumValue<T>()
    {
        var v = Enum.GetValues(typeof(T));
        return (T)v.GetValue(new System.Random().Next(0, v.Length - 1));
    }

    public static float Scale(float oldMin, float oldMax, float newMin, float newMax, float oldValue)
    {
        var oldRange = oldMax - oldMin;
        var newRange = newMax - newMin;

        return (((oldValue - oldMin) * newRange) / oldRange) + newMin;
    }

    public static float ScaleValueInRange(float min1, float max1, float min2, float max2, float input)
    {
        return Mathf.Lerp(min1, max1, Mathf.InverseLerp(min2, max2, input));
    }
}