using System.Collections.Generic;
using UnityEngine;

public class MapEditor : MonoBehaviour
{
    public Cell cellPrefab;

    [Range(0, 250)]
    public int GrassChunkMax = 100;

    [Range(0, 250)]
    public int GrassChunkMin = 50;

    [Range(0, 250)]
    public int GrassChunks = 25;

    public MapGrid MapGrid;

    [Range(16, 256)]
    public int MapSize = 64;
    private static MapEditor _instance;

    public static MapEditor Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("MapEditor").GetComponent<MapEditor>();
            }

            return _instance;
        }
    }

    public void AddCellIfValid(int x, int y, List<Cell> cells)
    {
        if (x >= 0 && x < MapGrid.Map.GetLength(0) && y >= 0 && y < MapGrid.Map.GetLength(1))
        {
            cells.Add(MapGrid.Map[x, y]);
        }
    }

    public void CreateCell(int x, int y)
    {
        var cell = Instantiate(cellPrefab, MapGrid.transform, true);
        cell.transform.position = new Vector3(x, y);
        cell.Sprite.sprite = SpriteStore.Instance.GetRandomSpriteOfType("Water");
        
        // water and any unpathable cells are -1
        cell.TravelCost = -1;
        cell.Coordinates = new Coordinates(x, y);
        cell.name = cell.Coordinates.ToString();

        MapGrid.Map[x, y] = cell;

        if (MapGrid.DebugCoordinates)
        {
            cell.Text = cell.Coordinates.ToStringOnSeparateLines();
        }
    }

    public void CreateMap()
    {
        var width = MapSize;
        var height = MapSize;

        MapGrid.Map = new Cell[width, height];

        for (var y = 0; y < width; y++)
        {
            for (var x = 0; x < height; x++)
            {
                CreateCell(x, y);
            }
        }

        for (var y = 0; y < width; y++)
        {
            for (var x = 0; x < height; x++)
            {
                var cell = MapGrid.Map[x, y];

                if (x > 0)
                {
                    cell.SetNeighbor(Direction.W, MapGrid.Map[x - 1, y]);

                    if (y > 0)
                    {
                        cell.SetNeighbor(Direction.SW, MapGrid.Map[x - 1, y - 1]);

                        if (x < width - 1)
                        {
                            cell.SetNeighbor(Direction.SE, MapGrid.Map[x + 1, y - 1]);
                        }
                    }
                }

                if (y > 0)
                {
                    cell.SetNeighbor(Direction.S, MapGrid.Map[x, y - 1]);
                }
            }
        }

        for (int i = 0; i < GrassChunks; i++)
        {
            var col = new Color(Random.value, Random.value, Random.value);
            foreach (var cell in MapGrid.GetRandomChunk(Random.Range(GrassChunkMin, GrassChunkMax)))
            {
                cell.Sprite.sprite = SpriteStore.Instance.GetRandomSpriteOfType("Grass");
                cell.TravelCost = 1;
            }
        }

        MapGrid.ResetSearchPriorities();
    }

    public void Awake()
    {
        CreateMap();
    }
}