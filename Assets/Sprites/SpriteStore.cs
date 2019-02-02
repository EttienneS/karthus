using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpriteStore : MonoBehaviour
{
    public List<Sprite> MapSprites;
    public List<Sprite> ItemSprites;

    public Dictionary<string, Sprite> AllSprites;

    public Dictionary<string, List<Sprite>> MapSpriteTypeDictionary;
    private static SpriteStore _instance;

    public List<Sprite> FrontSprites;
    public List<Sprite> SideSprites;
    public List<Sprite> BackSprites;

    public Dictionary<int, List<Sprite>> CreatureSprite = new Dictionary<int, List<Sprite>>();

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
        try
        {
            return MapSpriteTypeDictionary[typeName][(int)(Random.value * MapSpriteTypeDictionary[typeName].Count())];
        }
        catch
        {
            throw new Exception($"No sprite found of type: {typeName}");
        }
    }

    internal Sprite GetSpriteByName(string spriteName)
    {
        try
        {
            return AllSprites[spriteName];
        }
        catch
        {
            throw new Exception($"No sprite found of name: {spriteName}");
        }
    }

    private void Awake()
    {
        MapSpriteTypeDictionary = new Dictionary<string, List<Sprite>>();

        foreach (var sprite in MapSprites)
        {
            var typeName = sprite.name.Split('_')[0];

            if (!MapSpriteTypeDictionary.ContainsKey(typeName))
            {
                MapSpriteTypeDictionary.Add(typeName, new List<Sprite>());
            }

            MapSpriteTypeDictionary[typeName].Add(sprite);
        }

        var creature = -1;

        var side = new List<Sprite>();
        var back = new List<Sprite>();
        var front = new List<Sprite>();
        for (int i = 0; i < SideSprites.Count; i++)
        {
            side.Add(SideSprites[i]);
            back.Add(BackSprites[i]);
            front.Add(FrontSprites[i]);

            if (i != 0 && (i + 1) % 4 == 0)
            {
                creature++;
                side.AddRange(back);
                side.AddRange(front);
                CreatureSprite.Add(creature, side.ToList());

                side.Clear();
                back.Clear();
                front.Clear();
            }
        }

        AllSprites = new Dictionary<string, Sprite>();

        SideSprites.ForEach(AddSpriteToAll);
        FrontSprites.ForEach(AddSpriteToAll);
        BackSprites.ForEach(AddSpriteToAll);
        MapSprites.ForEach(AddSpriteToAll);
        ItemSprites.ForEach(AddSpriteToAll);
    }

    public void AddSpriteToAll(Sprite sprite)
    {
        if (!AllSprites.ContainsKey(sprite.name))
        {
            AllSprites.Add(sprite.name, sprite);
        }
    }

    internal Sprite GetRandomSpriteOfType(CellType cellType)
    {
        return GetRandomSpriteOfType(cellType.ToString());
    }
}