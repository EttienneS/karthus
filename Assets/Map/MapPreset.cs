using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapPreset
{
    private SortedDictionary<float, CellType> _mapKey = new SortedDictionary<float, CellType>();

    public MapPreset(float freq, params (float min, CellType type)[] param)
    {
        Frequency = freq;

        foreach (var p in param)
        {
            Add(p.min, p.type);
        }
    }

    public float Frequency { get; set; }

    public void Add(float min, CellType cell)
    {
        _mapKey.Add(min, cell);
    }

    public CellType GetCellType(int x, int y)
    {
        var value = Noise.Perlin3D(new Vector2(x, y), Frequency);

        foreach (var kvp in _mapKey.Reverse())
        {
            if (value > kvp.Key)
            {
                return kvp.Value;
            }
        }

        return _mapKey.Last().Value;
    }
}