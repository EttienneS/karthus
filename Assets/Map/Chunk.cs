using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{
    public List<Cell> Cells;
    internal Tilemap FloorMap;
    internal Tilemap GroundMap;
    internal Tilemap StructureMap;
    internal int X;
    internal int Y;

    public void Start()
    {
        transform.position = new Vector3(X * Game.Map.ChunkSize, Y * Game.Map.ChunkSize);

        var maps = GetComponentsInChildren<Tilemap>();
        GroundMap = maps.FirstOrDefault(m => m.name == "Ground");
        FloorMap = maps.FirstOrDefault(m => m.name == "Floor");
        StructureMap = maps.FirstOrDefault(m => m.name == "Structure");
    }

    public Vector3Int GetChunkCoordinate(Cell cell)
    {
        return new Vector3Int(cell.X % Game.Map.ChunkSize, cell.Y % Game.Map.ChunkSize, 0);
    }

    public void LinkToChunk(Chunk chunk)
    {
        // link edges to the given chunk edges
        var size = Game.Map.ChunkSize;

        var firstX = (X * size);
        var firstY = (Y * size);

        if (chunk.X < X)
        {
            // link to chunk on the left (west)
            foreach (var cell in Cells.Where(c => c.X == firstX))
            {
                cell.SetNeighbor(Direction.W, Game.Map.CellLookup[(cell.X - 1, cell.Y)]);
            }
        }
        else if (chunk.X > X)
        {
            // link to chunk on the right (east)
            foreach (var cell in Cells.Where(c => c.X == firstX + size))
            {
                cell.SetNeighbor(Direction.E, Game.Map.CellLookup[(cell.X + 1, cell.Y)]);
            }
        }
        else if (chunk.Y < Y)
        {
            // link to chunk below (south)
            foreach (var cell in Cells.Where(c => c.Y == firstY))
            {
                cell.SetNeighbor(Direction.S, Game.Map.CellLookup[(cell.X, cell.Y - 1)]);
            }

        }
        else if (chunk.Y > Y)
        {
            // link to chunk above (north)
            foreach (var cell in Cells.Where(c => c.Y == firstY))
            {
                cell.SetNeighbor(Direction.N, Game.Map.CellLookup[(cell.X, cell.Y + 1)]);
            }
        }
    }

    internal void Draw()
    {
        // draw ground
        var groundTiles = Cells.Select(c => c.GroundTile).ToArray();
        var groundCoords = Cells.Select(GetChunkCoordinate).ToArray();

        GroundMap.SetTiles(groundCoords, null);
        GroundMap.SetTiles(groundCoords, groundTiles);

        var structures = Cells
                         .Where(c => c.Structure != null)
                         .Select(c => c.Structure)
                         .ToList();

        if (structures.Count > 0)
        {
            var structureTiles = structures.Select(c => c.Tile).ToArray();
            var structureCoords = structures.Select(c => GetChunkCoordinate(c.Cell)).ToArray();
            StructureMap.SetTiles(structureCoords, structureTiles);
        }

        var floors = Cells
                        .Where(c => c.Floor != null)
                        .Select(c => c.Floor)
                        .ToList();

        if (floors.Count > 0)
        {
            var floorTiles = floors.Select(c => c.Tile).ToArray();
            var floorCoords = floors.Select(c => GetChunkCoordinate(c.Cell)).ToArray();
            FloorMap.SetTiles(floorCoords, floorTiles);
        }
    }

    public Cell CreateCell(int x, int y)
    {
        var cell = new Cell
        {
            X = x,
            Y = y,
            SearchPhase = 0,
            Chunk = (X, Y)
        };

        Game.Map.CellLookup.Add((x, y), cell);

        return cell;
    }

    internal void MakeCells()
    {
        var size = Game.Map.ChunkSize;

        var firstX = (X * size);
        var firstY = (Y * size);

        Cells = new List<Cell>();

        for (var y = firstY; y < firstY + size; y++)
        {
            for (var x = firstX; x < firstX + size; x++)
            {
                Cells.Add(CreateCell(x, y));
            }
        }

        // link cell to the others in the chunk
        for (var y = firstY; y < firstY + size; y++)
        {
            for (var x = firstX; x < firstX + size; x++)
            {
                var cell = Game.Map.CellLookup[(x, y)];
                if (x > firstX)
                {
                    cell.SetNeighbor(Direction.W, Game.Map.CellLookup[(x - 1, y)]);

                    if (y > firstY)
                    {
                        cell.SetNeighbor(Direction.SW, Game.Map.CellLookup[(x - 1, y - 1)]);

                        if (x < firstX - 1)
                        {
                            cell.SetNeighbor(Direction.SE, Game.Map.CellLookup[(x + 1, y - 1)]);
                        }
                    }
                }

                if (y > firstY)
                {
                    cell.SetNeighbor(Direction.S, Game.Map.CellLookup[(x, y - 1)]);
                }
            }
        }
    }

    internal void Populate()
    {
        Cells.ForEach(c => c.Populate());
    }

    internal void SetTile(int x, int y, Tile tile, Map.TileLayer layer)
    {
        var pos = new Vector3Int(x % Game.Map.ChunkSize, y % Game.Map.ChunkSize, 0);
        Tilemap tilemap = null;
        switch (layer)
        {
            case Map.TileLayer.Ground:
                tilemap = GroundMap;
                break;

            case Map.TileLayer.Floor:
                tilemap = FloorMap;
                break;

            case Map.TileLayer.Structure:
                tilemap = StructureMap;
                break;
        }
        tilemap.SetTile(pos, null);
        tilemap.SetTile(pos, tile);
    }
}