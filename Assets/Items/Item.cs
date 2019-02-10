using Newtonsoft.Json;
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

    public ItemData Data;

    public Cell Cell { get; set; }

    internal void Load(string structureData)
    {
        Data = JsonUtility.FromJson<ItemData>(structureData);
        SpriteRenderer.sprite = SpriteStore.Instance.GetSpriteByName(Data.SpriteName);
    }
}

[Serializable]
public class ItemData
{
    public int Id;

    public string ItemType;

    public bool Reserved;

    public string SpriteName;

    public string Name;

    public int StockpileId { get; set; }

    public ItemProperty[] Properties;

    public string GetPropertyValue(string key)
    {
        return Properties.First(p => p.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)).Value;
    }

    [JsonIgnore]
    public Item LinkedGameObject
    {
        get
        {
            return ItemController.Instance.ItemDataLookup[this];
        }
    }
}

[Serializable]
public class ItemProperty
{
    public string Key;

    public string Value;
}