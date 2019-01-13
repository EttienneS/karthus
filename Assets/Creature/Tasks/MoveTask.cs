
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveTask : ITask
{
    public Cell NextCell;
    private float _journeyLength;
    private List<Cell> Path = new List<Cell>();
    private float startTime;
    private Vector3 targetPos;

    public MoveTask(Cell targetCell)
    {
        TargetCell = targetCell;
        TaskId = $"Move to {TargetCell}";
    }

    public Cell TargetCell { get; set; }
    public string TaskId { get; set; }
    public bool Done(Creature creature)
    {
        return creature.CurrentCell == TargetCell;
    }

    public void Start(Creature creature)
    {
    }

    private int _navigationFailureCount;

    public void Update(Creature creature)
    {
        if (creature.CurrentCell != TargetCell)
        {
            if (NextCell == null)
            {
                if (Path == null || !Path.Any())
                {
                    Path = Pathfinder.FindPath(creature.CurrentCell, TargetCell);
                }

                if (Path == null)
                {
                    // failure, task is no longer possible
                    Pathfinder.InvalidPath(creature.CurrentCell, TargetCell);
                    _navigationFailureCount++;

                    if (_navigationFailureCount > 10)
                    {
                        _navigationFailureCount = 0;
                        // failed to find a path too many times, short circuit
                        TargetCell = creature.CurrentCell;
                        return;
                    }
                }

                NextCell = Path[Path.IndexOf(creature.CurrentCell) - 1];
                if (NextCell.TravelCost < 0)
                {
                    // something changed the path making it unusable
                    Pathfinder.InvalidPath(creature.CurrentCell, TargetCell);
                    Path = null;
                }
                else
                {
                    // found valid next cell
                    targetPos = NextCell.GetCreaturePosition();

                    // calculate the movement journey to the next cell, include the cell travelcost to make moving through
                    // difficults cells take longer
                    _journeyLength = Vector3.Distance(creature.CurrentCell.transform.position, targetPos) + NextCell.TravelCost;

                    if (creature.SpriteAnimator != null)
                    {
                        creature.SpriteAnimator.MoveDirection = MapGrid.Instance.GetDirection(creature.CurrentCell, NextCell);
                    }
                    startTime = Time.time;
                }
            }

            if (NextCell != null && creature.transform.position != targetPos)
            {
                // move between two cells
                var distCovered = (Time.time - startTime) * creature.Speed;
                var fracJourney = distCovered / _journeyLength;
                creature.transform.position = Vector3.Lerp(creature.CurrentCell.transform.position,
                                          targetPos,
                                          fracJourney);
            }
            else
            {
                // reached next cell
                NextCell.AddCreature(creature);
                creature.See();

                NextCell = null;
                Path = null;
            }
        }
    }

    public override string ToString()
    {
        return $"Moving to {TargetCell}";
    }

}
