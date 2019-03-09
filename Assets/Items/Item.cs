using Newtonsoft.Json;
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

    public CellData Cell { get; set; }

    internal void Load(string itemData)
    {
        Data = JsonConvert.DeserializeObject<ItemData>(itemData, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });

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

    public Dictionary<string, string> Properties;

    [JsonIgnore]
    public Item LinkedGameObject
    {
        get
        {
            return ItemController.Instance.ItemDataLookup[this];
        }
    }
}

