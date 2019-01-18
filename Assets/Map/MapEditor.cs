using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    Dirt, Forest, Grass, Mountain, Stone, Water
}

public class MapEditor : MonoBehaviour
{
    public Cell cellPrefab;

    public bool Generating = false;
    public MapGrid MapGrid;

    [Range(4, 196)]
    public int MapSize = 64;

    public bool ShowGeneration = true;
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
        cell.CellType = CellType.Water;

        cell.Coordinates = new Coordinates(x, y);
        cell.name = cell.Coordinates.ToString();

        MapGrid.Map[x, y] = cell;

        if (MapGrid.DebugCoordinates)
        {
            cell.Text = cell.Coordinates.ToStringOnSeparateLines();
        }
    }

    public IEnumerator CreateMap()
    {
        var width = MapSize;
        var height = MapSize;

        MapGrid.Map = new Cell[width, height];

        if (ShowGeneration) yield return null;

        for (var y = 0; y < width; y++)
        {
            for (var x = 0; x < height; x++)
            {
                CreateCell(x, y);
            }
        }

        if (ShowGeneration) yield return null;

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
        if (ShowGeneration) yield return null;

        // generate bedrock
        for (int i = 0; i < MapSize / 2; i++)
        {
            foreach (var cell in MapGrid.GetRandomChunk(Random.Range(1 + (MapSize / 6), 1 + (MapSize / 3))))
            {
                cell.CellType = CellType.Stone;
            }
        }
        if (ShowGeneration) yield return null;

        // grow mountains
        foreach (var cell in MapGrid.Map)
        {
            if (cell.CellType != CellType.Stone)
            {
                continue;
            }

            if (cell.CountNeighborsOfType(null) +
                cell.CountNeighborsOfType(CellType.Mountain) +
                cell.CountNeighborsOfType(CellType.Stone) > 6)
            {
                cell.CellType = CellType.Mountain;
            }
        }
        if (ShowGeneration) yield return null;

        // generate landmasses
        for (int i = 0; i < MapSize; i++)
        {
            foreach (var cell in MapGrid.GetRandomChunk(Random.Range(MapSize, MapSize * 2)))
            {
                if (cell.CellType == CellType.Water)
                {
                    cell.CellType = CellType.Grass;
                }
            }
        }
        if (ShowGeneration) yield return null;

        // bleed water, this enlarges bodies of water
        // creates more natural looking coastlines/rivers
        foreach (var cell in MapGrid.Map)
        {
            if (cell.CellType == CellType.Water)
            {
                continue;
            }

            var waterN = cell.CountNeighborsOfType(CellType.Water);
            if (waterN > 2 && Random.value > 1.0f - (waterN / 10f))
            {
                cell.CellType = CellType.Water;
            }
        }

        if (ShowGeneration) yield return null;

        // create coast
        foreach (var cell in MapGrid.Map)
        {
            if (cell.CellType != CellType.Grass)
            {
                // already water skip
                continue;
            }

            if (cell.CountNeighborsOfType(CellType.Water) > 0)
            {
                cell.CellType = CellType.Dirt;
            }
        }
        if (ShowGeneration) yield return null;

        // bleed desert
        foreach (var cell in MapGrid.Map)
        {
            if (cell.CellType == CellType.Water)
            {
                // already water skip
                continue;
            }

            if (cell.CountNeighborsOfType(CellType.Dirt) > 2 && Random.value > 0.5)
            {
                cell.CellType = CellType.Dirt;
            }
        }
        if (ShowGeneration) yield return null;

        // create forest
        foreach (var cell in MapGrid.Map)
        {
            if (cell.CellType == CellType.Water)
            {
                // water skip
                continue;
            }

            if (cell.CountNeighborsOfType(null) +
                cell.CountNeighborsOfType(CellType.Grass) +
                cell.CountNeighborsOfType(CellType.Forest) > 6
                && Random.value > 0.2)
            {
                cell.CellType = CellType.Forest;
            }
        }

        if (ShowGeneration) yield return null;

        foreach (var cell in MapGrid.Map)
        {
            switch (cell.CellType)
            {
                case CellType.Forest:
                    if (Random.value > 0.6)
                    {
                        cell.AddContent(ItemController.Instance.GetItem("TreeStump").gameObject, true);
                    }
                    break;
                case CellType.Stone:
                    for (int i = 0; i < Random.Range(1, 5); i++)
                    {
                        cell.AddContent(ItemController.Instance.GetItem("Rock").gameObject, true);
                    }
                    break;
            }
        }
        if (ShowGeneration) yield return null;

        MapGrid.ResetSearchPriorities();
        Pathfinder.FlushPathCache();
        CreatureController.Instance.SpawnCreatures();

        if (ShowGeneration) yield return null;
    }

    private void Update()
    {
        if (!Generating)
        {
            Generating = true;
            StartCoroutine("CreateMap");
        }
    }
}