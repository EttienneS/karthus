using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkRenderer : MonoBehaviour
{
    public List<Cell> Cells;

    public Chunk Data;

    public bool GroundDrawn;
    public Tilemap GroundMap;

    public Cell CreateCell(int x, int y)
    {
        var cell = new Cell
        {
            X = x,
            Y = y,
            SearchPhase = 0,
            Chunk = Data
        };

        Game.Instance.Map.CellLookup.Add((x, y), cell);

        return cell;
    }

    public Vector3Int GetChunkCoordinate(Cell cell)
    {
        return new Vector3Int(cell.X % Game.Instance.Map.ChunkSize, cell.Y % Game.Instance.Map.ChunkSize, 0);
    }

    public void LinkToChunk(ChunkRenderer chunk)
    {
        // link edges to the given chunk edges
        var size = Game.Instance.Map.ChunkSize;

        var firstX = (Data.X * size);
        var firstY = (Data.Y * size);

        if (chunk.Data.X < Data.X)
        {
            // link to chunk on the left (west)
            foreach (var cell in Cells.Where(c => c.X == firstX))
            {
                cell.SetNeighbor(Direction.W, Game.Instance.Map.CellLookup[(cell.X - 1, cell.Y)]);
            }
        }
        else if (chunk.Data.X > Data.X)
        {
            // link to chunk on the right (east)
            foreach (var cell in Cells.Where(c => c.X == firstX + size))
            {
                cell.SetNeighbor(Direction.E, Game.Instance.Map.CellLookup[(cell.X + 1, cell.Y)]);
            }
        }
        else if (chunk.Data.Y < Data.Y)
        {
            // link to chunk below (south)
            foreach (var cell in Cells.Where(c => c.Y == firstY))
            {
                cell.SetNeighbor(Direction.S, Game.Instance.Map.CellLookup[(cell.X, cell.Y - 1)]);
            }
        }
        else if (chunk.Data.Y > Data.Y)
        {
            // link to chunk above (north)
            foreach (var cell in Cells.Where(c => c.Y == firstY))
            {
                cell.SetNeighbor(Direction.N, Game.Instance.Map.CellLookup[(cell.X, cell.Y + 1)]);
            }
        }
    }

    public void Start()
    {
        //using (var sw = new Instrumenter(nameof(ChunkRenderer)))
        {
            transform.position = new Vector3(Data.X * Game.Instance.Map.ChunkSize, Data.Y * Game.Instance.Map.ChunkSize);
            DrawGround();
            Populate();
            UpdateInterlocked();
        }
    }

    private void UpdateInterlocked()
    {
        foreach (var wall in Cells.Where(c => c.Structure?.IsInterlocking() == true).Select(c => c.Structure))
        {
            wall.UpdateInterlocking();
        }
    }

    internal void MakeCells()
    {
        var size = Game.Instance.Map.ChunkSize;

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
                var cell = Game.Instance.Map.CellLookup[(x, y)];
                if (x > firstX)
                {
                    cell.SetNeighbor(Direction.W, Game.Instance.Map.CellLookup[(x - 1, y)]);

                    if (y > firstY)
                    {
                        cell.SetNeighbor(Direction.SW, Game.Instance.Map.CellLookup[(x - 1, y - 1)]);

                        if (x < firstX - 1)
                        {
                            cell.SetNeighbor(Direction.SE, Game.Instance.Map.CellLookup[(x + 1, y - 1)]);
                        }
                    }
                }

                if (y > firstY)
                {
                    cell.SetNeighbor(Direction.S, Game.Instance.Map.CellLookup[(x, y - 1)]);
                }
            }
        }
    }

    internal void Populate()
    {
        Cells.ForEach(c => c.Populate());
        Data.Populated = true;
    }

    internal void SetTile(int x, int y, Tile tile)
    {
        var pos = new Vector3Int(x % Game.Instance.Map.ChunkSize, y % Game.Instance.Map.ChunkSize, 0);

        GroundMap.SetTile(pos, null);
        GroundMap.SetTile(pos, tile);
    }

    private void DrawGround()
    {
        var groundTiles = Cells.Select(c => c.GroundTile).ToArray();
        var groundCoords = Cells.Select(GetChunkCoordinate).ToArray();

        GroundMap.SetTiles(groundCoords, null);
        GroundMap.SetTiles(groundCoords, groundTiles);
        GroundDrawn = true;
    }
}