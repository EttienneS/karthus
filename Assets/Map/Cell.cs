using System;
using TMPro;
using UnityEngine;
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

    public Creature ContainedCreature;

    internal void AddCreature(Creature creature)
    {
        if (creature.CurrentCell != null)
        {
            creature.CurrentCell.RemoveCreature(creature);
        }

        ContainedCreature = creature;
        creature.CurrentCell = this;
        creature.transform.position = transform.position + new Vector3(0, 0, -0.25f);
    }

    private void RemoveCreature(Creature creature)
    {
        ContainedCreature = null;
        creature.CurrentCell = null;
    }

    

    public Coordinates Coordinates;

    public Cell[] Neighbors = new Cell[8];

    public int TravelCost = -1;

    private TextMeshPro _textMesh;
    public SpriteRenderer Border { get; private set; }
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
        if (TravelCost == -1)
        {
            if (Random.value > 0.98f)
            {
                Sprite.sprite = SpriteStore.Instance.GetRandomSpriteOfType("Water");
                RandomlyFlipSprite();
            }
        }
    }

    private void Awake()
    {
        Border = transform.Find("Border").GetComponent<SpriteRenderer>();
        Sprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();

        RandomlyFlipSprite();
    }

    private void OnMouseDown()
    {
        if (this.Border.enabled)
        {
            GameController.Instance.Player.SetTarget(this);
        }
        else
        {
            foreach (var cell in MapGrid.Instance.Map)
            {
                cell.DisableBorder();
                if (MapGrid.Instance.DebugPathfinding)
                {
                    cell.Text = "";
                    cell.Distance = 0;
                }
            }

            Pathfinder.ShowPath(Pathfinder.FindPath(GameController.Instance.Player.CurrentCell, this));

            if (MapGrid.Instance.DebugPathfinding)
            {
                foreach (var cell in MapGrid.Instance.Map)
                {
                    if (cell.Distance != 0)
                    {
                        cell.Text = cell.Distance.ToString();
                    }
                }
            }
        }
    }

    private void RandomlyFlipSprite()
    {
        Sprite.flipX = Random.value < 0.5f;
        Sprite.flipY = Random.value < 0.5f;
    }
}