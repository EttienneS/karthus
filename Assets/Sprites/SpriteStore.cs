using LPC.Spritesheet.Generator.Enums;
using Structures;
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

    private Dictionary<(Race, Gender), List<Sprite>> _characterSprites;

    internal Dictionary<(Race, Gender), List<Sprite>> CharacterSprites
    {
        get
        {
            if (_characterSprites == null)
            {
                //Debug.Log("load item sprites");

                _characterSprites = new Dictionary<(Race, Gender), List<Sprite>>();

                foreach (Race race in Enum.GetValues(typeof(Race)))
                {
                    if (race == Race.Any)
                    {
                        continue;
                    }
                    foreach (Gender gender in Enum.GetValues(typeof(Gender)))
                    {
                        if (gender == Gender.Either)
                        {
                            continue;
                        }
                        _characterSprites.Add((race, gender), Resources.LoadAll<Sprite>($"Sprites/Chars/{race}_{gender}").ToList());
                    }
                }
            }

            return _characterSprites;
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
            }

            return _mapSprites;
        }
    }

    public void Awake()
    {
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

    internal Sprite GetInterlockingSprite(Structure structure)
    {
        // _H == ─
        var type = "_H";

        var n = structure.Cell.IsInterlocking(Direction.N);
        var s = structure.Cell.IsInterlocking(Direction.S);
        var e = structure.Cell.IsInterlocking(Direction.E);
        var w = structure.Cell.IsInterlocking(Direction.W);

        if (n && e && s && w)
        {
            // _X == ┼
            type = "_X";
        }
        else if (n && s && !e && !w)
        {
            // _V == │
            type = "_V";
        }
        else if (n && !s && e & !w)
        {
            // _C == └
            type = "_C";
        }
        else if (n && !s && !e & w)
        {
            // _C_F == ┘
            type = "_C_F";
        }
        else if (!n && s && e & !w)
        {
            // _CT == ┌
            type = "_CT";
        }
        else if (!n && s && !e & w)
        {
            // _CT_F == ┐
            type = "_CT_F";
        }
        else if (n && s && e & !w)
        {
            // _TS == ├
            type = "_TS";
        }
        else if (n && s && !e & w)
        {
            // _TS_F == ┤
            type = "_TS_F";
        }
        else if (n && !s && e & w)
        {
            // _T_F == ┴
            type = "_T_F";
        }
        else if (!n && s && e & w)
        {
            // _T == ┬
            type = "_T";
        }
        else if ((!n && s && !e & !w) || (n && !s && !e & !w))
        {
            // _V == │
            type = "_V";
        }

        return GetSprite(structure.SpriteName + type);
    }

    internal Sprite GetCreatureSprite(Race race, Gender gender)
    {
        return CharacterSprites[(race, gender)].GetRandomItem();
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

    internal Sprite GetCreatureSprite(string spriteName, ref int index)
    {
        if (spriteName.Contains("-X"))
        {
            index++;
            var tempName = spriteName.Replace("-X", $"-{index}");

            if (!CreatureSprites.ContainsKey(tempName))
            {
                index = 1;
                tempName = spriteName.Replace("-X", $"-{index}");
            }
            spriteName = tempName;
        }

        if (CreatureSprites.ContainsKey(spriteName))
        {
            return CreatureSprites[spriteName];
        }

        return GetPlaceholder();
    }

    internal Sprite GetPlaceholder()
    {
        return GetSprite("Placeholder");
    }

    internal Sprite GetSprite(string spriteName)
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

    internal Sprite GetSpriteForTerrainType(string spriteName)
    {
        var typeString = spriteName.ToString();
        if (MapSpriteTypeDictionary.ContainsKey(typeString))
        {
            return MapSpriteTypeDictionary[typeString];
        }
        else
        {
            return MapSpriteTypeDictionary[MapSpriteTypeDictionary.Keys.Where(k => k.StartsWith(typeString)).GetRandomItem()];
        }
    }
}