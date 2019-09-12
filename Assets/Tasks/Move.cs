using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class Move : Task
{
    public Cell TargetCoordinates;

    [JsonIgnore] private float _journeyLength;
    [JsonIgnore] private Cell _nextCell;
    [JsonIgnore] private List<Cell> _path = new List<Cell>();
    [JsonIgnore] private float _startTime;
    [JsonIgnore] private Vector3 _targetPos;

    public Move()
    {
    }

    public Move(Cell targetCoordinates, int maxSpeed = int.MaxValue)
    {
        TargetCoordinates = targetCoordinates;
        MaxSpeed = maxSpeed;

        Message = $"Moving to {TargetCoordinates}";
    }

    public int MaxSpeed { get; set; }

    public override bool Done()
    {
        if (Creature == null || Creature.Cell == null)
        {
            return false;
        }

        if (Creature == null || Creature.Cell == null)
        {
            return false;
        }

        if (Creature.Cell == TargetCoordinates)
        {
            return true;
        }

        if (_nextCell == null)
        {
            var currentCreatureCell = Creature.Cell;

            if (_path == null || _path.Count == 0)
            {
                _path = Pathfinder.FindPath(currentCreatureCell,
                    TargetCoordinates,
                    Creature.Mobility);
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
            else
            {
                // found valid next cell
                _targetPos = _nextCell.ToMapVector();

                // calculate the movement journey to the next cell, include the cell travelcost to make moving through
                // difficults cells take longer
                _journeyLength = Vector3.Distance(currentCreatureCell.ToMapVector(), _targetPos) + _nextCell.TravelCost;

                Creature.Facing = Game.Map.GetDirection(currentCreatureCell, _nextCell);
                _startTime = Time.time;
            }
        }

        if (_nextCell != null && Creature.CreatureRenderer.transform.position != _targetPos)
        {
            // move between two cells
            var distCovered = (Time.time - _startTime) * Mathf.Min(Creature.Speed, MaxSpeed);
            var fracJourney = distCovered / _journeyLength;
            Creature.CreatureRenderer.transform.position = Vector3.Lerp(Creature.Cell.ToMapVector(),
                                      _targetPos,
                                      fracJourney);
        }
        else
        {
            // reached next cell
            Creature.Cell = _nextCell;
            Creature.CreatureRenderer.MainRenderer.SetBoundMaterial(_nextCell);
            _nextCell = null;
            _path = null;
        }

        return Creature.Cell == TargetCoordinates;
    }
}