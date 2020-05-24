using System;
using UnityEngine;

[Serializable]
public class MapData
{
    public int ChunkSize = 240;
    public bool CreateWater = true;

    [Range(0, 10)]
    public int CreaturesToSpawn = 3;

    public bool Flat = false;

    public AnimationCurve HeightCurve;

    [Range(1, 20)]
    public float HeightScale;

    public bool Populate = true;

    public string Seed;

    [Range(1, 10)]
    public int Size = 2;

    public float StructureLevel = 1.5f;
    public float WaterLevel = 1.5f;
}