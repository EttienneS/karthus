using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoronoiLib.Structures;
using Random = UnityEngine.Random;

public static class CloneHelper
{
    // from: https://stackoverflow.com/questions/78536/deep-cloning-objects

    /// <summary>
    /// Perform a deep Copy of the object, using Json as a serialisation method. NOTE: Private members are not cloned using this method.
    /// </summary>
    /// <typeparam name="T">The type of object being copied.</typeparam>
    /// <param name="source">The object instance to copy.</param>
    /// <returns>The copied object.</returns>
    public static T CloneJson<T>(this T source)
    {
        // Don't serialize a null object, simply return the default for that object
        if (ReferenceEquals(source, null))
        {
            return default;
        }

        // initialize inner objects individually
        // for example in default constructor some list property initialized with some values,
        // but in 'source' these items are cleaned -
        // without ObjectCreationHandling.Replace default constructor values will be added to result
        var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
    }
}

public static class ColorExtensions
{
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

    internal static Color GetColorFromHex(string hexString)
    {
        Color col;

        if (ColorUtility.TryParseHtmlString(hexString, out col))
        {
            return col;
        }

        throw new Exception("Unable to parse color");
    }

    internal static Color GetRandomHairColor()
    {
        return ColorConstants.HairArray[Random.Range(0, ColorConstants.HairArray.Length - 1)];
    }

    internal static Color GetRandomSkinColor()
    {
        return ColorConstants.SkinArray[Random.Range(0, ColorConstants.SkinArray.Length - 1)];
    }
}

public static class Geometry
{
    // Explanation of PointInTriangle method:
    // youtu.be/HYAgJN3x4GA?list=PLFt_AvWsXl0cD2LPxcjxVjWTQLxJqKpgZ
    public static bool PointInTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        double s1 = C.y - A.y;
        double s2 = C.x - A.x;
        double s3 = B.y - A.y;
        double s4 = P.y - A.y;

        double w1 = (A.x * s1 + s4 * s2 - P.x * s1) / (s3 * s2 - (B.x - A.x) * s1);
        double w2 = (s4 - w1 * s3) / s1;
        return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
    }
}

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

public static class ListHelpers
{
    public static T GetRandomItem<T>(this IEnumerable<T> list)
    {
        return list.ElementAt(Random.Range(0, list.Count() - 1));
    }
}

public static class RenderHelpers
{
    public static void SetBoundMaterial(this SpriteRenderer renderer, Cell cell)
    {
        if (cell.Bound)
        {
            renderer.material = Game.MaterialController.DefaultMaterial;
        }
        else
        {
            renderer.material = Game.MaterialController.AbyssMaterial;
        }

        renderer.color = cell.Color;
    }
}

public static class ScrollRectExtensions
{
    public static void ScrollToBottom(this ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }

    public static void ScrollToTop(this ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 1);
    }
}

public static class TextureHelpers
{
    public static Texture2D Clone(this Texture2D sourceTexture)
    {
        Texture2D clonedTexture = new Texture2D(sourceTexture.width, sourceTexture.height);

        clonedTexture.SetPixels(sourceTexture.GetPixels(0, 0, sourceTexture.width, sourceTexture.height));
        clonedTexture.Apply();

        return clonedTexture;
    }

    public static Texture2D Combine(this Texture2D target, Sprite source)
    {
        return target.Combine(source, Vector2.zero);
    }

    public static Texture2D Combine(this Texture2D target, Sprite source, Vector2 offSet)
    {
        var pixels = source.texture.GetPixels((int)source.textureRect.x,
                                              (int)source.textureRect.y,
                                              (int)source.textureRect.width,
                                              (int)source.textureRect.height);

        var x = 0;
        var y = 0;
        var offsetX = (int)offSet.x;
        var offsetY = (int)offSet.y;

        foreach (var pixel in pixels)
        {
            if (pixel.a > 0)
            {
                var pixX = x + offsetX;
                var pixY = y + offsetY;

                if (pixX <= target.width && pixY <= target.height)
                {
                    target.SetPixel(x + offsetX, y + offsetY, pixel);
                }
            }

            x++;

            if (x >= source.textureRect.width)
            {
                x = 0;
                y++;
            }
        }

        target.Apply();
        return target;
    }

    public static Texture2D GetSolidTexture(int width, int height, Color color)
    {
        var tex = new Texture2D(width, height);
        for (var x = 0; x < tex.width; x++)
        {
            for (var y = 0; y < tex.height; y++)
            {
                tex.SetPixel(x, y, color);
            }
        }

        return tex;
    }

    public static void ScaleToGridSize(this Texture2D texture, int width, int height)
    {
        TextureScale.scale(texture, width * Map.PixelsPerCell, height * Map.PixelsPerCell);
    }
}
