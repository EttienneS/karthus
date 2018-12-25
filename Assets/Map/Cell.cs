using TMPro;
using UnityEngine;

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

    public Coordinates Coordinates;

    public Cell[] Neighbors = new Cell[8];

    public int TravelCost = 1;
    public SpriteRenderer Border { get; private set; }
    public int Distance { get; set; }
    public Cell NextWithSamePriority { get; set; }
    public Cell PathFrom { get; set; }

    public int SearchHeuristic { private get; set; }

    public int SearchPhase { get; set; }

    public int SearchPriority => Distance + SearchHeuristic;

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

    private void Awake()
    {
        
        Border = transform.Find("Border").GetComponent<SpriteRenderer>();
    }

    private TextMeshPro _textMesh;
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

    private void OnMouseDown()
    {
        var manager = GameObject.Find("MapManager").GetComponent<MapGrid>();

        if (manager.Flip)
        {
            manager.Cell1 = this;
            manager.Flip = false;
        }
        else
        {
            manager.Cell2 = this;
            manager.Flip = true;
        }

        foreach (var cell in manager.Map)
        {
            cell.DisableBorder();
        }

        Pathfinder.ShowPath(Pathfinder.FindPath(manager.Cell1, manager.Cell2));
    }

    private void Update()
    {
    }
}