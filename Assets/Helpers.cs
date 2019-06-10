using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public static class Helpers
{
    public static List<string> ParseItemString(string itemString)
    {
        //"10:Wood"
        //"1-3:Food"
        var items = new List<string>();
        var parts = itemString.Split(':');

        var type = parts[1];
        var countString = parts[0].Split('-');

        int count = 0;
        if (countString.Length > 1)
        {
            var min = int.Parse(countString[0]);
            var max = int.Parse(countString[1]);

            count = Random.Range(min, max);
        }
        else
        {
            count = int.Parse(countString[0]);
        }

        for (var i = 0; i < count; i++)
        {
            items.Add(type);
        }

        return items;
    }

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
}

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

public static class ListHelpers
{
    public static T GetRandomItem<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count - 1)];
    }
}

public static class TextureHelpers
{
    public static Texture2D Combine(this Texture2D target, Sprite source)
    {
        return target.Combine(source, Vector2.zero);
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
        counter++;
        File.WriteAllBytes($@"C:\ext\testtex {counter}.png", target.EncodeToPNG());

        return target;
    }

    private static int counter = 0;

    public static Texture2D Clone(this Texture2D sourceTexture)
    {
        Texture2D clonedTexture = new Texture2D(sourceTexture.width, sourceTexture.height);

        clonedTexture.SetPixels(sourceTexture.GetPixels(0, 0, sourceTexture.width, sourceTexture.height));
        clonedTexture.Apply();

        return clonedTexture;
    }

    public static void ScaleToGridSize(this Texture2D texture, int width, int height)
    {
        TextureScale.scale(texture, width * MapConstants.PixelsPerCell, height * MapConstants.PixelsPerCell);
    }
}

public static class ColorExtensions
{
    public static float[] ToFloatArray(this Color color)
    {
        return new[] { color.r, color.g, color.b, color.a };
    }

    public static Color ToColor(this float[] arr)
    {
        return new Color(arr[0], arr[1], arr[2], arr[3]);
    }

    public static Color GetRandomColor(float alpha = 1.0f)
    {
        return new Color(Random.value, Random.value, Random.value, alpha);
    }
}

public static class ScrollRectExtensions
{
    public static void ScrollToTop(this ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 1);
    }

    public static void ScrollToBottom(this ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
}