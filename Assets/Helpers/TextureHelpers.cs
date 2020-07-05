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

    public static void ScaleToGridSize(this Texture2D texture, int width, int height)
    {
        TextureScale.scale(texture, width * Map.PixelsPerCell, height * Map.PixelsPerCell);
    }
}