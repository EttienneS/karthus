using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Structure : IEntity
{
    public bool Buildable;
    public string FactionName { get; set; }
    public string InUseBy;
    public string Layer;
    public Dictionary<ManaColor, int> ManaValue;
    public string Material;
    public string Name;
    public Dictionary<string, string> Properties = new Dictionary<string, string>();
    public string ShiftX;
    public string ShiftY;
    public string Size;
    public SpellBase Spell;
    public string SpriteName;
    public string StructureType;
    public List<Task> Tasks = new List<Task>();
    public float TravelCost;

    [JsonIgnore]
    private int _width, _height = -1;

    public Structure()
    {
    }

    public Structure(string name, string sprite)
    {
        Name = name;
        SpriteName = sprite;
    }

    public Cell Cell { get; set; }

    [JsonIgnore]
    public int Height
    {
        get
        {
            ParseHeight();
            return _width;
        }
    }

    public string Id { get; set; }

    [JsonIgnore]
    public bool InUseByAnyone
    {
        get
        {
            return !string.IsNullOrEmpty(InUseBy);
        }
    }

    public bool IsBluePrint { get; private set; }

    public ManaPool ManaPool { get; set; }

    public void Awake()
    {
        if (ManaPool == null)
        {
            ManaPool = new ManaPool(this);
        }
    }

    [JsonIgnore]
    public int Width
    {
        get
        {
            ParseHeight();
            return _width;
        }
    }

    [JsonIgnore]
    public Task Task { get; set; }

    [JsonIgnore]
    public Tile Tile
    {
        get
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            if (IsWall())
            {
                tile.sprite = Game.SpriteStore.GetWallSprite(this);
            }
            else
            {
                tile.sprite = Game.SpriteStore.GetSprite(SpriteName);
            }

            if (IsBluePrint)
            {
                tile.color = ColorConstants.BluePrintColor;
            }
            else
            {
                tile.color = Cell.Color;
            }

            return tile;
        }
    }


    internal void HideOutline()
    {
        if (_outline != null)
        {
            _outline.Kill();
        }
    }

    public bool IsWall()
    {
        return StructureType == "Wall";
    }

    public static Structure GetFromJson(string json)
    {
        return JsonConvert.DeserializeObject<Structure>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });
    }

    public void SetBluePrintState(bool state)
    {
        IsBluePrint = state;
        Game.StructureController.RefreshStructure(this);
    }

    public bool ValidateCellLocationForStructure(Cell cell)
    {
        if (!cell.Buildable)
        {
            return false;
        }
        return true;
    }

    internal void Free()
    {
        InUseBy = string.Empty;
    }

    internal void Reserve(string reservedBy)
    {
        InUseBy = reservedBy;
    }

    internal void SetStatusSprite(Sprite sprite)
    {
        //throw new NotImplementedException();
    }

    private void ParseHeight()
    {
        if (_width == -1 || _height == -1)
        {
            if (!string.IsNullOrEmpty(Size))
            {
                var parts = Size.Split('x');
                _height = int.Parse(parts[0]);
                _width = int.Parse(parts[1]);
            }
            else
            {
                _width = 1;
                _height = 1;
            }
        }
    }

    public void Damage(int amount, ManaColor type)
    {
        if (!ManaPool.Empty())
        {
            var mana = ManaPool.GetRandomManaColorFromPool();
            ManaPool.BurnMana(mana, amount);
        }
        Game.StructureController.DestroyStructure(this);
    }

    private Effect _outline;

    internal void ShowOutline()
    {
        _outline = Game.EffectController
                       .SpawnSpriteEffect(Cell, "CellOutline", float.MaxValue)
                       .Regular()
                       .Pulsing(0.1f);
    }
}