using System;

[Serializable]
public class MapGenerationData
{
    public int ChunkSize = 60;
    public bool CreateWater = true;
    public int CreaturesToSpawn = 3;
    public bool Populate = true;
    public string Seed;
    public int Size = 1;
    public float WaterLevel = 0.15f;

    public MapGenerationData()
    {
    }

    public MapGenerationData(string seed)
    {
        Seed = seed;
    }

    public static MapGenerationData Instance { get; set; }
}