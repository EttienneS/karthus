using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class CellData : IEquatable<CellData>
{
    public CellType CellType;
    public int X;

    public int Y;

    [JsonIgnore]
    public CellData[] Neighbors = new CellData[8];

    public Structure Structure;
    internal Color Color;
    private IEntity _binding;

    private float _height;

    public IEntity Binding
    {
        get
        {
            return _binding;
        }
        set
        {
            _binding = value;
            RefreshColor();
        }
    }

    [JsonIgnore]
    public bool Bound
    {
        get
        {
            return Binding != null;
        }
    }

    public bool Buildable
    {
        get
        {
            return Bound && TravelCost > 0 && Structure == null;
        }
    }

    [JsonIgnore]
    public float Distance { get; set; }

    public float Height
    {
        get
        {
            return _height;
        }
        set
        {
            _height = value;
            CellType = Game.MapGenerator.MapPreset.GetCellType(_height);
            RefreshColor();
        }
    }

    [JsonIgnore]
    public CellData NextWithSamePriority { get; set; }

    public bool Pathable
    {
        get
        {
            return Bound && TravelCost > 0;
        }
    }

    [JsonIgnore]
    public CellData PathFrom { get; set; }

    [JsonIgnore]
    public int SearchHeuristic { private get; set; }

    [JsonIgnore]
    public int SearchPhase { get; set; }

    [JsonIgnore]
    public int SearchPriority => (int)Distance + SearchHeuristic;

    [JsonIgnore]
    public float TravelCost
    {
        get
        {
            switch (CellType)
            {
                case CellType.Water:
                case CellType.Mountain:
                    return -1;
            }

            return Structure != null && !Structure.IsBluePrint ? Structure.TravelCost : 1.5f;
        }
    }


    [JsonIgnore]
    public Tile Tile
    {
        get
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = Game.SpriteStore.GetSpriteForTerrainType(CellType);
            tile.color = Color;
            return tile;
        }
    }

    public CellData GetNeighbor(Direction direction)
    {
        return Neighbors[(int)direction];
    }

    public void RefreshColor()
    {
        const float totalShade = 1f;
        const float maxShade = 0.2f;
        var baseColor = new Color(totalShade, Bound ? totalShade : 0.6f, totalShade, Bound ? 1f : 0.6f);

        var range = Game.MapGenerator.MapPreset.GetCellTypeRange(CellType);
        var scaled = Helpers.Scale(range.Item1, range.Item2, 0f, maxShade, Height);

        Color = new Color(baseColor.r - scaled, baseColor.g - scaled, baseColor.b - scaled, baseColor.a);
    }

    public void SetNeighbor(Direction direction, CellData cell)
    {
        Neighbors[(int)direction] = cell;
        cell.Neighbors[(int)direction.Opposite()] = this;
    }

    internal CellData GetRandomNeighbor()
    {
        var neighbors = Neighbors.Where(n => n != null).ToList();
        return neighbors[Random.Range(0, neighbors.Count - 1)];
    }

    public void SetStructure(Structure structure)
    {
        if (Structure != null)
        {
            Game.StructureController.DestroyStructure(Structure);
        }

        structure.Cell = this;
        foreach (var cell in structure.GetCellsForStructure(this))
        {
            cell.Structure = structure;
        }
    }

    internal int CountNeighborsOfType(CellType? cellType)
    {
        if (!cellType.HasValue)
        {
            return Neighbors.Count(n => n == null);
        }

        return Neighbors.Count(n => n != null && n.CellType == cellType.Value);
    }

    internal IEnumerable<CreatureData> GetCreatures()
    {
        return Game.CreatureController.CreatureLookup.Where(c => c.Key.Cell == this).Select(c => c.Key);
    }

    internal void CreateStructure(string structureName, string faction = FactionConstants.World)
    {
        SetStructure(Game.StructureController.GetStructure(structureName, FactionController.Factions[faction]));
    }



    public static CellData FromPosition(Vector2 position)
    {
        // add half a unit to each position to account for offset (cells are at point 0,0 in the very center)
        position += new Vector2(0.5f, 0.5f);

        return Game.MapGrid.GetCellAtCoordinate(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
    }

    public Vector2 ToMapVector()
    {
        // add half a unit to each position to account for offset (cells are at point 0,0 in the very center)
        return new Vector2(Mathf.FloorToInt(X) + 0.5f, Mathf.FloorToInt(Y) + 0.5f);
    }

    public Vector3 ToTopOfMapVector()
    {
        return new Vector3(Mathf.FloorToInt(X) + 0.5f, Mathf.FloorToInt(Y) + 0.5f, -1);
    }

    public static bool operator !=(CellData obj1, CellData obj2)
    {
        if (ReferenceEquals(obj1, null))
        {
            return !ReferenceEquals(obj2, null);
        }

        return !obj1.Equals(obj2);
    }

    public static bool operator ==(CellData obj1, CellData obj2)
    {
        if (ReferenceEquals(obj1, null))
        {
            return ReferenceEquals(obj2, null);
        }

        return obj1.Equals(obj2);
    }

    public int DistanceTo(CellData other)
    {
        return (X < other.X ? other.X - X : X - other.X)
                + (Y < other.Y ? other.Y - Y : Y - other.Y);
    }

    public bool Equals(CellData other)
    {
        if (ReferenceEquals(other, null))
        {
            return false;
        }

        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        var other = obj as CellData;
        if (other == null)
        {
            return false;
        }

        return this == other;
    }

    public override int GetHashCode()
    {
        return $"{X}:{Y}".GetHashCode();
    }

    public override string ToString()
    {
        return "Cell: (" + X + ", " + Y + ")";
    }

    public string ToStringOnSeparateLines()
    {
        return $"X: {X}\nY: {Y}";
    }

    internal Vector3Int ToVector3Int()
    {
        return new Vector3Int(X, Y, 0);
    }
}