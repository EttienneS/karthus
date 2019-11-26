using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Biome
{
    private float[,] _noiseMap;

    public Biome()
    {
    }

    public Biome(string name, params BiomeRegion[] regions)
    {
        Name = name;
        BiomeRegions = regions.ToList();
    }

    public List<BiomeRegion> BiomeRegions { get; set; }
    public string Name { get; set; }

    [JsonIgnore]
    public float[,] NoiseMap
    {
        get
        {
            if (_noiseMap == null)
            {
                _noiseMap = Noise.GenerateNoiseMap(Game.Map.Width * 2, Game.Map.Height * 2,
                                                   Random.Range(1, 10000),
                                                   Random.Range(25, 40),
                                                   4, 0.4f, 4, new Vector2(0, 0));
            }
            return _noiseMap;
        }
    }

    public float GetCellHeight(int x, int y)
    {
        return NoiseMap[x, y];
    }

    public BiomeRegion GetRegion(float value)
    {
        var regions = new List<BiomeRegion>();
        foreach (var region in BiomeRegions)
        {
            if (value >= region.Min && value <= region.Max)
            {
                regions.Add(region);
            }
        }

        return regions.GetRandomItem();
    }

    internal (float, float) GetCellTypeRange(string cellType)
    {
        var region = BiomeRegions.First(r => r.SpriteName == cellType);
        return (region.Min, region.Max);
    }
}

public class BiomeRegion
{
    public BiomeRegion()
    {
    }

    public BiomeRegion(float min, float max, string spriteName, float travelCost, params (string structure, float probablity)[] contents)
    {
        Min = min;
        Max = max;
        TravelCost = travelCost;

        Content = new Dictionary<string, float>();
        foreach (var (structure, probablity) in contents)
        {
            Content.Add(structure, probablity);
        }

        SpriteName = spriteName;
    }

    public Dictionary<string, float> Content { get; set; }
    public float Max { get; set; }
    public float Min { get; set; }
    public string SpriteName { get; set; }
    public float TravelCost { get; set; } = 1f;

    internal string GetContent()
    {
        if (Content.Count == 0)
        {
            return string.Empty;
        }

        var random = Random.value;
        var options = Content.Where(c => random <= c.Value).Select(o => o.Key).ToList();
        if (options.Count > 0)
        {
            return options.GetRandomItem();
        }

        return string.Empty;
    }
}