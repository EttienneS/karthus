using Assets.Helpers;
using Newtonsoft.Json;
using Structures;
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
    public bool Buildable
    {
        get
        {
            return TravelCost > 0 && Structure == null;
        }
    }

    [JsonIgnore]
    public List<Creature> Creatures
    {
        get
        {
            return Game.Instance.IdService.CreatureLookup.Values.Where(c => c.Cell == this).ToList();
        }
    }

    [JsonIgnore]
    public float Distance { get; set; }

    [JsonIgnore]
    public Structure Floor
    {
        get
        {
            return Game.Instance.IdService.StructureCellLookup.ContainsKey(this) ? Game.Instance.IdService.StructureCellLookup[this].Find(s => s.IsFloor()) : null;
        }
    }

    [JsonIgnore]
    public IEnumerable<Item> Items
    {
        get
        {
            return Game.Instance.IdService.ItemLookup.Values.Where(i => i.Cell == this);
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
    public Structure Structure
    {
        get
        {
            return Game.Instance.IdService.StructureCellLookup.ContainsKey(this) ? Game.Instance.IdService.StructureCellLookup[this].Find(s => !s.IsFloor()) : null;
        }
    }

    [JsonIgnore]
    public float TravelCost
    {
        get
        {
            return Structure?.IsBluePrint == false ? Structure.TravelCost : BiomeRegion.TravelCost;
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
        return Game.Instance.Map.GetCellAtCoordinate(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.z));
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
                + (Z < other.Z ? other.Z - Z : Z - other.Z);
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

    public bool IsInterlocking(Direction direction)
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

    public bool Pathable(Mobility mobility)
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
        if (Structure != null)
        {
            Game.Instance.StructureController.DestroyStructure(Structure);
        }

        if (Floor != null)
        {
            Game.Instance.StructureController.DestroyStructure(Floor);
        }
    }

    internal Structure CreateStructure(string structureName, string faction = FactionConstants.World)
    {
        var structure = Game.Instance.StructureController.SpawnStructure(structureName, this, Game.Instance.FactionController.Factions[faction]);
        return structure;
    }

    internal bool Empty()
    {
        return Structure == null && Floor == null;
    }

    internal IEnumerable<Creature> GetEnemyCreaturesOf(string faction)
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

    internal void Populate()
    {
        if (SaveManager.SaveToLoad != null)
        {
            return;
        }

        if (Structure?.Name == "Reserved")
        {
            Game.Instance.StructureController.DestroyStructure(Structure);
        }

        if (!Empty())
        {
            return;
        }

        var content = BiomeRegion.GetContent();

        if (!string.IsNullOrEmpty(content))
        {
            if (Game.Instance.StructureController.StructureTypeFileMap.ContainsKey(content))
            {
                var structure = Game.Instance.StructureController
                                    .SpawnStructure(content,
                                                    this,
                                                    Game.Instance.FactionController.Factions[FactionConstants.World]);
            }
            else
            {
                Game.Instance.ItemController.SpawnItem(content, this);
            }
        }
    }
}