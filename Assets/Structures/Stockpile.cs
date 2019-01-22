using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Stockpile : MonoBehaviour
{
    internal string ItemType;
    internal int MaxConcurrentTasks = 3;
    internal string StockpileId = Guid.NewGuid().ToString();
    internal int Size = 12;
    internal List<ITask> ActiveTasks = new List<ITask>();

    private Cell _cell;
    internal Cell Cell
    {
        get
        {
            if (_cell == null)
            {
                _cell = GetComponentInParent<Cell>();
            }

            return _cell;
        }
    }

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
    private TextMeshPro _textMesh;

    public TextMeshPro GetTextMesh()
    {
        if (_textMesh == null)
        {
            _textMesh = transform.Find("Text").GetComponent<TextMeshPro>();
        }
        return _textMesh;
    }


    private List<Item> _items = new List<Item>();

    public Item GetItemOfType(string itemType)
    {
        return _items.FirstOrDefault(i => i.Data.ItemType == itemType && !i.Data.Reserved);
    }

    public void AddItem(Item item)
    {
        item.Data.Reserved = false;
        item.SpriteRenderer.sortingLayerName = "Item";

        Cell.AddContent(item.gameObject, true);
        item.Data.StockpileId = StockpileId;
        _items.Add(item);
    }

    public Item GetItem(Item item)
    {
        _items.Remove(item);
        item.Data.StockpileId = null;
        return item;
    }

    private void Update()
    {
        Text = ItemType;

        ActiveTasks.RemoveAll(t => t.Done());

        if (ActiveTasks.Count < MaxConcurrentTasks && _items.Count < Size)
        {
            ActiveTasks.Add(Taskmaster.Instance.AddTask(new StockpileItem(ItemType, this)));
        }
    }
}