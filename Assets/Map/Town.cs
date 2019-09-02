using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Town
{
    public CellData Center;
    public int Radius;
    public List<Core> Cores;
    public int CoreCount;

    public Town(CellData center, int cores, int radius)
    {
        Center = center;
        Cores = new List<Core>();
        CoreCount = cores;
        Radius = radius;
    }

    public bool ValidatePosition()
    {
        var cells = Game.MapGrid.GetCircle(Center.Coordinates, Radius * 2);
        return cells.Count(c => c.TravelCost >= 0) / cells.Count >= 0.75;
    }

    public void Generate()
    {
        for (int i = 0; i < CoreCount; i++)
        {
            var offset = Random.Range(5, 8);
            var core = new Core(Game.MapGrid.GetRandomRadian(Center.Coordinates, Radius + offset), offset);
            if (!core.Valid(this))
            {
                continue;
            }

            core.Propagte(0.9f, 10, 0.9f, this);

            Cores.Add(core);
        }

        foreach (var core in Cores)
        {
            Game.MapGenerator.MakeRoad(Center, core.Center, false);
            core.Draw();
        }
    }

    public bool CellAvailable(CellData cell)
    {
        foreach (var core in Cores)
        {
            if (!core.CellAvailable(cell))
            {
                return false;
            }
        }

        return true;
    }
}
