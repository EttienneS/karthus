using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Biome
{
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
    public float Rarity { get; set; }

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

        if (regions.Count == 0)
        {
            Debug.LogWarning($"{Name} biome does not contain an entry for {value}");
            return BiomeRegions[0];
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