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

    public Cost Cost { get; set; } = new Cost();

    [JsonIgnore]
    public Cell Cell { get; set; }

    public string[] Categories { get; set; }

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

    public string FactionName { get; set; }

    public string Id { get; set; }

    [JsonIgnore]
    public IEntity InUseBy
    {
        get
        {
            if (string.IsNullOrEmpty(InUseById))
            {
                return null;
            }
            return InUseById.GetEntity();
        }
        set
        {
            InUseById = value.Id;
        }
    }

    [JsonIgnore]
    public bool InUseByAnyone
    {
        get
        {
            return InUseBy != null;
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

    internal void HideOutline()
    {
        if (_outline != null)
        {
            _outline.Kill();
        }
    }

    internal void ShowOutline()
    {
        _outline = Game.VisualEffectController
                       .SpawnSpriteEffect(this, Vector, "CellOutline", float.MaxValue);
        _outline.Regular();
    }

    public override string ToString()
    {
        return $"{Name} ({Amount}) - {Id}";
    }
}