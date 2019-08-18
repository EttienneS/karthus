using System;
using UnityEngine;

public class Badge : MonoBehaviour
{
    internal SpriteRenderer SpriteRenderer;
    internal IEntity Entity;

    public void Update()
    {
        if (Entity != null)
        {
            transform.position = Entity.Coordinates.ToTopOfMapVector();
        }
    }

    public void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    internal void SetSprite(string iconName)
    {
        SpriteRenderer.sprite = Game.SpriteStore.GetSpriteByName(iconName);
    }

    internal void Destroy()
    {
        Destroy(gameObject);
    }

    internal void Follow(IEntity entity)
    {
        Entity = entity;
    }
}