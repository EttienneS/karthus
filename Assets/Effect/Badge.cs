using System;
using UnityEngine;

public class Badge : MonoBehaviour
{
    internal SpriteRenderer SpriteRenderer;

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
}