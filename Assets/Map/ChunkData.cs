using Assets.Map;
using Assets.ServiceLocator;
using Newtonsoft.Json;
using System.Collections.Generic;

public class Chunk
{
    public List<Cell> Cells;


    public Chunk(int x, int y)
    {
        X = x;
        Z = y;
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
}