using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cell : MonoBehaviour
{
    internal List<Creature> ContainedCreatures = new List<Creature>();

    public Coordinates Coordinates;

    internal Cell[] Neighbors = new Cell[8];

    internal float TravelCost = -1;

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

    public float Distance { get; set; }

    //public SpriteRenderer Fog { get; private set; }

    public Cell NextWithSamePriority { get; set; }

    public Cell PathFrom { get; set; }

    public int SearchHeuristic { private get; set; }

    public int SearchPhase { get; set; }

    public int SearchPriority => (int)Distance + SearchHeuristic;

    public bool Filled { get; set; }
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

    public void AddContent(GameObject gameObject, bool scatter = false)
    {
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
        //if (!Fog.enabled)
        //{
        // animate cells if fog is nog enabled
        if (CellType == CellType.Water)
        {
            if (Random.value > 0.98f)
            {
                CellType = CellType.Water;
            }
        }
        //}
    }

    internal void AddCreature(Creature creature)
    {
        if (creature.CurrentCell != null)
        {
            creature.CurrentCell.RemoveCreature(creature);
        }

        ContainedCreatures.Add(creature);
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