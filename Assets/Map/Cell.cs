using Assets.Creature;
using Assets.Item;
using Assets.Map;
using Assets.ServiceLocator;
using Assets.Structures;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cell : PathableCell
{
    public BiomeRegion BiomeRegion;

    private List<Cell> _nonNullNeighbours;

    public List<CreatureData> Creatures
    {
        get
        {
            return Loc.GetIdService().CreatureIdLookup.Values.Where(c => c.Cell == this).ToList();
        }
    }

    public Structure Floor
    {
        get
        {
            return Structures.Find(s => s.IsFloor());
        }
    }

    public IEnumerable<ItemData> Items
    {
        get
        {
            return Loc.GetIdService().ItemIdLookup.Values.Where(i => i.Cell == this);
        }
    }

    public new List<Cell> NonNullNeighbors
    {
        get
        {
            if (_nonNullNeighbours == null)
            {
                _nonNullNeighbours = base.NonNullNeighbors.ConvertAll(c => c as Cell).ToList();
            }
            return _nonNullNeighbours;
        }
    }

    public List<Structure> Structures
    {
        get
        {
            return Loc.GetIdService().StructureCellLookup.ContainsKey(this)
                    ? Loc.GetIdService().StructureCellLookup[this] : new List<Structure>();
        }
    }

    public override float TravelCost
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
        return Loc.GetMap().GetCellAtCoordinate(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.z));
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
            Loc.GetStructureController().DestroyStructure(structure);
        }
    }

    internal bool ContainsItems()
    {
        return Items.Any();
    }

    internal Structure CreateStructure(string structureName, string faction = FactionConstants.World)
    {
        var structure = Loc.GetStructureController().SpawnStructure(structureName, this, Loc.GetFactionController().Factions[faction]);
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