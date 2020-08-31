using System;
using UnityEngine;

[Serializable]
public class MapData
{
    public int ChunkSize = 240;
    public bool CreateWater = true;

    [Range(0, 100)]
    public int CreaturesToSpawn = 3;

    public bool Flat = false;

    public bool Populate = true;

    public string Seed;

    [Range(1, 10)]
    public int Size = 2;

    public float WaterLevel = 0.15f;
}