using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class Move : TaskBase
{
    public Coordinates TargetCoordinates;

    [JsonIgnore] private float _journeyLength;
    [JsonIgnore] private Cell _nextCell;
    [JsonIgnore] private List<Cell> _path = new List<Cell>();
    [JsonIgnore] private float _startTime;
    [JsonIgnore] private Vector3 _targetPos;

    public Move(Coordinates targetCoordinates, int maxSpeed = int.MaxValue)
    {
        TargetCoordinates = targetCoordinates;
        MaxSpeed = maxSpeed;

        _targetCell = MapGrid.Instance.GetCellAtCoordinate(TargetCoordinates);
    }

    public int MaxSpeed { get; set; }
    private Cell _targetCell { get; set; }

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

        if (_nextCell == null)
        {
            var currentCreatureCell = MapGrid.Instance.GetCellAtCoordinate(Creature.Coordinates);

            if (_path == null || _path.Count == 0)
            {
                _path = Pathfinder.FindPath(currentCreatureCell, _targetCell);
            }

            if (_path == null)
            {
                throw new CancelTaskException("Unable to find path");
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
                _targetPos = _nextCell.GetCreaturePosition();

                // calculate the movement journey to the next cell, include the cell travelcost to make moving through
                // difficults cells take longer
                _journeyLength = Vector3.Distance(currentCreatureCell.transform.position, _targetPos) + _nextCell.TravelCost;

                Creature.MoveDirection = MapGrid.Instance.GetDirection(currentCreatureCell, _nextCell);
                _startTime = Time.time;
            }
        }

        if (_nextCell != null && Creature.LinkedGameObject.transform.position != _targetPos)
        {
            // move between two cells
            var distCovered = (Time.time - _startTime) * Mathf.Min(Creature.Speed, MaxSpeed);
            var fracJourney = distCovered / _journeyLength;
            Creature.LinkedGameObject.transform.position = Vector3.Lerp(Creature.CurrentCell.LinkedGameObject.transform.position,
                                      _targetPos,
                                      fracJourney);
        }
        else
        {
            // reached next cell
            Creature.Coordinates = _nextCell.Data.Coordinates;
            _nextCell = null;
            _path = null;
        }
    }
}