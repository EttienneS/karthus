using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Stockpile : MonoBehaviour
{
    internal List<TaskBase> ActiveTasks = new List<TaskBase>();
    internal Coordinates Coordinates;
    internal string ItemType;
    internal int MaxConcurrentTasks = 3;
    internal int Size = 12;
    internal string StockpileId = Guid.NewGuid().ToString();
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

        MapGrid.Instance.GetCellAtCoordinate(Coordinates).AddContent(item.LinkedGameObject.gameObject, true);
        item.StockpileId = StockpileId;
        _items.Add(item);
    }

    public ItemData GetItem(ItemData item)
    {
        _items.Remove(item);
        item.StockpileId = null;
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

        Text = ItemType;

        ActiveTasks.RemoveAll(t => t.Done());

        if (ActiveTasks.Count < MaxConcurrentTasks && _items.Count < Size)
        {
            ActiveTasks.Add(Taskmaster.Instance.AddTask(new StockpileItem(ItemType, this)));
        }
    }
}