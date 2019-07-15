using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinder
{
    private static CellPriorityQueue _searchFrontier;
    private static int _searchFrontierPhase;

    internal static float Distance(CellData fromCell, CellData toCell, Mobility mobility)
    {
        var distance = 0f;

        try
        {
            foreach (var cell in FindPath(fromCell, toCell, mobility))
            {
                distance += cell.TravelCost;
            }

            return distance;
        }
        catch
        {
            return float.MaxValue;
        }
    }

    public static List<CellData> FindPath(CellData fromCell, CellData toCell, Mobility mobility)
    {
        if (fromCell != null && toCell != null)
        {
            if (Search(fromCell, toCell, mobility))
            {
                var path = new List<CellData>
                {
                    toCell
                };

                var current = toCell;
                while (current != fromCell)
                {
                    current = current.PathFrom;
                    path.Add(current);
                }

                return path;
            }
        }

        return null;
    }

    public static CellData GetClosestOpenCell(CellData[] map, CellData origin, Mobility mobility)
    {
        return GetReachableCells(map, origin, 3, mobility).FirstOrDefault();
    }

    public static float GetPathCost(IEnumerable<CellData> path)
    {
        return path.Where(cell => cell != null).Sum(cell => cell.TravelCost);
    }

    public static IEnumerable<CellData> GetReachableCells(CellData[] map, CellData startPoint, int speed, Mobility mobility)
    {
        if (startPoint == null)
        {
            return new List<CellData>();
        }

        var reachableCells = (from cell in
                    map.Where(c =>
                        c != null && c.Coordinates.DistanceTo(startPoint.Coordinates) <= speed)
                              let path = FindPath(startPoint, cell, mobility)
                              let pathCost = GetPathCost(path) - startPoint.TravelCost
                              where path.Count > 0 && pathCost <= speed
                              select cell)
            .Distinct()
            .ToList();

        reachableCells.Remove(startPoint);

        return reachableCells;
    }

    internal static float GetPathCost(CellData fromCell, CellData toCell, Mobility mobility)
    {
        var path = FindPath(fromCell, toCell, mobility);

        if (path == null)
        {
            return float.MaxValue;
        }

        return GetPathCost(path);
    }

    private static bool Search(CellData fromCell, CellData toCell, Mobility mobility)
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

                if ((mobility != Mobility.Fly) && neighbor.TravelCost < 0)
                {
                    continue;
                }

                var neighborTravelCost = neighbor.TravelCost;

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