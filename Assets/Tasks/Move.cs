﻿using System.Collections.Generic;
using UnityEngine;

public class Move : CreatureTask
{
    public Cell TargetCoordinates;

    private float _journeyLength;
    private Cell _nextCell;
    private List<Cell> _path = new List<Cell>();
    private float _startTime;
    private Vector3 _targetPos;

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

    public override bool Done(Creature creature)
    {
        if (creature == null || creature.Cell == null)
        {
            return false;
        }

        if (creature.Cell == TargetCoordinates)
        {
            return true;
        }

        if (_nextCell == null)
        {
            var currentCreatureCell = creature.Cell;

            if (_path == null || _path.Count == 0)
            {
                _path = Pathfinder.FindPath(currentCreatureCell,
                    TargetCoordinates,
                    creature.Mobility);
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

                creature.Facing = Game.Map.GetDirection(currentCreatureCell, _nextCell);
                _startTime = Time.time;
            }
        }

        if (creature.Cell == _nextCell)
        {
            _nextCell = null;
            _path = null;
        }
        else
        {
            creature.MoveTo(_nextCell);
        }

        return creature.Cell == TargetCoordinates;
    }
}