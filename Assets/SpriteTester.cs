using LPC.Spritesheet.Generator;
using LPC.Spritesheet.Generator.Enums;
using LPC.Spritesheet.ResourceManager;
using UnityEngine;
using Animation = LPC.Spritesheet.Generator.Interfaces.Animation;

public class SpriteTester : MonoBehaviour
{
    // the attached renderer
    private SpriteRenderer _renderer;

    // object to hold the sheet definition
    private CharacterSpriteSheet _characterSpriteSheet;

    public Animation Animation;
    public Orientation Orientation;

    // keep track of the current animation frame
    private int _frame;

    // keep track of time to only update the frame every second
    private float _delta;

    private void Start()
    {
        // get the attached renderer
        _renderer = GetComponent<SpriteRenderer>();

        // create a generator (this loads everything in the resource manager into memory, so if you need a few of these keep this as a singleton somewhere)
        var generator = new CharacterSpriteGenerator(new EmbeddedResourceManager());

        // generate the sprite, this will go over and select random items and all 27 layers and merge them into a single texture (expensive, takes ~200ms)
        _characterSpriteSheet = new CharacterSpriteSheet(generator.GetRandomCharacterSprite());
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