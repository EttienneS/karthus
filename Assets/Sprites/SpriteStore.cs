using LPC.Spritesheet.Generator;
using LPC.Spritesheet.Interfaces;
using LPC.Spritesheet.Renderer;
using LPC.Spritesheet.ResourceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Animation = LPC.Spritesheet.Interfaces.Animation;

public class SpriteStore : MonoBehaviour
{
    public Dictionary<string, Sprite> CreatureSprites = new Dictionary<string, Sprite>();

    private Dictionary<string, Sprite> _itemSprites;

    private Dictionary<string, Sprite> _mapSprites;

    public EmbeddedResourceManager ResourceManager { get; set; }
    public CharacterSpriteGenerator Generator { get; set; }
    public UnityTexture2dRenderer Renderer { get; set; }

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
            }

            return _mapSprites;
        }
    }

    public void Awake()
    {
        ResourceManager = new EmbeddedResourceManager();
        Generator = new CharacterSpriteGenerator(ResourceManager);
        Renderer = new UnityTexture2dRenderer(ResourceManager);
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

    public class CharacterSpriteSheet
    {
        public Dictionary<Animation, Dictionary<Orientation, Sprite[]>> AnimationArray = new Dictionary<Animation, Dictionary<Orientation, Sprite[]>>();

        public CharacterSpriteSheet(Texture2D texture)
        {
            foreach (var renderConstant in RendererConstants.SpriteSheetAnimationDefinition)
            {
                var parts = renderConstant.Key.Split('_');
                var animation = (Animation)Enum.Parse(typeof(Animation), parts[0]);
                var orientation = (Orientation)Enum.Parse(typeof(Orientation), parts[1]);

                if (!AnimationArray.ContainsKey(animation))
                {
                    AnimationArray.Add(animation, new Dictionary<Orientation, Sprite[]>());
                }

                var sprites = new Sprite[renderConstant.Value.frames];
                for (int x = 0; x < renderConstant.Value.frames; x++)
                {
                    sprites[x] = Sprite.Create(texture, new Rect(x * RendererConstants.SpriteWidth,
                                                                 renderConstant.Value.row * RendererConstants.SpriteHeight,
                                                                 RendererConstants.SpriteWidth,
                                                                 RendererConstants.SpriteHeight), 
                                                        new Vector2(0.5f, 0.5f), 
                                                        RendererConstants.SpriteHeight);
                }
                AnimationArray[animation].Add(orientation, sprites);
            } 
        }

    }

    public Sprite GetRandomSprite()
    {
        var tex = Renderer.GetFullSpriteSheet(Generator.GetRandomCharacterSprite());
        var cs = new CharacterSpriteSheet(tex);

        return cs.AnimationArray[Animation.Shoot][Orientation.Back][0];
    }

    internal Sprite GetBodySprite(string spriteName, int index = 1)
    {
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