using System.Collections.Generic;
using System.Linq;

public class MapPreset
{
    private SortedDictionary<float, CellType> _mapKey = new SortedDictionary<float, CellType>();

    private float[,] _noiseMap;

    public MapPreset(params (float min, CellType type)[] param)
    {
        _noiseMap = Noise.GenerateNoiseMap(Game.MapGrid.MapSize, Game.MapGrid.MapSize,
            Game.MapGrid.Seed,
            Game.MapGrid.Scale,
            Game.MapGrid.Octaves,
            Game.MapGrid.Persistance,
            Game.MapGrid.Lancunarity,
            Game.MapGrid.Offset);

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

        return (0f, 0f);
    }
}