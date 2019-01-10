using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public Cell CurrentCell;

    public Cell TargetCell;

    private List<Cell> Path = new List<Cell>();

    public SpriteAnimator SpriteAnimator;

    public void Start()
    {
        SpriteAnimator = GetComponent<SpriteAnimator>();
    }

    public void Act()
    {
        if (TargetCell != null && CurrentCell != TargetCell && Path != null)
        {
            var nextStep = Path[Path.IndexOf(CurrentCell) - 1];

            if (nextStep.TravelCost < 0)
            {
                Pathfinder.InvalidPath(CurrentCell, TargetCell);
                Path = Pathfinder.FindPath(CurrentCell, TargetCell);
            }
            else
            {
                MoveToCell(nextStep);
            }
        }
    }

    private float speed = 6f;
    private float startTime;
    private float journeyLength;
    private Vector3 startPos;
    private Vector3 endPos;

    public void Update()
    {
        if (startPos != endPos)
        {
            // Distance moved = time * speed.
            var distCovered = (Time.time - startTime) * speed;

            // Fraction of journey completed = current distance divided by total distance.
            var fracJourney = distCovered / journeyLength;

            // Set our position as a fraction of the distance between the markers.

            var lerped = Vector3.Lerp(startPos, CurrentCell.transform.position, fracJourney);
            transform.position = new Vector3(lerped.x, lerped.y, -0.25f);
        }
    }

    public void MoveToCell(Cell cell)
    {
        if (SpriteAnimator != null)
        {
            SpriteAnimator.MoveDirection = MapGrid.Instance.GetDirection(CurrentCell, cell);
        }

        cell.AddCreature(this);
        startTime = Time.time;
        startPos = transform.position;
        endPos = new Vector3(cell.transform.position.x, cell.transform.position.y, -0.25f);
        journeyLength = Vector3.Distance(startPos, endPos);

        foreach (var c in MapGrid.Instance.GetCircle(cell, 5))
        {
            c.Fog.enabled = false;
        }
    }

    public void SetTarget(Cell cell)
    {
        TargetCell = cell;
        Path = Pathfinder.FindPath(CurrentCell, TargetCell);
    }
}