using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Stockpile : MonoBehaviour
{
    public StockpileData Data = new StockpileData();

    private List<ItemData> _items = new List<ItemData>();
    private TextMeshPro _textMesh;

    public string Text
    {
        get
        {
            return GetTextMesh().text;
        }
        set
        {
            GetTextMesh().enabled = true;
            GetTextMesh().text = value;
        }
    }

    public void AddItem(ItemData item)
    {
        item.Reserved = false;
        item.LinkedGameObject.SpriteRenderer.sortingLayerName = "Item";

        MapGrid.Instance.GetCellAtCoordinate(Data.Coordinates).AddContent(item.LinkedGameObject.gameObject, true);
        item.StockpileId = Data.Id;
        _items.Add(item);
    }

    public ItemData GetItem(ItemData item)
    {
        _items.Remove(item);
        item.StockpileId = 0;
        return item;
    }

    public ItemData GetItemOfType(string itemType)
    {
        return _items.FirstOrDefault(i => i.ItemType == itemType && !i.Reserved);
    }

    public TextMeshPro GetTextMesh()
    {
        if (_textMesh == null)
        {
            _textMesh = transform.Find("Text").GetComponent<TextMeshPro>();
        }
        return _textMesh;
    }

    private void Update()
    {
        if (TimeManager.Instance.Paused) return;

        Text = Data.ItemType;

        Data.ActiveTasks.RemoveAll(t => t.Done());

        if (Data.ActiveTasks.Count < Data.MaxConcurrentTasks && _items.Count < Data.Size)
        {
            Data.ActiveTasks.Add(Taskmaster.Instance.AddTask(new StockpileItem(Data.ItemType, Data.Id)));
        }
    }
}

public class StockpileData
{
    [JsonIgnore]
    public List<TaskBase> ActiveTasks = new List<TaskBase>();

    public Coordinates Coordinates;
    public string ItemType;
    public int MaxConcurrentTasks = 3;
    public int Size = 12;
    public int Id ;
}
