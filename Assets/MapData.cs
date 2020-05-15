using System;
using UnityEngine;

[Serializable]
public class MapData
{
    internal int ChunkSize = 240;
    public bool CreateWater = true;
    public bool Flat = false;

    public AnimationCurve HeightCurve;

    [Range(1, 20)]
    public float HeightScale;

    public bool Populate = true;

    [Range(1, 10)]
    public int Size = 2;

    public float WaterLevel = 1.5f;

    [Range(0, 10)]
    public int CreaturesToSpawn = 3;

    public string Seed;

}