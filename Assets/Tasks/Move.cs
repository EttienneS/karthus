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

    public Move(Cell targetCoordinates, int maxSpeed = int.MaxValue)
    {
        TargetX = targetCoordinates.Vector.x;
        TargetY = targetCoordinates.Vector.y;
        MaxSpeed = maxSpeed;

        Message = $"Moving to {TargetX}:{TargetY}";
    }

    public int MaxSpeed { get; set; }

    [JsonIgnore]
    public Cell TargetCoordinateCell
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

        if (creature.Cell == TargetCoordinateCell)
        {
            // close final little gap
            if (TargetX != creature.X || TargetY != creature.Y)
            {
                creature.MoveTo(TargetX, TargetY);
                return false;
            }
            return true;
        }

        if (_nextCell == null)
        {
            var currentCreatureCell = creature.Cell;

            if (_path == null || _path.Count == 0)
            {
                _path = Pathfinder.FindPath(currentCreatureCell, TargetCoordinateCell, creature.Mobility);
            }

            if (_path == null)
            {
                throw new TaskFailedException("Unable to find path");
            }

            _nextCell = _path[_path.IndexOf(currentCreatureCell) - 1];
            if (_nextCell.TravelCost < 0)
            {
                // something changed the path making it unusable
                _path = null;
            }
        }

        if (creature.Cell == _nextCell)
        {
            _nextCell = null;
            _path = null;
        }
        else
        {
            creature.MoveTo(_nextCell.Vector);
        }


        return false;
    }
}