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

    public CellType GetCellType(int x, int y)
    {
        var value = _noiseMap[x, y];

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
}