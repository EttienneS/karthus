using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MapGrid : MonoBehaviour
{
    public Cell[] cellPrefabs;

    public int Width = 15;
    public int Height = 15;

    public Cell[] Map;

    void Start()
    {
        Map = new Cell[Width * Height];
        var i = 0;
        for (var y = 0; y < Width; y++)
        {
            for (var x = 0; x < Height; x++)
            {
                CreateCell(x, y, i++);
            }
        }
        Map.ToString();
    }

    public void CreateCell(int x, int y, int i)
    {
        var cell = Instantiate(cellPrefabs[Random.Range(0, cellPrefabs.Length)], transform, true);
        cell.transform.position = new Vector3(x, y);
        cell.name = $"Cell {x}:{y}";

        if (x > 0)
        {
            cell.SetNeighbor(Direction.W, Map[i - 1]);

            if (y > 0)
            {
                cell.SetNeighbor(Direction.SW, Map[i - Width - 1]);
                cell.SetNeighbor(Direction.SE, Map[i - Width + 1]);
            }
        }

        if (y > 0)
        {
            cell.SetNeighbor(Direction.S, Map[i - Width]);
        }

        Map[i] = cell;
    }

    void Update()
    {
        
    }

    public Cell GetCellAtPoint(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        var coordinates = Coordinates.FromPosition(position);
        var index = coordinates.X + coordinates.Y * Width + coordinates.Y / 2;
        var cell = Map[index];

        return cell;
    }


}
