using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkRenderer : MonoBehaviour
{
    public List<Cell> Cells;

    public Chunk Data;

    public bool FloorDrawn;
    public Tilemap FloorMap;
    public bool GroundDrawn;
    public Tilemap GroundMap;
    public bool StructureDrawn;
    public Tilemap StructureMap;
    private List<Cell> _cellsToPopulate;

    public Cell CreateCell(int x, int y)
    {
        var cell = new Cell
        {
            X = x,
            Y = y,
            SearchPhase = 0,
            Chunk = Data
        };

        Game.Map.CellLookup.Add((x, y), cell);

        return cell;
    }

    public Vector3Int GetChunkCoordinate(Cell cell)
    {
        return new Vector3Int(cell.X % Game.Map.ChunkSize, cell.Y % Game.Map.ChunkSize, 0);
    }

    public void LinkToChunk(ChunkRenderer chunk)
    {
        // link edges to the given chunk edges
        var size = Game.Map.ChunkSize;

        var firstX = (Data.X * size);
        var firstY = (Data.Y * size);

        if (chunk.Data.X < Data.X)
        {
            // link to chunk on the left (west)
            foreach (var cell in Cells.Where(c => c.X == firstX))
            {
                cell.SetNeighbor(Direction.W, Game.Map.CellLookup[(cell.X - 1, cell.Y)]);
            }
        }
        else if (chunk.Data.X > Data.X)
        {
            // link to chunk on the right (east)
            foreach (var cell in Cells.Where(c => c.X == firstX + size))
            {
                cell.SetNeighbor(Direction.E, Game.Map.CellLookup[(cell.X + 1, cell.Y)]);
            }
        }
        else if (chunk.Data.Y < Data.Y)
        {
            // link to chunk below (south)
            foreach (var cell in Cells.Where(c => c.Y == firstY))
            {
                cell.SetNeighbor(Direction.S, Game.Map.CellLookup[(cell.X, cell.Y - 1)]);
            }
        }
        else if (chunk.Data.Y > Data.Y)
        {
            // link to chunk above (north)
            foreach (var cell in Cells.Where(c => c.Y == firstY))
            {
                cell.SetNeighbor(Direction.N, Game.Map.CellLookup[(cell.X, cell.Y + 1)]);
            }
        }
    }

    public void Start()
    {
        transform.position = new Vector3(Data.X * Game.Map.ChunkSize, Data.Y * Game.Map.ChunkSize);
    }

    public void Update()
    {
        if (!GroundDrawn)
        {
            DrawGround();
        }
        else if (!Data.Populated)
        {
            Populate();
        }
        else if (!FloorDrawn)
        {
            DrawFloors();
        }
        else if (!StructureDrawn)
        {
            DrawStructures();
        }
    }

    internal void MakeCells()
    {
        var size = Game.Map.ChunkSize;

        var firstX = (Data.X * size);
        var firstY = (Data.Y * size);

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
        if (_cellsToPopulate == null)
        {
            _cellsToPopulate = Cells.ToList();
        }

        var range = Mathf.Min(10, _cellsToPopulate.Count);
        var current = _cellsToPopulate.GetRange(0, range);
        current.ForEach(c => c.Populate());
        _cellsToPopulate.RemoveRange(0, range);

        if (_cellsToPopulate.Count == 0)
        {
            Data.Populated = true;
        }
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

    private void DrawFloors()
    {
        var floors = Cells.Where(c => c.Floor != null)
                          .Select(c => c.Floor)
                          .ToList();

        if (floors.Count > 0)
        {
            var floorTiles = floors.Select(c => c.Tile).ToArray();
            var floorCoords = floors.Select(c => GetChunkCoordinate(c.Cell)).ToArray();
            FloorMap.SetTiles(floorCoords, floorTiles);
        }
        FloorDrawn = true;
    }

    private void DrawGround()
    {
        var groundTiles = Cells.Select(c => c.GroundTile).ToArray();
        var groundCoords = Cells.Select(GetChunkCoordinate).ToArray();

        GroundMap.SetTiles(groundCoords, null);
        GroundMap.SetTiles(groundCoords, groundTiles);
        GroundDrawn = true;
    }

    private void DrawStructures()
    {
        var structures = Cells.Where(c => c.Structure != null)
                              .Select(c => c.Structure)
                              .ToList();

        if (structures.Count > 0)
        {
            var structureTiles = structures.Select(c => c.Tile).ToArray();
            var structureCoords = structures.Select(c => GetChunkCoordinate(c.Cell)).ToArray();
            StructureMap.SetTiles(structureCoords, structureTiles);
        }
        StructureDrawn = true;
    }
}