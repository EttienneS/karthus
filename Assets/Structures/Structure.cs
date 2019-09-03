﻿using Newtonsoft.Json;
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
    public List<TaskBase> Tasks = new List<TaskBase>();
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

    public Coordinates Coordinates { get; set; }

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

    public ManaPool ManaPool { get; set; } = new ManaPool();

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
    public TaskBase Task { get; set; }


    [JsonIgnore]
    public Tile Tile
    {
        get
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = Game.SpriteStore.GetSpriteByName(SpriteName);

            if (IsBluePrint)
            {
                tile.color = ColorConstants.BluePrintColor;
            }
            else
            {
                tile.color = Game.MapGrid.GetCellAtCoordinate(Coordinates).Color;
            }

            return tile;
        }
    }

    public static Structure GetFromJson(string json)
    {
        return JsonConvert.DeserializeObject<Structure>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });
    }

    public List<CellData> GetCellsForStructure(Coordinates origin)
    {
        List<CellData> cells = new List<CellData>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                cells.Add(Game.MapGrid.GetCellAtCoordinate(new Coordinates(origin.X + x, origin.Y + y)));
            }
        }

        return cells;
    }

    public void SetBluePrintState(bool state)
    {
        IsBluePrint = state;
        Game.StructureController.RefreshStructure(this);
    }

    public bool ValidateCellLocationForStructure(CellData CellData)
    {
        foreach (var cell in GetCellsForStructure(CellData.Coordinates))
        {
            if (!cell.Buildable)
            {
                return false;
            }
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
}