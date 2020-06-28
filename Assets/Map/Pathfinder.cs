using Assets.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinder
{
    private static Dictionary<string, List<Cell>> _pathCache = new Dictionary<string, List<Cell>>();
    private static CellPriorityQueue _searchFrontier;
    private static int _searchFrontierPhase;

    public static List<Cell> FindPath(Cell fromCell, Cell toCell, Mobility mobility)
    {
        var id = $"{fromCell}_{toCell}_{mobility}";

        using (Instrumenter.Start(id))
        {
            if (fromCell != null && toCell != null)
            {
                if (_pathCache.ContainsKey(id))
                {
                    Debug.Log("Use cached path");
                    return _pathCache[id];
                }
                else
                {
                    if (Search(fromCell, toCell, mobility))
                    {
                        var path = new List<Cell> { toCell };

                        var current = toCell;
                        while (current != fromCell)
                        {
                            current = current.PathFrom;
                            path.Add(current);
                        }

                        CachePath(fromCell, toCell, mobility, path);
                        return path;
                    }
                    else
                    {
                        InvalidatePath(fromCell, toCell, mobility);
                    }
                }
            }

            return null;
        }
    }

    public static Cell GetClosestOpenCell(Cell[] map, Cell origin, Mobility mobility)
    {
        return GetReachableCells(map, origin, 3, mobility).FirstOrDefault();
    }

    public static float GetPathCost(IEnumerable<Cell> path)
    {
        return path.Where(cell => cell != null).Sum(cell => cell.TravelCost);
    }

    public static IEnumerable<Cell> GetReachableCells(Cell[] map, Cell startPoint, int speed, Mobility mobility)
    {
        if (startPoint == null)
        {
            return new List<Cell>();
        }

        var reachableCells = EnumerateCells(map, startPoint, speed, mobility)
                                            .Distinct()
                                            .ToList();

        reachableCells.Remove(startPoint);

        return reachableCells;
    }

    internal static float Distance(Cell fromCell, Cell toCell, Mobility mobility)
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

    internal static float GetPathCost(Cell fromCell, Cell toCell, Mobility mobility)
    {
        var path = FindPath(fromCell, toCell, mobility);

        if (path == null)
        {
            return float.MaxValue;
        }

        return GetPathCost(path);
    }

    private static void CachePath(Cell fromCell, Cell toCell, Mobility mobility, List<Cell> path)
    {
        var id = $"{fromCell}_{toCell}_{mobility}";
        var reverse = $"{toCell}_{fromCell}_{mobility}";

        if (_pathCache.ContainsKey(id))
        {
            _pathCache[id] = path;
        }
        else
        {
            _pathCache.Add(id, path);
        }

        path.Reverse();
        if (_pathCache.ContainsKey(reverse))
        {
            _pathCache[reverse] = path;
        }
        else
        {
            _pathCache.Add(reverse, path);
        }
    }

    private static IEnumerable<Cell> EnumerateCells(Cell[] map, Cell startPoint, int speed, Mobility mobility)
    {
        foreach (var cell in map.Where(c => c != null &&
                                            c.DistanceTo(startPoint) <= speed))
        {
            var path = FindPath(startPoint, cell, mobility);
            var pathCost = GetPathCost(path) - startPoint.TravelCost;
            if (path.Count > 0 && pathCost <= speed)
            {
                yield return cell;
            }
        }
    }

    private static void InvalidatePath(Cell fromCell, Cell toCell, Mobility mobility)
    {
        var id = $"{fromCell}_{toCell}_{mobility}";
        var reverse = $"{toCell}_{fromCell}_{mobility}";

        if (_pathCache.ContainsKey(id))
        {
            _pathCache.Remove(id);
        }

        if (_pathCache.ContainsKey(reverse))
        {
            _pathCache.Remove(reverse);
        }
    }

    private static bool Search(Cell fromCell, Cell toCell, Mobility mobility)
    {
        try
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

                    if (neighbor?.Pathable(mobility) != true)
                    {
                        continue;
                    }

                    var neighborTravelCost = 1f;
                    if (mobility != Mobility.Fly)
                    {
                        neighborTravelCost = neighbor.TravelCost;
                    }

                    if (neighbor == null
                        || neighbor.SearchPhase > _searchFrontierPhase)
                    {
                        continue;
                    }

                    if (neighborTravelCost < 0)
                    {
                        continue;
                    }

                    var distance = current.Distance + neighborTravelCost;
                    if (neighbor.SearchPhase < _searchFrontierPhase)
                    {
                        neighbor.SearchPhase = _searchFrontierPhase;
                        neighbor.Distance = distance;
                        neighbor.PathFrom = current;
                        neighbor.SearchHeuristic = neighbor.DistanceTo(toCell);
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
        catch (System.Exception ex)
        {
            Debug.LogWarning("Pathing error: " + ex.ToString());
            throw;
        }
    }
}