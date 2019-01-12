using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpriteAnimator : MonoBehaviour
{
    internal Sprite[] BackSprites;
    internal Sprite[] FrontSprites;
    internal Sprite[] SideSprites;

    public Direction MoveDirection = Direction.S;

    private float deltaTime = 0;
    private int frame;
    private float frameSeconds = 0.3f;
    private SpriteRenderer SpriteRenderer;

    private void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();

        var creatureId = Random.Range(0, SpriteStore.Instance.CreatureSprite.Keys.Count - 1);

        var sprites = SpriteStore.Instance.CreatureSprite[creatureId];

        BackSprites = sprites.Where(s => s.name.StartsWith("all_back", StringComparison.InvariantCultureIgnoreCase)).ToArray();
        FrontSprites = sprites.Where(s => s.name.StartsWith("all_front", StringComparison.InvariantCultureIgnoreCase)).ToArray();
        SideSprites = sprites.Where(s => s.name.StartsWith("all_side", StringComparison.InvariantCultureIgnoreCase)).ToArray();
    }

    private void Update()
    {
        Sprite[] sprites;
        switch (MoveDirection)
        {
            case Direction.N:
                sprites = BackSprites;
                break;

            case Direction.SE:
            case Direction.NE:
            case Direction.E:
                sprites = SideSprites;
                SpriteRenderer.flipX = true;
                break;

            case Direction.S:
                sprites = FrontSprites;
                break;
            //case Direction.NW:
            //case Direction.SW:
            //case Direction.W:
            default:
                sprites = SideSprites;
                SpriteRenderer.flipX = false;
                break;
        }

        deltaTime += Time.deltaTime;

        if (deltaTime > frameSeconds)
        {
            deltaTime = 0;
            frame++;
            if (frame >= sprites.Length)
            {
                frame = 0;
            }
            SpriteRenderer.sprite = sprites[frame];
        }
    }
}