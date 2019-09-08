using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class Core
{
    public List<CellData> Cells;
    public CellData Center;
    public int Radius;
    public List<Core> SubCores;
    public float damper = 0.5f;
    public float Momentum;

    public Core Parent;

    public Core(CellData center, int radius, float momentum, Core parent)
    {
        Parent = parent;
        Center = center;
        Radius = radius;
        Momentum = momentum * damper;
        Cells = Game.MapGrid.GetCircle(Center.Coordinates, radius);
        SubCores = new List<Core>();
    }

    public Core GetRoot()
    {
        if (Parent == null)
        {
            return this;
        }
        else
        {
            return Parent.GetRoot();
        }
    }

    public bool CellAvailable(CellData cell)
    {
        foreach (var core in SubCores)
        {
            if (core.Cells.Contains(cell))
            {
                return false;
            }
            if (!core.CellAvailable(cell))
            {
                return false;
            }
        }

        return true;
    }

    public const int MinStructureSize = 8;
    public const int MaxStructureSize = 11;
    public const int MinDistance = 10;
    public const int MaxDistance = 15;

    public void Propagate()
    {
        if (Momentum < 0.1f)
        {
            return;
        }

        var radians = new List<int> { 60, 120, 180, 240, 300, 360 };
        for (int i = 0; i < 5; i++)
        {
            var rad = radians[Random.Range(0, radians.Count - 1)];
            radians.Remove(rad);

           // rad += Random.Range(-15, 15);
            var offPoint = Game.MapGrid.GetCellAttRadian(Center.Coordinates, Random.Range(MinDistance, MaxDistance), rad);
            if (offPoint == null)
            {
                continue;
            }

            if (!TryGetSubCore(offPoint, out var core))
            {
                continue;
            }

            SubCores.Add(core);
        }
    }

    public bool TryGetSubCore(CellData center, out Core core)
    {
        if (center == null)
        {
            core = null;
            return false;
        }
        core = new Core(center, Random.Range(MinStructureSize, MaxStructureSize), Momentum, this);

        return core.Valid();
    }

    internal void Link()
    {
        foreach (var core in SubCores)
        {
            Game.MapGenerator.MakeRoad(Center, core.Center);
            core.Link();
        }
    }

    internal void Draw()
    {
        if (SubCores.Count == 0)
        {
            var originX = Center.Coordinates.X - (Radius / 2);
            var originY = Center.Coordinates.Y - (Radius / 2);

            var square = Game.MapGrid.GetRectangle(originX, originY, Radius, Radius);
            var subSquare = Game.MapGrid.GetRectangle(originX + 1, originY + 1, Radius - 2, Radius - 2);

            square.RemoveAll(c => subSquare.Contains(c));

            foreach (var cell in square)
            {
                cell.SetStructure(Game.StructureController.GetStructure("Stone Wall", FactionController.WorldFaction));
            }

            foreach (var cell in subSquare)
            {
                cell.SetStructure(Game.StructureController.GetStructure("Wood Tile", FactionController.WorldFaction));
            }

            foreach (var cell in square)
            {
                if (cell.Neighbors.Count(c => c?.Structure?.Name == "Road") >= 2)
                {
                    cell.SetStructure(Game.StructureController.GetStructure("Wood Tile", FactionController.WorldFaction));
                    break;
                }
            }
        }
        else
        {
            foreach (var cell in Game.MapGrid.GetCircle(Center.Coordinates, 4))
            {
                if (cell.Structure == null)
                    cell.SetStructure(Game.StructureController.GetStructure("Road", FactionController.WorldFaction));
            }

            foreach (var subCore in SubCores)
            {
                subCore.Draw();
            }
        }
    }

    internal bool Valid()
    {
        var root = GetRoot();
        foreach (var cell in Cells)
        {
            if (cell.TravelCost < 0)
            {
                return false;
            }
            if (!root.CellAvailable(cell))
            {
                return false;
            }
        }

        return true;
    }
}