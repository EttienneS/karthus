using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cell : MonoBehaviour
{
    internal CellData Data = new CellData();

    internal Cell[] Neighbors = new Cell[8];

    private float _lastUpdate;

    public SpriteRenderer Border { get; private set; }

    public CellType CellType
    {
        get
        {
            return Data.CellType;
        }
        set
        {
            Data.CellType = value;

            switch (value)
            {
                case CellType.Mountain:
                case CellType.Water:
                    TravelCost = -1;
                    break;

                case CellType.Stone:
                    TravelCost = 5;
                    break;

                default:
                    TravelCost = 1;
                    break;
            }

            Terrain.sprite = SpriteStore.Instance.GetRandomSpriteOfType(Data.CellType);
            RandomlyFlipSprite();
        }
    }

    public float Distance { get; set; }

    public Cell NextWithSamePriority { get; set; }

    public Cell PathFrom { get; set; }

    public int SearchHeuristic { private get; set; }

    public int SearchPhase { get; set; }

    public int SearchPriority => (int)Distance + SearchHeuristic;

    public SpriteRenderer Terrain { get; private set; }

    internal float TravelCost
    {
        get
        {
            if (Data.Structure != null)
            {
                return Data.Structure.TravelCost;
            }

            return Data.BaseTravelCost;
        }
        set
        {
            Data.BaseTravelCost = value;
        }
    }

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
                item.Cell.Data.ContainedItems.Remove(item.Data);
            }

            if (Data.Stockpile != null && Data.Stockpile.ItemType == item.Data.ItemType)
            {
                Data.Stockpile.AddItem(item.Data);
            }

            scatter = true;

            Data.ContainedItems.Add(item.Data);
            item.Cell = this;

            item.SpriteRenderer.sortingOrder = item.Data.Id;
        }
        else if (structure != null)
        {
            structure.Data.Coordinates = Data.Coordinates;
            Data.Structure = structure.Data;
            if (structure.Data.Scatter)
            {
                scatter = true;
                scatterIntensity = 0.3f;
            }

            structure.SpriteRenderer.sortingOrder = structure.Data.Id;
        }
        else if (stockpile != null)
        {
            stockpile.Data.Coordinates = Data.Coordinates;
            Data.Stockpile = stockpile.Data;
        }

        gameObject.transform.SetParent(transform);
        gameObject.transform.position = transform.position;

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

    public void DisableBorder()
    {
        Border.enabled = false;
    }

    public void EnableBorder(Color color)
    {
        Border.color = color;
        Border.enabled = true;
    }

    public Cell GetNeighbor(Direction direction)
    {
        return Neighbors[(int)direction];
    }

    public void SetNeighbor(Direction direction, Cell cell)
    {
        Neighbors[(int)direction] = cell;
        cell.Neighbors[(int)direction.Opposite()] = this;
    }

    public void Update()
    {
        if (CellType == CellType.Water)
        {
            _lastUpdate += Time.deltaTime;

            if (_lastUpdate > Random.Range(0.5f, 1f))
            {
                CellType = CellType.Water;
                _lastUpdate = 0;
            }
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

    internal Vector3 GetCreaturePosition()
    {
        return transform.position;
    }

    private void Awake()
    {
        var gridStructure = transform.Find("CellStructure");

        //Fog = gridStructure.transform.Find("Fog").GetComponent<SpriteRenderer>();
        Border = gridStructure.transform.Find("Border").GetComponent<SpriteRenderer>();
        Terrain = gridStructure.transform.Find("Terrain").GetComponent<SpriteRenderer>();
    }

    private void RandomlyFlipSprite()
    {
        Terrain.flipX = Random.value < 0.5f;
        Terrain.flipY = Random.value < 0.5f;
    }
}

[Serializable]
public class CellData
{
    public float BaseTravelCost = -1;

    public CellType CellType;

    public List<ItemData> ContainedItems = new List<ItemData>();

    public Coordinates Coordinates;

    public StockpileData Stockpile;

    public StructureData Structure;

    [JsonIgnore]
    public Cell LinkedGameObject
    {
        get
        {
            return MapGrid.Instance.GetCellAtCoordinate(Coordinates);
        }
    }
}