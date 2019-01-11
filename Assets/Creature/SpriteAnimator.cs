using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    public Sprite[] BackSprites;
    public Sprite[] FrontSprites;
    public Sprite[] SideSprites;

    public Direction MoveDirection = Direction.S;

    private float deltaTime = 0;
    private int frame;
    private float frameSeconds = 0.3f;
    private SpriteRenderer SpriteRenderer;

    private void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
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