using System.Collections;
using System.Collections.Generic;
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

    public SpriteRenderer Border { get; private set; }

    public void SetNeighbor(Direction direction, Cell cell)
    {
        Neighbors[(int)direction] = cell;
        cell.Neighbors[(int)direction.Opposite()] = this;
    }

    void Awake()
    {
        Border = transform.Find("Border").GetComponent<SpriteRenderer>();
    }

    public void DisableBorder()
    {
        Border.enabled = false;
    }

    private void OnMouseDown()
    {
        if (!Border.enabled)
        {
            EnableBorder(Color.red);
        }
        else
        {
            DisableBorder();
        }
    }


    public void EnableBorder(Color color)
    {
        Border.color = color;
        Border.enabled = true;
    }

    void Update()
    {

    }
}
