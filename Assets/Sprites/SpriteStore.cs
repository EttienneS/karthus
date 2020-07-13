using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteStore : MonoBehaviour
{
    private Dictionary<string, Sprite> _iconSprites;

    internal Dictionary<string, Sprite> IconSprites
    {
        get
        {
            if (_iconSprites == null)
            {
                //Debug.Log("load item sprites");

                _iconSprites = new Dictionary<string, Sprite>();

                var sprites = Resources.LoadAll<Sprite>("Sprites/Icons").ToList();
                sprites.AddRange(Resources.LoadAll<Sprite>("Sprites/Gui"));

                foreach (var sprite in sprites)
                {
                    _iconSprites.Add(sprite.name, sprite);
                }
                // Debug.Log("load item sprites");
            }

            return _iconSprites;
        }
    }

    internal bool FacingUp(Direction facing)
    {
        switch (facing)
        {
            case Direction.NW:
            case Direction.NE:
            case Direction.N:
                return true;

            default:
                return false;
        }
    }

    internal Sprite GetPlaceholder()
    {
        return GetSprite("Placeholder");
    }

    internal Sprite GetSprite(string spriteName)
    {
        try
        {
            if (!IconSprites.ContainsKey(spriteName))
            {
                spriteName = spriteName.Replace(" ", "");
            }

            if (IconSprites.ContainsKey(spriteName))
            {
                return IconSprites[spriteName];
            }

            Debug.LogWarning($"No sprite for: {spriteName}");
            return GetPlaceholder();
        }
        catch
        {
            throw new Exception($"No sprite found with name: {spriteName}");
        }
    }
}