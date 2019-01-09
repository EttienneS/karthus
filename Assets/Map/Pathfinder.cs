using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class Pathfinder
{
    private static CellPriorityQueue _searchFrontier;
    private static int _searchFrontierPhase;

    private static Dictionary<string, List<Cell>> _pathCache = new Dictionary<string, List<Cell>>();

    public static void FlushPathCache()
    {
        _pathCache = new Dictionary<string, List<Cell>>();
    }

    public static void InvalidPath(Cell fromCell, Cell toCell)
    {
        var pathId = fromCell.name + toCell.name;
        var pathIdInverse = toCell.name + fromCell.name;

        if (_pathCache.ContainsKey(pathId))
        {
            _pathCache.Remove(pathId);
        }

        if (_pathCache.ContainsKey(pathIdInverse))
        {
            _pathCache.Remove(pathIdInverse);
        }
    }

    public static List<Cell> FindPath(Cell fromCell, Cell toCell)
    {
        if (fromCell != null && toCell != null)
        {
            var pathId = fromCell.name + toCell.name;
            var pathIdInverse = toCell.name + fromCell.name;

            if (!_pathCache.ContainsKey(pathId))
            {
                if (_pathCache.ContainsKey(pathIdInverse))
                {
                    var p = _pathCache[pathIdInverse].Select(c => c).ToList();
                    p.Reverse();
                    _pathCache.Add(pathId, p);
                }
                else
                {
                    var path = new List<Cell>();

                    if (Search(fromCell, toCell))
                    {
                        path.Add(toCell);

                        var current = toCell;
                        while (current != fromCell)
                        {
                            current = current.PathFrom;
                            path.Add(current);
                        }

                        _pathCache.Add(pathId, path);
                    }
                    else
                    {
                        _pathCache.Add(pathId, null);
                    }
                }
            }
            return _pathCache[pathId];
        }

        return null;
    }

    public static Cell GetClosestOpenCell(Cell[] map, Cell origin)
    {
        return GetReachableCells(map, origin, 3).FirstOrDefault();
    }

    public static int GetPathCost(IEnumerable<Cell> path)
    {
        return path.Where(cell => cell != null).Sum(cell => cell.TravelCost);
    }

    public static IEnumerable<Cell> GetReachableCells(Cell[] map, Cell startPoint, int speed)
    {
        if (startPoint == null)
        {
            return new List<Cell>();
        }

        var reachableCells = (from cell in
                    map.Where(c =>
                        c != null && c.Coordinates.DistanceTo(startPoint.Coordinates) <= speed)
                              let path = FindPath(startPoint, cell)
                              let pathCost = GetPathCost(path) - startPoint.TravelCost
                              where path.Count > 0 && pathCost <= speed
                              select cell)
            .Distinct()
            .ToList();

        reachableCells.Remove(startPoint);

        return reachableCells;
    }

    public static void ShowPath(List<Cell> path)
    {
        foreach (var cell in path)
        {
            cell.EnableBorder(Color.white);
        }

        path.First().EnableBorder(Color.red);
        path.Last().EnableBorder(Color.blue);
    }

    private static bool Search(Cell fromCell, Cell toCell)
    {
        _searchFrontierPhase += 2;

        if (_searchFrontier == null)
        {
            _searchFrontier = new CellPriorityQueue();
        }
        else
        {
            _searchFrontier.Clear();
        }

        fromCell.SearchPhase = _searchFrontierPhase;
        fromCell.Distance = 0;
        _searchFrontier.Enqueue(fromCell);

        while (_searchFrontier.Count > 0)
        {
            var current = _searchFrontier.Dequeue();
            current.SearchPhase++;

            if (current == toCell)
            {
                return true;
            }

            for (var d = Direction.N; d <= Direction.NW; d++)
            {
                var neighbor = current.GetNeighbor(d);
                if (neighbor == null
                    || neighbor.SearchPhase > _searchFrontierPhase)
                {
                    continue;
                }

                if (neighbor.TravelCost < 0)
                {
                    continue;
                }

                var neighborTravelCost = neighbor.TravelCost;

                // todo: Revist later, the concept of diagonal movement costing more _can_ be cool but maybe not 
                // worth the extra complexity to understand
                //if (neighbor.Coordinates.X != current.Coordinates.X && neighbor.Coordinates.Y != current.Coordinates.Y)
                //{
                //    // if both are not true then we are moving diagonally (add 50% cost)
                //    neighborTravelCost = Mathf.FloorToInt(neighborTravelCost * 1.5f);
                //}

                var distance = current.Distance + neighborTravelCost;
                if (neighbor.SearchPhase < _searchFrontierPhase)
                {
                    neighbor.SearchPhase = _searchFrontierPhase;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic = neighbor.Coordinates.DistanceTo(toCell.Coordinates);
                    _searchFrontier.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
                {
                    var oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    _searchFrontier.Change(neighbor, oldPriority);
                }
            }
        }

        return false;
    }
}