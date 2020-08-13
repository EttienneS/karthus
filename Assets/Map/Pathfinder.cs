using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinder
{
    private static CellPriorityQueue _searchFrontier;
    private static int _searchFrontierPhase;

    public static List<Cell> FindPath(Cell fromCell, Cell toCell, Mobility mobility)
    {
        if (fromCell != null && toCell != null)
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

                return path;
            }
        }

        return null;
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

                    if (neighbor?.PathableWith(mobility) != true)
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