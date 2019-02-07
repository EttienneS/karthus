using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Structure : MonoBehaviour
{
    internal StructureData Data = new StructureData();
    internal SpriteRenderer SpriteRenderer;

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

    internal void Load(string structureData)
    {
        Data = StructureData.GetFromJson(structureData);
        SpriteRenderer.sprite = SpriteStore.Instance.GetSpriteByName(Data.SpriteName);
        SetTiledMode(SpriteRenderer, Data.Tiled);
    }

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
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
    [JsonIgnore]
    public List<ItemData> ContainedItems = new List<ItemData>();

    public Coordinates Coordinates;

    public bool IsBluePrint;

    public string Name;

    public List<string> RequiredItemTypes;

    public string SpriteName;

    public bool Tiled;

    public float TravelCost;

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
        ContainedItems.Add(item);

        if (RequiredItemTypes.Contains(item.ItemType))
        {
            RequiredItemTypes.Remove(item.ItemType);
        }
    }

    public void DestroyContainedItems()
    {
        foreach (var item in ContainedItems.ToList())
        {
            ItemController.Instance.DestroyItem(item);
        }
        ContainedItems.Clear();
    }

    public bool ReadyToBuild()
    {
        return IsBluePrint && RequiredItemTypes.Count == 0;
    }

    public void ToggleBluePrintState(bool force = false)
    {
        if (IsBluePrint || force)
        {
            LinkedGameObject.SpriteRenderer.color = new Color(0.3f, 1f, 1f, 0.4f);
            LinkedGameObject.SpriteRenderer.material.SetFloat("_EffectAmount", 1f);
        }
        else
        {
            LinkedGameObject.SpriteRenderer.color = Color.white;
            LinkedGameObject.SpriteRenderer.material.SetFloat("_EffectAmount", 0f);
        }
    }
}