using System.Collections.Generic;
using UnityEngine;

public class MapGrid : MonoBehaviour
{
    public Cell cellPrefab;

    public int Height = 15;
    public Cell[,] Map;
    public int Width = 15;

    public void CreateCell(int x, int y, int i)
    {
        var cell = Instantiate(cellPrefab, transform, true);
        cell.transform.position = new Vector3(x, y);

        cell.Coordinates = new Coordinates(x, y);
        cell.name = cell.Coordinates.ToString();

        if (x > 0)
        {
            cell.SetNeighbor(Direction.W, Map[x - 1, y]);

            if (y > 0)
            {
                cell.SetNeighbor(Direction.SW, Map[x - 1, y - 1]);
                //cell.SetNeighbor(Direction.SE, Map[x + 1, y - 1]);
            }
        }

        if (y > 0)
        {
            cell.SetNeighbor(Direction.S, Map[x, y - 1]);
        }

        Map[x, y] = cell;
        cell.Text = cell.Coordinates.ToStringOnSeparateLines();
    }

    public Cell GetCellAtPoint(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        var coordinates = Coordinates.FromPosition(position);

        return Map[coordinates.X, coordinates.Y];
    }

    public Cell Cell1;
    public Cell Cell2;

    public bool Flip;

    private void Start()
    {
        Map = new Cell[Width, Height];
        var i = 0;
        for (var y = 0; y < Width; y++)
        {
            for (var x = 0; x < Height; x++)
            {
                CreateCell(x, y, i++);
            }
        }

        Cell1 = Map[(int)(Random.value * 64), (int)(Random.value * 64)];
        Cell2 = Map[(int)(Random.value * 64), (int)(Random.value * 64)];

        // weird hack to fix the canvas overlay, we move the canvas over by a % of the width/height, this
        // fixes the offset to align the labels with the cells underneath them
        // I am pretty sure this is caused by the scaling (the canvas is very big and then scaled down to fit the grid)
        //GameObject.Find("MapCanvas").transform.position = new Vector3(Width * 0.19f, -(Height * 0.19f));
    }

    private void Update()
    {
        //for (int i = Width / 2; i > 0; i--)
        //{
        //    var color = new Color(Random.value, Random.value, Random.value);
        //    foreach (var cell in GetCircle(Map[Width / 2, Height / 2], i))
        //    {
        //        cell.EnableBorder(color);
        //    }
        //}
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
    public void AddCellIfValid(int x, int y, List<Cell> cells)
    {
        if (x >= 0 && x < Map.GetLength(0) && y >= 0 && y < Map.GetLength(1))
        {
            cells.Add(Map[x, y]);
        }
    }

}