using UnityEngine;

public static class MathHelper
{
    public static float ScaleValueInRange(float min1, float max1, float min2, float max2, float input)
    {
        return Mathf.Lerp(min1, max1, Mathf.InverseLerp(min2, max2, input));
    }
}
