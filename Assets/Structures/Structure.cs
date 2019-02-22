using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Structure : MonoBehaviour
{
    internal StructureData Data = new StructureData();
    internal SpriteRenderer SpriteRenderer;
    internal SpriteRenderer StatusSprite;

    public static void SetTiledMode(SpriteRenderer spriteRenderer, bool tiled)
    {
        if (!tiled)
        {
            spriteRenderer.drawMode = SpriteDrawMode.Simple;
        }
        else
        {
            spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            spriteRenderer.size = Vector2.one;
        }
    }

    public void LoadSprite()
    {
        SpriteRenderer.sprite = SpriteStore.Instance.GetSpriteByName(Data.SpriteName);
        SetTiledMode(SpriteRenderer, Data.Tiled);
    }

    internal void Load(string structureData)
    {
        Data = StructureData.GetFromJson(structureData);
        LoadSprite();
    }

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();

        StatusSprite = transform.Find("Status").GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (TimeManager.Instance.Paused) return;

        if (Data.IsBluePrint && !Taskmaster.Instance.ContainsJob(name))
        {
            Taskmaster.Instance.AddTask(new Build(Data, Data.Coordinates));
        }
    }
}

[Serializable]
public class StructureData
{
    public bool Buildable;
    public Coordinates Coordinates;

    public int Id;
    public bool IsBluePrint;
    public bool Scatter;
    public string Name;

    public List<string> Require;
    public List<string> Yield;

    public string SpriteName;

    public string StructureType;

    public bool Tiled;

    public float TravelCost;

    public static List<string> ParseItemString(string itemString)
    {
        //"10:Wood"
        //"1-3:Food"
        var items = new List<string>();
        var parts = itemString.Split(':');

        var type = parts[1];
        var countString = parts[0].Split('-');

        int count = 0;
        if (countString.Length > 1)
        {
            var min = int.Parse(countString[0]);
            var max = int.Parse(countString[1]);

            count = Random.Range(min, max);
        }
        else
        {
            count = int.Parse(countString[0]);
        }

        for (var i = 0; i < count; i++)
        {
            items.Add(type);
        }

        return items;
    }

    public void SpawnYield(CellData cell)
    {
        foreach (var yieldString in Yield)
        {
            foreach (var item in ParseItemString(yieldString))
            {
                cell.AddContent(ItemController.Instance.GetItem(item).gameObject);
            }
        }
    }

    [JsonIgnore]
    public Structure LinkedGameObject
    {
        get
        {
            return StructureController.Instance.GetStructureForData(this);
        }
    }

    public static StructureData GetFromJson(string json)
    {
        return JsonUtility.FromJson<StructureData>(json);
    }

    public void AddItem(ItemData item)
    {
        if (Require.Contains(item.ItemType))
        {
            Require.Remove(item.ItemType);
        }
    }

    public void SetBlueprintState(bool blueprint)
    {
        if (blueprint)
        {
            LinkedGameObject.SpriteRenderer.color = new Color(0.3f, 1f, 1f, 0.4f);
            IsBluePrint = true;
        }
        else
        {
            LinkedGameObject.SpriteRenderer.color = new Color(0.6f, 0.6f, 0.6f);
            IsBluePrint = false;
        }
    }
}