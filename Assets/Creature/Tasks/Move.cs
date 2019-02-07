using System;
using System.Collections.Generic;
using UnityEngine;

public class Move : ITask
{
    public Cell NextCell;
    public Creature Creature { get; set; }
    private float _journeyLength;
    private int _navigationFailureCount;
    private List<Cell> Path = new List<Cell>();
    private float startTime;
    private Vector3 targetPos;

    public Move(Cell targetCell, int maxSpeed = int.MaxValue)
    {
        TargetCell = targetCell;
        TaskId = $"Move to {TargetCell}";
        MaxSpeed = maxSpeed;
    }

    public Queue<ITask> SubTasks { get; set; }
    public Cell TargetCell { get; set; }
    public string TaskId { get; set; }

    public int MaxSpeed { get; set; }

    public bool Done()
    {
        return Creature.Data.CurrentCell.LinkedGameObject == TargetCell;
    }

    public override string ToString()
    {
        return $"Moving to {TargetCell}";
    }

    public void Update()
    {
        if (Creature.Data.CurrentCell.LinkedGameObject != TargetCell)
        {
            if (NextCell == null)
            {
                if (Path == null || Path.Count == 0)
                {
                    Path = Pathfinder.FindPath(Creature.Data.CurrentCell.LinkedGameObject, TargetCell);
                }

                if (Path == null)
                {
                    throw new CancelTaskException("Unable to find path");
                }

                NextCell = Path[Path.IndexOf(Creature.Data.CurrentCell.LinkedGameObject) - 1];
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
                    _journeyLength = Vector3.Distance(Creature.Data.CurrentCell.LinkedGameObject.transform.position, targetPos) + NextCell.TravelCost;

                    if (Creature.SpriteAnimator != null)
                    {
                        Creature.SpriteAnimator.MoveDirection = MapGrid.Instance.GetDirection(Creature.Data.CurrentCell.LinkedGameObject, NextCell);
                    }
                    startTime = Time.time;
                }
            }

            if (NextCell != null && Creature.transform.position != targetPos)
            {
                // move between two cells
                var distCovered = (Time.time - startTime) * Mathf.Min(Creature.Data.Speed, MaxSpeed);
                var fracJourney = distCovered / _journeyLength;
                Creature.transform.position = Vector3.Lerp(Creature.Data.CurrentCell.LinkedGameObject.transform.position,
                                          targetPos,
                                          fracJourney);
            }
            else
            {
                // reached next cell
                NextCell.MoveToCell(Creature);

                NextCell = null;
                Path = null;
            }
        }
    }
}