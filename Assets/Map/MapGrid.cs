using System.Collections.Generic;
using UnityEngine;

public class MapGrid : MonoBehaviour
{
    public Cell Cell1;
    public Cell Cell2;
    public Cell cellPrefab;

    public bool DebugCoordinates;
    public bool DebugPathfinding = false;
    public int Height = 15;
    public Cell[,] Map;
    public int Width = 15;

    private static MapGrid _instance;

    public static MapGrid Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("MapManager").GetComponent<MapGrid>();
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

    public void CreateCell(int x, int y)
    {
        var cell = Instantiate(cellPrefab, transform, true);
        cell.transform.position = new Vector3(x, y);

        cell.Coordinates = new Coordinates(x, y);
        cell.name = cell.Coordinates.ToString();

        Map[x, y] = cell;

        if (DebugCoordinates)
        {
            cell.Text = cell.Coordinates.ToStringOnSeparateLines();
        }

    }

    public void CreateMap()
    {
        Map = new Cell[Width, Height];
        for (var y = 0; y < Width; y++)
        {
            for (var x = 0; x < Height; x++)
            {
                CreateCell(x, y);
            }
        }

        for (var y = 0; y < Width; y++)
        {
            for (var x = 0; x < Height; x++)
            {
                var cell = Map[x, y];

                if (x > 0)
                {
                    cell.SetNeighbor(Direction.W, Map[x - 1, y]);

                    if (y > 0)
                    {
                        cell.SetNeighbor(Direction.SW, Map[x - 1, y - 1]);

                        if (x < Width - 1)
                        {
                            cell.SetNeighbor(Direction.SE, Map[x + 1, y - 1]);
                        }
                    }
                }

                if (y > 0)
                {
                    cell.SetNeighbor(Direction.S, Map[x, y - 1]);
                }
            }
        }
    }

    public Cell GetCellAtPoint(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
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
        return Map[(int)(Random.value * Width), (int)(Random.value * Height)];
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

    private void Start()
    {
        CreateMap();
        Cell1 = Map[Width / 2, Height / 2];
    }

    private void Update()
    {
        var cell = GetRandomCell();
        //HighlightCells(GetCircle(cell, (int)(Random.value * 10)), new Color(Random.value, Random.value, Random.value));
        //HighlightCells(GetRectangle(cell.Coordinates.X,cell.Coordinates.Y, (int)(Random.value * 20), (int)(Random.value * 20)), new Color(Random.value, Random.value, Random.value));
    }
}