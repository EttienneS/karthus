using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public enum CellType
{
    Dirt, Forest, Grass, Mountain, Stone, Water
}

public class MapEditor : MonoBehaviour
{
    public Cell cellPrefab;

    public bool Generating = false;
    public MapGrid MapGrid;

    
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
        if (x >= 0 && x < MapGrid.MapSize && y >= 0 && y < MapGrid.MapSize)
        {
            cells.Add(MapGrid.CellLookup[(x, y)]);
        }
    }

    public Cell CreateCell(int x, int y)
    {
        var cell = Instantiate(cellPrefab, MapGrid.transform, true);
        cell.transform.position = new Vector3(x, y);
        cell.CellType = CellType.Water;

        cell.Data.Coordinates = new Coordinates(x, y);
        cell.name = cell.Data.Coordinates.ToString();

        MapGrid.Cells.Add(cell);

        return cell;
    }

    public IEnumerator CreateMap()
    {

        MapGrid.Cells = new List<Cell>();

        if (ShowGeneration) yield return null;

        for (var y = 0; y < MapGrid.MapSize; y++)
        {
            for (var x = 0; x < MapGrid.MapSize; x++)
            {
                CreateCell(x, y);
            }
        }

        if (ShowGeneration) yield return null;

        MapGrid.LinkNeighbours();
        if (ShowGeneration) yield return null;

        // generate bedrock
        for (int i = 0; i < MapGrid.MapSize / 2; i++)
        {
            foreach (var cell in MapGrid.GetRandomChunk(Random.Range(1 + (MapGrid.MapSize / 6), 1 + (MapGrid.MapSize / 3))))
            {
                cell.CellType = CellType.Stone;
            }
        }
        if (ShowGeneration) yield return null;

        // grow mountains
        foreach (var cell in MapGrid.Cells)
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
        for (int i = 0; i < MapGrid.MapSize; i++)
        {
            foreach (var cell in MapGrid.GetRandomChunk(Random.Range(MapGrid.MapSize, MapGrid.MapSize * 2)))
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
        foreach (var cell in MapGrid.Cells)
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
        foreach (var cell in MapGrid.Cells)
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
        foreach (var cell in MapGrid.Cells)
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
        foreach (var cell in MapGrid.Cells)
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

        foreach (var cell in MapGrid.Cells)
        {
            switch (cell.CellType)
            {
                case CellType.Forest:
                    if (Random.value > 0.95)
                    {
                        cell.AddContent(StructureController.Instance.GetStructure("Tree").gameObject, false);
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