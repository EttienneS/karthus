using System.Collections.Generic;
using UnityEngine;

public class Move : TaskBase
{
    public Cell NextCell;
    private float _journeyLength;
    private List<Cell> Path = new List<Cell>();
    private float startTime;
    private Vector3 targetPos;
    private Cell _targetCell { get; set; }

    public Move(Coordinates targetCoordinates, int maxSpeed = int.MaxValue)
    {
        TargetCoordinates = targetCoordinates;
        TaskId = $"Move to {targetCoordinates}";
        MaxSpeed = maxSpeed;

        _targetCell = MapGrid.Instance.GetCellAtCoordinate(TargetCoordinates);
    }

    public Coordinates TargetCoordinates;

    

    public int MaxSpeed { get; set; }

    public override bool Done()
    {
        return Creature.Coordinates == TargetCoordinates;
    }

    public override string ToString()
    {
        return $"Moving to {TargetCoordinates}";
    }

    public override void Update()
    {
        if (Creature.Coordinates == TargetCoordinates)
            return;

        if (NextCell == null)
        {
            var currentCreatureCell = MapGrid.Instance.GetCellAtCoordinate(Creature.Coordinates);

            if (Path == null || Path.Count == 0)
            {
                Path = Pathfinder.FindPath(currentCreatureCell, _targetCell);
            }

            if (Path == null)
            {
                throw new CancelTaskException("Unable to find path");
            }

            NextCell = Path[Path.IndexOf(currentCreatureCell) - 1];
            if (NextCell.TravelCost < 0)
            {
                // something changed the path making it unusable
                Path = null;
            }
            else
            {
                // found valid next cell
                targetPos = NextCell.GetCreaturePosition();

                // calculate the movement journey to the next cell, include the cell travelcost to make moving through
                // difficults cells take longer
                _journeyLength = Vector3.Distance(currentCreatureCell.transform.position, targetPos) + NextCell.TravelCost;

                Creature.MoveDirection = MapGrid.Instance.GetDirection(currentCreatureCell, NextCell);
                startTime = Time.time;
            }
        }

        if (NextCell != null && Creature.LinkedGameObject.transform.position != targetPos)
        {
            // move between two cells
            var distCovered = (Time.time - startTime) * Mathf.Min(Creature.Speed, MaxSpeed);
            var fracJourney = distCovered / _journeyLength;
            Creature.LinkedGameObject.transform.position = Vector3.Lerp(Creature.CurrentCell.LinkedGameObject.transform.position,
                                      targetPos,
                                      fracJourney);
        }
        else
        {
            // reached next cell
            Creature.Coordinates = NextCell.Data.Coordinates;
            NextCell = null;
            Path = null;
        }
    }
}