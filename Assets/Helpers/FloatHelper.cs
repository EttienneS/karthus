using System.Globalization;
using UnityEngine;

public static class FloatHelper
{
    public static bool AlmostEquals(this float double1, float double2, float precision = 0.0001f)
    {
        // do not make precision lower (more?) than 0.0001f or certain things like movement will stop working
        return Mathf.Abs(double1 - double2) <= precision;
    }

    public static float ToFloat(this string input)
    {
        return float.Parse(input, CultureInfo.InvariantCulture);
    }

    public static string GlobalizeFloatString(string input)
    {
        return input.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
    }
}