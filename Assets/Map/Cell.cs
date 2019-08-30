using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellData
{
    public CellType CellType;
    public Coordinates Coordinates;
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
            CellType = Game.MapGrid.MapPreset.GetCellType(_height);
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
            if (!Bound)
            {
                return 25;
            }

            switch (CellType)
            {
                case CellType.Water:
                case CellType.Mountain:
                    return -1;
            }

            return Structure != null && !Structure.IsBluePrint ? Structure.TravelCost : 1;
        }
    }

    public CellData GetNeighbor(Direction direction)
    {
        return Neighbors[(int)direction];
    }

    public void RefreshColor()
    {
        const float totalShade = 1f;
        const float maxShade = 0.4f;
        var baseColor = new Color(totalShade, Bound ? totalShade : 0.6f, totalShade, Bound ? 1f : 0.6f);

        var range = Game.MapGrid.MapPreset.GetCellTypeRange(CellType);
        var scaled = Helpers.Scale(range.Item1, range.Item2, 0f, maxShade, Height);

        Color = new Color(baseColor.r - scaled, baseColor.g - scaled, baseColor.b - scaled, baseColor.a);
    }

    public void SetNeighbor(Direction direction, CellData cell)
    {
        Neighbors[(int)direction] = cell;
        cell.Neighbors[(int)direction.Opposite()] = this;
    }

    public void SetStructure(Structure structure)
    {
        if (Structure != null)
        {
            Game.StructureController.DestroyStructure(Structure);
        }

        structure.Coordinates = Coordinates;
        foreach (var cell in structure.GetCellsForStructure(Coordinates))
        {
            cell.Structure = structure;
        }

        Game.StructureController.RefreshStructure(structure);
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
        return Game.CreatureController.CreatureLookup.Where(c => c.Key.Coordinates == Coordinates).Select(c => c.Key);
    }
}