using LPC.Spritesheet.Generator.Enums;
using LPC.Spritesheet.Generator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LPC.Spritesheet.Generator
{
    public static class TextureRenderer
    {
        public static Texture2D GetFullSheetTexture(ICharacterSpriteDefinition sprite)
        {
            return GetSpriteSheet(sprite, new RectInt(0, 0, Settings.SheetWidth, Settings.SheetHeight));
        }

        public static Texture2D GetPartialSpriteSheet(ICharacterSpriteDefinition sprite, Interfaces.Animation animation, Orientation orientation)
        {
            var (row, _) = Settings.SpriteSheetAnimationDefinition[(animation, orientation)];
            return GetSpriteSheet(sprite, new RectInt(0, row * Settings.SpriteWidth, Settings.SheetWidth, Settings.SpriteHeight));
        }

        public static Texture2D GetSingleSprite(ICharacterSpriteDefinition sprite, Interfaces.Animation animation, Orientation orientation, int frame)
        {
            var (row, frames) = Settings.SpriteSheetAnimationDefinition[(animation, orientation)];

            if (frame >= frames)
            {
                throw new IndexOutOfRangeException($"Out of range, Cannot get more than frame count ({frames - 1})");
            }
            return GetSpriteSheet(sprite, new RectInt(frame * Settings.SpriteWidth, row * Settings.SpriteWidth, Settings.SpriteWidth, Settings.SpriteHeight));
        }

        public static Texture2D GetSpriteSheet(ICharacterSpriteDefinition sprite, RectInt rectangle)
        {
            var srcRectange = new RectInt(0, 0, rectangle.width, rectangle.height);
            var newImage = new Texture2D(srcRectange.width, srcRectange.height, TextureFormat.RGBA32, true);

            try
            {
                var layers = new List<Texture2D>();
                layers.AddRange(Settings.GetOrderedLayersDescending(sprite.Layers).Select(l => GetTexture(l.SpriteData, rectangle)));

                for (int x = 0; x < rectangle.width; x++)
                {
                    for (int y = 0; y < rectangle.height; y++)
                    {
                        var firstLayer = true;
                        foreach (var layer in layers)
                        {
                            var newPixel = layer.GetPixel(rectangle.x + x, rectangle.y + y);
                            if (newPixel.a == 1)
                            {
                                newImage.SetPixel(x, y, newPixel);
                                break;
                            }
                            else if (firstLayer)
                            {
                                // the first layer ignores the alpha rule, overriding everything
                                newImage.SetPixel(x, y, newPixel);
                            }

                            firstLayer = false;
                        }
                    }
                }

                newImage.filterMode = FilterMode.Point;
                newImage.Apply();
                return newImage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Texture2D GetTexture(byte[] spriteData, RectInt rectangle)
        {
            var texture = new Texture2D(rectangle.width, rectangle.height);
            texture.LoadImage(spriteData);
            return texture;
        }
    }
}