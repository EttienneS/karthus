using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGrid : MonoBehaviour
{
    public bool DebugCoordinates;
    public bool DebugPathfinding = false;

    [Range(0f, 1f)]
    public float JitterProbability = 0.25f;

    public Cell[,] Map;

    private static MapGrid _instance;

    private CellPriorityQueue _searchFrontier = new CellPriorityQueue();

    private int _searchFrontierPhase;

    public static MapGrid Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("MapGrid").GetComponent<MapGrid>();
            }

            return _instance;
        }
    }

    public void AddCellIfValid(int x, int y, List<Cell> cells)
    {
        if (x >= 0 && x < Map.GetLength(0) && y >= 0 && y < Map.GetLength(1))
        {
            cells.Add(Map[x, y]);
        }
    }

    public Cell GetCellAtPoint(Vector3 position)
    {
        var coordinates = Coordinates.FromPosition(position);
        return Map[coordinates.X, coordinates.Y];
    }

    public List<Cell> GetCircle(Cell center, int radius)
    {
        var cells = new List<Cell>();
        var centerX = center.Coordinates.X;
        var centerY = center.Coordinates.Y;

        for (var x = centerX - radius; x <= centerX; x++)
        {
            for (var y = centerY - radius; y <= centerY; y++)
            {
                if ((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY) <= radius * radius)
                {
                    // calculate and add cell for each of the four points
                    AddCellIfValid(centerX - (x - centerX), centerY - (y - centerY), cells);
                    AddCellIfValid(centerX + (x - centerX), centerY + (y - centerY), cells);
                    AddCellIfValid(centerX - (x - centerX), centerY + (y - centerY), cells);
                    AddCellIfValid(centerX + (x - centerX), centerY - (y - centerY), cells);
                }
            }
        }

        return cells;
    }

    public Cell GetRandomCell()
    {
        return Map[(int)(Random.value * (Map.GetLength(0) - 1)), (int)(Random.value * (Map.GetLength(1) - 1))];
    }

    public List<Cell> GetRandomChunk(int chunkSize)
    {
        _searchFrontierPhase++;
        var firstCell = GetRandomCell();
        firstCell.SearchPhase = _searchFrontierPhase;
        firstCell.Distance = 0;
        firstCell.SearchHeuristic = 0;
        _searchFrontier.Enqueue(firstCell);

        var center = firstCell.Coordinates;
        int size = 0;

        var chunk = new List<Cell>();
        while (size < chunkSize && _searchFrontier.Count > 0)
        {
            var current = _searchFrontier.Dequeue();
            chunk.Add(current);
            size++;

            for (var d = Direction.N; d <= Direction.NW; d++)
            {
                var neighbor = current.GetNeighbor(d);
                if (neighbor && neighbor.SearchPhase < _searchFrontierPhase)
                {
                    neighbor.SearchPhase = _searchFrontierPhase;
                    neighbor.Distance = neighbor.Coordinates.DistanceTo(center);
                    neighbor.SearchHeuristic = Random.value < JitterProbability ? 1 : 0;
                    _searchFrontier.Enqueue(neighbor);
                }
            }
        }

        _searchFrontier.Clear();

        return chunk;
    }

    public Cell GetRandomPathableCell()
    {
        var cell = GetRandomCell();

        while (cell.TravelCost < 1)
        {
            cell = GetRandomCell();
        }

        return cell;
    }

    public List<Cell> GetRectangle(int x, int y, int width, int height)
    {
        var cells = new List<Cell>();

        for (var i = x; i < x + width; i++)
        {
            for (var k = y; k < y + height; k++)
            {
                AddCellIfValid(i, k, cells);
            }
        }

        return cells;
    }

    public void HighlightCells(List<Cell> cells, Color color)
    {
        foreach (var cell in cells)
        {
            cell.EnableBorder(color);
        }
    }

    internal Item FindClosestItemOfType(Cell centerPoint, string type, bool allowStockpiled)
    {
        if (allowStockpiled)
        {
            var stockpiledItem = StockpileController.Instance.FindClosestItemInStockpile(type, centerPoint);

            if (stockpiledItem != null)
            {
                return stockpiledItem;
            }
        }

        var closest = float.MaxValue;

        var searchRadius = 3;
        var maxRadius = Map.GetLength(0);
        var checkedCells = new HashSet<Cell>();
        do
        {
            var searchArea = Instance.GetCircle(centerPoint, searchRadius);
            searchArea.Reverse();
            foreach (var cell in searchArea)
            {
                if (checkedCells.Add(cell))
                {
                    foreach (var item in cell.GetComponentsInChildren<Item>())
                    {
                        if (item != null && item.Data.ItemType == type && !item.Data.Reserved)
                        {
                            if (!string.IsNullOrEmpty(item.Data.StockpileId))
                            {
                                continue;
                            }

                            var distance = Pathfinder.Distance(centerPoint, cell);
                            if (distance < closest)
                            {
                                closest = distance;
                                return item;
                            }
                        }
                    }
                }
            }

            searchRadius += 3;
        }
        while (searchRadius <= maxRadius);

        return null;
    }

    internal Direction GetDirection(Cell fromCell, Cell toCell)
    {
        var direction = Direction.S;

        if (fromCell != null && toCell != null)
        {
            var x = fromCell.Coordinates.X - toCell.Coordinates.X;
            var y = fromCell.Coordinates.Y - toCell.Coordinates.Y;

            if (x < 0 && y == 0)
            {
                direction = Direction.E;
            }
            else if (x < 0 && y > 0)
            {
                direction = Direction.NE;
            }
            else if (x < 0 && y < 0)
            {
                direction = Direction.SE;
            }
            else if (x > 0 && y == 0)
            {
                direction = Direction.W;
            }
            else if (x > 0 && y > 0)
            {
                direction = Direction.NW;
            }
            else if (x > 0 && y < 0)
            {
                direction = Direction.SW;
            }
            else if (y < 0)
            {
                direction = Direction.N;
            }
        }

        return direction;
    }

    internal void ResetSearchPriorities()
    {
        // ensure that all cells have their phases reset
        for (var y = 0; y < Map.GetLength(1); y++)
        {
            for (var x = 0; x < Map.GetLength(0); x++)
            {
                Map[x, y].SearchPhase = 0;
            }
        }
    }
}