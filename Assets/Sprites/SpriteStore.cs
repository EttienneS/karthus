using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpriteStore : MonoBehaviour
{
    internal List<Sprite> BackSprites = new List<Sprite>();
    internal List<Sprite> FrontSprites = new List<Sprite>();
    internal List<Sprite> ItemSprites = new List<Sprite>();
    internal List<Sprite> MapSprites = new List<Sprite>();
    internal List<Sprite> SideSprites = new List<Sprite>();
    private Dictionary<string, Sprite> _allSprites;

    private Dictionary<int, List<Sprite>> _creatureSprite;

    private Dictionary<string, List<Sprite>> _mapSprites;
    

    public void LoadResources()
    {
        ItemSprites.AddRange(Resources.LoadAll<Sprite>("Sprites/Item"));
        ItemSprites.AddRange(Resources.LoadAll<Sprite>("Sprites/Gui"));
        MapSprites.AddRange(Resources.LoadAll<Sprite>("Sprites/Map"));

        BackSprites.AddRange(Resources.LoadAll<Sprite>("Sprites/Character/all_back"));
        FrontSprites.AddRange(Resources.LoadAll<Sprite>("Sprites/Character/all_front"));
        SideSprites.AddRange(Resources.LoadAll<Sprite>("Sprites/Character/all_side"));
    }

    internal Sprite GetPlaceholder()
    {
        return GetSpriteByName("Placeholder");
    }

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

    internal Dictionary<string, List<Sprite>> MapSpriteTypeDictionary
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

    public void AddSpriteToAll(Sprite sprite)
    {
        if (!AllSprites.ContainsKey(sprite.name))
        {
            AllSprites.Add(sprite.name, sprite);
        }
    }

    internal Sprite GetSpriteByName(string spriteName)
    {
        try
        {
            if (!AllSprites.ContainsKey(spriteName))
            {
                spriteName = spriteName.Replace(" ", "");
            }

            if (AllSprites.ContainsKey(spriteName))
            {
                return AllSprites[spriteName];
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