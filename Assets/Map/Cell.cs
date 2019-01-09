using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class Cell : MonoBehaviour
{
    // A generic container for a block of 2D space in the game world
    // Supports:
    //      World context (neighbouring cells)
    //      Pathfinding (A*)
    //          SearchHeuristic
    //          SearchPhase
    //          SearchPriority
    //          Distance
    //          PathOrigin
    //          PathFrom
    //          NextWithSamePriority
    //      Containing other game objects and controlling their layer/visibility
    //      'Fog of War' states (unexplored, known, visible)
    //      Highlighting/Selection
    //      Borders (selectively highlighting certain edges)

    public List<Creature> ContainedCreature = new List<Creature>();

    internal void AddCreature(Creature creature)
    {
        if (creature.CurrentCell != null)
        {
            creature.CurrentCell.RemoveCreature(creature);
        }

        ContainedCreature.Add(creature);
        creature.CurrentCell = this;
        creature.transform.position = transform.position + new Vector3(0, 0, -0.25f);
    }

    private void RemoveCreature(Creature creature)
    {
        ContainedCreature.Remove(creature);
    }

    public Coordinates Coordinates;

    public Cell[] Neighbors = new Cell[8];

    public int TravelCost = -1;

    private TextMeshPro _textMesh;
    public SpriteRenderer Border { get; private set; }
    public SpriteRenderer Fog { get; private set; }
    public int Distance { get; set; }
    public Cell NextWithSamePriority { get; set; }
    public Cell PathFrom { get; set; }
    public int SearchHeuristic { private get; set; }
    public int SearchPhase { get; set; }
    public int SearchPriority => Distance + SearchHeuristic;
    public SpriteRenderer Sprite { get; private set; }
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
            if (TravelCost == -1)
            {
                if (Random.value > 0.98f)
                {
                    CellType = CellType.Water;
                }
            }
        }
    }

    private void Awake()
    {
        Fog = transform.Find("Fog").GetComponent<SpriteRenderer>();
        Border = transform.Find("Border").GetComponent<SpriteRenderer>();
        Sprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
    }

    internal int CountNeighborsOfType(CellType? cellType)
    {
        if (!cellType.HasValue)
        {
            return Neighbors.Count(n => n == null);
        }

        return Neighbors.Count(n => n != null && n.CellType == cellType.Value);
    }



    private void OnMouseDown()
    {
        foreach (var creature in CreatureController.Instance.Creatures)
        {
            creature.SetTarget(this);
        }
    }

    private CellType _cellType;
    public CellType CellType
    {
        get
        {
            return _cellType;
        }
        set
        {
            _cellType = value;

            if (value == CellType.Water)
            {
                TravelCost = -1;
            }
            else
            {
                TravelCost = 1;
            }

            Sprite.sprite = SpriteStore.Instance.GetRandomSpriteOfType(_cellType);
            RandomlyFlipSprite();
        }
    }

    private void RandomlyFlipSprite()
    {
        Sprite.flipX = Random.value < 0.5f;
        Sprite.flipY = Random.value < 0.5f;
    }
}