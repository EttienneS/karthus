using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CellData
{
    public string Binding;

    public CellType CellType;

    public Coordinates Coordinates;

    [JsonIgnore]
    public CellData[] Neighbors = new CellData[8];

    public StructureData Structure;

    [JsonIgnore]
    public bool Bound
    {
        get
        {
            return !string.IsNullOrEmpty(Binding);
        }
    }

    [JsonIgnore]
    public float Distance { get; set; }

    [JsonIgnore]
    public CellData NextWithSamePriority { get; set; }

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

    public bool Buildable
    {
        get
        {
            return Bound && TravelCost > 0 && Structure == null;
        }
    }

    public bool Pathable
    {
        get
        {
            return Bound && TravelCost > 0;
        }
    }

    public void AddContent(GameObject gameObject, bool force = false)
    {
        var structure = gameObject.GetComponent<Structure>();

        var scatterIntensity = 0.3f;
        var scatter = false;

        gameObject.transform.position = Coordinates.ToMapVector();

        if (structure != null)
        {
            if (force)
            {
                if (Structure != null)
                {
                    Game.StructureController.DestroyStructure(Structure);
                }
            }

            structure.Data.Coordinates = Coordinates;
            foreach (var cell in structure.Data.GetCellsForStructure(Coordinates))
            {
                cell.Structure = structure.Data;
            }

            structure.Shift();
            structure.SpriteRenderer.sortingOrder = MapConstants.MapSize - Coordinates.Y;

            ColorStructure();
        }

        if (scatter)
        {
            gameObject.transform.position += new Vector3(Random.Range(-scatterIntensity, scatterIntensity), Random.Range(-scatterIntensity, scatterIntensity), 0);
        }
    }

    public CellData GetNeighbor(Direction direction)
    {
        return Neighbors[(int)direction];
    }

    public void SetNeighbor(Direction direction, CellData cell)
    {
        Neighbors[(int)direction] = cell;
        cell.Neighbors[(int)direction.Opposite()] = this;
    }

    internal int CountNeighborsOfType(CellType? cellType)
    {
        if (!cellType.HasValue)
        {
            return Neighbors.Count(n => n == null);
        }

        return Neighbors.Count(n => n != null && n.CellType == cellType.Value);
    }

    internal void ColorStructure()
    {
        if (Structure != null)
        {
            var renderer = Structure.LinkedGameObject.SpriteRenderer;

            if (Bound)
            {
                renderer.color = Structure.IsBluePrint ? ColorConstants.BluePrintColor : ColorConstants.BaseColor;
            }
            else
            {
                renderer.color = ColorConstants.UnboundStructureColor;
            }
        }
    }
}