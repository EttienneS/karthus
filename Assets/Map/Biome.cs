using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Biome
{
    public SortedDictionary<float, CellType> Map = new SortedDictionary<float, CellType>();

    private float[,] _noiseMap;

    public Biome()
    {
    }

    public Biome(params (float min, CellType type)[] param)
    {
        foreach (var p in param)
        {
            Add(p.min, p.type);
        }
    }

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

    public void Add(float min, CellType cell)
    {
        Map.Add(min, cell);
    }

    public float GetCellHeight(int x, int y)
    {
        return NoiseMap[x, y];
    }

    public CellType GetCellType(float value)
    {
        var reversedMap = Map.Reverse();
        foreach (var kvp in reversedMap)
        {
            if (value > kvp.Key)
            {
                return kvp.Value;
            }
        }

        return reversedMap.Last().Value;
    }

    internal (float, float) GetCellTypeRange(CellType cellType)
    {
        if (Map.Count > 1)
        {
            var reversedMap = Map.Reverse();
            var last = 0f;

            foreach (var kvp in reversedMap)
            {
                if (cellType == kvp.Value)
                {
                    return (kvp.Key, last);
                }
                last = kvp.Key;
            }
        }
        return (0f, 1f);
    }
}