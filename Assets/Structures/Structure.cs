﻿using System;
using System.Collections;
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
        SpriteRenderer.size = Vector2.one;
    }

    void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (BluePrint)
        {
            if (!Taskmaster.Instance.ContainsJob(name))
            {
                Taskmaster.Instance.AddTask(new Build(this, GetComponentInParent<Cell>()));
            }            
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

    private List<Item> _containedItems = new List<Item>();

    public void AddItem(Item item)
    {
        _containedItems.Add(item);

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
        foreach (var item in _containedItems.ToList())
        {
            ItemController.Instance.DestoyItem(item);
        }
        _containedItems.Clear();
    }

    public static StructureData GetFromJson(string json)
    {
        return JsonUtility.FromJson<StructureData>(json);
    }


}
