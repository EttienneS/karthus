using Assets.Helpers;
using UnityEngine;

namespace Assets.Sprites
{
    public static class TextureCreator
    {
        public static Texture2D CreateTextureFromHeightMap(int width, int height, float[,] heights)
        {
            var colors = new Color[width, height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    colors[x, y] = new Color(heights[x, y], heights[x, y], heights[x, y]);
                }
            }
            return CreateTextureFromColorMap(width, height, colors);
        }

        public static Texture2D CreateTextureFromColorMap(int width, int height, Color[,] color)
        {
            //using (Instrumenter.Start())
            {
                var texture = new Texture2D(width, height)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                var pixels = new Color[width * height];

                var i = 0;
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        pixels[i] = color[x, y];
                        i++;
                    }
                }
                texture.SetPixels(pixels);
                texture.Apply();
                return texture;
            }
        }
    }
}