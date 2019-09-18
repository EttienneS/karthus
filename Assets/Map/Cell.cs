using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Cell : IEquatable<Cell>
{
    public CellType CellType;

    public List<CreatureData> Creatures = new List<CreatureData>();

    public Structure Floor;

    [JsonIgnore]
    public Cell[] Neighbors = new Cell[8];

    public Structure Structure;
    public int X;

    public int Y;
    internal Color Color;
    private IEntity _binding;

    private float _fluidLevel;
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

    [JsonIgnore]
    public bool DrawnOnce { get; set; }

    public float FluidLevel
    {
        get
        {
            return _fluidLevel;
        }
        set
        {
            _fluidLevel = value;
            if (_fluidLevel < 0)
            {
                _fluidLevel = 0;
            }
            Game.PhysicsController.Track(this);
        }
    }

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

    public float Level
    {
        get
        {
            var height = Height + FluidLevel;
            if (Structure?.IsWall() == true)
            {
                height += 2;
            }

            return height;
        }
    }

    [JsonIgnore]
    public Cell NextWithSamePriority { get; set; }

    public bool Pathable
    {
        get
        {
            return Bound && TravelCost > 0;
        }
    }

    [JsonIgnore]
    public Cell PathFrom { get; set; }

    [JsonIgnore]
    public int SearchHeuristic { private get; set; }

    [JsonIgnore]
    public int SearchPhase { get; set; }

    [JsonIgnore]
    public int SearchPriority => (int)Distance + SearchHeuristic;

    [JsonIgnore]
    public Tile Tile
    {
        get
        {
            DrawnOnce = true;
            var tile = ScriptableObject.CreateInstance<Tile>();

            if (Floor == null || FluidLevel > 0)
            {
                if (FluidLevel > 0)
                {
                    tile.sprite = Game.SpriteStore.GetSpriteForTerrainType(CellType.Water);
                    tile.color = new Color(0f, 0f, 1f - (FluidLevel / 2), 1f);
                }
                else
                {
                    tile.sprite = Game.SpriteStore.GetSpriteForTerrainType(CellType);
                    tile.color = Color;
                }
            }
            else
            {
                tile.sprite = Game.SpriteStore.GetSprite(Floor.SpriteName);
                tile.color = Color;
            }

            return tile;
        }
    }

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

    public static Cell FromPosition(Vector2 position)
    {
        // add half a unit to each position to account for offset (cells are at point 0,0 in the very center)
        position += new Vector2(0.5f, 0.5f);
        return Game.Map.GetCellAtCoordinate(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
    }

    public static bool operator !=(Cell obj1, Cell obj2)
    {
        if (ReferenceEquals(obj1, null))
        {
            return !ReferenceEquals(obj2, null);
        }

        return !obj1.Equals(obj2);
    }

    public static bool operator ==(Cell obj1, Cell obj2)
    {
        if (ReferenceEquals(obj1, null))
        {
            return ReferenceEquals(obj2, null);
        }

        return obj1.Equals(obj2);
    }

    public int DistanceTo(Cell other)
    {
        return (X < other.X ? other.X - X : X - other.X)
                + (Y < other.Y ? other.Y - Y : Y - other.Y);
    }

    public bool Equals(Cell other)
    {
        if (ReferenceEquals(other, null))
        {
            return false;
        }

        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        var other = obj as Cell;
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

    public Cell GetNeighbor(Direction direction)
    {
        return Neighbors[(int)direction];
    }

    public bool IsWall(Direction direction)
    {
        var neighbor = Neighbors[(int)direction];

        if (neighbor == null)
        {
            return false;
        }

        if (neighbor.Structure == null)
        {
            return false;
        }

        return neighbor.Structure.IsWall();
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

    public void SetNeighbor(Direction direction, Cell cell)
    {
        Neighbors[(int)direction] = cell;
        cell.Neighbors[(int)direction.Opposite()] = this;
    }

    public void SetStructure(Structure structure)
    {
        if (structure.StructureType != "Floor")
        {
            if (Structure != null)
            {
                Game.StructureController.DestroyStructure(Structure);
            }

            structure.Cell = this;
            Structure = structure;
        }
        else
        {
            if (Floor != null)
            {
                Game.StructureController.DestroyStructure(Floor);
            }
            structure.Cell = this;
            Floor = structure;
        }
    }

    public Vector2 ToMapVector()
    {
        // add half a unit to each position to account for offset (cells are at point 0,0 in the very center)
        return new Vector2(Mathf.FloorToInt(X) + 0.5f, Mathf.FloorToInt(Y) + 0.5f);
    }

    public override string ToString()
    {
        return "Cell: (" + X + ", " + Y + ")";
    }

    public string ToStringOnSeparateLines()
    {
        return $"X: {X}\nY: {Y}";
    }

    public Vector3 ToTopOfMapVector()
    {
        return new Vector3(Mathf.FloorToInt(X) + 0.5f, Mathf.FloorToInt(Y) + 0.5f, -1);
    }

    public void UpdateTile()
    {
        Game.Map.Tilemap.SetTile(new Vector3Int(X, Y, 0), Tile);
        if (Structure != null)
        {
            Game.StructureController.RefreshStructure(Structure);
        }
    }

    internal void Clear()
    {
        if (Structure != null)
        {
            Game.StructureController.DestroyStructure(Structure);
        }

        if (Floor != null)
        {
            Game.StructureController.DestroyStructure(Floor);
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

    internal Structure CreateStructure(string structureName, string faction = FactionConstants.World)
    {
        var structure = Game.StructureController.GetStructure(structureName, FactionController.Factions[faction]);
        SetStructure(structure);
        return structure;
    }

    internal bool Empty()
    {
        return Structure == null && Floor == null;
    }

    internal IEnumerable<CreatureData> GetEnemyCreaturesOf(string faction)
    {
        return Creatures.Where(c => c.FactionName != faction);
    }

    internal Cell GetRandomNeighbor()
    {
        var neighbors = Neighbors.Where(n => n != null).ToList();
        return neighbors[Random.Range(0, neighbors.Count - 1)];
    }

    internal Vector3Int ToVector3Int()
    {
        return new Vector3Int(X, Y, 0);
    }

    internal void UpdatePhysics()
    {
        const float minLevel = 0.1f;
        if (FluidLevel <= minLevel)
        {
            UpdateTile();
            return;
        }

        var drop = Mathf.Max(minLevel, FluidLevel / 2);
        var options = Neighbors.Where(n => n != null && n.Level <= Level).ToList();

        if (options.Count > 0)
        {
            var lower = options.GetRandomItem();
            lower.FluidLevel += drop;
            FluidLevel -= drop;
            UpdateTile();
        }
    }
}