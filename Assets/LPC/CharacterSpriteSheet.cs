using LPC.Spritesheet.Generator.Enums;
using LPC.Spritesheet.Generator.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using Animation = LPC.Spritesheet.Generator.Interfaces.Animation;

namespace LPC.Spritesheet.Generator
{
    public class CharacterSpriteSheet
    {
        private Dictionary<Animation, Dictionary<Orientation, Sprite[]>> _animationDictionary = new Dictionary<Animation, Dictionary<Orientation, Sprite[]>>();

        public Texture2D Texture;

        public CharacterSpriteSheet(ICharacterSpriteDefinition characterSprite)
        {
            Texture = TextureRenderer.GetFullSheetTexture(characterSprite);

            foreach (var renderConstant in Settings.SpriteSheetAnimationDefinition)
            {
                if (!_animationDictionary.ContainsKey(renderConstant.Key.animation))
                {
                    _animationDictionary.Add(renderConstant.Key.animation, new Dictionary<Orientation, Sprite[]>());
                }
                var sprites = new Sprite[renderConstant.Value.frames];
                for (int frame = 0; frame < renderConstant.Value.frames; frame++)
                {
                    sprites[frame] = Sprite.Create(Texture, new Rect(frame * Settings.SpriteWidth,
                                                                     (20 - renderConstant.Value.row) * Settings.SpriteHeight,
                                                                     Settings.SpriteWidth,
                                                                     Settings.SpriteHeight),
                                                            new Vector2(0.5f, 0.5f),
                                                            Settings.PixelsPerUnit);
                }
                _animationDictionary[renderConstant.Key.animation].Add(renderConstant.Key.orientation, sprites);
            }
        }

        public Sprite GetFrame(Animation animation, Orientation orientation, ref int frame)
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
}
