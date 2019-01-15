using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cell : MonoBehaviour
{
    public List<Creature> ContainedCreature = new List<Creature>();

    public Coordinates Coordinates;

    public Cell[] Neighbors = new Cell[8];

    public int TravelCost = -1;

    private CellType _cellType;

    private TextMeshPro _textMesh;

    public SpriteRenderer Border { get; private set; }

    public List<GameObject> CellContents
    {
        get
        {
            var allChildren = new List<GameObject>();

            foreach (Transform child in transform)
            {
                if (child.gameObject.name == "CellStructure")
                {
                    continue;
                }

                allChildren.Add(child.gameObject);
            }
            return allChildren;
        }
    }

    public CellType CellType
    {
        get
        {
            return _cellType;
        }
        set
        {
            _cellType = value;

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

            Terrain.sprite = SpriteStore.Instance.GetRandomSpriteOfType(_cellType);
            RandomlyFlipSprite();
        }
    }

    public int Distance { get; set; }

    public SpriteRenderer Fog { get; private set; }

    public Cell NextWithSamePriority { get; set; }

    public Cell PathFrom { get; set; }

    public int SearchHeuristic { private get; set; }

    public int SearchPhase { get; set; }

    public int SearchPriority => Distance + SearchHeuristic;

    public Structure Structure { get; set; }
    public SpriteRenderer Terrain { get; private set; }

    public string Text
    {
        get
        {
            return TextMesh.text;
        }
        set
        {
            TextMesh.enabled = true;
            TextMesh.text = value;
        }
    }

    public TextMeshPro TextMesh
    {
        get
        {
            if (_textMesh == null)
            {
                _textMesh = transform.Find("Text").GetComponent<TextMeshPro>();
            }
            return _textMesh;
        }
    }

    public void AddContent(GameObject gameObject, bool rotateRandomly = false)
    {
        gameObject.transform.SetParent(transform);
        gameObject.transform.position = transform.position;

        if (rotateRandomly)
        {
            gameObject.transform.Rotate(0, 0, Random.Range(-30f, 30f));
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
        if (!Fog.enabled)
        {
            // animate cells if fog is nog enabled
            if (CellType == CellType.Water)
            {
                if (Random.value > 0.98f)
                {
                    CellType = CellType.Water;
                }
            }
        }
    }

    internal void AddCreature(Creature creature)
    {
        if (creature.CurrentCell != null)
        {
            creature.CurrentCell.RemoveCreature(creature);
        }

        ContainedCreature.Add(creature);
        creature.CurrentCell = this;
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

        Fog = gridStructure.transform.Find("Fog").GetComponent<SpriteRenderer>();
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
        ContainedCreature.Remove(creature);
    }
}