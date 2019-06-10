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

    public List<Sprite> HairSprites = new List<Sprite>();
    public List<Sprite> HeadSprites = new List<Sprite>();
    public List<Sprite> FaceSprites = new List<Sprite>();
    public List<Sprite> NeckSprites = new List<Sprite>();
    public List<Sprite> ArmSprites = new List<Sprite>();
    public List<Sprite> PantSprites = new List<Sprite>();
    public List<Sprite> SleeveSprites = new List<Sprite>();
    public List<Sprite> TorsoSprites = new List<Sprite>();
    public List<Sprite> HandSprites = new List<Sprite>();
    public List<Sprite> PelvisSprites = new List<Sprite>();
    public List<Sprite> LegSprites = new List<Sprite>();
    public List<Sprite> FootSprites = new List<Sprite>();

    public Dictionary<string, Sprite> FixedSprites = new Dictionary<string, Sprite>();

    public void LoadCreatureSprites()
    {
        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites/Creature/Fixed"))
        {
            FixedSprites.Add(sprite.name, sprite);
        }

        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites/Creature/Face"))
        {
            if (sprite.name.StartsWith("face", StringComparison.OrdinalIgnoreCase))
            {
                FaceSprites.Add(sprite);
            }
        }

        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites/Creature/Feet"))
        {
            FootSprites.Add(sprite);
        }

        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites/Creature/Hair"))
        {
            if (sprite.name.IndexOf("woman", StringComparison.OrdinalIgnoreCase) >= 0)
                continue;
            HairSprites.Add(sprite);
        }

        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites/Creature/Skin"))
        {
            if (sprite.name.IndexOf("head", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                HeadSprites.Add(sprite);
            }

            if (sprite.name.IndexOf("arm", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ArmSprites.Add(sprite);
            }

            if (sprite.name.IndexOf("hand", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                HandSprites.Add(sprite);
            }

            if (sprite.name.IndexOf("leg", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                LegSprites.Add(sprite);
            }

            if (sprite.name.IndexOf("neck", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                NeckSprites.Add(sprite);
            }
        }

        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites/Creature/Tops"))
        {
            if (sprite.name.IndexOf("arm", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (sprite.name.IndexOf("short", StringComparison.OrdinalIgnoreCase) >= 0 || sprite.name.IndexOf("short", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                SleeveSprites.Add(sprite);
            }
            else
            {
                TorsoSprites.Add(sprite);
            }
        }

        foreach (var sprite in Resources.LoadAll<Sprite>("Sprites/Creature/Bottoms"))
        {
            if (sprite.name.Contains("_"))
            {
                if (sprite.name.IndexOf("short", StringComparison.OrdinalIgnoreCase) >= 0 || sprite.name.IndexOf("short", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                PantSprites.Add(sprite);
            }
            else
            {
                PelvisSprites.Add(sprite);
            }
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

    internal Sprite GetFixedCreatureSprite(string spriteName)
    {
        if (FixedSprites.ContainsKey(spriteName))
        {
            return FixedSprites[spriteName];
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