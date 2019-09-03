using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteStore : MonoBehaviour
{
    public Dictionary<string, Sprite> CreatureSprites = new Dictionary<string, Sprite>();

    private Dictionary<string, Sprite> _itemSprites;

    private Dictionary<string, Sprite> _mapSprites;

    internal Dictionary<string, Sprite> ItemSprites
    {
        get
        {
            if (_itemSprites == null)
            {
                //Debug.Log("load item sprites");

                _itemSprites = new Dictionary<string, Sprite>();

                var sprites = Resources.LoadAll<Sprite>("Sprites/Item").ToList();
                sprites.AddRange(Resources.LoadAll<Sprite>("Sprites/Gui"));

                foreach (var sprite in sprites)
                {
                    _itemSprites.Add(sprite.name, sprite);
                }
                // Debug.Log("load item sprites");
            }

            return _itemSprites;
        }
    }

    internal Dictionary<string, Sprite> MapSpriteTypeDictionary
    {
        get
        {
            if (_mapSprites == null)
            {
                // Debug.Log("load map sprites");
                LoadCreatureSprites();

                _mapSprites = new Dictionary<string, Sprite>();

                foreach (var sprite in Resources.LoadAll<Sprite>("Sprites/Map"))
                {
                    var typeName = sprite.name.Split('_')[0];

                    if (!_mapSprites.ContainsKey(typeName))
                    {
                        MapSpriteTypeDictionary.Add(typeName, sprite);
                    }
                }
                //  Debug.Log("load map sprites");
            }

            return _mapSprites;
        }
    }

    public void LoadCreatureSprites()
    {
        //  Debug.Log("load creature sprites");

        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites/Creature"))
        {
            CreatureSprites.Add(sprite.name, sprite);
        }

        //  Debug.Log("load creature sprites");
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

    internal Sprite GetCreatureSprite(string spriteName)
    {
        if (CreatureSprites.ContainsKey(spriteName))
        {
            return CreatureSprites[spriteName];
        }

        return GetPlaceholder();
    }

    internal Sprite GetPlaceholder()
    {
        return GetSpriteByName("Placeholder");
    }

    internal Sprite GetSpriteByName(string spriteName)
    {
        try
        {
            if (!ItemSprites.ContainsKey(spriteName))
            {
                spriteName = spriteName.Replace(" ", "");
            }

            if (ItemSprites.ContainsKey(spriteName))
            {
                return ItemSprites[spriteName];
            }
            return GetPlaceholder();
        }
        catch
        {
            throw new Exception($"No sprite found with name: {spriteName}");
        }
    }

    internal Sprite GetSpriteForTerrainType(CellType cellType)
    {
        return MapSpriteTypeDictionary[cellType.ToString()];
    }
}