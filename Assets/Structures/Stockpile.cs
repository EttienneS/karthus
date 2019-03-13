using Newtonsoft.Json;
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

    public ItemData GetItemOfCategory(string category)
    {
        return Data.Items.FirstOrDefault(i => i.Category == category && !i.Reserved);
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

        Text = Data.ItemCategory;

        if (Taskmaster.Instance.Tasks.OfType<StockpileItem>().Count(t => t.StockpileId == Data.Id) < Data.MaxConcurrentTasks)
        {
            Taskmaster.Instance.AddTask(new StockpileItem(Data.ItemCategory, Data.Id), Data.GetGameId());
        }
    }
}

public class StockpileData
{
    [JsonIgnore]
    public List<ItemData> Items = new List<ItemData>();

    public Coordinates Coordinates;
    public string ItemCategory;
    public int MaxConcurrentTasks = 3;
    public int Size = 24;
    public int Id;

    internal void AddItem(ItemData item)
    {
        Items.Add(item);
        item.StockpileId = Id;
    }
}