using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class Item : MonoBehaviour
{
    internal SpriteRenderer SpriteRenderer;

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    [SerializeField]
    public ItemData Data;

    public Cell Cell { get; set; }

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
    [SerializeField]
    public string ItemType;
    [SerializeField]
    public bool Reserved;
    [SerializeField]
    public string SpriteName;
    [SerializeField]
    public string Name;

    [SerializeField]
    public string StockpileId { get; set; }

    [SerializeField]
    public ItemProperty[] Properties;
}

[Serializable]
public class ItemProperty
{
    [SerializeField]
    public string Key;
    [SerializeField]
    public string Value;
}