using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Construct
{
    public string Floor;
    public Dictionary<char, string> Key;
    public string Name;
    public List<string> Plan;
    public string SpriteName;

    [JsonIgnore]
    public int Width
    {
        get
        {
            return Plan[0].Length;
        }
    }

    [JsonIgnore]
    public int Height
    {
        get
        {
            return Plan.Count;
        }
    }

    [JsonIgnore]
    private Sprite _sprite;

    [JsonIgnore]
    internal Sprite Sprite
    {
        get
        {
            if (_sprite == null)
            {
                var texture = new Texture2D(Width * MapConstants.PixelsPerCell, Width * MapConstants.PixelsPerCell);

                var y = 0;
                var x = 0;

                // easier to just flip the plan order than to invert the drawing
                var flippedPlan = Plan.ToList();
                flippedPlan.Reverse();

                foreach (var line in flippedPlan)
                {
                    foreach (var character in line)
                    {
                        var startX = x * MapConstants.PixelsPerCell;
                        var startY = y * MapConstants.PixelsPerCell;

                        Texture2D sourceTexture;
                        if (character == ' ')
                        {
                            sourceTexture = Game.SpriteStore.GetSpriteByName(Floor).texture;
                        }
                        else
                        {
                            sourceTexture = Game.SpriteStore.GetSpriteByName(Key[character]).texture;
                        }

                        var constructTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
                        constructTexture.SetPixels(sourceTexture.GetPixels(0, 0, sourceTexture.width, sourceTexture.height));
                        constructTexture.Apply();

                        TextureScale.scale(constructTexture, MapConstants.PixelsPerCell, MapConstants.PixelsPerCell);

                        for (var subTexX = 0; subTexX < MapConstants.PixelsPerCell; subTexX++)
                        {
                            for (var subTexY = 0; subTexY < MapConstants.PixelsPerCell; subTexY++)
                            {
                                var pixel = constructTexture.GetPixel(subTexX, subTexY);
                                texture.SetPixel(startX + subTexX,
                                                 startY + subTexY,
                                                 pixel);
                            }
                        }

                        x++;
                    }
                    y++;
                }
                texture.Apply();

                _sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), MapConstants.PixelsPerCell);
            }

            return _sprite;
        }
    }

    internal bool ValidateStartPos(CellData cellData)
    {
        return true;
    }
}