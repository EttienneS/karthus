using LPC.Spritesheet.Generator;
using LPC.Spritesheet.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using Animation = LPC.Spritesheet.Interfaces.Animation;

public class CharacterSpriteSheet
{
    private Dictionary<Animation, Dictionary<Orientation, Sprite[]>> _animationDictionary = new Dictionary<Animation, Dictionary<Orientation, Sprite[]>>();

    public CharacterSpriteSheet(Texture2D texture)
    {
        foreach (var renderConstant in RendererConstants.SpriteSheetAnimationDefinition)
        {
            if (!_animationDictionary.ContainsKey(renderConstant.Key.animation))
            {
                _animationDictionary.Add(renderConstant.Key.animation, new Dictionary<Orientation, Sprite[]>());
            }
            var sprites = new Sprite[renderConstant.Value.frames];
            for (int frame = 0; frame < renderConstant.Value.frames; frame++)
            {
                sprites[frame] = Sprite.Create(texture, new Rect(frame * RendererConstants.SpriteWidth,
                                                                 (20 - renderConstant.Value.row) * RendererConstants.SpriteHeight,
                                                                 RendererConstants.SpriteWidth,
                                                                 RendererConstants.SpriteHeight),
                                                        new Vector2(0.5f, 0.5f),
                                                        Map.PixelsPerCell);
            }
            _animationDictionary[renderConstant.Key.animation].Add(renderConstant.Key.orientation, sprites);
        }
    }

    public Sprite GetSprite(Animation animation, Orientation orientation, ref int frame)
    {
        var sprites = _animationDictionary[animation][orientation];
        frame++;

        if (frame >= sprites.Length)
        {
            frame = 0;
        }

        return sprites[frame];
    }
}