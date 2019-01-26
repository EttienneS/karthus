using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item : MonoBehaviour
{
    internal SpriteRenderer SpriteRenderer;

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public ItemData Data;

    public string GetPropertyValue(string key)
    {
        return Data.Properties.First(p => p.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)).Value;
    }

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

    public string StockpileId { get; set; }

    public ItemProperty[] Properties;

   
}

[Serializable]
public class ItemProperty
{
    public string Key;
    public string Value;
}