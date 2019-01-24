using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Structure : MonoBehaviour
{
    internal SpriteRenderer SpriteRenderer;
    internal StructureData Data = new StructureData();

    internal bool BluePrint
    {
        get
        {
            return Data.IsBluePrint;
        }
        set
        {
            Data.IsBluePrint = value;

            if (Data.IsBluePrint)
            {
                SpriteRenderer.color = new Color(0.3f, 1f, 1f, 0.4f);
                SpriteRenderer.material.SetFloat("_EffectAmount", 1f);
            }
            else
            {
                SpriteRenderer.color = Color.white;
                SpriteRenderer.material.SetFloat("_EffectAmount", 0f);
            }
        }
    }

    internal void Load(string structureData)
    {
        Data = StructureData.GetFromJson(structureData);
        SpriteRenderer.sprite = SpriteStore.Instance.GetSpriteByName(Data.SpriteName);
        SetTiledMode(SpriteRenderer, Data.Tiled);
    }

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

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (TimeManager.Instance.Paused) return;

        if (BluePrint && !Taskmaster.Instance.ContainsJob(name))
        {
            Taskmaster.Instance.AddTask(new Build(this, GetComponentInParent<Cell>()));
        }
    }
}

[Serializable]
public class StructureData
{
    public string Name;
    public string SpriteName;
    public List<string> RequiredItemTypes;
    public bool IsBluePrint;
    public bool Tiled;
    public float TravelCost;

    public List<Item> ContainedItems = new List<Item>();

    public void AddItem(Item item)
    {
        ContainedItems.Add(item);

        if (RequiredItemTypes.Contains(item.Data.ItemType))
        {
            RequiredItemTypes.Remove(item.Data.ItemType);
        }
    }

    public bool ReadyToBuild()
    {
        return IsBluePrint && RequiredItemTypes.Count == 0;
    }

    public void DestroyContainedItems()
    {
        foreach (var item in ContainedItems.ToList())
        {
            ItemController.Instance.DestoyItem(item);
        }
        ContainedItems.Clear();
    }

    public static StructureData GetFromJson(string json)
    {
        return JsonUtility.FromJson<StructureData>(json);
    }
}