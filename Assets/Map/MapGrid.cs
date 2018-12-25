using UnityEngine;
using UnityEngine.UI;

public class MapGrid : MonoBehaviour
{
    public Cell cellPrefab;

    public int Height = 15;
    public Cell[] Map;
    public int Width = 15;

    public void CreateCell(int x, int y, int i)
    {
        var cell = Instantiate(cellPrefab, transform, true);
        cell.transform.position = new Vector3(x, y);

        cell.Coordinates = new Coordinates(x, y);
        cell.name = cell.Coordinates.ToString();

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
        cell.Text = cell.Coordinates.ToStringOnSeparateLines();
    }

    public Cell GetCellAtPoint(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        var coordinates = Coordinates.FromPosition(position);
        var index = coordinates.X + (coordinates.Y * Width) + (coordinates.Y / 2);
        
        return Map[index];
    }

    public Cell Cell1;
    public Cell Cell2;

    public bool Flip;

    private void Start()
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

        Cell1 = Map[(int)(Random.value * Map.Length)];
        Cell2 = Map[(int)(Random.value * Map.Length)];

        // weird hack to fix the canvas overlay, we move the canvas over by a % of the width/height, this
        // fixes the offset to align the labels with the cells underneath them
        // I am pretty sure this is caused by the scaling (the canvas is very big and then scaled down to fit the grid)
        //GameObject.Find("MapCanvas").transform.position = new Vector3(Width * 0.19f, -(Height * 0.19f));
    }

    private void Update()
    {
    }
}