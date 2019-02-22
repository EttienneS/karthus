using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CellData
{
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

            return Structure != null ? Structure.TravelCost : 1;
        }
    }

    public CellType CellType;

    public List<ItemData> ContainedItems = new List<ItemData>();

    public Coordinates Coordinates;

    public StockpileData Stockpile;

    public StructureData Structure;

    [JsonIgnore]
    public CellData[] Neighbors = new CellData[8];

    public void AddContent(GameObject gameObject)
    {
        var item = gameObject.GetComponent<Item>();
        var structure = gameObject.GetComponent<Structure>();
        var stockpile = gameObject.GetComponent<Stockpile>();

        var scatterIntensity = 0.3f;
        var scatter = false;
        if (item != null)
        {
            if (item.Cell != null)
            {
                item.Cell.ContainedItems.Remove(item.Data);
            }

            if (Stockpile != null && Stockpile.ItemType == item.Data.ItemType)
            {
                Stockpile.AddItem(item.Data);
            }

            scatter = true;

            ContainedItems.Add(item.Data);
            item.Cell = this;

            item.SpriteRenderer.sortingOrder = item.Data.Id;
        }
        else if (structure != null)
        {
            structure.Data.Coordinates = Coordinates;
            Structure = structure.Data;
            if (structure.Data.Scatter)
            {
                scatter = true;
                scatterIntensity = 0.3f;
            }

            structure.SpriteRenderer.sortingOrder = structure.Data.Id;
        }
        else if (stockpile != null)
        {
            stockpile.Data.Coordinates = Coordinates;
            Stockpile = stockpile.Data;
        }

        gameObject.transform.position = Coordinates.ToMapVector();

        if (scatter)
        {
            //gameObject.transform.Rotate(0, 0, Random.Range(-45f, 45f));
            gameObject.transform.position += new Vector3(Random.Range(-scatterIntensity, scatterIntensity), Random.Range(-scatterIntensity, scatterIntensity), 0);
        }

        if (structure != null)
        {
            if (structure.SpriteRenderer.bounds.size.x > 1.0f)
            {
                gameObject.transform.position += new Vector3(structure.SpriteRenderer.bounds.size.x / 2, 0);
            }

            if (structure.SpriteRenderer.bounds.size.y > 1.0f)
            {
                gameObject.transform.position += new Vector3(0, structure.SpriteRenderer.bounds.size.y / 2);
            }
        }
    }
}