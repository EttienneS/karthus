using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpriteStore : MonoBehaviour
{
    public List<Sprite> MapSprites;
    public List<Sprite> ItemSprites;

    private Dictionary<string, Sprite> _allSprites;

    public Dictionary<string, Sprite> AllSprites
    {
        get
        {
            if (_allSprites == null)
            {
                _allSprites = new Dictionary<string, Sprite>();
                ItemSprites.ForEach(AddSpriteToAll);
            }

            return _allSprites;
        }
    }

    private Dictionary<string, List<Sprite>> _mapSprites;

    public Dictionary<string, List<Sprite>> MapSpriteTypeDictionary
    {
        get
        {
            if (_mapSprites == null)
            {
                Debug.Log("load map sprites");

                _mapSprites = new Dictionary<string, List<Sprite>>();

                foreach (var sprite in MapSprites)
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

    private static SpriteStore _instance;

    public List<Sprite> FrontSprites;
    public List<Sprite> SideSprites;
    public List<Sprite> BackSprites;

    private Dictionary<int, List<Sprite>> _creatureSprite;

    public Dictionary<int, List<Sprite>> CreatureSprite
    {
        get
        {
            if (_creatureSprite == null)
            {
                Debug.Log("load creature sprites");
                _creatureSprite = new Dictionary<int, List<Sprite>>();
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

                SideSprites.ForEach(AddSpriteToAll);
                FrontSprites.ForEach(AddSpriteToAll);
                BackSprites.ForEach(AddSpriteToAll);
            }
            return _creatureSprite;
        }
    }

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

    internal Sprite GetSpriteByName(string spriteName)
    {
        try
        {
            return AllSprites[spriteName];
        }
        catch
        {
            throw new Exception($"No sprite found with name: {spriteName}");
        }
    }

    public void AddSpriteToAll(Sprite sprite)
    {
        if (!AllSprites.ContainsKey(sprite.name))
        {
            AllSprites.Add(sprite.name, sprite);
        }
    }

    internal Color[] GetGroundTextureFor(string typeName, int size)
    {
        return MapSpriteTypeDictionary[typeName][0].texture.GetPixels(Random.Range(0, 150), Random.Range(0, 150), size, size);
    }
}