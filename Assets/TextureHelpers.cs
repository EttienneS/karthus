using UnityEngine;

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
