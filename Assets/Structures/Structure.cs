﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

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

    internal void Shift()
    {
        if (!string.IsNullOrEmpty(Data.Scale))
        {
            float scale = Helpers.GetValueFromFloatRange(Data.Scale);
            transform.localScale = new Vector3(0, scale, 0);
        }

        if (!string.IsNullOrEmpty(Data.ShiftX))
        {
            float shiftX = Helpers.GetValueFromFloatRange(Data.ShiftX);
            transform.position += new Vector3(shiftX, 0, 0);
        }

        if (!string.IsNullOrEmpty(Data.ShiftY))
        {
            float shiftY = Helpers.GetValueFromFloatRange(Data.ShiftY);
            transform.position += new Vector3(0, shiftY, 0);
        }
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


public class StructureData
{
    public bool Buildable;
    public Coordinates Coordinates;

    public int Id;
    public bool IsBluePrint;
    public string Layer;
    public string Name;
    public List<string> RequireStrings;
    public string Scale;
    public string ShiftX;
    public string ShiftY;
    public string SpriteName;
    public string StructureType;
    public bool Tiled;
    public float TravelCost;
    public List<string> Yield;

    private List<string> _require;

    [JsonIgnore]
    public List<string> Require
    {
        get
        {
            if (_require == null)
            {
                _require = new List<string>();
                foreach (var reqString in RequireStrings)
                {
                    _require.AddRange(Helpers.ParseItemString(reqString));
                }
            }
            return _require;
        }
    }

    public List<TaskBase> Tasks = new List<TaskBase>();

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
        return JsonConvert.DeserializeObject<StructureData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });
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

    public void SpawnYield(CellData cell)
    {
        foreach (var yieldString in Yield)
        {
            foreach (var item in Helpers.ParseItemString(yieldString))
            {
                cell.AddContent(ItemController.Instance.GetItem(item).gameObject);
            }
        }
    }
}