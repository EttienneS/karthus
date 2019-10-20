using System.Collections.Generic;
using System.Linq;

public class Biome
{
    private SortedDictionary<float, CellType> _mapKey = new SortedDictionary<float, CellType>();

    private float[,] _noiseMap;

    public Biome(params (float min, CellType type)[] param)
    {
        _noiseMap = Noise.GenerateNoiseMap(Game.Map.Width, Game.Map.Height,
            Game.Map.Seed,
            Game.Map.Scale,
            Game.Map.Octaves,
            Game.Map.Persistance,
            Game.Map.Lancunarity,
            Game.Map.Offset);

        foreach (var p in param)
        {
            Add(p.min, p.type);
        }
    }

    public void Add(float min, CellType cell)
    {
        _mapKey.Add(min, cell);
    }

    public float GetCellHeight(int x, int y)
    {
        return _noiseMap[x, y];
    }

    public CellType GetCellType(float value)
    {
        var reversedMap = _mapKey.Reverse();
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
        if (_mapKey.Count > 1)
        {
            var reversedMap = _mapKey.Reverse();
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