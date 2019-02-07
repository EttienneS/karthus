using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cell : MonoBehaviour
{
    internal List<Creature> ContainedCreatures = new List<Creature>();
    internal CellData Data = new CellData();
    internal Cell[] Neighbors = new Cell[8];
    internal Stockpile Stockpile;
    internal Structure Structure;
    private float _lastUpdate;

    private TextMeshPro _textMesh;

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
            if (Structure != null)
            {
                return Structure.Data.TravelCost;
            }

            return Data.BaseTravelCost;
        }
        set
        {
            Data.BaseTravelCost = value;
        }
    }

    public void AddContent(GameObject gameObject, bool scatter = false)
    {
        var item = gameObject.GetComponent<Item>();
        var structure = gameObject.GetComponent<Structure>();
        var stockpile = gameObject.GetComponent<Stockpile>();

        if (item != null)
        {
            if (item.Cell != null)
            {
                item.Cell.Data.ContainedItems.Remove(item.Data);
            }
            Data.ContainedItems.Add(item.Data);
            item.Cell = this;
        }
        else if (structure != null)
        {
            structure.Cell = this;
            Structure = structure;
        }
        else if (stockpile != null)
        {
            stockpile.Cell = this;
            Stockpile = stockpile;
        }

        gameObject.transform.SetParent(transform);
        gameObject.transform.position = transform.position;

        if (scatter)
        {
            gameObject.transform.Rotate(0, 0, Random.Range(-45f, 45f));
            gameObject.transform.position += new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
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

    internal void AddCreature(Creature creature)
    {
        if (creature.Data.CurrentCell != null)
        {
            creature.Data.CurrentCell.LinkedGameObject.RemoveCreature(creature);
        }

        ContainedCreatures.Add(creature);
        creature.Data.CurrentCell = Data;
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

    private void RemoveCreature(Creature creature)
    {
        ContainedCreatures.Remove(creature);
    }
}

[Serializable]
public class CellData
{
    [SerializeField]
    internal float BaseTravelCost = -1;

    [SerializeField]
    internal CellType CellType;

    [SerializeField]
    internal List<ItemData> ContainedItems = new List<ItemData>();

    [SerializeField]
    internal Coordinates Coordinates;

    public Cell LinkedGameObject
    {
        get
        {
            return MapGrid.Instance.GetCellAtCoordinate(Coordinates);
        }
    }
}