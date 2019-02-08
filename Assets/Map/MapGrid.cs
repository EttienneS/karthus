using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGrid : MonoBehaviour
{
    public List<Cell> Cells;

    [Range(4, 300)]
    public int MapSize = 64;

    internal const float JitterProbability = 0.25f;
    private static MapGrid _instance;
    private Dictionary<(int x, int y), Cell> _cellLookup;
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

    public Dictionary<(int x, int y), Cell> CellLookup
    {
        get
        {
            if (_cellLookup == null)
            {
                _cellLookup = new Dictionary<(int x, int y), Cell>();

                foreach (var cell in Cells)
                {
                    _cellLookup.Add((cell.Data.Coordinates.X, cell.Data.Coordinates.Y), cell);
                }
            }
            return _cellLookup;
        }
    }

    public void AddCellIfValid(int x, int y, List<Cell> cells)
    {
        if (x >= 0 && x < MapSize && y >= 0 && y < MapSize)
        {
            cells.Add(CellLookup[(x, y)]);
        }
    }

    public Cell GetCellAtCoordinate(Coordinates coordintes)
    {
        return CellLookup[(coordintes.X, coordintes.Y)];
    }

    public Cell GetCellAtPoint(Vector3 position)
    {
        var coordinates = Coordinates.FromPosition(position);
        return CellLookup[(coordinates.X, coordinates.Y)];
    }

    public List<Cell> GetCircle(Cell center, int radius)
    {
        var cells = new List<Cell>();
        var centerX = center.Data.Coordinates.X;
        var centerY = center.Data.Coordinates.Y;

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

    internal void ClearCache()
    {
        _cellLookup = null;
    }

    public Cell GetRandomCell()
    {
        return CellLookup[((int)(Random.value * (MapSize - 1)), (int)(Random.value * (MapSize - 1)))];
    }

    public List<Cell> GetRandomChunk(int chunkSize)
    {
        _searchFrontierPhase++;
        var firstCell = GetRandomCell();
        firstCell.SearchPhase = _searchFrontierPhase;
        firstCell.Distance = 0;
        firstCell.SearchHeuristic = 0;
        _searchFrontier.Enqueue(firstCell);

        var center = firstCell.Data.Coordinates;
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
                    neighbor.Distance = neighbor.Data.Coordinates.DistanceTo(center);
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

    internal void DestroyCell(Cell cell)
    {
        foreach (var item in cell.Data.ContainedItems.ToArray())
        {
            ItemController.Instance.DestroyItem(item);
        }

        if (cell.Data.Structure != null)
        {
            StructureController.Instance.DestroyStructure(cell.Data.Structure);
        }

        if (cell.Data.Stockpile != null)
        {
            StockpileController.Instance.DestroyStockpile(cell.Data.Stockpile);
        }

        Destroy(cell.gameObject);
    }

    internal Direction GetDirection(Cell fromCell, Cell toCell)
    {
        var direction = Direction.S;

        if (fromCell != null && toCell != null)
        {
            var x = fromCell.Data.Coordinates.X - toCell.Data.Coordinates.X;
            var y = fromCell.Data.Coordinates.Y - toCell.Data.Coordinates.Y;

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

    internal void LinkNeighbours()
    {
        for (var y = 0; y < MapSize; y++)
        {
            for (var x = 0; x < MapSize; x++)
            {
                var cell = CellLookup[(x, y)];

                if (x > 0)
                {
                    cell.SetNeighbor(Direction.W, CellLookup[(x - 1, y)]);

                    if (y > 0)
                    {
                        cell.SetNeighbor(Direction.SW, CellLookup[(x - 1, y - 1)]);

                        if (x < MapSize - 1)
                        {
                            cell.SetNeighbor(Direction.SE, CellLookup[(x + 1, y - 1)]);
                        }
                    }
                }

                if (y > 0)
                {
                    cell.SetNeighbor(Direction.S, CellLookup[(x, y - 1)]);
                }
            }
        }

        
    }

    internal void ResetSearchPriorities()
    {
        // ensure that all cells have their phases reset
        for (var y = 0; y < MapSize; y++)
        {
            for (var x = 0; x < MapSize; x++)
            {
                CellLookup[(x, y)].SearchPhase = 0;
            }
        }
    }
}