using Assets.Creature;
using Assets.Item;
using Assets.Map;
using Assets.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cell : IEquatable<Cell>
{
    [JsonIgnore]
    public BiomeRegion BiomeRegion;

    [JsonIgnore]
    public Cell[] Neighbors = new Cell[8];

    public int X;

    [JsonIgnore]
    public float Y;

    public int Z;

    [JsonIgnore]
    public List<CreatureData> Creatures
    {
        get
        {
            return Game.Instance.IdService.CreatureIdLookup.Values.Where(c => c.Cell == this).ToList();
        }
    }

    [JsonIgnore]
    public float Distance { get; set; }

    [JsonIgnore]
    public Structure Floor
    {
        get
        {
            return Structures.Find(s => s.IsFloor());
        }
    }

    [JsonIgnore]
    public IEnumerable<ItemData> Items
    {
        get
        {
            return Game.Instance.IdService.ItemIdLookup.Values.Where(i => i.Cell == this);
        }
    }

    [JsonIgnore]
    public Cell NextWithSamePriority { get; set; }

    [JsonIgnore]
    public IEnumerable<Cell> NonNullNeighbors
    {
        get
        {
            return Neighbors.Where(n => n != null);
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
    public List<Structure> Structures
    {
        get
        {
            return Game.Instance.IdService.StructureCellLookup.ContainsKey(this)
                    ? Game.Instance.IdService.StructureCellLookup[this] : new List<Structure>();
        }
    }

    [JsonIgnore]
    public float TravelCost
    {
        get
        {
            if (Structures.Count == 0)
            {
                return BiomeRegion.TravelCost;
            }
            return Structures.Sum(s => s.TravelCost);
        }
    }

    [JsonIgnore]
    public Vector3 Vector
    {
        get
        {
            return new Vector3(X + 0.5f, Y, Z + 0.5f);
        }
    }

    public static Cell FromPosition(Vector3 position)
    {
        // add half a unit to each position to account for offset (cells are at point 0,0 in the very center)
        position += new Vector3(0.5f, 0, 0.5f);
        return MapController.Instance.GetCellAtCoordinate(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.z));
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
        // to handle cases where a diagonal does not count as adjecent
        if (Neighbors.Contains(other))
        {
            return 1;
        }

        return (X < other.X ? other.X - X : X - other.X) +
               (Z < other.Z ? other.Z - Z : Z - other.Z);
    }

    public bool Equals(Cell other)
    {
        if (ReferenceEquals(other, null))
        {
            return false;
        }

        return X == other.X && Z == other.Z;
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
        return $"{X}:{Z}".GetHashCode();
    }

    public Cell GetNeighbor(Direction direction)
    {
        return Neighbors[(int)direction];
    }

    public bool PathableWith(Mobility mobility)
    {
        switch (mobility)
        {
            case Mobility.Walk:
                return TravelCost > 0;

            case Mobility.Fly:
                return true;
        }

        return false;
    }

    public void SetNeighbor(Direction direction, Cell cell)
    {
        Neighbors[(int)direction] = cell;
        cell.Neighbors[(int)direction.Opposite()] = this;
    }

    public override string ToString()
    {
        return $"{BiomeRegion.Name} ({X},{Z})";
    }

    public string ToStringOnSeparateLines()
    {
        return $"X: {X}\nY: {Z}";
    }

    internal void Clear()
    {
        foreach (var structure in Structures)
        {
            Game.Instance.StructureController.DestroyStructure(structure);
        }
    }

    internal bool ContainsItems()
    {
        return Items.Any();
    }

    internal Structure CreateStructure(string structureName, string faction = FactionConstants.World)
    {
        var structure = Game.Instance.StructureController.SpawnStructure(structureName, this, Game.Instance.FactionController.Factions[faction]);
        return structure;
    }

    internal bool Empty()
    {
        return Structures.Count == 0;
    }

    internal IEnumerable<CreatureData> GetEnemyCreaturesOf(string faction)
    {
        return Creatures.Where(c => c.FactionName != faction);
    }

    internal Cell GetPathableNeighbour()
    {
        return NonNullNeighbors.Where(n => n.TravelCost > 0).GetRandomItem();
    }

    internal Cell GetRandomNeighbor()
    {
        var neighbors = Neighbors.Where(n => n != null).ToList();
        return neighbors[Random.Range(0, neighbors.Count - 1)];
    }

    internal float GetStructureValue(string name)
    {
        var total = 0f;
        foreach (var structure in Structures)
        {
            total += structure.GetValue(name);
        }
        return total;
    }

    internal bool HasStructureValue(string v)
    {
        return Structures.Any(s => s.HasValue(v));
    }
}