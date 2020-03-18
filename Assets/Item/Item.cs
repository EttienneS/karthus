using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item : IEntity
{
    private (float X, float Y) _coords;

    private VisualEffect _outline;

    public int Amount { get; set; } = 1;

    public string[] Categories { get; set; }
    [JsonIgnore]
    public Cell Cell
    {
        get
        {
            return Game.Map.GetCellAtCoordinate(Coords);
        }
        set
        {
            Coords = (value.Vector.x, value.Vector.y);
        }
    }

    public (float X, float Y) Coords
    {
        get
        {
            return _coords;
        }
        set
        {
            _coords = value;

            Renderer?.UpdatePosition();
        }
    }

    public Cost Cost { get; set; } = new Cost();
    public string FactionName { get; set; }

    public string Id { get; set; }

    [JsonIgnore]
    public bool InUseByAnyone
    {
        get
        {
            return !string.IsNullOrEmpty(InUseById);
        }
    }

    public string InUseById { get; set; }

    public List<VisualEffectData> LinkedVisualEffects { get; set; } = new List<VisualEffectData>();

    public string Name { get; set; }

    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

    [JsonIgnore]
    public ItemRenderer Renderer { get; set; }

    public string SpriteName { get; set; }

    public Dictionary<string, float> ValueProperties { get; set; } = new Dictionary<string, float>();

    [JsonIgnore]
    public Vector2 Vector
    {
        get
        {
            return new Vector2(Coords.X, Coords.Y);
        }
    }

    public static Item GetFromJson(string json)
    {
        return json.LoadJson<Item>();
    }

    public bool CanUse(IEntity entity)
    {
        if (string.IsNullOrEmpty(InUseById))
        {
            return true;
        }
        return InUseById.GetEntity() == entity;
    }

    public void Free()
    {
        InUseById = null;
    }

    public void Reserve(IEntity entity)
    {
        InUseById = entity.Id;
    }

    public override string ToString()
    {
        return $"{Name} ({Amount}) - {Id}";
    }

    internal void HideOutline()
    {
        if (_outline != null)
        {
            _outline.Kill();
        }
    }

    internal bool IsType(string type)
    {
        if (Name.Equals(type, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (Categories == null)
            return false;

        return Categories.Contains(type, StringComparer.OrdinalIgnoreCase);
    }
    internal void ShowOutline()
    {
        _outline = Game.VisualEffectController
                       .SpawnSpriteEffect(this, Vector, "CellOutline", float.MaxValue);
        _outline.Regular();
    }
    internal Item Split(int amount)
    {
        Amount -= amount;
        return Game.ItemController.SpawnItem(Name, Cell, amount);
    }
}