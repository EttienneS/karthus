using LPC.Spritesheet.Generator.Enums;
using System.Collections.Generic;
using UnityEngine;
using Animation = LPC.Spritesheet.Generator.Interfaces.Animation;

namespace LPC.Spritesheet.Generator
{
    public class CharacterSpriteSheet
    {
        private Dictionary<Animation, Dictionary<Orientation, Sprite[]>> _bodyAnimationDictionary = new Dictionary<Animation, Dictionary<Orientation, Sprite[]>>();
        private Dictionary<Animation, Dictionary<Orientation, Sprite[]>> _clothesAnimationDictionary;
        private Dictionary<Animation, Dictionary<Orientation, Sprite[]>> _weaponAnimationDictionary;

        public CharacterSpriteSheet(Sprite body)
        {
            _bodyAnimationDictionary = BuildAnimationDictionary(body.texture);
        }

        public void SetClothes(Sprite clothes)
        {
            _clothesAnimationDictionary = BuildAnimationDictionary(clothes.texture);
        }

        public void SetWeapon(Sprite weapon)
        {
            _weaponAnimationDictionary = BuildAnimationDictionary(weapon.texture);
        }

        private Dictionary<Animation, Dictionary<Orientation, Sprite[]>> BuildAnimationDictionary(Texture2D texture2D)
        {
            var animationDictionary = new Dictionary<Animation, Dictionary<Orientation, Sprite[]>>();
            foreach (var renderConstant in Settings.SpriteSheetAnimationDefinition)
            {
                if (!animationDictionary.ContainsKey(renderConstant.Key.animation))
                {
                    animationDictionary.Add(renderConstant.Key.animation, new Dictionary<Orientation, Sprite[]>());
                }
                var sprites = new Sprite[renderConstant.Value.frames];
                for (int frame = 0; frame < renderConstant.Value.frames; frame++)
                {
                    sprites[frame] = Sprite.Create(texture2D, new Rect(frame * Settings.SpriteWidth,
                                                                      (20 - renderConstant.Value.row) * Settings.SpriteHeight,
                                                                      Settings.SpriteWidth,
                                                                      Settings.SpriteHeight),
                                                              new Vector2(0.5f, 0.5f),
                                                              Settings.PixelsPerUnit);
                }
                animationDictionary[renderConstant.Key.animation].Add(renderConstant.Key.orientation, sprites);
            }
            return animationDictionary;
        }

        public (Sprite body, Sprite clothes, Sprite weapon) GetFrame(Animation animation, Orientation orientation, ref int frame)
        {
            var body = GetSprites(_bodyAnimationDictionary, animation, orientation);
            var clothes = GetSprites(_clothesAnimationDictionary, animation, orientation);
            var weapon = GetSprites(_weaponAnimationDictionary, animation, orientation);

            frame++;
            if (frame >= body.Length)
            {
                frame = 0;
            }

            return (body[frame], clothes?[frame], weapon?[frame]);
        }

        public Sprite[] GetSprites(Dictionary<Animation, Dictionary<Orientation, Sprite[]>> dict, Animation animation, Orientation orientation)
        {
            if (dict == null)
            {
                return null;
            }
            return dict[animation][orientation];
        }
    }
}