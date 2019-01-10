using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteStore : MonoBehaviour
{
    public List<Sprite> Sprites;
    public Dictionary<string, List<Sprite>> SpriteTypeDictionary;
    private static SpriteStore _instance;

    public static SpriteStore Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("SpriteStore").GetComponent<SpriteStore>();
            }

            return _instance;
        }
    }

    internal Sprite GetRandomSpriteOfType(string typeName)
    {
        return SpriteTypeDictionary[typeName][(int)(UnityEngine.Random.value * SpriteTypeDictionary[typeName].Count())];
    }

    internal Sprite GetSpriteByName(string spriteName)
    {
        return Sprites.First(s => s.name.Equals(spriteName, StringComparison.InvariantCultureIgnoreCase));
    }

    private void Awake()
    {
        SpriteTypeDictionary = new Dictionary<string, List<Sprite>>();

        foreach (var sprite in Sprites)
        {
            var typeName = sprite.name.Split('_')[0];

            if (!SpriteTypeDictionary.ContainsKey(typeName))
            {
                SpriteTypeDictionary.Add(typeName, new List<Sprite>());
            }

            SpriteTypeDictionary[typeName].Add(sprite);
        }
    }

    internal Sprite GetRandomSpriteOfType(CellType cellType)
    {
        return GetRandomSpriteOfType(cellType.ToString());
    }
}