using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinder
{
    private static CellPriorityQueue _searchFrontier;
    private static int _searchFrontierPhase;

   
    public static List<Cell> FindPath(Cell fromCell, Cell toCell)
    {
        var path = new List<Cell>
        {
            fromCell
        };

        if (fromCell != null && toCell != null)
        {
            if (Search(fromCell, toCell))
            {
                var current = toCell;
                while (current != fromCell)
                {
                    current = current.PathFrom;
                    path.Add(current);
                }

                path.Add(toCell);
            }
        }

        return path;
    }

    public static Cell GetClosestOpenCell(Cell[] map, Cell origin)
    {
        return GetReachableCells(map, origin, 3).FirstOrDefault();
    }

    public static int GetPathCost(IEnumerable<Cell> path)
    {
        return path.Where(cell => cell != null).Sum(cell => cell.TravelCost);
    }

    public static IEnumerable<Cell> GetReachableCells(Cell[] map, Cell actorLocation, int speed)
    {
        if (actorLocation == null)
        {
            return new List<Cell>();
        }

        var reachableCells = (from cell in
                    map.Where(c =>
                        c != null && c.Coordinates.DistanceTo(actorLocation.Coordinates) <= speed)
                              let path = FindPath(actorLocation, cell)
                              let pathCost = GetPathCost(path) - actorLocation.TravelCost
                              where path.Count > 0 && pathCost <= speed
                              select cell)
            .Distinct()
            .ToList();

        reachableCells.Remove(actorLocation);

        return reachableCells;
    }

    public static void ShowPath(List<Cell> path)
    {
        foreach (var cell in path)
        {
            cell.EnableBorder(Color.white);
        }

        path[0].EnableBorder(Color.red);
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

            for (var d = Direction.NE; d <= Direction.NW; d++)
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

                if (neighbor.Coordinates.X != current.Coordinates.X && neighbor.Coordinates.Y != current.Coordinates.Y)
                {
                    // if both are not true then we are moving diagonally (add 50% cost)
                    neighborTravelCost *= 2;
                }

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