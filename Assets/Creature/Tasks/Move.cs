using System.Collections.Generic;
using System.Linq;
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

    public Move(Cell targetCell)
    {
        TargetCell = targetCell;
        TaskId = $"Move to {TargetCell}";
    }

    public Queue<ITask> SubTasks { get; set; }
    public Cell TargetCell { get; set; }
    public string TaskId { get; set; }

    public bool Done()
    {
        return Creature.CurrentCell == TargetCell;
    }

    public override string ToString()
    {
        return $"Moving to {TargetCell}";
    }

    public void Update()
    {
        if (Creature.CurrentCell != TargetCell)
        {
            if (NextCell == null)
            {
                if (Path == null || !Path.Any())
                {
                    Path = Pathfinder.FindPath(Creature.CurrentCell, TargetCell);
                }

                if (Path == null)
                {
                    // failure, task is no longer possible
                    Pathfinder.InvalidPath(Creature.CurrentCell, TargetCell);
                    _navigationFailureCount++;

                    if (_navigationFailureCount > 10)
                    {
                        _navigationFailureCount = 0;
                        // failed to find a path too many times, short circuit
                        TargetCell = Creature.CurrentCell;
                        return;
                    }
                }

                NextCell = Path[Path.IndexOf(Creature.CurrentCell) - 1];
                if (NextCell.TravelCost < 0)
                {
                    // something changed the path making it unusable
                    Pathfinder.InvalidPath(Creature.CurrentCell, TargetCell);
                    Path = null;
                }
                else
                {
                    // found valid next cell
                    targetPos = NextCell.GetCreaturePosition();

                    // calculate the movement journey to the next cell, include the cell travelcost to make moving through
                    // difficults cells take longer
                    _journeyLength = Vector3.Distance(Creature.CurrentCell.transform.position, targetPos) + NextCell.TravelCost;

                    if (Creature.SpriteAnimator != null)
                    {
                        Creature.SpriteAnimator.MoveDirection = MapGrid.Instance.GetDirection(Creature.CurrentCell, NextCell);
                    }
                    startTime = Time.time;
                }
            }

            if (NextCell != null && Creature.transform.position != targetPos)
            {
                // move between two cells
                var distCovered = (Time.time - startTime) * Creature.Speed;
                var fracJourney = distCovered / _journeyLength;
                Creature.transform.position = Vector3.Lerp(Creature.CurrentCell.transform.position,
                                          targetPos,
                                          fracJourney);
            }
            else
            {
                // reached next cell
                NextCell.AddCreature(Creature);
                Creature.See();

                NextCell = null;
                Path = null;
            }
        }
    }
}