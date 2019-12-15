using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class Item : IEntity
{
    private (float X, float Y) _coords;

    private VisualEffect _outline;

    public int Amount { get; set; }

    [JsonIgnore]
    public Cell Cell { get; set; }

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

    public Dictionary<ManaColor, float> ManaValue { get; set; } = new Dictionary<ManaColor, float>();

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
}