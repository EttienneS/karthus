using LPC.Spritesheet.Generator;
using LPC.Spritesheet.Generator.Enums;
using LPC.Spritesheet.ResourceManager;
using System.Diagnostics;
using UnityEngine;
using Animation = LPC.Spritesheet.Generator.Interfaces.Animation;
using Debug = UnityEngine.Debug;

public static class StatGen
{
    private static CharacterSpriteGenerator _gen;

    public static CharacterSpriteGenerator Gen
    {
        get
        {
            if (_gen == null)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                // create a generator (this loads everything in the resource manager into memory, so if you need a few of these keep this as a singleton somewhere)
                _gen = new CharacterSpriteGenerator(new EmbeddedResourceManager());
                sw.Stop();
                Debug.Log($"Created generator in {sw.ElapsedMilliseconds}");
            }
            return _gen;
        }
    }
}

public class SpriteTester : MonoBehaviour
{
    public Animation Animation;

    public Orientation Orientation;

    // object to hold the sheet definition
    private CharacterSpriteSheet _characterSpriteSheet;

    // keep track of time to only update the frame every second
    private float _delta;

    // keep track of the current animation frame
    private int _frame;

    // the attached renderer
    private SpriteRenderer _renderer;
    private void Start()
    {
        // get the attached renderer
        _renderer = GetComponent<SpriteRenderer>();
        Stopwatch sw = new Stopwatch();

        sw.Start();
        // generate the sprite, this will go over and select random items and all 27 layers and merge them into a single texture (expensive, takes ~200ms)
        _characterSpriteSheet = new CharacterSpriteSheet(StatGen.Gen.GetRandomCharacterSprite(Race.Human));
        sw.Stop();
        Debug.Log($"Created sprite in {sw.ElapsedMilliseconds}");
    }

    private void Update()
    {
        _delta += Time.deltaTime;

        if (_delta > 1f)
        {
            _delta = 0;
            // send in a refrence to the frame integer, will increment and go over the items (if it goes over it will reset to 0 to loop the animation)
            _renderer.sprite = _characterSpriteSheet.GetFrame(Animation, Orientation, ref _frame);
        }
    }
}