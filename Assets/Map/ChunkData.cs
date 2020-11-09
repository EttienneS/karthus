using Assets.Map;
using Assets.ServiceLocator;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chunk
{
    public Chunk(int x, int y, ChunkCell[,] cells) : this(x, y)
    {
        ChunkCells = cells;
    }

    public Chunk(int x, int y) : this()
    {
        X = x;
        Z = y;
    }

    public Chunk()
    {

    }

    [JsonIgnore]
    public (int x, int y) Coords
    {
        get
        {
            return (X, Z);
        }
    }

    public int X { get; set; }
    public int Z { get; set; }
    public bool Populated { get; set; }

    [JsonIgnore]
    public ChunkRenderer Renderer
    {
        get
        {
            return Loc.GetMap().Chunks[Coords];
        }
    }

    [JsonIgnore]
    public ChunkCell[,] ChunkCells { get; set; }

  
}