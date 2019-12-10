using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class Move : CreatureTask
{
    public float TargetX;
    public float TargetY;

    private Cell _nextCell;
    private List<Cell> _path = new List<Cell>();

    public Move()
    {
    }

    public Move(Cell targetCoordinates, int maxSpeed = int.MaxValue) : this()
    {
        TargetX = targetCoordinates.Vector.x;
        TargetY = targetCoordinates.Vector.y;
        MaxSpeed = maxSpeed;

        Message = $"Moving to {TargetX}:{TargetY}";
    }

    public int MaxSpeed { get; set; }

    [JsonIgnore]
    public Cell TargetCell
    {
        get
        {
            return Game.Map.GetCellAtCoordinate(TargetX, TargetY);
        }
    }

    public override bool Done(Creature creature)
    {
        if (creature == null || creature.Cell == null)
        {
            return false;
        }

        var currentCell = creature.Cell;

        if (!TargetCell.Pathable(creature.Mobility))
        {
            Debug.LogError("Target path unreachable");
            throw new TaskFailedException("Unable to find path");
        }

        if (_path == null || _path.Count == 0)
        {
            if (currentCell != TargetCell)
            {
                _path = Pathfinder.FindPath(currentCell, TargetCell, creature.Mobility);

                if (_path == null)
                {
                    Debug.LogError("Path failed");
                    throw new TaskFailedException("Unable to find path");
                }
                else
                {
                    _nextCell = _path[_path.IndexOf(currentCell) - 1];
                }
            }
            else
            {
                // ensure we move to the center of the cell
                _path = new List<Cell> { TargetCell };
                _nextCell = TargetCell;
            }
        }

        if (currentCell == _nextCell)
        {
            // ensure we are right in the middle of the next cell
            if (GapClosed(creature, _nextCell))
            {
                if (currentCell == TargetCell)
                {
                    // final cell reached, done!
                    return true;
                }
                else
                {
                    // next cell reached, move path on to the next one
                    _nextCell = _path[_path.IndexOf(currentCell) - 1];
                }
            }
        }
        else if (!_nextCell.Pathable(creature.Mobility))
        {
            // something changed the path making it unusable
            // reset path to null to allow the pathfinder to search again
            _path = null;
            Debug.LogWarning("Invalid path");
            return false;
        }

        creature.MoveTo(_nextCell.Vector);

        return false;
    }

    private bool GapClosed(Creature creature, Cell target)
    {
        var vx = target.Vector.x;
        var vy = target.Vector.y;

        // close final little gap
        if (vx != creature.X || vy != creature.Y)
        {
            creature.MoveTo(vx, vy);
            return false;
        }
        return true;
    }
}