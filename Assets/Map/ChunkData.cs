using Newtonsoft.Json;

public class Chunk
{
    public Chunk(int x, int y)
    {
        X = x;
        Y = y;
    }

    [JsonIgnore]
    public (int x, int y) Coords
    {
        get
        {
            return (X, Y);
        }
    }

    public int X { get; set; }
    public int Y { get; set; }
    public bool Populated { get; set; }

    [JsonIgnore]
    public ChunkRenderer Renderer
    {
        get
        {
            return Game.Instance.Map.Chunks[Coords];
        }
    }
}