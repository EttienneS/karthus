using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGrid : MonoBehaviour
{
    public Cell[] cellPrefabs;

    public int Width = 15;
    public int Height = 15;

    public Cell[] Map;

    // Start is called before the first frame update
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

    // Update is called once per frame
    void Update()
    {

    }
}
