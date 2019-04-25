using Newtonsoft.Json;
using System.Collections.Generic;
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
    public Sprite Sprite
    {
        get
        {
            return Game.SpriteStore.GetSpriteByName(SpriteName);
        }
    }

    internal Sprite GetConstructSprite()
    {
        var texture = new Texture2D(Width * MapConstants.PixelsPerCell, Width * MapConstants.PixelsPerCell);

        var y = 0;
        var x = 0;

        foreach (var line in Plan)
        {
            var startX = x * MapConstants.PixelsPerCell;
            var startY = y * MapConstants.PixelsPerCell;

            foreach (var character in line)
            {
                Texture2D constructTexture;
                if (character == ' ')
                {
                    constructTexture = Game.SpriteStore.GetSpriteByName(Floor).texture;
                }
                else
                {
                    constructTexture = Game.SpriteStore.GetSpriteByName(Key[character]).texture;
                }

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

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), MapConstants.PixelsPerCell);
    }

    internal bool ValidateStartPos(CellData cellData)
    {
        return true;
    }
}