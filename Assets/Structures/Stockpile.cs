using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Stockpile : MonoBehaviour
{
    public StockpileData Data = new StockpileData();

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


    public ItemData GetItem(ItemData item)
    {
        Data.Items.Remove(item);
        item.StockpileId = 0;
        return item;
    }

    public ItemData GetItemOfType(string itemType)
    {
        return Data.Items.FirstOrDefault(i => i.ItemType == itemType && !i.Reserved);
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

        if (Data.ActiveTasks.Count < Data.MaxConcurrentTasks && Data.Items.Count < Data.Size)
        {
            Data.ActiveTasks.Add(Taskmaster.Instance.AddTask(new StockpileItem(Data.ItemType, Data.Id)));
        }
    }
}

public class StockpileData
{
    [JsonIgnore]
    public List<TaskBase> ActiveTasks = new List<TaskBase>();

    [JsonIgnore]
    public List<ItemData> Items = new List<ItemData>();

    public Coordinates Coordinates;
    public string ItemType;
    public int MaxConcurrentTasks = 3;
    public int Size = 12;
    public int Id ;

    internal void AddItem(ItemData item)
    {
        Items.Add(item);
        item.StockpileId = Id;
    }
}
