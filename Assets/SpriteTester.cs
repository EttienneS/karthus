using LPC.Spritesheet.Generator;
using LPC.Spritesheet.Interfaces;
using LPC.Spritesheet.Renderer;
using LPC.Spritesheet.ResourceManager;
using System.Collections.Generic;
using UnityEngine;
using Animation = LPC.Spritesheet.Interfaces.Animation;

public class SpriteTester : MonoBehaviour
{
    private SpriteRenderer _renderer;

    public Animation Animation;
    public Orientation Orientation;
    public int Frame;

    public float Elapsed = 1f;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        Elapsed += Time.deltaTime;

        if (Elapsed > 1.0f)
        {
            Elapsed = 0;
            _renderer.sprite = TestSheet.CharacterSpriteSheet.GetSprite(Animation, Orientation, ref Frame);
        }
    }
}

public static class TestSheet
{
    private static List<CharacterSpriteSheet> _characterSpriteSheets;
    private static CharacterSpriteSheet _current;

    private static readonly EmbeddedResourceManager ResourceManager = new EmbeddedResourceManager();
    private static readonly CharacterSpriteGenerator Generator = new CharacterSpriteGenerator(ResourceManager);
    private static readonly UnityTexture2dRenderer Renderer = new UnityTexture2dRenderer(ResourceManager);
    private static int _counter = 999999;

    public static CharacterSpriteSheet CharacterSpriteSheet
    {
        get
        {
            _counter++;
            if (_characterSpriteSheets == null)
            {
                _characterSpriteSheets = new List<CharacterSpriteSheet>();

                for (int i = 0; i < 20; i++)
                {
                    var character = new CharacterSprite(RandomHelper.Random.Next(10) > 5 ? Gender.Male : Gender.Female);
                    character.AddLayer(Generator.GetSprites(SpriteLayer.Body, character.Gender).GetRandomItem());
                    character.AddLayer(Generator.GetSprites(SpriteLayer.Eyes, character.Gender).GetRandomItem());
                    character.AddLayer(Generator.GetSprites(SpriteLayer.Clothes, character.Gender).GetRandomItem());
                    character.AddLayer(Generator.GetSprites(SpriteLayer.Legs, character.Gender).GetRandomItem());
                    character.AddLayer(Generator.GetSprites(SpriteLayer.Shoes, character.Gender).GetRandomItem());
                    character.AddLayer(Generator.GetSprites(SpriteLayer.Hair, character.Gender).GetRandomItem());

                    //_characterSpriteSheet = new CharacterSpriteSheet(Renderer.GetFullSpriteSheet(Generator.GetRandomCharacterSprite()));
                    _characterSpriteSheets.Add(new CharacterSpriteSheet(Renderer.GetFullSpriteSheet(character)));
                }
            }

            if (_counter > 100)
            {
                _counter = 0;
                _current = _characterSpriteSheets.GetRandomItem();
            }

            return _current;
        }
    }
}