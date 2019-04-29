using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
public static class ListHelpers
{
    public static T GetRandomItem<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count - 1)];
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