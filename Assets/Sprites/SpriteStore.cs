using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpriteStore : MonoBehaviour
{
    internal Dictionary<string, Sprite> ItemSprites
    {
        get
        {
            if (_itemSprites == null)
            {
                _itemSprites = new Dictionary<string, Sprite>();

                var sprites = Resources.LoadAll<Sprite>("Sprites/Item").ToList();
                sprites.AddRange(Resources.LoadAll<Sprite>("Sprites/Gui"));

                foreach (var sprite in sprites)
                {
                    _itemSprites.Add(sprite.name, sprite);
                }
            }

            return _itemSprites;
        }
    }

    private Dictionary<string, List<Sprite>> _mapSprites;
    private Dictionary<string, Sprite> _itemSprites;

    internal Sprite GetPlaceholder()
    {
        return GetSpriteByName("Placeholder");
    }


    public Dictionary<string, Sprite> CreatureSprites = new Dictionary<string, Sprite>();

    public void LoadCreatureSprites()
    {
        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites/Creature"))
        {
            CreatureSprites.Add(sprite.name, sprite);
        }
    }

    internal Dictionary<string, List<Sprite>> MapSpriteTypeDictionary
    {
        get
        {
            if (_mapSprites == null)
            {
                LoadCreatureSprites();
                Debug.Log("load map sprites");

                _mapSprites = new Dictionary<string, List<Sprite>>();

                foreach (var sprite in Resources.LoadAll<Sprite>("Sprites/Map"))
                {
                    var typeName = sprite.name.Split('_')[0];

                    if (!_mapSprites.ContainsKey(typeName))
                    {
                        MapSpriteTypeDictionary.Add(typeName, new List<Sprite>());
                    }

                    _mapSprites[typeName].Add(sprite);
                }
            }
            return _mapSprites;
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
        var typeList = MapSpriteTypeDictionary[cellType.ToString()];
        return typeList[Random.Range(0, typeList.Count - 1)];
    }
}