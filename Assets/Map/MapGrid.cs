using System.Collections.Generic;
using UnityEngine;

public class MapGrid : MonoBehaviour
{
    public Cell Cell1;
    public Cell Cell2;

    public bool DebugCoordinates;
    public bool DebugPathfinding = false;
    public Cell[,] Map;

    private static MapGrid _instance;

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
        return Map[(int)(Random.value * (Map.GetLength(0) - 1)), (int)(Random.value * (Map.GetLength(1) - 1))];
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
}