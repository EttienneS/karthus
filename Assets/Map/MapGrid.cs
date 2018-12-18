using UnityEngine;

public class MapGrid : MonoBehaviour
{
    public Cell[] cellPrefabs;

    public int Height = 15;
    public Cell[] Map;
    public int Width = 15;

    public void CreateCell(int x, int y, int i)
    {
        var cell = Instantiate(cellPrefabs[Random.Range(0, cellPrefabs.Length)], transform, true);
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
    }

    public Cell GetCellAtPoint(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        var coordinates = Coordinates.FromPosition(position);
        var index = coordinates.X + coordinates.Y * Width + coordinates.Y / 2;
        var cell = Map[index];

        return cell;
    }

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

        var cell1 = Map[(int)(Random.value * Map.Length)];
        var cell2 = Map[(int)(Random.value * Map.Length)];

        Pathfinder.ShowPath(Pathfinder.FindPath(cell1, cell2));
    }

    private void Update()
    {
    }
}