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

    [Range(1, 6)]
    public int LOD = 1;

    public bool Populate = true;

    [Range(1, 10)]
    public int Size = 2;

    public float WaterLevel = 1.5f;
}