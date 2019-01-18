using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    internal SpriteRenderer SpriteRenderer;

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public ItemData Data;

    internal void Load(string structureData)
    {
        Data = JsonUtility.FromJson<ItemData>(structureData);
        SpriteRenderer.sprite = SpriteStore.Instance.GetSpriteByName(Data.SpriteName);
    }
}

[Serializable]
public class ItemData
{
    public string ItemType;
    public bool Reserved;
    public string SpriteName;
    public string Name;
}