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

        var value = 0f;
        if (scaleStringParts.Length > 1)
        {
            var min = float.Parse(scaleStringParts[0]);
            var max = float.Parse(scaleStringParts[1]);

            value = Random.Range(min, max);
        }

        return value;
    }

    public static bool AlmostEquals(this float double1, float double2, float precision = 0.0001f)
    {
        // do not make precision lower (more?) than 0.0001f or certain things like movement will stop working
        return Mathf.Abs(double1 - double2) <= precision;
    }

    public static string WildcardToRegex(string pattern)
    {
        return Regex.Escape(pattern).Replace("*", ".*").Replace("?", ".");
    }

    public static float ToFloat(this string input)
    {
        return float.Parse(input, CultureInfo.InvariantCulture);
    }

    public static Vector3 ToVector3(this string input)
    {
        var parts = input.Split(',');
        switch (parts.Length)
        {
            case 1:
                return Vector3.one * parts[0].ToFloat();

            case 2:
                return new Vector3(parts[0].ToFloat(), parts[1].ToFloat());

            case 3:
                return new Vector3(parts[0].ToFloat(), parts[1].ToFloat(), parts[2].ToFloat());

            default:
                throw new InvalidCastException("Input not in correct format: value,value,value");
        }
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