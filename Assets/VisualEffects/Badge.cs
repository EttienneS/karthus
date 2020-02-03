using UnityEngine;

public class Badge : MonoBehaviour
{
    internal SpriteRenderer SpriteRenderer;
    internal IEntity Entity;

    public void Update()
    {
        if (Entity != null)
        {
            transform.position = Entity.Vector;
        }
    }

    public void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    internal void SetSprite(string iconName)
    {
        SpriteRenderer.sprite = Game.SpriteStore.GetSprite(iconName);
    }

    internal void Destroy()
    {
        if (gameObject != null)
        {
            Game.Instance.AddItemToDestroy(gameObject);
        }
    }

    internal void Follow(IEntity entity)
    {
        Entity = entity;
        SpriteRenderer.transform.localScale = new Vector3(0.5f, 0.5f);
    }
}