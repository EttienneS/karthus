using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public Cell CurrentCell;

    public Cell NextCell;

    public float Speed = 5f;
    public Cell TargetCell;

    public ITask Task;
    private float _journeyLength;
    private List<Cell> Path = new List<Cell>();

    private SpriteAnimator SpriteAnimator;
    private float startTime;

    private Vector3 targetPos;

    public void See()
    {
        foreach (var c in MapGrid.Instance.GetCircle(CurrentCell, 5))
        {
            c.Fog.enabled = false;
        }
    }

    public void SetTarget(Cell cell)
    {
        TargetCell = cell;
    }

    public void Start()
    {
        SpriteAnimator = GetComponent<SpriteAnimator>();
    }
    public void Update()
    {
        if (MoveToTargetCell())
        {
            Task = null;
        }
    }

    private bool MoveToTargetCell()
    {
        if (TargetCell == null)
        {
            // at the cell 
            return true;
        }

        if (CurrentCell != TargetCell)
        {
            if (NextCell == null)
            {
                if (Path == null || !Path.Any())
                {
                    Path = Pathfinder.FindPath(CurrentCell, TargetCell);
                }

                if (Path == null)
                {
                    // failure, task is no longer possible
                    Pathfinder.InvalidPath(CurrentCell, TargetCell);
                    return true;
                }

                NextCell = Path[Path.IndexOf(CurrentCell) - 1];
                if (NextCell.TravelCost < 0)
                {
                    // something changed the path making it unusable
                    Pathfinder.InvalidPath(CurrentCell, TargetCell);
                    Path = null;
                }
                else
                {
                    // found valid next cell
                    targetPos = NextCell.GetCreaturePosition();

                    // calculate the movement journey to the next cell, include the cell travelcost to make moving through
                    // difficults cells take longer
                    _journeyLength = Vector3.Distance(CurrentCell.transform.position, targetPos) + NextCell.TravelCost;

                    if (SpriteAnimator != null)
                    {
                        SpriteAnimator.MoveDirection = MapGrid.Instance.GetDirection(CurrentCell, NextCell);
                    }
                    startTime = Time.time;
                }
            }

            if (NextCell != null && transform.position != targetPos)
            {
                // move between two cells
                var distCovered = (Time.time - startTime) * Speed;
                var fracJourney = distCovered / _journeyLength;
                transform.position = Vector3.Lerp(CurrentCell.transform.position,
                                          targetPos,
                                          fracJourney);
            }
            else
            {
                // reached next cell
                See();
                NextCell.AddCreature(this);
                NextCell = null;
                Path = null;
            }

            //  not yet at final destination
            return false;
        }
        else
        {
            // final destination reached or is unreachable
            return true;
        }
    }

    internal void DoTask()
    {
        Task.DoTask(this);
    }
}